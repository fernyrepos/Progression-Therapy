using RimWorld;
using Verse;

namespace ProgressionTherapy;

public class StatPart_MentalStability : StatPart
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!TryGetNeed(req, out var need)) return;
        var nerf = ProgressionTherapyMod.settings.globalMentalBreakNerf;
        val += nerf - need.CurLevelPercentage * nerf;
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!TryGetNeed(req, out var need)) return null;
        var nerf = ProgressionTherapyMod.settings.globalMentalBreakNerf;
        var offset = nerf - need.CurLevelPercentage * nerf;
        return offset != 0 ? "PT_MentalStabilityOffset".Translate(offset.ToStringPercentSigned()) : null;
    }

    private static bool TryGetNeed(StatRequest req, out Need_MentalStability need)
    {
        need = null;
        if (!ProgressionTherapyMod.settings.enableMentalStability) return false;
        if (req.Thing is not Pawn pawn || pawn.RaceProps.Humanlike is false) return false;
        return pawn.needs.TryGetNeed<Need_MentalStability>() is { } n && (need = n) != null;
    }
}
