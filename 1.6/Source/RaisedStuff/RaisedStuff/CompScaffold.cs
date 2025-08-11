using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RaisedStuff;

[StaticConstructorOnStartup]
public class CompScaffold : ThingComp
{
    private static readonly Texture2D RaiseLevelCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise");
    private static readonly Texture2D LowerLevelCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/TempLower");
    private static readonly Texture2D LevelLevelCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport");

    public CompProperties_Scaffold Props => (CompProperties_Scaffold)props;

    public int level;

    public int targetLevel;

    public float workRemaining;

    protected RaisedStuffManager cachedLevelManager;
    public RaisedStuffManager LevelManager {
        get {
            if (cachedLevelManager == null) cachedLevelManager = parent.Map.GetComponent<RaisedStuffManager>();
            return cachedLevelManager;
        }
    }

    public void UpdateLevelWithWork(int targLevel)
    {
        if (targLevel != targetLevel)
        {
            targetLevel = targLevel;
            if (targLevel == level)
            {
                workRemaining = 0;
                return;
            }
            workRemaining = parent.GetStatValue(StatDefOf.WorkToBuild);
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (Find.Selector.SelectedObjects.Count == 1)
        {
            yield return new Gizmo_PlatformLevel(this);
        }
        else
        {
            // raise platform

            Command_Action command_raise = new Command_Action();
            command_raise.defaultLabel = "Raise platforms";
            command_raise.defaultDesc = "Raise platform height by one.";
            command_raise.icon = RaiseLevelCommandTex;

            if (level >= Props.maxLevel) command_raise.Disable("Cannot raise above level {0}.".Formatted(Props.maxLevel));
            else command_raise.action = () => UpdateLevelWithWork(targetLevel + 1);

            yield return command_raise;

            // lower platform

            Command_Action command_lower = new Command_Action();
            command_lower.defaultLabel = "Lower platforms";
            command_lower.defaultDesc = "Lower platform height by one.";
            command_lower.icon = LowerLevelCommandTex;

            if (targetLevel <= Props.minLevel) command_lower.Disable("Already at level {0}.".Formatted(Props.minLevel));
            else command_lower.action = () => UpdateLevelWithWork(targetLevel - 1);

            yield return command_lower;

            // level platforms

            Command_Action command_level = new Command_Action();
            command_level.defaultLabel = "Level platforms";
            command_level.defaultDesc = "Level platform heights to that of the lowest member.";
            command_level.icon = LevelLevelCommandTex;
            command_level.action = delegate
            {
                int ttarg = Find.Selector.SelectedObjects.Select(x => (x as ThingWithComps)?.TryGetComp<CompScaffold>().targetLevel ?? 999).Min();
                UpdateLevelWithWork(Math.Min(Math.Max(ttarg, Props.minLevel), Props.maxLevel));
            };

            yield return command_level;
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (!respawningAfterLoad)
        {
            level = Props.level;
            targetLevel = level;
        }
        UpdateLevel(level);
    }
    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        UpdateLevel(0);
    }
    public void UpdateLevel(int targLevel)
    {
        level = targLevel;
        foreach (IntVec3 c in parent.OccupiedRect())
        {
            LevelManager.raisedGrid[c] = level;
        }
    }

    public override string CompInspectStringExtra()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Platform level {0}".Formatted(level));
        if (workRemaining > 0)
        {
            sb.AppendInNewLine("{0} platform to level {1}".Formatted(targetLevel > level ? "Raising" : "Lowering", targetLevel));
            sb.AppendInNewLine("WorkLeft".Translate() + ": " + workRemaining.ToStringWorkAmount());
        }
        return sb.ToString();
    }

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref level, "Level", Props.level);
        Scribe_Values.Look(ref targetLevel, "targetLevel", Props.level);
        Scribe_Values.Look(ref workRemaining, "workRemaining", 0);
    }
}
