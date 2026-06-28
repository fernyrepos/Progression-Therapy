using System.Collections.Generic;
using System.Linq;
using ProgressionEducation;
using RimWorld;
using UnityEngine;
using Verse;

namespace ProgressionTherapy;

public enum TherapyFocusType
{
    ImproveMood,
    WorkThroughMemory,
    MentalStability,
    RestTraumaticPassion
}

[HotSwappable]
public class TherapyClassLogic : ClassSubjectLogic
{
    public TherapyFocusType focusType = TherapyFocusType.ImproveMood;
    public ThoughtDef targetMemoryDef;
    public Def targetTraumaticSkill;

    private const float MentalStabilityProgressMultiplier = 0.05f;
    private const int TraumaticPassionSemesterGoal = 60000;
    private const float MentalStabilityProgressDivisor = 10000f;
    private const float SocialLevelToScoreFactor = 0.02f;

    public TherapyClassLogic() { }
    public TherapyClassLogic(StudyGroup parent) : base(parent) { }

    public override bool IsEnabled => ProgressionTherapyMod.settings.enableTherapy;
    public override int MaxStudents => 1;
    public override string Label => "PT_Therapy".Translate();
    public override string Description => "PT_TherapyDesc".Translate();
    public override string BenchLabel => DefsOf.PT_TherapyCouch.label;
    public override string TeacherRoleLabel => "PT_TherapistRole".Translate();
    public override string StudentRoleLabel => "PT_PatientRole".Translate();
    public override JobDef LearningJob => DefsOf.PT_AttendTherapyClass;
    public override bool IsInfinite => focusType == TherapyFocusType.ImproveMood;
    public override int DefaultSemesterGoal => focusType == TherapyFocusType.ImproveMood ? 0 : 10000;

    public override float LearningSpeedModifier => 1f;

    public override float ProgressPerTick
    {
        get
        {
            if (studyGroup.teacher == null) return 0f;
            var baseScore = Mathf.Max(0, CalculateTeacherScore(studyGroup.teacher) * studyGroup.classroom.ClassSpeed);

            return focusType switch
            {
                TherapyFocusType.MentalStability => baseScore * MentalStabilityProgressMultiplier * ProgressionTherapyMod.settings.mentalStabilitySpeedMultiplier,
                TherapyFocusType.ImproveMood => baseScore * ProgressionTherapyMod.settings.moodSpeedMultiplier,
                TherapyFocusType.WorkThroughMemory => (baseScore * ProgressionTherapyMod.settings.memorySpeedMultiplier) * 1.5f,
                TherapyFocusType.RestTraumaticPassion => baseScore * ProgressionTherapyMod.settings.passionSpeedMultiplier,
                _ => baseScore
            };
        }
    }

    public override void ApplyLearningTick(Pawn student, int delta)
    {
        base.ApplyLearningTick(student, delta);
        if (focusType == TherapyFocusType.ImproveMood)
        {
            if (student.IsHashIntervalTick(1250))
            {
                student.needs.mood.thoughts.memories.TryGainMemory(DefsOf.PT_TherapyMoodBoost);
            }
        }
        else if (focusType == TherapyFocusType.MentalStability && ProgressionTherapyMod.settings.enableMentalStability && student.needs.TryGetNeed<Need_MentalStability>() is { } need)
        {
            need.CurLevel += ProgressPerTick * delta / MentalStabilityProgressDivisor;
        }
    }

    public override void GrantCompletionRewards()
    {
        var student = studyGroup.students.FirstOrDefault();
        if (student == null) return;

        if (focusType == TherapyFocusType.WorkThroughMemory)
        {
            var mem = student.needs.mood.thoughts.memories.Memories.FirstOrDefault(m => m.def == targetMemoryDef);
            if (mem != null)
            {
                student.needs.mood.thoughts.memories.RemoveMemory(mem);
            }
            student.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Catharsis);
        }
        else if (focusType == TherapyFocusType.RestTraumaticPassion)
        {
            AlphaSkillsCompat.RemoveTraumaticPassion(student, targetTraumaticSkill as SkillDef);
        }
    }

    public override string GetCompletionLetterLabel()
    {
        return "PT_TherapyCompleted".Translate();
    }

    public override string GetCompletionLetterText()
    {
        var student = studyGroup.students.FirstOrDefault();
        return focusType switch
        {
            TherapyFocusType.ImproveMood => "PT_TherapyCompletedDesc_ImproveMood".Translate(student),
            TherapyFocusType.WorkThroughMemory => "PT_TherapyCompletedDesc_WorkThroughMemory".Translate(student, targetMemoryDef.LabelCap),
            TherapyFocusType.MentalStability => "PT_TherapyCompletedDesc_MentalStability".Translate(student),
            TherapyFocusType.RestTraumaticPassion => "PT_TherapyCompletedDesc_RestTraumaticPassion".Translate(student, targetTraumaticSkill.LabelCap),
            _ => null
        };
    }

    public override HashSet<ThingDef> GetValidLearningBenches()
    {
        cachedValidLearningBenches ??= [DefsOf.PT_TherapyCouch];
        return cachedValidLearningBenches;
    }

    public override string GetReport()
    {
        if (studyGroup.ClassIsActive() is false)
        {
            return "PT_JobReport_WaitingForPatient".Translate();
        }
        return "PT_JobReport_PerformingTherapy".Translate();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref focusType, "focusType", TherapyFocusType.ImproveMood);
        Scribe_Defs.Look(ref targetMemoryDef, "targetMemoryDef");
        Scribe_Defs.Look(ref targetTraumaticSkill, "targetTraumaticSkill");
    }

    public override ClassSubjectLogic DeepClone(StudyGroup parent) => new TherapyClassLogic(parent)
    {
        focusType = focusType,
        targetMemoryDef = targetMemoryDef,
        targetTraumaticSkill = targetTraumaticSkill
    };

    public override float CalculateStudentScore(Pawn p) => 1f;

    public override float CalculateTeacherScore(Pawn teacher)
    {
        if (!IsTeacherQualified(teacher).Accepted) return 0f;
        return Mathf.Max(0, teacher.skills.GetSkill(SkillDefOf.Social).Level * CalculateSocialImpactFactor(teacher) * SocialLevelToScoreFactor);
    }

    public override AcceptanceReport IsStudentQualified(Pawn student)
    {
        var baseReport = base.IsStudentQualified(student);
        if (!baseReport.Accepted)
        {
            return baseReport;
        }
        if (student.Downed || student.InMentalState || student.Drafted)
        {
            return new AcceptanceReport("PT_PatientUnavailable".Translate(student.LabelShort));
        }
        return AcceptanceReport.WasAccepted;
    }

    public override AcceptanceReport IsTeacherQualified(Pawn teacher)
    {
        var social = teacher.skills.GetSkill(SkillDefOf.Social);
        if (social.Level < 15) return new AcceptanceReport("PT_TherapistNotQualified".Translate(teacher.LabelShort));
        var baseReport = base.IsTeacherQualified(teacher);
        if (!baseReport.Accepted)
        {
            return baseReport;
        }
        if (teacher.Downed || teacher.InMentalState || teacher.Drafted)
        {
            return new AcceptanceReport("PT_TherapistUnavailable".Translate(teacher.LabelShort));
        }
        return AcceptanceReport.WasAccepted;
    }

    private IEnumerable<Thought_Memory> GetValidMemories(Pawn pawn)
    {
        foreach (var mem in pawn.needs.mood.thoughts.memories.Memories)
        {
            if (mem.MoodOffset() < 0 && mem.def.DurationTicks > GenDate.TicksPerDay) yield return mem;
        }
    }

    public override void DrawConfigurationUI(Rect rect, ref float curY, IClassDialog classDialog)
    {
        var student = studyGroup.students.FirstOrDefault();
        if (student == null) return;

        DrawFocusTypeSelector(rect, ref curY, student);
        curY += 30f;

        switch (focusType)
        {
            case TherapyFocusType.WorkThroughMemory:
                DrawWorkThroughMemoryUI(rect, ref curY, student);
                break;
            case TherapyFocusType.RestTraumaticPassion:
                DrawRestTraumaticPassionUI(rect, ref curY, student);
                break;
            case TherapyFocusType.MentalStability:
                DrawMentalStabilityUI(rect, ref curY, student, classDialog);
                break;
        }

        var progressPerTick = ProgressPerTick;
        if (progressPerTick > 0 && !studyGroup.subjectLogic.IsInfinite)
        {
            var progressRemaining = studyGroup.semesterGoal - studyGroup.currentProgress;
            var estimatedTicks = Mathf.CeilToInt(progressRemaining / progressPerTick);
            Widgets.Label(new Rect(rect.x, curY, 360f, 25f),
                "PE_StudyTimeNeeded".Translate(estimatedTicks.ToStringTicksToPeriod()));
            curY += 30f;
            var sessionsNeeded = Mathf.Ceil(
                (float)estimatedTicks / (GenDate.TicksPerHour * studyGroup.Duration));
            Widgets.Label(new Rect(rect.x, curY, 360f, 25f),
                "PE_StudySessionsNeeded".Translate(sessionsNeeded.ToString("F0")
                    .Colorize(ColoredText.DateTimeColor)));
            curY += 30f;
        }
    }
    private void DrawFocusTypeSelector(Rect rect, ref float curY, Pawn student)
    {
        Widgets.Label(new Rect(rect.x, curY, 150f, 25f), "PT_TherapeuticFocus".Translate());
        if (Widgets.ButtonText(new Rect(rect.x + 160f, curY, 200f, 25f), GetFocusLabel(focusType)))
        {
            var options = new List<FloatMenuOption>();
            if (ProgressionTherapyMod.settings.enableImproveMood) options.Add(new FloatMenuOption("PT_Focus_ImproveMood".Translate(), () => { focusType = TherapyFocusType.ImproveMood; studyGroup.semesterGoal = 0; }));
            if (ProgressionTherapyMod.settings.enableWorkingMemories && GetValidMemories(student).Any()) options.Add(new FloatMenuOption("PT_Focus_WorkThroughMemory".Translate(), () => { focusType = TherapyFocusType.WorkThroughMemory; studyGroup.semesterGoal = DefaultSemesterGoal; targetMemoryDef = GetValidMemories(student).FirstOrDefault()?.def; }));
            if (ProgressionTherapyMod.settings.enableMentalStability) options.Add(new FloatMenuOption("PT_Focus_MentalStability".Translate(), () => { focusType = TherapyFocusType.MentalStability; studyGroup.semesterGoal = 6500; }));
            if (AlphaSkillsCompat.IsActive && AlphaSkillsCompat.GetTraumaticSkills(student).Any()) options.Add(new FloatMenuOption("PT_Focus_RestTraumaticPassion".Translate(), () => { focusType = TherapyFocusType.RestTraumaticPassion; studyGroup.semesterGoal = TraumaticPassionSemesterGoal; targetTraumaticSkill = AlphaSkillsCompat.GetTraumaticSkills(student).First(); }));
            Find.WindowStack.Add(new FloatMenu(options));
        }
    }

    private void DrawWorkThroughMemoryUI(Rect rect, ref float curY, Pawn student)
    {
        Widgets.Label(new Rect(rect.x, curY, 150f, 25f), "PT_SelectMemory".Translate());

        var memory = student.needs.mood.thoughts.memories.Memories.FirstOrDefault(m => m.def == targetMemoryDef);
        var buttonLabel = memory != null ? (TaggedString)memory.LabelCap : "None".Translate();

        if (memory != null)
        {
            var moodRect = new Rect(rect.x + 110f, curY, 40f, 25f);
            var prevAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = ColorLibrary.RedReadable;
            Widgets.Label(moodRect, memory.MoodOffset().ToString("F0"));
            GUI.color = Color.white;
            Text.Anchor = prevAnchor;
        }

        if (Widgets.ButtonText(new Rect(rect.x + 160f, curY, 200f, 25f), buttonLabel))
        {
            Find.WindowStack.Add(new FloatMenu(GetValidMemories(student).GroupBy(m => m.def).Select(g => g.First()).Select(localMem => new FloatMenuOption(localMem.LabelCap, () => targetMemoryDef = localMem.def)).ToList()));
        }
        curY += 30f;
    }

    private void DrawRestTraumaticPassionUI(Rect rect, ref float curY, Pawn student)
    {
        Widgets.Label(new Rect(rect.x, curY, 150f, 25f), "PT_SelectSkill".Translate());
        if (Widgets.ButtonText(new Rect(rect.x + 160f, curY, 200f, 25f), targetTraumaticSkill?.LabelCap ?? "None".Translate())) Find.WindowStack.Add(new FloatMenu(AlphaSkillsCompat.GetTraumaticSkills(student).Select(localSk => new FloatMenuOption(localSk.LabelCap, () => targetTraumaticSkill = localSk)).ToList()));
        curY += 30f;
    }

    private string GetFocusLabel(TherapyFocusType focus) => focus switch
    {
        TherapyFocusType.ImproveMood => "PT_Focus_ImproveMood".Translate(),
        TherapyFocusType.WorkThroughMemory => "PT_Focus_WorkThroughMemory".Translate(),
        TherapyFocusType.MentalStability => "PT_Focus_MentalStability".Translate(),
        TherapyFocusType.RestTraumaticPassion => "PT_Focus_RestTraumaticPassion".Translate(),
        _ => "None".Translate()
    };

    private void DrawMentalStabilityUI(Rect rect, ref float curY, Pawn student, IClassDialog classDialog)
    {
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(new Rect(rect.x, curY, 360f, 25f), "PE_SemesterGoal".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        curY += 30f;
        if (classDialog is Dialog_EditClass && studyGroup.currentProgress > 0)
        {
            Widgets.Label(new Rect(rect.x, curY, 150f, 25f), "PE_SemesterProgress".Translate());
            Widgets.Label(new Rect(rect.x + 160f, curY, 200f, 25f), (studyGroup.currentProgress / 100f).ToString("F0") + "%");
            curY += 30f;
        }
        var currentGoal = studyGroup.semesterGoal / 100f;
        currentGoal = Widgets.HorizontalSlider(new Rect(rect.x, curY, 360f, 25f), currentGoal, 1f, 100f, leftAlignedLabel: "0%", rightAlignedLabel: "100%", roundTo: 1f);
        studyGroup.semesterGoal = (int)(currentGoal * 100f);
        curY += 30f;

        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(new Rect(rect.x, curY - 15f, 360f, 25f), "PT_StabilityProgress".Translate(currentGoal));
        Text.Anchor = TextAnchor.UpperLeft;
        curY += 20f;
    }
}
