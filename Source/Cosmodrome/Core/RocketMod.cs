using System;
using System.Collections.Generic;
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
        private static RocketModSettings settings;

        private static readonly Listing_Standard standard = new Listing_Standard();

        private static List<StatSettings> statsSettings = new List<StatSettings>();

        private static List<DilationSettings> dilationSettings = new List<DilationSettings>();

        private static string searchString = "";

        private static int frameCounter = 0;

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
                settings = GetSettings<RocketModSettings>();
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
            if (RocketPrefs.WarmingUp)
                return;
            else
            {
                UpdateStats();
                UpdateDilationDefs();
                UpdateExceptions();
                base.WriteSettings();
            }
        }

        public static void DoSettings(Rect inRect, bool doStats = true, Action<Listing_Standard> extras = null)
        {
            ReadStats();
            ReadDilationSettings();

            var font = Text.Font;
            var anchor = Text.Anchor;
            var style = Text.CurFontStyle.fontStyle;
            var rect = inRect;
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
            try { if (frameCounter++ % 5 == 0 && !RocketPrefs.WarmingUp) settings.Write(); }
            catch (Exception er) { Log.Warning($"ROCKETMAN:[NOTANERROR] Writing settings failed with error {er}"); }
            Text.Font = font;
            Text.Anchor = anchor;
            Text.CurFontStyle.fontStyle = style;
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

        public static void DoStatSettings(Rect rect)
        {
            UpdateStats();
            var counter = 0;
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Small;
            searchString = Widgets
                .TextArea(rect.TopPartPixels(25), searchString)
                .ToLower();
            rect.yMin += 35;
            Widgets.DrawMenuSection(rect);
            rect.yMax -= 5;
            rect.xMax -= 5;
            Widgets.BeginScrollView(rect.ContractedBy(1), ref scrollPositionStatSettings,
                new Rect(Vector2.zero, new Vector2(rect.width - 15, statsSettings.Count * 54)));
            Text.Font = GameFont.Tiny;
            Vector2 size = new Vector2(rect.width - 20, 54);
            Rect curRect = new Rect(new Vector2(2, 2), size);
            foreach (var settings in statsSettings)
            {
                if (searchString.Trim() == "" || settings.stat.ToLower().Contains(searchString))
                {
                    Rect rowRect = curRect.ContractedBy(5);
                    Text.Font = GameFont.Tiny;
                    Text.Anchor = TextAnchor.MiddleLeft;
                    if (counter % 2 == 0)
                        Widgets.DrawBoxSolid(curRect, new Color(0.2f, 0.2f, 0.2f));
                    Widgets.DrawHighlightIfMouseover(curRect);
                    Widgets.Label(rowRect.TopHalf(), string.Format("{0}. {1} set to expire in {2} ticks", counter++,
                        settings.stat,
                        settings.expireAfter));
                    settings.expireAfter =
                        (byte)Widgets.HorizontalSlider(rowRect.BottomHalf(), settings.expireAfter, 0, 255);
                    curRect.y += size.y;
                }
            }
            Widgets.EndScrollView();
            Text.Font = font;
            Text.Anchor = anchor;

            foreach (var setting in statsSettings)
                RocketStates.StatExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index] = (byte)setting.expireAfter;

            if (!RocketPrefs.WarmingUp && (WarmUpMapComponent.current?.Finished ?? true))
            {
                instance.WriteSettings();
                UpdateExceptions();
            }
        }

        public static void ReadStats()
        {
            if (statsSettings == null || statsSettings.Count == 0) return;

            foreach (var setting in statsSettings)
                setting.expireAfter = RocketStates.StatExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index];
        }

        public static void ReadDilationSettings()
        {
            if (dilationSettings == null || dilationSettings.Count == 0) return;

            foreach (var setting in dilationSettings)
            {
                if (DefDatabase<ThingDef>.defsByName.TryGetValue(setting.def, out var td))
                    setting.dilated = RocketStates.DilatedDefs[td.index];
                else
                    Log.Warning("ROCKETMAN: Failed to find stat upon reloading!");
            }
        }

        [Main.OnDefsLoaded]
        public static void UpdateDilationDefs()
        {
            if (dilationSettings == null) dilationSettings = new List<DilationSettings>();
            var failed = false;
            var defs = DefDatabase<ThingDef>.AllDefs.Where(
                d => d.race != null).ToList();
            if (statsSettings.Count != defs.Count())
            {
                dilationSettings.Clear();
                foreach (var def in defs)
                    dilationSettings.Add(new DilationSettings()
                    {
                        def = def.defName,
                        dilated = def.race.Animal && !def.race.IsMechanoid && !def.race.Humanlike
                    });
            }

            foreach (var setting in dilationSettings)
            {
                if (setting?.def != null && DefDatabase<ThingDef>.defsByName.TryGetValue(setting.def, out ThingDef def))
                    RocketStates.DilatedDefs[def.index] = setting.dilated;
                else
                {
                    failed = true;
                    break;
                }
            }
            if (failed)
            {
                Log.Warning("SOYUZ: Failed to reindex the ThingDef database");
                statsSettings.Clear();

                UpdateStats();
            }
        }

        public static void Reset()
        {
            var defs = DefDatabase<StatDef>.AllDefs;
            statsSettings.Clear();
            foreach (var def in defs)
                statsSettings.Add(new StatSettings
                { stat = def.defName, expireAfter = def.defName.PredictValueFromString() });
            var failed = false;
            foreach (var setting in statsSettings)
            {
                if (setting?.stat != null && DefDatabase<StatDef>.defsByName.TryGetValue(setting.stat, out StatDef def))
                    RocketStates.StatExpiry[def.index] = (byte)setting.expireAfter;
                else
                {
                    failed = true;
                    break;
                }
            }
            if (failed)
            {
                Log.Warning("SOYUZ: Failed to reindex the statDef database");
                statsSettings.Clear();

                UpdateStats();
            }
            dilationSettings.Clear();
            UpdateDilationDefs();
            UpdateExceptions();
        }

        [Main.OnDefsLoaded]
        public static void UpdateStats()
        {
            if (statsSettings == null) statsSettings = new List<StatSettings>();

            var defs = DefDatabase<StatDef>.AllDefs;
            if (statsSettings.Count != defs.Count())
            {
                statsSettings.Clear();
                foreach (var def in defs)
                    statsSettings.Add(new StatSettings
                    { stat = def.defName, expireAfter = def.defName.PredictValueFromString() });
            }
            bool failed = false;
            foreach (StatSettings settings in statsSettings)
            {
                if (settings?.stat != null && DefDatabase<StatDef>.defsByName.TryGetValue(settings.stat, out StatDef def))
                    RocketStates.StatExpiry[def.index] = (byte)settings.expireAfter;
                else
                {
                    failed = true;
                    break;
                }
            }
            if (failed)
            {
                Log.Warning("Failed to reindex the statDef database");
                statsSettings.Clear();

                UpdateStats();
            }

            UpdateExceptions();
        }

        public class StatSettings : IExposable
        {
            public int expireAfter;
            public string stat;

            public void ExposeData()
            {
                Scribe_Values.Look(ref stat, "statDef");
                Scribe_Values.Look(ref expireAfter, "expiryTime", 5);
            }
        }

        public class DilationSettings : IExposable
        {
            public bool dilated = true;
            public string def;

            public void ExposeData()
            {
                Scribe_Values.Look(ref def, "def");
                Scribe_Values.Look(ref dilated, "dilated");
            }
        }

        public class RocketModSettings : ModSettings
        {
            public override void ExposeData()
            {
                base.ExposeData();
                if (Scribe.mode == LoadSaveMode.Saving && RocketPrefs.WarmingUp && !(WarmUpMapComponent.current?.Finished ?? true)) WarmUpMapComponent.current.AbortWarmUp();
                if (Scribe.mode == LoadSaveMode.LoadingVars) ReadStats();

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
                Scribe_Values.Look(ref RocketPrefs.UniversalCacheAge, "universalCacheAge");
                Scribe_Values.Look(ref RocketPrefs.MainButtonToggle, "mainButtonToggle", true);
                Scribe_Values.Look(ref RocketPrefs.CorpsesRemovalEnabled, "corpsesRemovalEnabled", false);
                Scribe_Collections.Look(ref statsSettings, "statsSettings", LookMode.Deep);
                Scribe_Collections.Look(ref dilationSettings, "dilationSettings", LookMode.Deep);
                RocketPrefs.TimeDilationCaravans = false;
                foreach (var action in Main.onScribe)
                    action.Invoke();
                UpdateExceptions();
            }
        }
    }
}