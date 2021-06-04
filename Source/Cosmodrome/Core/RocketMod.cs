using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RocketMan
{
    public partial class RocketMod : Mod
    {
        private static readonly Listing_Standard standard = new Listing_Standard();

        private static List<StatSettings> statsSettings = new List<StatSettings>();

        public static RocketModSettings Settings;

        public static RocketMod instance;

        public static Vector2 scrollPositionStatSettings = Vector2.zero;

        public RocketMod(ModContentPack content) : base(content)
        {
            Finder.Mod = this;
            Finder.ModContentPack = content;
            try
            {
                if (!Directory.Exists(RocketEnvironmentInfo.CustomConfigFolderPath))
                {
                    Directory.CreateDirectory(RocketEnvironmentInfo.CustomConfigFolderPath);
                }
                if (RocketEnvironmentInfo.IsDevEnv)
                {
                    Log.Warning("ROCKETMAN: YOU ARE LOADING AN EXPERIMENTAL PLUGIN!");
                    LoadPlugins("Gagarin.dll", "Gagarin");
                }
                LoadPlugins("Soyuz.dll", "Soyuz");
                LoadPlugins("Proton.dll", "Proton");
                LoadPlugins("Rocketeer.dll", "Rocketeer");
                RocketAssembliesInfo.Assemblies.Add(content.assemblies.loadedAssemblies[0]);
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETMAN: loading plugin failed {er.Message}:{er.StackTrace}");
            }
            finally
            {
                Main.ReloadActions();
                foreach (var action in Main.onInitialization)
                    action.Invoke();
                instance = this;
                Settings = GetSettings<RocketModSettings>();
                UpdateExceptions();
            }
        }

        private static void LoadPlugins(string pluginAssemblyName, string name)
        {
            ModContentPack mod = Finder.ModContentPack;
            string filePath = Path.Combine(RocketEnvironmentInfo.PluginsFolderPath, pluginAssemblyName);
            if (true
                && File.Exists(filePath)
                && LoadedModManager.runningMods.Any(m => m.Name.Contains(name)) == false)
            {
                Log.Message($"ROCKETMAN: Plugin found at {filePath}");
                byte[] rawAssembly = File.ReadAllBytes(filePath);

                Assembly asm;
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                if (assemblies.All(a => a != null && a.GetName().Name != name))
                {
                    asm = AppDomain.CurrentDomain.Load(rawAssembly);
                    Log.Message(asm.GetName().Name);
                }
                else
                {
                    asm = assemblies.First(a => a.GetName().Name == name);
                }
                if (mod.assemblies.loadedAssemblies.Any(a => a.FullName == asm.FullName))
                {
                    return;
                }
                RocketAssembliesInfo.Assemblies.Add(asm);
                mod.assemblies.loadedAssemblies.Add(asm);
            }
        }

        public override string SettingsCategory()
        {
            return "RocketMan";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            DoSettings(inRect);
        }

        public override void WriteSettings()
        {
            if (!RocketPrefs.WarmingUp)
            {
                base.WriteSettings();
            }
        }

        public static void DoSettings(Rect inRect, bool doStats = true, Action<Listing_Standard> extras = null)
        {
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Rect rect = inRect;
                standard.Begin(rect);
                Text.Font = GameFont.Tiny;
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                bool enabled = RocketPrefs.Enabled;
                standard.CheckboxLabeled("RocketMan.Enable".Translate(), ref RocketPrefs.Enabled);
                bool mainButtonToggle = RocketPrefs.MainButtonToggle;
                standard.CheckboxLabeled("RocketMan.ShowIcon".Translate(), ref RocketPrefs.MainButtonToggle,
                        "RocketMan.ShowIcon.Description".Translate());
                if (RocketPrefs.MainButtonToggle != mainButtonToggle)
                {
                    MainButtonDef mainButton_WindowDef = DefDatabase<MainButtonDef>.GetNamed("RocketWindow", errorOnFail: false);
                    if (mainButton_WindowDef != null)
                    {
                        mainButton_WindowDef.buttonVisible = RocketPrefs.MainButtonToggle;
                        string state = RocketPrefs.MainButtonToggle ? "shown" : "hidden";
                        Log.Message($"ROCKETMAN: <color=red>MainButton</color> is now {state}!");
                    }
                }
                if (enabled != RocketPrefs.Enabled && !RocketPrefs.Enabled)
                {
                    ResetRocketDebugPrefs();
                }
                if (RocketPrefs.Enabled)
                {
                    standard.CheckboxLabeled("RocketMan.ProgressBar".Translate(), ref RocketPrefs.ShowWarmUpPopup,
                        "RocketMan.ProgressBar.Description".Translate());
                    standard.GapLine();
                    Text.CurFontStyle.fontStyle = FontStyle.Bold;
                    standard.Label("RocketMan.Junk".Translate());
                    Text.CurFontStyle.fontStyle = FontStyle.Normal;
                    standard.CheckboxLabeled("RocketMan.CorpseRemoval".Translate(), ref RocketPrefs.CorpsesRemovalEnabled,
                        "RocketMan.CorpseRemoval.Description".Translate());
                    standard.GapLine();
                    Text.CurFontStyle.fontStyle = FontStyle.Bold;
                    standard.Label("RocketMan.StatCacheSettings".Translate());
                    Text.CurFontStyle.fontStyle = FontStyle.Normal;
                    standard.CheckboxLabeled("RocketMan.Adaptive".Translate(), ref RocketPrefs.Learning, "RocketMan.Adaptive.Description".Translate());
                    standard.CheckboxLabeled("RocketMan.EnableGearStatCaching".Translate(), ref RocketPrefs.StatGearCachingEnabled);

                    standard.GapLine();
                    bool oldDebugging = RocketDebugPrefs.Debug;
                    standard.CheckboxLabeled("RocketMan.Debugging".Translate(), ref RocketDebugPrefs.Debug, "RocketMan.Debugging.Description".Translate());
                    if (oldDebugging != RocketDebugPrefs.Debug && !RocketDebugPrefs.Debug)
                    {
                        ResetRocketDebugPrefs();
                    }
                    if (RocketDebugPrefs.Debug)
                    {
                        standard.GapLine();
                        Text.CurFontStyle.fontStyle = FontStyle.Bold;
                        standard.Label("Debugging options");
                        Text.CurFontStyle.fontStyle = FontStyle.Normal;
                        standard.CheckboxLabeled("Enable Stat Logging (Will kill performance)", ref RocketDebugPrefs.StatLogging);
                        standard.CheckboxLabeled("Enable GlowGrid flashing", ref RocketDebugPrefs.DrawGlowerUpdates);
                        standard.CheckboxLabeled("Enable GlowGrid refresh", ref RocketPrefs.EnableGridRefresh);
                        standard.Gap();
                        if (standard.ButtonText("Disable debugging related stuff"))
                            ResetRocketDebugPrefs();
                    }
                }
                standard.End();
            });
        }

        public static void ResetRocketDebugPrefs()
        {
            RocketDebugPrefs.Debug = false;
            RocketDebugPrefs.Debug150MTPS = false;
            RocketDebugPrefs.DogData = false;
            RocketDebugPrefs.StatLogging = false;
            RocketDebugPrefs.FlashDilatedPawns = false;
            RocketDebugPrefs.AlwaysDilating = false;
            RocketPrefs.EnableGridRefresh = false;
            RocketPrefs.RefreshGrid = false;
            RocketStates.SingleTickIncrement = false;
        }

        private static bool statsLoaded = false;

        private static Dictionary<string, StatSettings> settingsByName = new Dictionary<string, StatSettings>();

        [Main.OnWorldLoaded]
        public static void ResolveStatSettings()
        {
            Log.Message("Resolved stats");
            foreach (StatSettings settings in statsSettings)
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
                    statsSettings.Add(settings);
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
            foreach (StatSettings settings in statsSettings)
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
            Scribe_Collections.Look(ref statsSettings, "statsSettings", LookMode.Deep);
            if (statsSettings == null)
            {
                statsSettings = new List<StatSettings>();
            }
        }

        public class StatSettings : IExposable
        {
            public float expireAfter;

            public string defName;

            public StatDef statDef;

            public StatSettings()
            {
            }

            public StatSettings(StatDef statDef)
            {
                this.statDef = statDef;
                this.defName = statDef.defName;
                this.expireAfter = Tools.PredictValueFromString(defName);
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref defName, "defName");
                Scribe_Values.Look(ref expireAfter, "expiryTime_newTemp", 5f);
            }
        }

        public class RocketModSettings : ModSettings
        {
            public override void ExposeData()
            {
                base.ExposeData();
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    if (RocketPrefs.WarmingUp && !(WarmUpMapComponent.current?.Finished ?? true))
                        WarmUpMapComponent.current.AbortWarmUp();
                }
                Scribe_Values.Look(ref RocketPrefs.Enabled, "enabled", true);
                Scribe_Values.Look(ref RocketPrefs.StatGearCachingEnabled, "statGearCachingEnabled", true);
                Scribe_Values.Look(ref RocketPrefs.Learning, "learning");
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
                Scribe_Values.Look(ref RocketPrefs.AgeOfGetValueUnfinalizedCache, "ageOfGetValueUnfinalizedCache");
                Scribe_Values.Look(ref RocketPrefs.MainButtonToggle, "mainButtonToggle", true);
                Scribe_Values.Look(ref RocketPrefs.CorpsesRemovalEnabled, "corpsesRemovalEnabled", false);
                RocketPrefs.TimeDilationCaravans = false;
                foreach (var action in Main.onScribe)
                    action.Invoke();
                UpdateExceptions();
            }
        }
    }
}