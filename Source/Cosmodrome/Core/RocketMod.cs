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
                RocketAssembliesInfo.Assemblies.AddRange(RocketAssembliesInfo.RocketManAssembliesInAppDomain);
                foreach (Assembly assembly in RocketAssembliesInfo.Assemblies)
                    Logger.Debug($"Found in AppDomain after loading assembly {assembly.FullName}", file: "Assemblies.log");
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
            GUIUtility.ClearGUIState();
        }

        private static readonly Listing_Collapsible.Group_Collapsible group = new Listing_Collapsible.Group_Collapsible();

        private static readonly Listing_Collapsible collapsible_general = new Listing_Collapsible();

        private static readonly Listing_Collapsible collapsible_junk = new Listing_Collapsible(group);

        private static readonly Listing_Collapsible collapsible_other = new Listing_Collapsible(group);

        private static readonly Listing_Collapsible collapsible_debug = new Listing_Collapsible(group);

        private static readonly Listing_Collapsible collapsible_experimental = new Listing_Collapsible(group);

        private static bool guiGroupCreated = false;

        public static void DoSettings(Rect inRect, bool doStats = true, Action<Listing_Standard> extras = null)
        {
            if (!guiGroupCreated)
            {
                guiGroupCreated = true;

                collapsible_junk.Group = group;
                group.Register(collapsible_junk);

                collapsible_other.Group = group;
                group.Register(collapsible_other);

                collapsible_debug.Group = group;
                group.Register(collapsible_debug);

                collapsible_experimental.Group = group;
                group.Register(collapsible_experimental);
            }
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                collapsible_general.Expanded = true;
                collapsible_general.Begin(inRect, KeyedResources.RocketMan_Settings, drawIcon: false, drawInfo: false);

                if (collapsible_general.CheckboxLabeled(KeyedResources.RocketMan_Enable, ref RocketPrefs.Enabled))
                {
                    ResetRocketDebugPrefs();
                }
                if (collapsible_general.CheckboxLabeled("RocketMan.ShowIcon".Translate(), ref RocketPrefs.MainButtonToggle, "RocketMan.ShowIcon.Description".Translate()))
                {
                    MainButtonDef mainButton_WindowDef = DefDatabase<MainButtonDef>.GetNamed("RocketWindow", errorOnFail: false);
                    if (mainButton_WindowDef != null)
                    {
                        mainButton_WindowDef.buttonVisible = RocketPrefs.MainButtonToggle;
                        string state = RocketPrefs.MainButtonToggle ? "shown" : "hidden";
                        Log.Message($"ROCKETMAN: <color=red>MainButton</color> is now {state}!");
                    }
                }
                collapsible_general.CheckboxLabeled("RocketMan.ProgressBar".Translate(), ref RocketPrefs.ShowWarmUpPopup, "RocketMan.ProgressBar.Description".Translate());
                collapsible_general.End(ref inRect);
                inRect.yMin += 5;

                if (RocketPrefs.Enabled)
                {
                    collapsible_junk.Begin(inRect, "RocketMan.GameSpeed".Translate());
                    collapsible_junk.CheckboxLabeled("RocketMan.DisableForcedSlowdowns".Translate(), ref RocketPrefs.DisableForcedSlowdowns, "RocketMan.DisableForcedSlowdowns.Description".Translate());
                    collapsible_junk.End(ref inRect);
                    inRect.yMin += 5;

                    collapsible_junk.Begin(inRect, "RocketMan.Junk".Translate());
                    collapsible_junk.CheckboxLabeled("RocketMan.CorpseRemoval".Translate(), ref RocketPrefs.CorpsesRemovalEnabled, "RocketMan.CorpseRemoval.Description".Translate());
                    collapsible_junk.End(ref inRect);
                    inRect.yMin += 5;

                    collapsible_other.Begin(inRect, "RocketMan.StatCacheSettings".Translate());

                    collapsible_other.CheckboxLabeled("RocketMan.Adaptive".Translate(), ref RocketPrefs.Learning, "RocketMan.Adaptive.Description".Translate());
                    collapsible_other.CheckboxLabeled("RocketMan.AdaptiveAlert.Label".Translate(), ref RocketPrefs.LearningAlertEnabled, "RocketMan.AdaptiveAlert.Description".Translate());
                    collapsible_other.CheckboxLabeled("RocketMan.EnableGearStatCaching".Translate(), ref RocketPrefs.StatGearCachingEnabled);
                    collapsible_other.End(ref inRect);
                    inRect.yMin += 5;

                    if (Prefs.DevMode || RocketEnvironmentInfo.IsDevEnv)
                    {
                        collapsible_experimental.Begin(inRect, KeyedResources.RocketMan_Experimental);
                        if (RocketEnvironmentInfo.IsDevEnv)
                        {
                            collapsible_experimental.CheckboxLabeled(KeyedResources.RocketMan_TranslationCaching, ref RocketPrefs.TranslationCaching);
                            collapsible_experimental.Line(1);
                        }
                        collapsible_experimental.Label(KeyedResources.RocketMan_Experimental_Description);
                        bool devKeyEnabled = File.Exists(RocketEnvironmentInfo.DevKeyFilePath);
                        if (collapsible_experimental.CheckboxLabeled(KeyedResources.RocketMan_Experimental_OptInBeta, ref devKeyEnabled))
                        {
                            if (!devKeyEnabled && File.Exists(RocketEnvironmentInfo.DevKeyFilePath))
                                File.Delete(RocketEnvironmentInfo.DevKeyFilePath);
                            if (devKeyEnabled && !File.Exists(RocketEnvironmentInfo.DevKeyFilePath))
                                File.WriteAllText(RocketEnvironmentInfo.DevKeyFilePath, "enabled");
                        }
                        collapsible_experimental.End(ref inRect);
                        inRect.yMin += 5;
                    }
                    collapsible_debug.Begin(inRect, "Debugging options");

                    if (collapsible_debug.CheckboxLabeled("RocketMan.Debugging".Translate(), ref RocketDebugPrefs.Debug, "RocketMan.Debugging.Description".Translate())
                    && !RocketDebugPrefs.Debug)
                    {
                        ResetRocketDebugPrefs();
                    }
                    if (RocketDebugPrefs.Debug)
                    {
                        collapsible_debug.Line(1);
                        collapsible_debug.CheckboxLabeled("Enable Stat Logging (Will kill performance)", ref RocketDebugPrefs.StatLogging);
                        collapsible_debug.CheckboxLabeled("Enable GlowGrid flashing", ref RocketDebugPrefs.DrawGlowerUpdates);
                        collapsible_debug.CheckboxLabeled("Enable GlowGrid refresh", ref RocketPrefs.EnableGridRefresh);
                        collapsible_debug.Gap();
                    }
                    collapsible_debug.End(ref inRect);
                }
            });
        }

        public static void ResetRocketDebugPrefs()
        {
            RocketDebugPrefs.Debug = false;
            RocketDebugPrefs.Debug150MTPS = false;
            RocketDebugPrefs.LogData = false;
            RocketDebugPrefs.StatLogging = false;
            RocketDebugPrefs.FlashDilatedPawns = false;
            RocketDebugPrefs.AlwaysDilating = false;
            RocketPrefs.EnableGridRefresh = false;
            RocketPrefs.RefreshGrid = false;
            RocketStates.SingleTickIncrement = false;
        }
    }
}