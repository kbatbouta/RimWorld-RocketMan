using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using RocketMan;
using UnityEngine;
using Verse;
using Verse.Noise;
using static Proton.GlowGridHelper;

namespace Proton
{
    public static class GlowGrid_Patch
    {
        static bool skipMarking = false;
        static Dictionary<Map, GlowGridHelper> helpers = new Dictionary<Map, GlowGridHelper>();

        static GlowGridHelper GetHelper(Map map)
        {
            if (!helpers.TryGetValue(map, out GlowGridHelper helper))
            {
                helpers[map] = helper = new GlowGridHelper(map);
            }
            return helper;
        }

        [ProtonPatch(typeof(GlowGrid), nameof(GlowGrid.RegisterGlower))]
        class GlowGrid_RegisterGlower_Patch
        {
            public static void Prefix(GlowGrid __instance, CompGlower newGlow)
            {                
                skipMarking = true;
                GetHelper(__instance.map).Register(newGlow);
            }

            public static void Postfix()
            {
                skipMarking = false;
            }
        }

        [ProtonPatch(typeof(GlowGrid), nameof(GlowGrid.DeRegisterGlower))]
        class GlowGrid_DeRegisterGlower_Patch
        {
            public static void Prefix(GlowGrid __instance, CompGlower oldGlow)
            {               
                skipMarking = true;
                GetHelper(__instance.map).DeRegister(oldGlow);
            }

            public static void Postfix()
            {
                skipMarking = false;
            }
        }

        [ProtonPatch(typeof(GlowGrid), nameof(GlowGrid.MarkGlowGridDirty))]
        class GlowGrid_MarkGlowGridDirty_Patch
        {
            public static void Prefix(GlowGrid __instance, IntVec3 loc)
            {
                if (!skipMarking && RocketPrefs.GlowGridOptimization && RocketPrefs.Enabled)
                {
                    GetHelper(__instance.map).MarkPositionDirty(loc);
                }
            }
        }

        [ProtonPatch(typeof(GlowGrid), nameof(GlowGrid.RecalculateAllGlow))]
        class GlowGrid_RecalculateAllGlow_Patch
        {
            static Stopwatch stopwatch = new Stopwatch();
            static float t1;
            static float t2;            

            public static bool Prefix(GlowGrid __instance)
            {
                if (!RocketPrefs.GlowGridOptimization || !RocketPrefs.Enabled)
                {
                    GetHelper(__instance.map).Reset();
                    return true;
                }
                //if (RocketPrefs.GlowGridOptimizationLimiter && !RocketDebugPrefs.Debug)
                //{
                //    int glowerNum = GetHelper(__instance.map).litGlowers.Count;
                //    if (glowerNum < 5)
                //    {
                //        GetHelper(__instance.map).Reset();
                //        return true;
                //    }
                //}
                if (__instance.glowGridDirty && Current.ProgramState == ProgramState.Playing)
                {
                    stopwatch.Restart();
                    skipMarking = true;
                    GetHelper(__instance.map).Recalculate();
                    skipMarking = false;
                    stopwatch.Stop();
                    t1 = stopwatch.ElapsedTicks / (float) Stopwatch.Frequency * 1000f;
                }
                stopwatch.Restart();
                return RocketDebugPrefs.Debug;
            }

            public static void Postfix()
            {
                if (RocketDebugPrefs.Debug && stopwatch.IsRunning && Current.ProgramState == ProgramState.Playing)
                {
                    stopwatch.Stop();
                    t2 = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency * 1000f;
                    Log.Message($"PROTON: GlowGrid vanilla took <color=orange>{Math.Round(t2,2)} MS</color> vs <color=orange>{Math.Round(t1, 2)} MS</color>. " +
                        $"Total improvement <color=orange>{Math.Round(t2 - t1, 2)} MS</color> or <color=orange>+{Math.Round(t2 / Mathf.Max(t1, 1e-4f), 2) * 100 - 100} %</color>");
                }
            }
        }        
    }
}

