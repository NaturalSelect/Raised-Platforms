using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RaisedStuff;

public class JobDriver_RebuildPlatform : JobDriver
{
    private CompScaffold Scaffold => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompScaffold>();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        PathEndMode pathEndMode = PathEndMode.Touch;

        yield return Toils_Goto.GotoThing(TargetIndex.A, pathEndMode);

        Toil rebuild = ToilMaker.MakeToil("MakeNewToils");
        rebuild.initAction = delegate
        {
            GenClamor.DoClamor(rebuild.actor, 15f, ClamorDefOf.Construction);
        };
        rebuild.tickIntervalAction = delegate (int delta)
        {
            Pawn actor = rebuild.actor;
            CompScaffold scaffold = Scaffold;
            actor.skills.Learn(SkillDefOf.Construction, 0.25f * (float)delta);

            actor.rotationTracker.FaceTarget(actor.CurJob.GetTarget(TargetIndex.A));
            float num = actor.GetStatValue(StatDefOf.ConstructionSpeed) * 1.7f * (float)delta;
            if (job.GetTarget(TargetIndex.A).Thing.Stuff != null)
            {
                num *= job.GetTarget(TargetIndex.A).Thing.Stuff.GetStatValueAbstract(StatDefOf.ConstructionSpeedFactor);
            }

            scaffold.workRemaining -= num;
            if (scaffold.workRemaining < 0)
            {
                scaffold.UpdateLevel(scaffold.targetLevel);
                actor.jobs.EndCurrentJob(JobCondition.Succeeded);
            }
        };
        rebuild.WithEffect(base.TargetThingA.def.repairEffect, TargetIndex.A);
        rebuild.FailOnCannotTouch(TargetIndex.A, pathEndMode);
        rebuild.defaultCompleteMode = ToilCompleteMode.Never;
        rebuild.activeSkill = () => SkillDefOf.Construction;
        rebuild.handlingFacing = true;

        yield return rebuild;
    }
}
