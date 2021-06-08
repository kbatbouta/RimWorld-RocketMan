using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;
using RocketMan;
using Verse;

namespace Soyuz.Patches
{
    public class WorldPawns_Patch
    {
        [SoyuzPatch(typeof(WorldPawns), nameof(WorldPawns.ExposeData))]
        public static class WorldPawns_ExposeData_Patch
        {
            public static void Postfix(WorldPawns __instance)
            {
                WorldPawnsTicker.SetDirty();
            }
        }

        [SoyuzPatch(typeof(WorldPawns), nameof(WorldPawns.DoMothballProcessing))]
        public static class WorldPawns_DoMothballProcessing_Patch
        {
            public static void Prefix(WorldPawns __instance)
            {
                WorldPawnsTicker.isActive = false;
            }

            public static void Postfix(WorldPawns __instance)
            {
                WorldPawnsTicker.isActive = true;
                WorldPawnsTicker.SetDirty();
            }
        }

        [SoyuzPatch(typeof(WorldPawns), nameof(WorldPawns.AddPawn))]
        public static class WorldPawns_AddPawn_Patch
        {
            public static void Prefix(Pawn p)
            {
                WorldPawnsTicker.SetDirty();
                WorldPawnsTicker.Register(p);
            }
        }

        [SoyuzPatch(typeof(WorldPawns), nameof(WorldPawns.Notify_PawnDestroyed))]
        public static class WorldPawns_Notify_PawnDestroyed_Patch
        {
            public static void Prefix(Pawn p)
            {
                WorldPawnsTicker.SetDirty();
                WorldPawnsTicker.Deregister(p);
            }
        }

        [SoyuzPatch(typeof(WorldPawns), nameof(WorldPawns.RemovePawn))]
        public static class WorldPawns_RemovePawn_Patch
        {
            public static void Prefix(Pawn p)
            {
                WorldPawnsTicker.SetDirty();
                WorldPawnsTicker.Deregister(p);
            }
        }

        [SoyuzPatch(typeof(WorldPawns), nameof(WorldPawns.ShouldAutoTendTo))]
        public class WorldPawns_ShouldAutoTendTo_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
                ILGenerator generator)
            {
                return instructions.MethodReplacer(
                    AccessTools.Method(typeof(Gen), nameof(Gen.IsHashIntervalTick), new[] { typeof(Thing), typeof(int) }),
                    AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsCustomTickInterval)));
            }
        }

        [SoyuzPatch(typeof(WorldPawns), nameof(WorldPawns.WorldPawnsTick))]
        public static class WorldPawns_WorldPawnsTick_Patch
        {
            private static readonly FieldInfo fAlivePawns =
                AccessTools.Field(typeof(WorldPawns), nameof(WorldPawns.pawnsAlive));

            public static void Prefix()
            {
                WorldPawnsTicker.isActive = true;
            }

            public static void Postfix()
            {
                WorldPawnsTicker.isActive = false;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var finished = false;

                for (var i = 0; i < codes.Count; i++)
                {
                    if (!finished)
                        if (codes[i].OperandIs(fAlivePawns))
                        {
                            finished = true;
                            yield return codes[i];
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call,
                                AccessTools.Method(typeof(WorldPawns_WorldPawnsTick_Patch),
                                    nameof(GetAlivePawns)));
                            continue;
                        }
                    yield return codes[i];
                }
            }

            private static HashSet<Pawn> GetAlivePawns(HashSet<Pawn> pawns, WorldPawns instance)
            {
                if (false
                    || !RocketPrefs.TimeDilation
                    || !RocketPrefs.TimeDilationWorldPawns
                    || !RocketPrefs.Enabled)
                    return pawns;
                return WorldPawnsTicker.GetPawns();
            }
        }
    }
}