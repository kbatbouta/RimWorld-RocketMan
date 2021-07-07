using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RocketMan.Optimizations
{
    // TODO check if we need to update this
    // [RocketPatch(typeof(FloatMenuMakerMap), nameof(FloatMenuMakerMap.AddJobGiverWorkOrders_NewTmp))]
    // public static class FloatMenuMakerMapMap_Patch
    // {
    //    public static MethodBase mPotentialWorkThings = AccessTools.Method(typeof(WorkGiver_Scanner), nameof(WorkGiver_Scanner.PotentialWorkThingsGlobal));
    //
    //    [HarmonyPriority(int.MaxValue)]
    //    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    //    {
    //        LocalBuilder cachedResult = generator.DeclareLocal(typeof(IEnumerable<Thing>));
    //        CodeInstruction[] codes = instructions.ToArray();
    //        bool finished = false;
    //        for (int i = 0; i < codes.Length; i++)
    //        {
    //            CodeInstruction code = codes[i];
    //
    //            if (!finished && code.opcode == OpCodes.Callvirt && code.OperandIs(mPotentialWorkThings))
    //            {
    //                finished = true;
    //                yield return code;
    //                yield return new CodeInstruction(OpCodes.Stloc_S, cachedResult.LocalIndex);
    //                yield return new CodeInstruction(OpCodes.Ldloc_S, cachedResult.LocalIndex);
    //                i++;
    //                yield return codes[i];
    //
    //                yield return new CodeInstruction(OpCodes.Ldloc_S, cachedResult.LocalIndex);
    //                int j = 0;
    //                while (!codes[i].OperandIs(mPotentialWorkThings) && j < 20)
    //                {
    //                    i++;
    //                    j++;
    //                }
    //                if (j >= 10)
    //                {
    //                    Exception er = new Exception();
    //                    Logger.Debug($"Patching failed! unable to find the second {mPotentialWorkThings}", er);
    //                    throw er;
    //                }
    //                continue;
    //            }
    //            yield return code;
    //        }
    //    }
    //
    //    public static Exception Cleanup(MethodBase original)
    //    {
    //        return null;
    //    }
    // }
}
