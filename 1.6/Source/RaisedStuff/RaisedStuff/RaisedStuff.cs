using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RaisedStuff;

public class RaisedStuff : Mod
{
    public static RaisedStuffSettings Settings;

    public RaisedStuff(ModContentPack content) : base(content)
    {
        Settings = GetSettings<RaisedStuffSettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Settings.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Raised Platforms";
    }
}
