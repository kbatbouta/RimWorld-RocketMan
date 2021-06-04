using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Verse;

namespace RocketMan
{
    public partial class RocketMod : Mod
    {
        public void LoadSettings()
        {
            bool settingsFound = false;
            try
            {
                if (File.Exists(RocketEnvironmentInfo.RocketSettingsPath))
                {
                    Scribe.loader.InitLoading(RocketEnvironmentInfo.RocketSettingsPath);
                    try
                    {
                        Scribe_Deep.Look(ref RocketMod.Settings, "ModSettings");
                        settingsFound = RocketMod.Settings != null;
                        if (RocketMod.Settings == null)
                            RocketMod.Settings = new RocketSettings();
                    }
                    catch (Exception er)
                    {
                        Log.Error($"ROCKETMAN: Error while scribing settings {er}");
                    }
                    finally
                    {
                        Scribe.loader.FinalizeLoading();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"ROCKETMAN: Caught exception while loading mod settings data for {Content.FolderName}. Generating fresh settings. The exception was: {ex.ToString()}");
                RocketMod.Settings = null;
            }
            if (RocketMod.Settings == null)
            {
                RocketMod.Settings = new RocketSettings();
            }
            if (!settingsFound)
            {
                WriteSettings();
            }
            foreach (var action in Main.onSettingsScribedLoaded)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception er)
                {
                    Log.Error($"ROCKETMAN: Error in post srcibe {action} with error {er}");
                }
            }
        }

        public override void WriteSettings()
        {
            if (RocketPrefs.WarmingUp && !(WarmUpMapComponent.current?.Finished ?? true))
            {
                WarmUpMapComponent.current.AbortWarmUp();
            }
            Scribe.saver.InitSaving(RocketEnvironmentInfo.RocketSettingsPath, "SettingsBlock");
            try
            {
                Scribe_Deep.Look(ref RocketMod.Settings, "ModSettings");
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETMAN: Error while scribing settings {er}");
            }
            finally
            {
                Scribe.saver.FinalizeSaving();
            }
        }

        public class RocketSettings : IExposable
        {
            public void ExposeData()
            {
                ScribeRocketPrefs();
                ScribeExtras();
                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    UpdateExceptions();
                }
                RocketPrefs.TimeDilationCaravans = false;
            }

            private void ScribeRocketPrefs()
            {
                Scribe_Values.Look(ref RocketPrefs.Enabled, "enabled", true);
                Scribe_Values.Look(ref RocketPrefs.Learning, "learning");
                Scribe_Values.Look(ref RocketPrefs.StatGearCachingEnabled, "statGearCachingEnabled", true);
                Scribe_Values.Look(ref RocketDebugPrefs.Debug, "debug", false);
                Scribe_Values.Look(ref RocketPrefs.ShowWarmUpPopup, "showWarmUpPopup", true);
                Scribe_Values.Look(ref RocketPrefs.AlertThrottling, "alertThrottling", true);
                Scribe_Values.Look(ref RocketPrefs.DisableAllAlert, "disableAllAlert", false);
                Scribe_Values.Look(ref RocketPrefs.TimeDilation, "timeDilation", true);
                Scribe_Values.Look(ref RocketPrefs.TimeDilationWildlife, "TimeDilationWildlife", true);
                Scribe_Values.Look(ref RocketPrefs.TimeDilationFire, "TimeDilationFire", false);
                Scribe_Values.Look(ref RocketPrefs.TimeDilationCaravans, "timeDilationCaravans", false);
                Scribe_Values.Look(ref RocketPrefs.TimeDilationVisitors, "timeDilationVisitors", false);
                Scribe_Values.Look(ref RocketPrefs.TimeDilationWorldPawns, "timeDilationWorldPawns", true);
                Scribe_Values.Look(ref RocketPrefs.TimeDilationColonyAnimals, "timeDialationColonyAnimals", true);
                Scribe_Values.Look(ref RocketPrefs.TimeDilationCriticalHediffs, "timeDilationCriticalHediffs", true);
                Scribe_Values.Look(ref RocketPrefs.MainButtonToggle, "mainButtonToggle", true);
                Scribe_Values.Look(ref RocketPrefs.CorpsesRemovalEnabled, "corpsesRemovalEnabled", false);
            }

            private void ScribeExtras()
            {
                foreach (var action in Main.onScribe)
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception er)
                    {
                        Log.Error($"ROCKETMAN: Error scribing settings with mod {Scribe.mode} in action {action} with error {er}");
                    }
                }
            }
        }
    }
}
