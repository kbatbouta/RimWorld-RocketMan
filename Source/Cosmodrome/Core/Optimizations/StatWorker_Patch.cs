using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RocketMan.Optimizations
{
    [RocketPatch(typeof(StatWorker), "GetValueUnfinalized", parameters: new[] { typeof(StatRequest), typeof(bool) })]
    internal static class StatWorker_GetValueUnfinalized_Interrupt_Patch
    {
        public static HashSet<MethodBase> callingMethods = new HashSet<MethodBase>();

        public static MethodBase m_Interrupt =
            AccessTools.Method(typeof(StatWorker_GetValueUnfinalized_Interrupt_Patch), "Interrupt");

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, m_Interrupt);

            foreach (var code in instructions)
                yield return code;
        }

        public static void Interrupt(StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            if (RocketPrefs.Learning && RocketDebugPrefs.StatLogging)
            {
                StackTrace trace = new StackTrace();
                StackFrame frame = trace.GetFrame(2);
                MethodBase method = frame.GetMethod();
                string handler = method.GetMethodPath();
                if (RocketDebugPrefs.Debug) Log.Message(string.Format("ROCKETMAN: called stats.GetUnfinalizedValue from {0}", handler));
                callingMethods.Add(method);
            }
        }
    }

    [RocketPatch]
    internal static class StatWorker_GetValueUnfinalized_Hijacked_Patch
    {
        internal static MethodBase m_GetValueUnfinalized = AccessTools.Method(typeof(StatWorker), "GetValueUnfinalized",
            new[] { typeof(StatRequest), typeof(bool) });

        internal static MethodBase m_GetValueUnfinalized_Replacemant =
            AccessTools.Method(typeof(StatWorker_GetValueUnfinalized_Hijacked_Patch), "Replacemant");

        internal static MethodBase m_GetValueUnfinalized_Transpiler =
            AccessTools.Method(typeof(StatWorker_GetValueUnfinalized_Hijacked_Patch), "Transpiler");

        internal static Dictionary<int, Tuple<float, int, int>> cache = new Dictionary<int, Tuple<float, int, int>>();

        public static void Dirty(Pawn pawn)
        {
            int signature = pawn.GetSignature(true);
            if (RocketDebugPrefs.Debug && RocketDebugPrefs.StatLogging)
                Log.Message(string.Format("ROCKETMAN: changed signature for pawn {0} to {1}", pawn, signature));
        }

        internal static IEnumerable<MethodBase> TargetMethodsUnfinalized()
        {
            yield return AccessTools.Method(typeof(BeautyUtility), "CellBeauty");
            yield return AccessTools.Method(typeof(BeautyUtility), "AverageBeautyPerceptible");
            yield return AccessTools.Method(typeof(StatExtension), "GetStatValue");
            yield return AccessTools.Method(typeof(StatWorker), "GetValue", new[] { typeof(StatRequest), typeof(bool) });

            foreach (var type in typeof(StatWorker).AllSubclassesNonAbstract())
                yield return AccessTools.Method(type, "GetValue", new[] { typeof(StatRequest), typeof(bool) });

            foreach (var type in typeof(StatPart).AllSubclassesNonAbstract())
                yield return AccessTools.Method(type, "TransformValue", new[] { typeof(StatRequest), typeof(float).MakeByRefType() });

            foreach (var type in typeof(StatExtension).AllSubclassesNonAbstract())
            {
                yield return AccessTools.Method(type, "GetStatValue");
                yield return AccessTools.Method(type, "GetStatValueAbstract");
            }
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            return TargetMethodsUnfinalized()
                .Where(m => true && m != null && m.IsValidTarget())
                .ToHashSet();
        }

        [HarmonyPriority(int.MaxValue)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(m_GetValueUnfinalized, m_GetValueUnfinalized_Replacemant);
        }

        [Main.OnTickLonger]
        public static void OnTickLonger() => cache.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float UpdateCache(int key, StatWorker statWorker, StatRequest req, bool applyPostProcess,
            int tick, Tuple<float, int, int> store, int signature)
        {
            float value = statWorker.GetValueUnfinalized(req, applyPostProcess);
            if (RocketPrefs.Learning && store != null)
            {
                float t = RocketStates.StatExpiry[statWorker.stat.index];
                float T = (tick - store.Item2);
                float a = Mathf.Abs(value - store.Item1) / Mathf.Max(value, store.Item1, 1f);
                RocketStates.StatExpiry[statWorker.stat.index] = Mathf.Clamp(
                        t - Mathf.Clamp(RocketPrefs.LearningRate * (T * a - t), -0.1f, 0.25f),
                        0f, 1024f);
            }
            cache[key] = new Tuple<float, int, int>(value, tick, req.thingInt?.GetSignature() ?? -1);
            return value;
        }

        private static float Replacemant(StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            var tick = GenTicks.TicksGame;
            if (true
                && RocketPrefs.Enabled
                && Current.Game != null
                && tick >= 600
                && !IgnoreMeDatabase.ShouldIgnore(statWorker.stat))
            {
                int key = Tools.GetKey(statWorker, req, applyPostProcess);
                int signature = req.thingInt?.GetSignature() ?? -1;

                if (!cache.TryGetValue(key, out var store))
                    return UpdateCache(key, statWorker, req, applyPostProcess, tick, store, signature);

                if (tick - store.Item2 - 1 > RocketStates.StatExpiry[statWorker.stat.index] || signature != store.Item3)
                {
                    cache.Remove(key);
                    return UpdateCache(key, statWorker, req, applyPostProcess, tick, store, signature);
                }
                return store.Item1;
            }
            return statWorker.GetValueUnfinalized(req, applyPostProcess);
        }
    }
}