using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse.Noise;
using Verse;

namespace RaisedStuff;

public class WorkGiver_RebuildPlatform : WorkGiver_Scanner
{

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Deadly;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t.Faction != pawn.Faction)
        {
            return null;
        }

        IntVec3 result;
        if (!pawn.CanReserveAndReach(t, PathEndMode.Touch, MaxPathDanger(pawn)))
        {
            return null;
        }

        if (t.IsBurning())
        {
            return null;
        }

        if (!(t.TryGetComp<CompScaffold>() is CompScaffold comp))
        {
            return null;
        }

        if (comp.workRemaining < 0)
        {
            return null;
        }

        if (comp.targetLevel == comp.level)
        {
            return null;
        }

        return JobMaker.MakeJob(RaisedStuffDefOf.MJ_RaisedStuff_RebuildPlatform, t);
    }

}

