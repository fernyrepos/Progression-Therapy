using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ProgressionTherapy
{
    public static class TraumaAndIntegrityCompat
    {
        private static readonly MethodInfo GetDataMethod;
        private static readonly FieldInfo TraumaticTraitField;
        private static readonly FieldInfo TemperedField;
        private static readonly FieldInfo TraumaField;
        public static bool IsActive { get; } = ModsConfig.IsActive("ferny.TraumaAndIntegrity");
        static TraumaAndIntegrityCompat()
        {
            if (!IsActive) return;
            var storeType = AccessTools.TypeByName("TraumaAndIntegrity.Pawn_ExposeData_Patch");
            GetDataMethod = AccessTools.Method(storeType, "GetTraumaIntegrityData", new[] { typeof(Pawn) });
            var dataType = AccessTools.TypeByName("TraumaAndIntegrity.TraumaIntegrityData");
            TraumaticTraitField = AccessTools.Field(dataType, "traumaticTrait");
            TemperedField = AccessTools.Field(dataType, "tempered");
            TraumaField = AccessTools.Field(dataType, "trauma");
        }

        public static TraitDef GetTraumaticTrait(Pawn pawn)
        {
            if (!IsActive) return null;
            var data = GetDataMethod.Invoke(null, new object[] { pawn });
            return (TraitDef)TraumaticTraitField.GetValue(data);
        }

        public static void ClearTraumaticTrait(Pawn pawn)
        {
            if (!IsActive) return;
            var data = GetDataMethod.Invoke(null, new object[] { pawn });
            TemperedField.SetValue(data, false);
            TraumaField.SetValue(data, 0f);
            TraumaticTraitField.SetValue(data, null);
        }
    }
}
