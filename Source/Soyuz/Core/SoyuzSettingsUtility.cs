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
            Scribe_Deep.Look(ref Context.Settings, "soyuzSettings");
            if (Scribe.mode != LoadSaveMode.Saving && pawnDefs != null)
                CheckExtras();
            RocketEnvironmentInfo.SoyuzLoaded = true;
        }

        public static void CacheSettings()
        {
            if (Context.Settings == null)
                Context.Settings = new SoyuzSettings();
            if (Context.Settings.raceSettings.Count == 0)
                CreateSettings();
            foreach (var element in Context.Settings.raceSettings)
            {
                if (element.pawnDef == null)
                {
                    element.ResolveContent();

                    if (element.pawnDef == null)
                        continue;
                }
                element.Cache();
            }
            CheckExtras();
        }

        public static void CheckExtras()
        {
            if (pawnDefs.Count == Context.DilationByDef.Count)
                return;
            bool foundAnything = false;
            foreach (var def in pawnDefs)
            {
                if (def?.race != null && !Context.DilationByDef.TryGetValue(def, out _))
                {
                    RaceSettings element;
                    Context.Settings.raceSettings.Add(element = new RaceSettings()
                    {
                        pawnDef = def,
                        name = def.defName,
                        enabled = def.race.Animal && !def.race.Humanlike && !def.race.IsMechanoid,
                        ignoreFactions = false
                    });
                    if (def.thingClass != typeof(Pawn))
                    {
                        element.enabled = false;
                        element.ignoreFactions = false;
                        element.ignorePlayerFaction = false;
                    }
                    element.Cache();
                    foundAnything = true;
                }
            }
            if (foundAnything && Scribe.mode == LoadSaveMode.Inactive)
                Finder.Mod.WriteSettings();
        }

        public static void CreateSettings()
        {
            Context.Settings.raceSettings.Clear();
            foreach (var def in pawnDefs)
            {
                RaceSettings element;
                Context.Settings.raceSettings.Add(element = new RaceSettings()
                {
                    pawnDef = def,
                    name = def.defName,
                    enabled = def.race.Animal && !def.race.Humanlike && !def.race.IsMechanoid,
                    ignoreFactions = false
                });
                if (def.thingClass != typeof(Pawn))
                {
                    element.enabled = false;
                    element.ignoreFactions = false;
                    element.ignorePlayerFaction = false;
                }
            }
            Finder.Mod.WriteSettings();
        }

        public static RaceSettings GetRaceSettings(this Pawn pawn)
        {
            if (pawn.def == null)
                return null;
            if (Context.DilationByDef.TryGetValue(pawn.def, out RaceSettings settings))
                return settings;
            ThingDef def = pawn.def;
            Context.Settings.raceSettings.Add(settings = new RaceSettings()
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