﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using HarmonyLib;
using Verse;

namespace Gagarin
{
    public static class LoadedModManager_Patch
    {
        public static bool InLoadModXML = false;

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.LoadModXML))]
        public static class LoadModXML_Patch
        {
            public static void Prefix()
            {
                InLoadModXML = true;

                if (File.Exists(GagarinEnvironmentInfo.HashFilePath))
                {
                    Context.AssetsHashes = AssetHashingUtility.Load(GagarinEnvironmentInfo.HashFilePath);
                }
            }

            public static void Postfix(IEnumerable<LoadableXmlAsset> __result)
            {
                Context.XmlAssets = new Dictionary<string, LoadableXmlAsset>(
                    __result.Select(a => new KeyValuePair<string, LoadableXmlAsset>(a.GetLoadableId(), a)));

                if (!Context.IsUsingCache)
                {
                    AssetHashingUtility.Dump(Context.AssetsHashes, GagarinEnvironmentInfo.HashFilePath);
                }
                InLoadModXML = false;

                if (!Context.IsUsingCache)
                {
                    if (File.Exists(GagarinEnvironmentInfo.UnifiedXmlFilePath))
                        File.Delete(GagarinEnvironmentInfo.UnifiedXmlFilePath);
                }
            }
        }


        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ApplyPatches))]
        public static class ApplyPatches_Patch
        {
            [HarmonyPriority(Priority.Last)]
            public static bool Prefix()
            {
                CachedDefHelper.Prepare();

                return !Context.IsUsingCache;
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ParseAndProcessXML))]
        public static class ParseAndProcessXML_Patch
        {
            [HarmonyPriority(Priority.Last)]
            public static void Postfix()
            {
                if (Context.IsUsingCache)
                    return;

                CachedDefHelper.Save();
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.CombineIntoUnifiedXML))]
        public static class CombineIntoUnifiedXML_Patch
        {
            [HarmonyPriority(Priority.Last)]
            public static bool Prefix(List<LoadableXmlAsset> xmls, ref XmlDocument __result, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
            {
                Context.DefsXmlAssets = assetlookup;
                Log.Warning($"GAGARIN: CombineIntoUnifiedXML has <color=red>Context.IsUsingCache={ Context.IsUsingCache }</color>");
                if (Context.IsUsingCache)
                {
                    CachedDefHelper.Load(__result = new XmlDocument(), assetlookup);
                    foreach (ModContentPack mod in Context.RunningMods)
                        mod.patches?.Clear();
                    return false;
                }
                return true;
            }
        }
    }
}

