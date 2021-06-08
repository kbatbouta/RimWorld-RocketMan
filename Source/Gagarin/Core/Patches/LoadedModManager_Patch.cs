using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Gagarin
{
    public static class LoadedModManager_Patch
    {
        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.LoadModXML))]
        public static class LoadModXML_Patch
        {
            public static void Prefix()
            {
                if (!Context.IsRecovering)
                {
                    try
                    {
                        Context.IsLoadingModXML = true;

                        if (File.Exists(GagarinEnvironmentInfo.HashFilePath))
                        {
                            Context.AssetsHashes = AssetHashingUtility.Load(GagarinEnvironmentInfo.HashFilePath);
                        }
                    }
                    catch (Exception er)
                    {
                        Context.IsRecovering = true;

                        Logger.Debug("GAGARIN: Loading error", er);
                        throw er;
                    }
                }
            }

            public static void Postfix(IEnumerable<LoadableXmlAsset> __result)
            {
                if (!Context.IsRecovering)
                {
                    try
                    {
                        Context.XmlAssets = new Dictionary<string, LoadableXmlAsset>(
                        __result.Select(a => new KeyValuePair<string, LoadableXmlAsset>(a.FullFilePath, a)));

                        if (!Context.IsUsingCache)
                        {
                            AssetHashingUtility.Dump(Context.AssetsHashes, GagarinEnvironmentInfo.HashFilePath);

                            if (File.Exists(GagarinEnvironmentInfo.UnifiedXmlFilePath))
                                File.Delete(GagarinEnvironmentInfo.UnifiedXmlFilePath);
                        }
                        Context.IsLoadingModXML = false;
                    }
                    catch (Exception er)
                    {
                        Context.IsRecovering = true;

                        Logger.Debug("GAGARIN: Loading error", er);
                        throw er;
                    }
                }
            }
        }

        [GagarinPatch(typeof(TKeySystem), nameof(TKeySystem.Parse))]
        public static class TKeySystem_Parse_Patch
        {
            public static void Postfix()
            {
                if (!Context.IsRecovering)
                {
                    DuplicateHelper.QueueReportProcessing();
                }
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ApplyPatches))]
        public static class ApplyPatches_Patch
        {
            [HarmonyPriority(Priority.Last)]
            public static bool Prefix()
            {
                if (!Context.IsRecovering)
                {
                    try
                    {
                        CachedDefHelper.Prepare();

                        return !Context.IsUsingCache;
                    }
                    catch (Exception er)
                    {
                        Context.IsRecovering = true;

                        Logger.Debug("GAGARIN: Loading error", er);
                        throw er;
                    }
                }
                return true;
            }

            public static void Postfix(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
            {
                if (!Context.IsRecovering && !Context.IsUsingCache)
                {
                    try
                    {
                        if (File.Exists(GagarinEnvironmentInfo.UnifiedPatchedOriginalXmlPath))
                            File.Delete(GagarinEnvironmentInfo.UnifiedPatchedOriginalXmlPath);

                        XmlWriterSettings settings = new XmlWriterSettings
                        {
                            CheckCharacters = false,
                            Indent = true,
                            NewLineChars = "\n"
                        };
                        using (XmlWriter writer = XmlWriter.Create(GagarinEnvironmentInfo.UnifiedPatchedOriginalXmlPath, settings))
                        {
                            xmlDoc.Save(writer);
                        }
                    }
                    finally
                    {
                    }
                }
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ParseAndProcessXML))]
        public static class ParseAndProcessXML_Patch
        {
            [HarmonyPriority(Priority.Last)]
            public static void Postfix()
            {
                if (!Context.IsRecovering && !Context.IsUsingCache)
                {
                    try
                    {
                        CachedDefHelper.Save();
                    }
                    catch (Exception er)
                    {
                        Context.IsRecovering = true;

                        Logger.Debug("GAGARIN: Loading error", er);
                        throw er;
                    }
                }
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.CombineIntoUnifiedXML))]
        public static class CombineIntoUnifiedXML_Patch
        {
            private static bool usedCache = false;

            [HarmonyPriority(Priority.Last)]
            public static bool Prefix(List<LoadableXmlAsset> xmls, ref XmlDocument __result, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
            {
                if (!Context.IsRecovering)
                {
                    try
                    {
                        Context.DefsXmlAssets = assetlookup;
                        Log.Warning($"GAGARIN: CombineIntoUnifiedXML has <color=red>Context.IsUsingCache={ Context.IsUsingCache }</color>");
                        if (Context.IsUsingCache)
                        {
                            usedCache = true;
                            CachedDefHelper.Load(__result = new XmlDocument(), assetlookup);
                            foreach (ModContentPack mod in Context.RunningMods)
                                mod.patches?.Clear();
                            return false;
                        }
                    }
                    catch (Exception er)
                    {
                        Context.IsRecovering = true;

                        Logger.Debug("GAGARIN: Loading error", er);
                        throw er;
                    }
                }
                return true;
            }

            [HarmonyPriority(Priority.First)]
            public static void Postfix(XmlDocument __result, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
            {
                if (!Context.IsRecovering && !usedCache && __result != null && !assetlookup.EnumerableNullOrEmpty())
                {
                    try
                    {
                        DuplicateHelper.ParseCreateReports(__result, assetlookup);
                    }
                    catch (Exception er)
                    {
                        Context.IsRecovering = true;

                        Logger.Debug("GAGARIN: Loading error", er);
                        throw er;
                    }
                }
            }
        }
    }
}

