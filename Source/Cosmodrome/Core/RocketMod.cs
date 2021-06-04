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

        public static RocketModSettings Settings;

        public static RocketMod instance;

        public static Vector2 scrollPositionStatSettings = Vector2.zero;

        public RocketMod(ModContentPack content) : base(content)
        {
            Finder.Mod = this;
            Finder.ModContentPack = content;
            Finder.PluginsLoader = new RocketPluginsLoader();
            try
            {
                foreach (Assembly assembly in Finder.PluginsLoader.LoadAll())
                {
                    RocketAssembliesInfo.Assemblies.Add(assembly);
                    if (!content.assemblies.loadedAssemblies.Any(a => a.GetName().Name == assembly.GetName().Name))
                        content.assemblies.loadedAssemblies.Add(assembly);
                    Log.Message($"<color=orange>ROCKETMAN</color>: Loaded <color=red>{assembly.FullName}</color>");
                }
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETMAN: loading plugin failed {er.Message}:{er.StackTrace}");
            }
            finally
            {
                if (!Directory.Exists(RocketEnvironmentInfo.CustomConfigFolderPath))
                {
                    Directory.CreateDirectory(RocketEnvironmentInfo.CustomConfigFolderPath);
                    Log.Message($"ROCKETMAN: Created RocketMan config folder at <color=orange>{RocketEnvironmentInfo.CustomConfigFolderPath}</color>");
                }
                Main.ReloadActions();
                foreach (var action in Main.onInitialization)
                    action.Invoke();
                instance = this;
                Settings = GetSettings<RocketModSettings>();
                UpdateExceptions();
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