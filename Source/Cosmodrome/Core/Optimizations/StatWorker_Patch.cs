using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RocketMan.Optimizations
{

    public static class StatWorker_Patch
    {
        [StructLayout(LayoutKind.Sequential, Size = 12)]
        private struct CachedUnit
        {
            public readonly float value;
            public readonly int signature;
            public readonly int tick;

            public CachedUnit(float value, int signature)
            {
                this.value = value;
                this.signature = signature;
                this.tick = GenTicks.TicksGame;
            }
        }

        private static CachedUnit store;

        private static bool hijackedCaller = false;

        private static Dictionary<int, CachedUnit> cache = new Dictionary<int, CachedUnit>();

        private static MethodBase mGetValueUnfinalized = AccessTools.Method(typeof(StatWorker), "GetValueUnfinalized", new[] { typeof(StatRequest), typeof(bool) });

        private static MethodBase mGetValueUnfinalized_Replacemant = AccessTools.Method(typeof(StatWorker_Patch), nameof(StatWorker_Patch.Replacemant));

        private static MethodBase mGetValueUnfinalized_Transpiler = AccessTools.Method(typeof(StatWorker_Patch), nameof(StatWorker_Patch.Hijack));

        private static FieldInfo fHijackedCaller = AccessTools.Field(typeof(StatWorker_Patch), nameof(hijackedCaller));

        public static void Dirty(Pawn pawn)
        {
            int signature = pawn.GetSignature(dirty: true);

            if (RocketDebugPrefs.Debug && RocketDebugPrefs.StatLogging && Prefs.LogVerbose)
                Log.Message(string.Format("ROCKETMAN: changed signature for pawn {0} to {1}", pawn, signature));
        }

        [Main.OnTickLonger]
        public static void OnTickLonger()
        {
            if (Rand.Chance(0.25f))
                cache.Clear();
        }

        [RocketPatch]
        private static class AutoPatcher_GetValueUnfinalized_Patch
        {
            public static HashSet<MethodBase> callingMethods = new HashSet<MethodBase>();

            public static MethodBase mInterrupt =
                AccessTools.Method(typeof(AutoPatcher_GetValueUnfinalized_Patch), "Interrupt");

            public static IEnumerable<MethodBase> TargetMethods()
            {
                foreach (var m in typeof(StatWorker)
                        .AllLeafSubclasses()
                        .Where(t => !t.IsAbstract)
                        .Select(t => AccessTools.Method(t, "GetValueUnfinalized", parameters: new[] { typeof(StatRequest), typeof(bool) }))
                        .Where(m => m != null && m.IsValidTarget())
                        .ToHashSet())
                    yield return m;
                MethodBase baseMethod = AccessTools.Method(typeof(StatWorker), nameof(StatWorker.GetValueUnfinalized));
                if (baseMethod != null && baseMethod.IsValidTarget())
                    yield return baseMethod;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                List<CodeInstruction> codes = instructions.ToList();
                Label l1 = generator.DefineLabel();

                yield return new CodeInstruction(OpCodes.Ldsfld, fHijackedCaller);
                yield return new CodeInstruction(OpCodes.Brtrue_S, l1);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, mInterrupt);

                if (codes[0].labels == null)
                    codes[0].labels = new List<Label>();
                codes[0].labels.Add(l1);
                foreach (var code in codes)
                    yield return code;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Interrupt(StatWorker statWorker)
            {
                int tick = GenTicks.TicksGame;
                if (RocketPrefs.Enabled && Current.Game != null && tick >= 600 && !IgnoreMeDatabase.ShouldIgnore(statWorker.stat))
                {
                    StackTrace trace = new StackTrace();
                    StackFrame frame = trace.GetFrame(2);
                    MethodBase method = frame.GetMethod();
                    try
                    {
                        Finder.Harmony.Patch(method, transpiler: new HarmonyMethod((MethodInfo)mGetValueUnfinalized_Transpiler));

                        Log.Message($"ROCKETMAN: Auto patched {method.GetMethodPath()}!");
                    }
                    catch (Exception er)
                    {
                        Logger.Debug($"ROCKETMAN: Auto patching failed {method.GetMethodPath()}!", er);
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> Hijack(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(mGetValueUnfinalized, mGetValueUnfinalized_Replacemant);
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Replacemant(StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            int tick = GenTicks.TicksGame;
            if (RocketPrefs.Enabled && Current.Game != null && tick >= 600 && !IgnoreMeDatabase.ShouldIgnore(statWorker.stat))
            {
                int key = Tools.GetKey(statWorker, req, applyPostProcess);
                int signature = req.thingInt?.GetSignature() ?? -1;

                if (!cache.TryGetValue(key, out store))
                    return UpdateCache(key, statWorker, req, applyPostProcess, tick, storeExists: false);

                if (tick - store.tick - 1 > RocketStates.StatExpiry[statWorker.stat.index] || signature != store.signature)
                {
                    cache.Remove(key);
                    return UpdateCache(key, statWorker, req, applyPostProcess, tick, storeExists: true);
                }
                return store.value;
            }
            return statWorker.GetValueUnfinalized(req, applyPostProcess);
        }

        private static float UpdateCache(int key, StatWorker statWorker, StatRequest req, bool applyPostProcess,
            int tick, bool storeExists = true)
        {
            Exception error = null;
            float value = -1;
            try
            {
                hijackedCaller = true;
                value = statWorker.GetValueUnfinalized(req, applyPostProcess);
            }
            catch (Exception er)
            {
                error = er;
            }
            finally
            {
                hijackedCaller = false;
                if (error != null)
                {
                    Logger.Debug("ROCKETMAN:[NOTROCKETMAN] RocketMan caught an error in StatWorker.GetValueUnfinalized. " +
                                 "RocketMan doesn't modify the inners of this method.", exception: error);
                    throw error;
                }
            }
            if (RocketPrefs.Learning && storeExists)
            {
                float t = RocketStates.StatExpiry[statWorker.stat.index];
                float T = tick - store.tick;
                float a = Mathf.Abs(value - store.value) / Mathf.Max(value, store.value, 1f);
                RocketStates.StatExpiry[statWorker.stat.index] = Mathf.Clamp(
                        t - Mathf.Clamp(RocketPrefs.LearningRate * (T * a - t), -0.1f, 0.25f),
                        0f, 1024f);
            }
            cache[key] = new CachedUnit(value, req.thingInt?.GetSignature() ?? -1);
            return value;
        }
    }
}