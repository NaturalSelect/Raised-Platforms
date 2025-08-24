using UnityEngine;
using Verse;

namespace RaisedStuff;

public class RaisedStuffSettings : ModSettings
{
    //public bool debugLog = false;

    public float lineOfSightHeightOffset = 0.75f;

    public float defaultRoofHeight = 1.2f;
    public float thickRoofHeight = 10f;

    public int levelCachingTickInterval = 60;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref lineOfSightHeightOffset, "lineOfSightHeightOffset", 0.75f);

        Scribe_Values.Look(ref defaultRoofHeight, "defaultRoofHeight", 1.2f);
        Scribe_Values.Look(ref thickRoofHeight, "thickRoofHeight", 10f);
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        // -- LoS calculation tweaks

        Listing_Standard listing = new Listing_Standard();
        listing.Begin(inRect);

        listing.Label("Line of sight settings");

        listing.Label($"  Line of sight height offset: {lineOfSightHeightOffset}");
        lineOfSightHeightOffset = listing.Slider(lineOfSightHeightOffset, 0f, 1f);
        lineOfSightHeightOffset = Mathf.Clamp(lineOfSightHeightOffset, 0f, 1f);

        listing.Label($"  Height offset of roofs: {defaultRoofHeight}");
        defaultRoofHeight = listing.Slider(defaultRoofHeight, 1f, 2f);
        defaultRoofHeight = Mathf.Clamp(defaultRoofHeight, 1f, 2f);

        listing.Label($"  Height offset of thick (mountain) roofs: {thickRoofHeight}");
        thickRoofHeight = listing.Slider(thickRoofHeight, 1f, 20f);
        thickRoofHeight = Mathf.Clamp(thickRoofHeight, 1f, 20f);

        // -- caching options

        listing.Label("Caching options");

        listing.Label($"  Column fill caching tick interval: {levelCachingTickInterval}");
        levelCachingTickInterval = (int)listing.Slider(levelCachingTickInterval, 1, 600);
        levelCachingTickInterval = Mathf.Clamp(levelCachingTickInterval, 1, 600);

        // -- reset to defaults

        listing.Label(" ");
        if (Widgets.ButtonText(new Rect(0f, listing.CurHeight, 180f, 29f), "Reset to Defaults"))
        {
            //debugLog = false;
            lineOfSightHeightOffset = 0.75f;
            defaultRoofHeight = 1.2f;
            thickRoofHeight = 10f;

            levelCachingTickInterval = 60;
}

        listing.End();
    }
}

