using System;
using System.Collections.Generic;
using System.Linq;
using RocketMan;
using Verse;

namespace Soyuz
{
    public static class SoyuzSettingsUtility
    {
        private static List<ThingDef> pawnDefs;

        [Main.OnDefsLoaded]
        public static void LoadSettings()
        {
            pawnDefs = DefDatabase<ThingDef>.AllDefs.Where(def => def.race != null).ToList();
            CacheSettings();
        }

        [Main.OnScribe]
        public static void PostScribe()
        {
            Scribe_Deep.Look(ref Context.settings, "soyuzSettings");
            if (Scribe.mode != LoadSaveMode.Saving && pawnDefs != null)
                CheckExtras();
            RocketEnvironmentInfo.SoyuzLoaded = true;
        }

        public static void CacheSettings()
        {
            if (Context.settings == null)
                Context.settings = new SoyuzSettings();
            if (Context.settings.raceSettings.Count == 0)
                CreateSettings();
            foreach (var element in Context.settings.raceSettings)
            {
                if (element.pawnDef == null)
                {
                    element.ResolveContent();
                    if (element.pawnDef == null) continue;
                }
                element.Cache();
            }
            CheckExtras();
        }

        public static void CheckExtras()
        {
            if (pawnDefs.Count == Context.dilationByDef.Count)
                return;
            bool foundAnything = false;
            foreach (var def in pawnDefs)
            {
                if (def?.race != null && !Context.dilationByDef.TryGetValue(def, out _))
                {
                    RaceSettings element;
                    Context.settings.raceSettings.Add(element = new RaceSettings()
                    {
                        pawnDef = def,
                        pawnDefName = def.defName,
                        dilated = def.race.Animal && !def.race.Humanlike && !def.race.IsMechanoid,
                        ignoreFactions = false
                    });
                    element.Cache();
                    foundAnything = true;
                }
            }
            if (foundAnything && Scribe.mode == LoadSaveMode.Inactive)
                Finder.Mod.WriteSettings();
        }

        public static void CreateSettings()
        {
            Context.settings.raceSettings.Clear();
            foreach (var def in pawnDefs)
            {
                Context.settings.raceSettings.Add(new RaceSettings()
                {
                    pawnDef = def,
                    pawnDefName = def.defName,
                    dilated = def.race.Animal && !def.race.Humanlike && !def.race.IsMechanoid,
                    ignoreFactions = false
                });
            }
            Finder.Mod.WriteSettings();
        }

        public static RaceSettings GetRaceSettings(this Pawn pawn)
        {
            if (pawn.def == null)
                return null;
            if (Context.dilationByDef.TryGetValue(pawn.def, out RaceSettings settings))
                return settings;
            ThingDef def = pawn.def;
            Context.settings.raceSettings.Add(settings = new RaceSettings()
            {
                pawnDef = def,
                pawnDefName = def.defName,
                dilated = def.race.Animal && !def.race.Humanlike && !def.race.IsMechanoid,
                ignoreFactions = false
            });
            settings.Cache();
            return settings;
        }
    }
}