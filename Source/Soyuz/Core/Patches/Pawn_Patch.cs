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
                        yield return new CodeInstruction(OpCodes.Brtrue_S, l1);

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsBeingThrottled)));
                        yield return new CodeInstruction(OpCodes.Brfalse_S, l1);

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.ShouldTick)));
                        yield return new CodeInstruction(OpCodes.Dup);
                        yield return new CodeInstruction(OpCodes.Stloc_S, localSkipper.LocalIndex);
                        yield return new CodeInstruction(OpCodes.Brtrue_S, l1);
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Pawn_Tick_Patch), nameof(Pawn_Tick_Patch.TickExtras)));

                            yield return new CodeInstruction(OpCodes.Pop); // remove "false" from the top of the stack
                            yield return new CodeInstruction(OpCodes.Ldc_I4_1); // push "true" to the stack                            
                        }
                        codes[i + 1].labels.Add(l1); // bool suspended = [patch] || base.Suspended;
                        continue;
                    }
                }
                if (codes[i].opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = codes[i].labels };
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.EndTick)));

                    yield return new CodeInstruction(OpCodes.Ldloc_S, localSkipper.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, l2);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.UpdateTimers)));

                    codes[i].labels = new List<Label>() { l2 };
                }
                yield return codes[i];
            }
        }

        private static Exception Finalizer(Pawn __instance, Exception __exception)
        {
            if (__exception != null)
            {
                __instance?.UpdateTimers();
                ContextualExtensions.Reset();

                Logger.Debug("SOYUZ:[README] !!IMPORTANT!! There is a large chance this error is not related to RocketMan at all. RocketMan has patch on pawns that enables throttling, it will cause it to appear in the stacktrace and in visual exceptions, but in 99.9% of the time it's not RocketMan causing this issue but something down the line. RocketMan patches Pawn.Tick function just to add the throttling ability, the new code doesn't have any known issues and the issue is most likely futher down the line ", exception: __exception);
            }
            return __exception;
        }

        private static void TickExtras(Pawn pawn)
        {
            bool jobTrackerTicked = false;
            if (pawn.Spawned)
            {
                pawn.stances?.StanceTrackerTick();
                //if (!pawn.OffScreen())
                //{
                //    pawn.drawer?.DrawTrackerTick();
                //    pawn.rotationTracker?.RotationTrackerTick();
                //}
                if (Context.CurJobSettings.throttleMode == JobThrottleMode.Partial && pawn.pather != null && !pawn.pather.MovingNow)
                {
                    Exception exception = null;

                    Context.PartiallyDilatedContext = true;
                    try
                    {
                        pawn.jobs.JobTrackerTick();
                    }
                    catch (Exception er)
                    {
                        exception = er;
                    }
                    finally
                    {
                        Context.PartiallyDilatedContext = false;

                        jobTrackerTicked = true;
                    }
                    if (exception != null)
                    {
                        throw exception;
                    }
                }
                if (RocketDebugPrefs.FlashDilatedPawns)
                    pawn.Map.debugDrawer.FlashCell(pawn.positionInt, !jobTrackerTicked ? 0.05f : 0.5f, $"{pawn.OffScreen()}{(jobTrackerTicked ? "P" : "F")}", 100);
            }
        }
    }
}