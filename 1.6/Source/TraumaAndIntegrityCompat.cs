using System.Reflection;
using HarmonyLib;
using ProgressionEducation;
using RimWorld;
using Verse;

namespace ProgressionTherapy
{   
    [HotSwappable]
    public static class TraumaAndIntegrityCompat
    {
        private static readonly MethodInfo GetDataMethod;
        private static readonly MethodInfo TraumaticTraitMethod;
        public static bool IsActive { get; } = ModsConfig.IsActive("ferny.TraumaAndIntegrity");
        static TraumaAndIntegrityCompat()
        {
            if (!IsActive) return;
            var storeType = AccessTools.TypeByName("TraumaAndIntegrity.Pawn_ExposeData_Patch");
            GetDataMethod = AccessTools.Method(storeType, "GetTraumaIntegrityData", new[] { typeof(Pawn) });
            var dataType = AccessTools.TypeByName("TraumaAndIntegrity.TraumaIntegrityData");
            TraumaticTraitMethod = AccessTools.Method(dataType, "GetTraumaticTrait");
        }

        public static TraitDef GetTraumaticTrait(Pawn pawn)
        {
            return GetResolvableTraumaticTrait(pawn)?.def;
        }

        public static Trait GetResolvableTraumaticTrait(Pawn pawn)
        {
            if (!IsActive || pawn?.story?.traits == null) return null;
            var data = GetDataMethod.Invoke(null, new object[] { pawn });
            var def = (TraitDef)TraumaticTraitMethod.Invoke(data, new object[] { pawn });
            if (def == null) return null;
            return pawn.story.traits.GetTrait(def);
        }
    }
}
