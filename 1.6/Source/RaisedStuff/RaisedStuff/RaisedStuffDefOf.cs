using Verse;
using RimWorld;

namespace RaisedStuff;

[DefOf]
public static class RaisedStuffDefOf
{
    public static JobDef MJ_RaisedStuff_RebuildPlatform;

    static RaisedStuffDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(RaisedStuffDefOf));
    }
}
