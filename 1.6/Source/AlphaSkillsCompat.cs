using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ProgressionTherapy;

public static class AlphaSkillsCompat
{
    private static readonly MethodInfo ClearCacheForMethod;
    private static readonly int TraumaticPassionIndex;
    public static bool IsActive { get; } = ModsConfig.IsActive("sarg.alphaskills");

    static AlphaSkillsCompat()
    {
        if (!IsActive) return;
        var passionDefType = AccessTools.TypeByName("VSE.Passions.PassionDef");
        var def = GenDefDatabase.GetDef(passionDefType, "AS_TraumaticPassion");
        TraumaticPassionIndex = (byte)def.index;
        var cacheType = AccessTools.TypeByName("VSE.Passions.LearnRateFactorCache");
        ClearCacheForMethod = AccessTools.Method(cacheType, "ClearCacheFor");
    }

    public static IEnumerable<SkillDef> GetTraumaticSkills(Pawn pawn)
    {
        if (!IsActive || TraumaticPassionIndex == 0) yield break;
        foreach (var skill in pawn.skills.skills)
        {
            if ((uint)skill.passion == TraumaticPassionIndex)
            {
                yield return skill.def;
            }
        }
    }

    public static void RemoveTraumaticPassion(Pawn pawn, SkillDef targetSkill)
    {
        if (!IsActive || targetSkill == null) return;
        var skill = pawn.skills.GetSkill(targetSkill);
        if (skill == null) return;
        if ((uint)skill.passion != (uint)TraumaticPassionIndex) return;
        skill.passion = Passion.Major;
        ClearCacheForMethod.Invoke(null, new object[] { skill, null });
    }
}