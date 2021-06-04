using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace RocketMan
{
    [StaticConstructorOnStartup]
    public static class RocketPrefs
    {
        public static FieldInfo[] SettingsFields;

        public static bool WarmingUp
        {
            get => WarmUpMapComponent.settingsBeingStashed;
        }

        [Main.SettingsField(warmUpValue: false)]
        public static bool Enabled = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool Learning = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool LabelCaching = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool AlertThrottling = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool DisableAllAlert = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool TimeDilation = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool TimeDilationCriticalHediffs = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool TimeDilationWorldPawns = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool TimeDilationVisitors = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool TimeDilationFire = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool TimeDilationCaravans = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool TimeDilationWildlife = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool TimeDilationColonyAnimals = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool TranslationCaching = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool ThoughtsCaching = true;

        public static bool RefreshGrid = false;

        public static bool EnableGridRefresh = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool StatGearCachingEnabled = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool CorpsesRemovalEnabled = true;

        public static bool ShowWarmUpPopup = true;

        public static bool MainButtonToggle = true;

        public static float LearningRate = 0.0005f;

        public static int AgeOfGetValueUnfinalizedCache = 0;
    }
}