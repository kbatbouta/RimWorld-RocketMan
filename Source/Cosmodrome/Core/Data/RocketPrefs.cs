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
        public static bool LearningAlertEnabled = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool AlertThrottling = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool DisableAllAlert = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool TimeDilation = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool TimeDilationCriticalHediffs = false;

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
        public static bool TimeDilationColonists = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool TimeDilationColonyAnimals = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool TranslationCaching = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool ThoughtsCaching = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool StatGearCachingEnabled = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool CorpsesRemovalEnabled = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool RefreshGrid = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool EnableGridRefresh = false;

        [Main.SettingsField(warmUpValue: true)]
        public static bool MainButtonToggle = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool DisableForcedSlowdowns = false;

        public static bool PauseAfterWarmup = false;

        public static bool ShowWarmUpPopup = true;

        public const float LearningRate = 0.0005f;
    }
}