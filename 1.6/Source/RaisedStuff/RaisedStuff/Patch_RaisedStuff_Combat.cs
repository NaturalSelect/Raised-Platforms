using HarmonyLib;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System;
using System.Linq;
using RimWorld;
using Verse.Sound;

namespace RaisedStuff;

// BULLET PATCH - bullets can fly over lower levels!
[HarmonyPatch(typeof(Projectile), "CanHit")]
public static class Patch_RaisedTarget_Projectile_FlyOver
{
    public static bool Prefix(Thing thing, Projectile __instance, ref bool __result, Vector3 ___origin, Vector3 ___destination)
    {
        RaisedStuffManager cachedLevelManager = __instance.Map.GetComponent<RaisedStuffManager>();
        if (thing.Spawned && thing != __instance.intendedTarget && cachedLevelManager.raisedGrid[___origin.ToIntVec3()] != 0 || cachedLevelManager.raisedGrid[___destination.ToIntVec3()] != 0)
        {
            __result = !RaisedStuffUtility.LineGoesAbove(___origin.ToIntVec3(), ___destination.ToIntVec3(), thing.Position, __instance.Map, cachedLevelManager : cachedLevelManager);
            return __result;
        }
        return true;
    }
}

// EXPLOSION PATCH - explosions should attenuate with height.
[HarmonyPatch(typeof(DamageWorker), "ExplosionCellsToHit", [typeof(IntVec3), typeof(Map), typeof(float), typeof(IntVec3?), typeof(IntVec3?), typeof(FloatRange?)])]
public static class Patch_RaisedTarget_Explosion_Attenuate
{
    public static void PostFix(IntVec3 center, Map map, float radius, ref IEnumerable<IntVec3> __result)
    {
        RaisedStuffManager cachedLevelManager = map.GetComponent<RaisedStuffManager>();
        __result = __result.Where(x => ((x - center).ToVector3() + Vector3.up * (cachedLevelManager.raisedGrid[x] - cachedLevelManager.raisedGrid[center])).magnitude <= radius);
    }
}


// GenSight Patches

[HarmonyPatch(typeof(GenSight), "LineOfSight", [typeof(IntVec3), typeof(IntVec3), typeof(Map), typeof(bool), typeof(Func<IntVec3, bool>), typeof(int), typeof(int)])]
public static class Patch_GenSight_LineOfSight
{
    public static bool Prefix(ref bool __result, IntVec3 start, IntVec3 end, Map map, bool skipFirstCell = false, Func<IntVec3, bool> validator = null, int halfXOffset = 0, int halfZOffset = 0)
    {
        if (!start.InBounds(map) || !end.InBounds(map))
        {
            __result = true;
            return false;
        }
        RaisedStuffManager cachedLevelManager = map.GetComponent<RaisedStuffManager>();
        if (cachedLevelManager.raisedGrid[start] != 0 || cachedLevelManager.raisedGrid[end] != 0)
        {
            __result = RaisedStuffUtility.LineOfSight(start, end, map, skipFirstCell, validator, halfXOffset, halfZOffset, cachedLevelManager : cachedLevelManager);
            return false;
        }
        return true;
    }
}
[HarmonyPatch(typeof(GenSight), "LineOfSight", [typeof(IntVec3), typeof(IntVec3), typeof(Map), typeof(CellRect), typeof(CellRect), typeof(Func<IntVec3, bool>), typeof(bool)])]
public static class Patch_GenSight_LineOfSightB
{
    public static bool Prefix(ref bool __result, IntVec3 start, IntVec3 end, Map map, CellRect startRect, CellRect endRect, Func<IntVec3, bool> validator = null, bool forLeaning = false)
    {
        if (!start.InBounds(map) || !end.InBounds(map))
        {
            __result = true;
            return false;
        }
        RaisedStuffManager cachedLevelManager = map.GetComponent<RaisedStuffManager>();
        if (cachedLevelManager.raisedGrid[start] != 0 || cachedLevelManager.raisedGrid[end] != 0)
        {
            __result = RaisedStuffUtility.LineOfSight(start, end, map, startRect, endRect, validator, forLeaning, cachedLevelManager: cachedLevelManager);
            return false;
        }
        return true;
    }
}

//[HarmonyPatch(typeof(GenSight), "LastPointOnLineOfSight")]





// BEAM PATCH - beams can fire up and down! (note - doesn't use GenSight logic, so needed to be separately implemented.)
[HarmonyPatch(typeof(Verb_ShootBeam), "TryGetHitCell")]
public static class Patch_RaisedTarget_Beam_HitCell
{
    public static bool Prefix(IntVec3 source, IntVec3 targetCell, out IntVec3 hitCell, VerbProperties ___verbProps, Thing ___caster, ref bool __result)
    {
        hitCell = default(IntVec3);
        RaisedStuffManager cachedLevelManager = ___caster.Map.GetComponent<RaisedStuffManager>();
        if (cachedLevelManager.raisedGrid[source] != 0 || cachedLevelManager.raisedGrid[targetCell] != 0)
        {
            IntVec3 intVec = GenSight.LastPointOnLineOfSight(source, targetCell, (IntVec3 c) => c.InBounds(___caster.Map) && RaisedStuffUtility.LineGoesAbove(source, targetCell, c, ___caster.Map, cachedLevelManager : cachedLevelManager), skipFirstCell: true);
            if (___verbProps.beamCantHitWithinMinRange && intVec.DistanceTo(source) < ___verbProps.minRange)
            {
                __result = false;
            }

            hitCell = (intVec.IsValid ? intVec : targetCell);
            __result = intVec.IsValid;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Verb_ShootBeam), "BurstingTick")] // why, ludeon, why
public static class Patch_RaisedTarget_Beam_BurstingTick
{
    public static bool Prefix(ref int ___ticksToNextPathStep, Thing ___caster, Verb_ShootBeam __instance, VerbProperties ___verbProps, ref MoteDualAttached ___mote, ref Effecter ___endEffecter, ref Sustainer ___sustainer)
    {
        RaisedStuffManager cachedLevelManager = ___caster.Map.GetComponent<RaisedStuffManager>();
        if (cachedLevelManager.raisedGrid[___caster.Position] != 0 || cachedLevelManager.raisedGrid[__instance.InterpolatedPosition.ToIntVec3()] != 0)
        {
            ___ticksToNextPathStep--;
            Vector3 vector = __instance.InterpolatedPosition;
            IntVec3 intVec = vector.ToIntVec3();
            Vector3 vector2 = __instance.InterpolatedPosition - ___caster.Position.ToVector3Shifted();
            float num = vector2.MagnitudeHorizontal();
            Vector3 normalized = vector2.Yto0().normalized;
            IntVec3 intVec2 = GenSight.LastPointOnLineOfSight(___caster.Position, intVec, (IntVec3 c) => RaisedStuffUtility.LineGoesAbove(___caster.Position, intVec, c, ___caster.Map, cachedLevelManager : cachedLevelManager), skipFirstCell: true);
            if (intVec2.IsValid)
            {
                num -= (intVec - intVec2).LengthHorizontal;
                vector = ___caster.Position.ToVector3Shifted() + normalized * num;
                intVec = vector.ToIntVec3();
            }

            Vector3 offsetA = normalized * ___verbProps.beamStartOffset;
            Vector3 vector3 = vector - intVec.ToVector3Shifted();
            if (___mote != null)
            {
                ___mote.UpdateTargets(new TargetInfo(___caster.Position, ___caster.Map), new TargetInfo(intVec, ___caster.Map), offsetA, vector3);
                ___mote.Maintain();
            }

            if (___verbProps.beamGroundFleckDef != null && Rand.Chance(___verbProps.beamFleckChancePerTick))
            {
                FleckMaker.Static(vector, ___caster.Map, ___verbProps.beamGroundFleckDef);
            }

            if (___endEffecter == null && ___verbProps.beamEndEffecterDef != null)
            {
                ___endEffecter = ___verbProps.beamEndEffecterDef.Spawn(intVec, ___caster.Map, vector3);
            }

            if (___endEffecter != null)
            {
                ___endEffecter.offset = vector3;
                ___endEffecter.EffectTick(new TargetInfo(intVec, ___caster.Map), TargetInfo.Invalid);
                ___endEffecter.ticksLeft--;
            }

            if (___verbProps.beamLineFleckDef != null)
            {
                float num2 = 1f * num;
                for (int i = 0; (float)i < num2; i++)
                {
                    if (Rand.Chance(___verbProps.beamLineFleckChanceCurve.Evaluate((float)i / num2)))
                    {
                        Vector3 vector4 = i * normalized - normalized * Rand.Value + normalized / 2f;
                        FleckMaker.Static(___caster.Position.ToVector3Shifted() + vector4, ___caster.Map, ___verbProps.beamLineFleckDef);
                    }
                }
            }

            ___sustainer?.Maintain();
            return false;
        }
        return true;
    }
}