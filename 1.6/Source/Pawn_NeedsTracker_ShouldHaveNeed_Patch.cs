using HarmonyLib;
using RimWorld;

namespace ProgressionTherapy;

[HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
public static class Pawn_NeedsTracker_ShouldHaveNeed_Patch
{
    public static void Postfix(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result)
    {
        if (nd == DefsOf.PT_MentalStability && __instance.ShouldHaveNeed(DefsOf.Mood) && __instance.pawn.RaceProps.Humanlike)
        {
            __result = ProgressionTherapyMod.settings.enableMentalStability;
        }
    }
}
