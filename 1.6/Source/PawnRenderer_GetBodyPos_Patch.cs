using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ProgressionTherapy;

[HarmonyPatch(typeof(PawnRenderer), "GetBodyPos")]
public static class PawnRenderer_GetBodyPos_Patch
{
    public static void Postfix(PawnRenderer __instance, ref bool showBody, ref Vector3 __result)
    {
        var pawn = __instance.pawn;
        var couch = pawn.GetCouchIfApplicable();
        if (couch != null)
        {
            showBody = true;
            var headCell = BedUtility.GetSleepingSlotPos(0, couch.Position, couch.Rotation, couch.def.size);
            var vector = headCell.ToVector3ShiftedWithAltitude(AltitudeLayer.LayingPawn);
            var rotation = couch.Rotation;
            rotation.AsInt += 2;
            var num = __instance.BaseHeadOffsetAt(Rot4.South).z + pawn.story.bodyType.bedOffset;
            var vector2 = rotation.FacingCell.ToVector3();
            __result = vector - vector2 * num;
        }
    }

    public static Thing GetCouchIfApplicable(this Pawn pawn)
    {
        if (pawn.pather?.Moving is false && pawn.jobs?.curDriver is JobDriver_AttendTherapyClass)
        {
            var thingList = pawn.Position.GetThingList(pawn.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (thingList[i].def == DefsOf.PT_TherapyCouch)
                {
                    return thingList[i];
                }
            }
        }
        return null;
    }
}
