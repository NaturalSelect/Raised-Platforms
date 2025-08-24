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
        RaisedStuffManager cachedLevelManager = map.GetComponent<RaisedStuffManager>();
        if (def.Size.x == 1 && def.Size.z == 1)
        {
            if (cachedLevelManager.raisedGrid[loc] != 0)
            {
                return new AcceptanceReport("RaisedStuff_CannotPlaceOverExistingPlatform".Translate());
            }
        }
        else
        {
            foreach (IntVec3 item in GenAdj.OccupiedRect(loc, rot, def.Size))
            {
                if (cachedLevelManager.raisedGrid[loc] != 0)
                {
                    return new AcceptanceReport("RaisedStuff_CannotPlaceOverExistingPlatform".Translate());
                }
            }
        }
        return true;
    }
}
