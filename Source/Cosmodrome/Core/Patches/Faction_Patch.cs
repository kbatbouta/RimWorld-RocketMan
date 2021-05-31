using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RocketMan
{
    public static class Faction_Patch
    {
        [RocketPatch(typeof(Faction), nameof(Faction.FactionTick))]
        public static class Faction_FactionTick_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                Label l1 = generator.DefineLabel();
                Label l2 = generator.DefineLabel();
                int i = 0;
                int length = instructions.Count();
                MethodBase mLogOnce = AccessTools.Method(typeof(Log), nameof(Log.ErrorOnce));
                foreach (CodeInstruction code in instructions)
                {
                    if (code.opcode == OpCodes.Ret && i == length - 1)
                    {
                        if (code.labels == null)
                        {
                            code.labels = new List<Label>();
                        }
                        code.labels.Add(l2);
                    }
                    if (code.opcode == OpCodes.Call && code.OperandIs(mLogOnce) && i >= length / 2)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Faction_FactionTick_Patch), nameof(Faction_FactionTick_Patch.FixMissingFactionLeader)));

                        yield return new CodeInstruction(OpCodes.Brfalse_S, l1);

                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Pop);

                        yield return new CodeInstruction(OpCodes.Br_S, l2);
                        if (code.labels == null)
                        {
                            code.labels = new List<Label>();
                        }
                        code.labels.Add(l1);
                    }
                    i++;
                    yield return code;
                }
            }

            public static bool FixMissingFactionLeader(Faction faction)
            {
                try
                {
                    if (faction.TryGenerateNewLeader())
                    {
                        Log.Message("ROCKETMAN: RocketMan just fixed Faction leader being null before it had any chance to cause any problems");
                        return true;
                    }
                }
                catch (Exception er)
                {
                    Log.ErrorOnce($"ROCKETMAN: Attempted to use <color=orange>Faction.TryGenerateNewLeader()</color> which Failed with code {er}", faction.loadID ^ 0x156BDDD);
                }
                return false;
            }
        }
    }
}
