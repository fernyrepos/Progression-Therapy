using ProgressionEducation;
using RimWorld;
using Verse.AI;

namespace ProgressionTherapy;

public class JobDriver_AttendTherapyClass : JobDriver_AttendClass
{
    protected override Toil MakeLearningToil()
    {
        var toil = base.MakeLearningToil();
        toil.AddPreInitAction(() => pawn.jobs.posture = PawnPosture.LayingInBed);
        toil.AddPreTickAction(() => pawn.jobs.posture = PawnPosture.LayingInBed);
        return toil;
    }
}
