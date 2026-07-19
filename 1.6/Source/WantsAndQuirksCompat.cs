using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using ProgressionEducation;
using RimWorld;
using Verse;

namespace ProgressionTherapy
{
    [HotSwappable]
    public static class WantsAndQuirksCompat
    {
        private static readonly MethodInfo HasTraumaticDesireMethod;
        private static readonly MethodInfo GetTraumaticDesireDefNamesMethod;
        private static readonly MethodInfo ResolveTraumaticDesireMethod;
        private static readonly System.Type WantDefType;
        public static bool IsActive { get; } = ModsConfig.IsActive("ferny.characterdevelopment");
        static WantsAndQuirksCompat()
        {
            if (!IsActive) return;
            var utilType = AccessTools.TypeByName("WantsAndQuirks.WantsAndQuirksUtility");
            HasTraumaticDesireMethod = AccessTools.Method(utilType, "HasTraumaticDesire", new[] { typeof(Pawn) });
            GetTraumaticDesireDefNamesMethod = AccessTools.Method(utilType, "GetTraumaticDesireDefNames", new[] { typeof(Pawn) });
            ResolveTraumaticDesireMethod = AccessTools.Method(utilType, "ResolveTraumaticDesire", new[] { typeof(Pawn), typeof(string) });
            WantDefType = GenTypes.GetTypeInAnyAssembly("WantsAndQuirks.WantDef");
        }

        public static bool HasTraumaticDesire(Pawn pawn)
        {
            if (!IsActive) return false;
            return (bool)HasTraumaticDesireMethod.Invoke(null, new object[] { pawn });
        }

        public static List<string> GetTraumaticDesireDefNames(Pawn pawn)
        {
            if (!IsActive) return new List<string>();
            return (List<string>)GetTraumaticDesireDefNamesMethod.Invoke(null, new object[] { pawn });
        }

        public static void ResolveTraumaticDesire(Pawn pawn, string defName)
        {
            if (!IsActive) return;
            ResolveTraumaticDesireMethod.Invoke(null, new object[] { pawn, defName });
        }

        public static string GetTraumaticDesireLabel(string defName)
        {
            if (!IsActive) return null;
            return GenDefDatabase.GetDef(WantDefType, defName).LabelCap;
        }
    }
}
