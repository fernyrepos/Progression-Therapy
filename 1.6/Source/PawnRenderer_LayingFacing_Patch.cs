using HarmonyLib;
using Verse;

namespace ProgressionTherapy;

[HarmonyPatch(typeof(PawnRenderer), "LayingFacing")]
public static class PawnRenderer_LayingFacing_Patch
{
    public static void Postfix(PawnRenderer __instance, ref Rot4 __result)
    {
        var pawn = __instance.pawn;
        var couch = pawn.GetCouchIfApplicable();
        if (couch != null)
        {
            __result = Rot4.South;
        }
    }
}
