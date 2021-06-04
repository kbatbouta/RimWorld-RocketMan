using System;
using System.Collections.Generic;
using System.Linq;
using RocketMan;
using Verse;

namespace Soyuz
{
    public static class SoyuzSettingsUtility
    {
        private static IEnumerable<ThingDef> allPawnDefs;

        [Main.OnDefsLoaded]
        public static void LoadSettings()
        {
            DefDatabase<ThingDef>.ResolveAllReferences();
            SoyuzSettingsUtility.allPawnDefs = DefDatabase<ThingDef>.AllDefs
                .Where(def => def.race != null);
            Context.Settings = Context.Settings ?? new SoyuzSettings();
            Context.Settings.AddAllDefs(allPawnDefs);
            Context.Settings.CacheAll();
        }

        [Main.OnScribe]
        public static void PostScribe()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Context.Settings?.AddAllDefs(allPawnDefs);
                Context.Settings = Context.Settings ?? new SoyuzSettings();
            }
            Scribe_Deep.Look(ref Context.Settings, "soyuzSettings");
            if (Scribe.mode != LoadSaveMode.Saving && allPawnDefs != null)
            {
                Context.Settings = Context.Settings ?? new SoyuzSettings();
                Context.Settings.AddAllDefs(allPawnDefs);
                Context.Settings.CacheAll();
            }
            RocketEnvironmentInfo.SoyuzLoaded = true;
        }

        public static RaceSettings GetRaceSettings(this Pawn pawn)
        {
            if (pawn?.def != null && Context.DilationByDef.TryGetValue(pawn.def, out RaceSettings settings))
            {
                return settings;
            }
            ThingDef def = pawn.def;
            Context.Settings.AllRaceSettings.Add(settings = new RaceSettings()
            {
                pawnDef = def,
                name = def.defName,
                enabled = def.race.Animal && !def.race.Humanlike && !def.race.IsMechanoid,
                ignoreFactions = false
            });
            if (settings.pawnDef.thingClass != typeof(Pawn))
            {
                settings.enabled = false;
                settings.ignoreFactions = false;
                settings.ignorePlayerFaction = false;
            }
            settings.Cache();
            return settings;
        }
    }
}