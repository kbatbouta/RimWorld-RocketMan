using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Gagarin
{
    [GagarinPatch]
    public static class LoadedModManager_Profiler
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return GagarinTargets.LoadedModManager_LoadModXML;
            yield return GagarinTargets.LoadedModManager_CombineIntoUnifiedXML;
            yield return GagarinTargets.LoadedModManager_ApplyPatches;
            // Here TKeyParser.Parse will starts
            yield return GagarinTargets.LoadedModManager_ParseAndProcessXML;
            yield return GagarinTargets.LoadedModManager_ClearCachedPatches;
        }

        private static int stageInt = 0;

        private static string[] stages = new string[]
        {
            "LoadModXML",
            "CombineIntoUnifiedXML",
            "ApplyPatches",
            "ParseAndProcessXML",
            "ClearCachedPatches",
        };

        private static Stopwatch stopwatch = new Stopwatch();

        [HarmonyPriority(1000)]
        public static void Prefix()
        {
            stopwatch.Reset();
            stopwatch.Start();
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix()
        {
            stopwatch.Stop();
            Log.Warning($"GAGARIN: <color=white>LoadedModManager.{stages[stageInt]}</color> took <color=red>{Math.Round((float)stopwatch.ElapsedMilliseconds / 1000f, 4)}</color> seconds");
            stageInt++;
        }
    }
}
