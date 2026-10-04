using HarmonyLib;
using UnityEngine;
using Verse;

namespace ProgressionTherapy
{
	public class ProgressionTherapyMod : Mod
	{
		public static ProgressionTherapySettings settings;
		public ProgressionTherapyMod(ModContentPack pack) : base(pack)
		{
			settings = GetSettings<ProgressionTherapySettings>();
			new Harmony("ProgressionTherapyMod").PatchAll();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			var listing = new Listing_Standard();
			listing.Begin(inRect);
			listing.CheckboxLabeled("PT_EnableTherapy".Translate(), ref settings.enableTherapy);
			listing.CheckboxLabeled("PT_EnableImproveMoodClasses".Translate(), ref settings.enableImproveMood);
			listing.CheckboxLabeled("PT_EnableMentalStabilitySystem".Translate(), ref settings.enableMentalStability);
			listing.CheckboxLabeled("PT_EnableWorkingThroughMemories".Translate(), ref settings.enableWorkingMemories);
			listing.Gap();
			listing.Label("PT_RequiredSocialSkill".Translate(settings.requiredSocialSkill));
			settings.requiredSocialSkill = Mathf.RoundToInt(listing.Slider(settings.requiredSocialSkill, 0f, 20f));
			listing.Gap();
			listing.Label("PT_GlobalNerfMentalBreakThreshold".Translate(settings.globalMentalBreakNerf.ToStringPercent()));
			settings.globalMentalBreakNerf = listing.Slider(settings.globalMentalBreakNerf, 0f, 1f);
			listing.Label("PT_SpeedMultiplierMood".Translate(settings.moodSpeedMultiplier.ToStringPercent()));
			settings.moodSpeedMultiplier = listing.Slider(settings.moodSpeedMultiplier, 0.1f, 10f);
			listing.Label("PT_SpeedMultiplierMemory".Translate(settings.memorySpeedMultiplier.ToStringPercent()));
			settings.memorySpeedMultiplier = listing.Slider(settings.memorySpeedMultiplier, 0.1f, 10f);
			listing.Label("PT_SpeedMultiplierPassion".Translate(settings.passionSpeedMultiplier.ToStringPercent()));
			settings.passionSpeedMultiplier = listing.Slider(settings.passionSpeedMultiplier, 0.1f, 10f);
			listing.Label("PT_SpeedMultiplierTrait".Translate(settings.traitSpeedMultiplier.ToStringPercent()));
			settings.traitSpeedMultiplier = listing.Slider(settings.traitSpeedMultiplier, 0.1f, 10f);
			listing.Label("PT_SpeedMultiplierDesire".Translate(settings.desireSpeedMultiplier.ToStringPercent()));
			settings.desireSpeedMultiplier = listing.Slider(settings.desireSpeedMultiplier, 0.1f, 10f);
			listing.Label("PT_SpeedMultiplierStability".Translate(settings.mentalStabilitySpeedMultiplier.ToStringPercent()));
			settings.mentalStabilitySpeedMultiplier = listing.Slider(settings.mentalStabilitySpeedMultiplier, 0.1f, 10f);
			listing.End();
		}

		public override string SettingsCategory() => Content.Name;
	}

	public class ProgressionTherapySettings : ModSettings
	{
		public bool enableTherapy = true;
		public bool enableImproveMood = true;
		public bool enableMentalStability = true;
		public bool enableWorkingMemories = true;
		public int requiredSocialSkill = 15;
		public float globalMentalBreakNerf = 0.25f;
		public float moodSpeedMultiplier = 1f;
		public float memorySpeedMultiplier = 1f;
		public float passionSpeedMultiplier = 1f;
		public float traitSpeedMultiplier = 1f;
		public float desireSpeedMultiplier = 1f;
		public float mentalStabilitySpeedMultiplier = 1f;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref enableTherapy, "enableTherapy", true);
			Scribe_Values.Look(ref enableImproveMood, "enableImproveMood", true);
			Scribe_Values.Look(ref enableMentalStability, "enableMentalStability", true);
			Scribe_Values.Look(ref enableWorkingMemories, "enableWorkingMemories", true);
			Scribe_Values.Look(ref requiredSocialSkill, "requiredSocialSkill", 15);
			Scribe_Values.Look(ref globalMentalBreakNerf, "globalMentalBreakNerf", 0.25f);
			Scribe_Values.Look(ref moodSpeedMultiplier, "moodSpeedMultiplier", 1f);
			Scribe_Values.Look(ref memorySpeedMultiplier, "memorySpeedMultiplier", 1f);
			Scribe_Values.Look(ref passionSpeedMultiplier, "passionSpeedMultiplier", 1f);
			Scribe_Values.Look(ref traitSpeedMultiplier, "traitSpeedMultiplier", 1f);
			Scribe_Values.Look(ref desireSpeedMultiplier, "desireSpeedMultiplier", 1f);
			Scribe_Values.Look(ref mentalStabilitySpeedMultiplier, "mentalStabilitySpeedMultiplier", 1f);
		}
	}
}
