using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RaisedStuff;

public class PlaceWorker_Scaffold : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        if (def.Size.x == 1 && def.Size.z == 1)
        {
            if (loc.RaisedGridLevelBase(map) > 0)
            {
                return new AcceptanceReport("Cannot build over existing platform.");
            }
        }
        else
        {
            foreach (IntVec3 item in GenAdj.OccupiedRect(loc, rot, def.Size))
            {
                if (loc.RaisedGridLevelBase(map) > 0)
                {
                    return new AcceptanceReport("Cannot build over existing platform.");
                }
            }
        }
        return true;
    }
}
