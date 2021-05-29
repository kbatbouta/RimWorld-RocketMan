using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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

    [RocketPatch()]
    internal static class StatWorker_GetValueUnfinalized_Hijacked_Patch
    {
        internal static MethodBase m_GetValueUnfinalized = AccessTools.Method(typeof(StatWorker), "GetValueUnfinalized",
            new[] { typeof(StatRequest), typeof(bool) });

        internal static MethodBase m_GetValueUnfinalized_Replacemant =
            AccessTools.Method(typeof(StatWorker_GetValueUnfinalized_Hijacked_Patch), "Replacemant");

        internal static MethodBase m_GetValueUnfinalized_Transpiler =
            AccessTools.Method(typeof(StatWorker_GetValueUnfinalized_Hijacked_Patch), "Transpiler");

        internal static Dictionary<int, int> signatures = new Dictionary<int, int>();

        internal static Dictionary<int, Tuple<float, int, int>> cache =
            new Dictionary<int, Tuple<float, int, int>>(1000);

        internal static List<Tuple<int, int, float>> requests = new List<Tuple<int, int, float>>();

        internal static Dictionary<int, float> expiryCache = new Dictionary<int, float>();
        internal static List<string> messages = new List<string>();

        internal static int counter;
        internal static int cleanUps;

        private static Stopwatch expiryStopWatch = new Stopwatch();

        internal static void ProcessExpiryCache()
        {
            if (requests.Count == 0 || Find.TickManager == null)
                return;
            expiryStopWatch.Reset();
            expiryStopWatch.Start();
            if (RocketPrefs.Learning && !Find.TickManager.Paused && Find.TickManager.TickRateMultiplier <= 3f)
                if (counter++ % 20 == 0 && expiryCache.Count != 0)
                {
                    foreach (var unit in expiryCache)
                    {
                        RocketStates.StatExpiry[unit.Key] = (byte)Mathf.Clamp(unit.Value, 0f, 255f);
                        cleanUps++;
                    }
                    expiryCache.Clear();
                }

            while (requests.Count > 0 && expiryStopWatch.ElapsedMilliseconds <= 1)
            {
                Tuple<int, int, float> request;
                request = requests.Pop();
                var statIndex = request.Item1;

                var deltaT = Mathf.Abs(request.Item2);
                var deltaX = Mathf.Abs(request.Item3);

                if (expiryCache.TryGetValue(statIndex, out var value))
                    expiryCache[statIndex] +=
                        Mathf.Clamp(RocketPrefs.LearningRate * (deltaT / 100 - deltaX * deltaT), -5, 5);
                else
                    expiryCache[statIndex] = RocketStates.StatExpiry[statIndex];
            }
            expiryStopWatch.Stop();
        }

        [Main.OnTickLong]
        public static void CleanCache()
        {
            if (Find.TickManager.TickRateMultiplier <= 3f)
                cache.Clear();
        }

        public static void Dirty(Pawn pawn)
        {
            var signature = pawn.GetSignature(true);
#if DEBUG
            if (RocketDebugPrefs.Debug) Log.Message(string.Format("ROCKETMAN: changed signature for pawn {0} to {1}", pawn, signature));
#endif
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
            var methods = TargetMethodsUnfinalized().Where(m => true
                                                                && m != null
                                                                && !m.IsAbstract
                                                                && m.HasMethodBody()
                                                                && !m.DeclaringType.IsAbstract).ToHashSet();

            return methods;
        }

        public static float UpdateCache(int key, StatWorker statWorker, StatRequest req, bool applyPostProcess,
            int tick, Tuple<float, int, int> store)
        {
            var value = statWorker.GetValueUnfinalized(req, applyPostProcess);
            if (RocketPrefs.Learning)
            {
                requests.Add(new Tuple<int, int, float>(statWorker.stat.index, tick - (store?.Item2 ?? tick),
                    Mathf.Abs(value - (store?.Item1 ?? value))));
                if (Rand.Chance(0.1f)) ProcessExpiryCache();
            }

            cache[key] = new Tuple<float, int, int>(value, tick, req.thingInt?.GetSignature() ?? -1);
            return value;
        }

        public static float Replacemant(StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            var tick = GenTicks.TicksGame;
            if (true
                && RocketPrefs.Enabled
                && Current.Game != null
                && tick >= 600
                && !IgnoreMeDatabase.ShouldIgnore(statWorker.stat))
            {
                var key = Tools.GetKey(statWorker, req, applyPostProcess);
                var signature = req.thingInt?.GetSignature() ?? -1;

                if (!cache.TryGetValue(key, out var store))
                    return UpdateCache(key, statWorker, req, applyPostProcess, tick, store);

                if (tick - store.Item2 - 1 > RocketStates.StatExpiry[statWorker.stat.index] || signature != store.Item3)
                    return UpdateCache(key, statWorker, req, applyPostProcess, tick, store);
                return store.Item1;
            }
            return statWorker.GetValueUnfinalized(req, applyPostProcess);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(m_GetValueUnfinalized, m_GetValueUnfinalized_Replacemant);
        }
    }
}