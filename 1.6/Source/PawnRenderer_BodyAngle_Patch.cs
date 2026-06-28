using HarmonyLib;
using Verse;

namespace ProgressionTherapy;

[HarmonyPatch(typeof(PawnRenderer), "BodyAngle")]
public static class PawnRenderer_BodyAngle_Patch
{
    public static void Postfix(PawnRenderer __instance, ref float __result)
    {
        var pawn = __instance.pawn;
        var couch = pawn.GetCouchIfApplicable();
        if (couch != null)
        {
            var rotation = couch.Rotation;
            rotation.AsInt += 2;
            __result = rotation.AsAngle;
        }
    }
}
