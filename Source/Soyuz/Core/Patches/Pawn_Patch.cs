using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RocketMan;
using Verse;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(Pawn), nameof(Pawn.Tick))]
    public class Pawn_Tick_Patch
    {
        private static MethodInfo mSuspended = AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.Suspended));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            var finished = false;

            var localSkipper = generator.DeclareLocal(typeof(bool));
            var l1 = generator.DefineLabel();
            var l2 = generator.DefineLabel();

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.BeginTick)));

            for (int i = 0; i < codes.Count; i++)
            {
                if (!finished)
                {
                    if (codes[i].OperandIs(mSuspended))
                    {
                        finished = true;
                        yield return codes[i];
                        yield return new CodeInstruction(OpCodes.Dup);
                        yield return new CodeInstruction(OpCodes.Brtrue_S, l2);

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsValidWildlifeOrWorldPawn)));
                        yield return new CodeInstruction(OpCodes.Brfalse_S, l2);

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.ShouldTick)));

                        yield return new CodeInstruction(OpCodes.Brtrue_S, l1);
                        {
                            yield return new CodeInstruction(OpCodes.Pop);

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Pawn_Tick_Patch), nameof(Pawn_Tick_Patch.TickExtras)));

                            yield return new CodeInstruction(OpCodes.Ldc_I4_1); // push suspended = true to the stack
                            yield return new CodeInstruction(OpCodes.Br_S, l2); // go to if(suspended)
                        }
                        yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = new List<Label>() { l1 } };
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.UpdateTimers)));
                        codes[i + 1].labels.Add(l2);
                        continue;
                    }
                }
                if (codes[i].opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = codes[i].labels };
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.EndTick)));
                    codes[i].labels = new List<Label>();
                }
                yield return codes[i];
            }

        }

        private static Exception Finalizer(Exception __exception)
        {
            if (__exception != null) ContextualExtensions.Reset();
            return __exception;
        }

        private static void TickExtras(Pawn pawn)
        {
            if (pawn.Spawned)
            {
                pawn.stances?.StanceTrackerTick();
                if (!pawn.OffScreen())
                {
                    pawn.drawer?.DrawTrackerTick();
                    pawn.rotationTracker?.RotationTrackerTick();
                }
            }
            if (RocketDebugPrefs.FlashDilatedPawns && pawn.Spawned)
                pawn.Map.debugDrawer.FlashCell(pawn.positionInt, 0.05f, $"{pawn.OffScreen()}", 100);
        }
    }
}