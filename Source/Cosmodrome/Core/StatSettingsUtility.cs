using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace RocketMan
{
    public static class StatSettingsUtility
    {
        private static bool statsLoaded = false;

        private static Dictionary<string, StatSettings> settingsByName = new Dictionary<string, StatSettings>();

        [Main.OnWorldLoaded]
        public static void ResolveStatSettings()
        {
            foreach (StatSettings settings in Finder.StatSettingsGroup.AllStatSettings)
            {
                if (settingsByName.TryGetValue(settings.defName, out _))
                    continue;
                settingsByName[settings.defName] = settings;
            }
            foreach (StatDef statDef in DefDatabase<StatDef>.AllDefs)
            {
                if (!settingsByName.TryGetValue(statDef.defName, out StatSettings settings))
                {
                    settings = new StatSettings(statDef);
                    settingsByName[statDef.defName] = settings;
                    Finder.StatSettingsGroup.AllStatSettings.Add(settings);
                }
                settings.statDef = statDef;
                RocketStates.StatExpiry[statDef.index] = settings.expireAfter;
            }
            statsLoaded = true;
        }

        private static void UpdateStatsSettings()
        {
            if (!RocketStates.DefsLoaded || !statsLoaded)
            {
                return;
            }
            foreach (StatSettings settings in Finder.StatSettingsGroup.AllStatSettings)
            {
                if (settings.statDef == null)
                    continue;
                settings.expireAfter = RocketStates.StatExpiry[settings.statDef.index];
            }
        }

        [Main.OnScribe]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
        private static void ScribeStatsSettings()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                UpdateStatsSettings();
            }
            Scribe_Deep.Look(ref Finder.StatSettingsGroup, "settingsGroup", LookMode.Deep);
            if (Finder.StatSettingsGroup == null)
            {
                Finder.StatSettingsGroup = new StatSettingsGroup();
            }
        }
    }
}
