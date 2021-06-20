using System;
using System.IO;
using RocketMan;
using Verse;

namespace Gagarin
{
    public class GagarinSettings : IExposable
    {
        public GagarinSettings()
        {
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref GagarinPrefs.TextureCachingEnabled, "TextureCachingEnabled", false);
            Scribe_Values.Look(ref GagarinPrefs.FilterMode, "FilterMode", (int)UnityEngine.FilterMode.Trilinear);
            Scribe_Values.Look(ref GagarinPrefs.MipMapBias, "MipMapBias", float.MinValue);
        }

        public static void LoadSettings()
        {
            bool settingsFound = false;
            try
            {
                if (File.Exists(GagarinEnvironmentInfo.GagarinSettingsFilePath))
                {
                    Scribe.loader.InitLoading(GagarinEnvironmentInfo.GagarinSettingsFilePath);
                    try
                    {
                        Scribe_Deep.Look(ref Context.Settings, "ModSettings");
                        settingsFound = Context.Settings != null;
                        if (Context.Settings == null)
                            Context.Settings = new GagarinSettings();
                    }
                    catch (Exception er)
                    {
                        Log.Error($"GAGARIN: Error while scribing settings {er}");
                        Logger.Debug("Error while scribing settings", exception: er);
                    }
                    finally
                    {
                        Scribe.loader.FinalizeLoading();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GAGARIN: Caught exception while loading mod settings data for {GagarinEnvironmentInfo.CacheFolderPath}. Generating fresh settings. The exception was: {ex.ToString()}");
                Context.Settings = null;
            }
            if (Context.Settings == null)
            {
                Context.Settings = new GagarinSettings();
            }
            if (!settingsFound)
            {
                WriteSettings();
            }
        }

        public static void WriteSettings()
        {
            Scribe.saver.InitSaving(GagarinEnvironmentInfo.GagarinSettingsFilePath, "SettingsBlock");
            try
            {
                Scribe_Deep.Look(ref Context.Settings, "ModSettings");
            }
            catch (Exception er)
            {
                Log.Error($"GAGARIN: Error while scribing settings {er}");
                Logger.Debug("Error while scribing settings", exception: er);
            }
            finally
            {
                Scribe.saver.FinalizeSaving();
            }
        }
    }
}
