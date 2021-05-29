using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Gagarin.Core
{
    public static class GaragrinTargets
    {
        public static readonly MethodBase mLoadModXML = AccessTools.Method(typeof(LoadedModManager), nameof(LoadedModManager.LoadModXML));

        public static readonly MethodBase mCombineIntoUnifiedXML = AccessTools.Method(typeof(LoadedModManager), nameof(LoadedModManager.CombineIntoUnifiedXML));

        public static readonly MethodBase mTKeySystemParse = AccessTools.Method(typeof(LoadedModManager), nameof(TKeySystem.Parse));

        public static readonly MethodBase mApplyPatches = AccessTools.Method(typeof(LoadedModManager), nameof(LoadedModManager.ApplyPatches));

        public static readonly MethodBase mParseAndProcessXML = AccessTools.Method(typeof(LoadedModManager), nameof(LoadedModManager.ParseAndProcessXML));

        public static readonly MethodBase mClearCachedPatches = AccessTools.Method(typeof(LoadedModManager), nameof(LoadedModManager.ClearCachedPatches));
    }
}
