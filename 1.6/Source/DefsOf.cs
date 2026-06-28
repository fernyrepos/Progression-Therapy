using RimWorld;
using Verse;

namespace ProgressionTherapy;

[DefOf]
public static class DefsOf
{
    public static JobDef PT_AttendTherapyClass;
    public static NeedDef PT_MentalStability;
    public static ThoughtDef PT_TherapyMoodBoost;
    public static ThingDef PT_TherapyCouch;
    public static NeedDef Mood;

    static DefsOf() => DefOfHelper.EnsureInitializedInCtor(typeof(DefsOf));
}
