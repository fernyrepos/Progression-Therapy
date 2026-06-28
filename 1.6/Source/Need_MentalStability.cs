using RimWorld;
using Verse;

namespace ProgressionTherapy;

public class Need_MentalStability(Pawn pawn) : Need(pawn)
{
    public override bool ShowOnNeedList => true;

    public override void NeedInterval()
    {
    }

    public override void SetInitialLevel()
    {
        var roll = Rand.Value;
        if (roll < 0.8f) CurLevelPercentage = Rand.Range(0.01f, 0.25f);
        else if (roll < 0.95f) CurLevelPercentage = Rand.Range(0.26f, 0.50f);
        else CurLevelPercentage = Rand.Range(0.51f, 1.0f);
    }
}
