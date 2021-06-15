using System;
using System.Collections.Generic;
using System.Linq;
using RocketMan;
using Verse;

namespace Soyuz
{
    public static class SoyuzSettingsUtility
    {
        private static readonly HashSet<ThingDef> processedDefs = new HashSet<ThingDef>();

        [Main.OnScribe]
        public static void OnScribe()
        {
            Scribe_Deep.Look(ref Context.Settings, "soyuzSettings_NewTemp");
            if (Context.Settings == null)
            {
                Context.Settings = new SoyuzSettings();
            }
        }

        [Main.OnSettingsScribedLoaded]
        public static void OnSettingsScribedLoaded()
        {
            Context.Settings.AllRaceSettings = Context.Settings.AllRaceSettings
                .AsParallel()
                .Where(s => s.def != null).ToList();
            foreach (RaceSettings settings in Context.Settings.AllRaceSettings)
            {
                processedDefs.Add(settings.def);
            }
            //
            // DefDatabase<ThingDef>.ResolveAllReferences();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs
                .AsParallel()
                .Where(d => d.race != null && !processedDefs.Contains(d)))
            {
                processedDefs.Add(def);
                bool disabled = def.thingClass != typeof(Pawn);
                Context.Settings.AllRaceSettings.Add(new RaceSettings()
                {
                    def = def,
                    enabled = def.race.Animal
                        && !disabled
                        && !def.race.Humanlike
                        && !def.race.IsMechanoid
                        && !IgnoreMeDatabase.ShouldIgnore(def),
                    ignoreFactions = false
                });
            }
            foreach (RaceSettings settings in Context.Settings.AllRaceSettings)
            {
                settings.Prepare();
            }
        }
    }
}