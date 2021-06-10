using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Xml.Serialization;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Proton
{
    public class GlowFlooder_Patch
    {
        //public static GlowerProperties CurrentGlowerProperties;

        //[ProtonPatch(typeof(GlowFlooder), nameof(GlowFlooder.AddFloodGlowFor))]
        //internal static class SAddFloodGlowFor_Patch
        //{
        //    public static void Prefix(CompGlower theGlower)
        //    {
        //        CurrentGlowerProperties = theGlower.GetProperties();
        //    }

        //    public static void Postfix(CompGlower theGlower)
        //    {
        //        if (RocketDebugPrefs.Debug && RocketDebugPrefs.DrawGlowerUpdates)
        //        {
        //            FlashIndices();
        //        }
        //        CurrentGlowerProperties = null;
        //    }

        //    private static void FlashIndices()
        //    {
        //        Map map = CurrentGlowerProperties.Parent.Map;
        //        CellIndices indices = map.cellIndices;
        //        foreach (int index in CurrentGlowerProperties.AllIndices)
        //        {
        //            map.debugDrawer.FlashCell(indices.IndexToCell(index), 0.1f, duration: 100, text: "f");
        //        }
        //    }
        //}

        //[ProtonPatch(typeof(GlowFlooder), nameof(GlowFlooder.SetGlowGridFromDist))]
        //internal static class SetGlowGridFromDist_Patch
        //{
        //    public static void Prefix()
        //    {
        //        CurrentGlowerProperties.Reset();
        //    }

        //    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
        //        ILGenerator generator)
        //    {
        //        var codes = instructions.ToList();
        //        for (var i = 0; i < codes.Count - 1; i++)
        //            yield return codes[i];

        //        yield return new CodeInstruction(OpCodes.Ldarg_1);
        //        yield return new CodeInstruction(OpCodes.Ldloc_1);
        //        yield return new CodeInstruction(OpCodes.Ldloc_0);
        //        yield return new CodeInstruction(OpCodes.Call,
        //            AccessTools.Method(typeof(SetGlowGridFromDist_Patch), nameof(PushIndex)));
        //        yield return codes.Last();
        //    }

        //    public static void PushIndex(int index, ColorInt color, float distance)
        //    {
        //        CurrentGlowerProperties.AllIndices.Add(index);
        //    }
        //}
    }
}
