using Verse;

namespace RaisedStuff;

public class CompProperties_Scaffold : CompProperties
{
    public int level = 1;

    public int maxLevel = 5;

    public int minLevel = 1;

    public string platformGizmolabel = "Level";
    public CompProperties_Scaffold()
    {
        compClass = typeof(CompScaffold);
    }
}
