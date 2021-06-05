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

        public static RocketSettings Settings;

        public static RocketMod Instance;

        public static Vector2 scrollPositionStatSettings = Vector2.zero;

        public RocketMod(ModContentPack content) : base(content)
        {
            Finder.Mod = Instance = this;
            Finder.ModContentPack = content;
            if (!Directory.Exists(RocketEnvironmentInfo.CustomConfigFolderPath))
            {
                Directory.CreateDirectory(RocketEnvironmentInfo.CustomConfigFolderPath);
                Log.Message($"ROCKETMAN: Created RocketMan config folder at <color=orange>{RocketEnvironmentInfo.CustomConfigFolderPath}</color>");
            }
            Logger.Initialize();
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
                Logger.Debug("Loading plugins failed", exception: er);
            }
            finally
            {
                Main.ReloadActions();
                foreach (var action in Main.onInitialization)
                    action.Invoke();
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

        public static void DoSettings(Rect inRect, bool doStats = true, Action<Listing_Standard> extras = null)
        {
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Rect rect = inRect;
                standard.Begin(rect);
                Text.Font = GameFont.Tiny;
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                bool enabled = RocketPrefs.Enabled;
                standard.CheckboxLabeled(KeyedResources.RocketMan_Enable, ref RocketPrefs.Enabled);
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
                    standard.CheckboxLabeled("RocketMan.AdaptiveAlert.Label".Translate(), ref RocketPrefs.LearningAlertEnabled, "RocketMan.AdaptiveAlert.Description".Translate());
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
    }
}