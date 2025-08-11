using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace RaisedStuff;

[StaticConstructorOnStartup]
public class Gizmo_PlatformLevel : Gizmo_Slider
{

    public CompScaffold platform;

    protected override float Target
    {
        get
        {
            return (platform.targetLevel - platform.Props.minLevel) / (float)(platform.Props.maxLevel - platform.Props.minLevel);
        }
        set
        {
            platform.UpdateLevelWithWork(Mathf.RoundToInt(value * (platform.Props.maxLevel - platform.Props.minLevel) + platform.Props.minLevel));
        }
    }
    protected override float ValuePercent => (platform.level - platform.Props.minLevel) / (float)(platform.Props.maxLevel - platform.Props.minLevel);
    protected override string Title => platform.Props.platformGizmolabel;
    protected override bool IsDraggable => true;
    protected override string BarLabel => platform.level + (platform.workRemaining > 0 ? " ({0})".Formatted(platform.targetLevel) : "");

    protected bool draggingBar;
    protected override bool DraggingBar
    {
        get
        {
            return draggingBar;
        }
        set
        {
            draggingBar = value;
        }
    }

    protected override IEnumerable<float> GetBarThresholds()
    {
        for (int i = platform.Props.minLevel; i < platform.Props.maxLevel; i++)
        {
            yield return i / (float)(platform.Props.maxLevel - platform.Props.minLevel);
        }
    }

    public Gizmo_PlatformLevel(CompScaffold plat)
    {
        platform = plat;
    }

    protected override string GetTooltip()
    {
        return "";
    }

}