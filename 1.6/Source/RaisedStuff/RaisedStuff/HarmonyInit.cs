using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoofTurrets;

[StaticConstructorOnStartup]
internal static class HarmonyInit
{
    static HarmonyInit()
    {
        var harmony = new Harmony("Mjeiouws.RoofTurrets");
        harmony.PatchAll();
    }
}
