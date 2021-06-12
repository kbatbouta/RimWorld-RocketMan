//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Emit;
//using System.Xml.Serialization;
//using HarmonyLib;
//using RocketMan;
//using Verse;

//namespace Proton
//{
//    public static class GlowGrid_Patch
//    {
//        //[ProtonPatch(typeof(GlowGrid), nameof(GlowGrid.RegisterGlower))]
//        //public static class RegisterGlower_Patch
//        //{
//        //    public static void Prefix(CompGlower newGlow)
//        //    {
//        //        newGlow.parent.Map?.GetGlowerTracker()?.Register(newGlow);
//        //    }
//        //}

//        //[ProtonPatch(typeof(GlowGrid), nameof(GlowGrid.DeRegisterGlower))]
//        //public static class DeRegisterGlower_Patch
//        //{
//        //    public static void Prefix(CompGlower oldGlow)
//        //    {
//        //        oldGlow.parent.Map?.GetGlowerTracker()?.DeRegister(oldGlow);
//        //    }
//        //}

//        //[ProtonPatch(typeof(GlowGrid), nameof(GlowGrid.MarkGlowGridDirty))]
//        //public static class MarkGlowGridDirty_Patch
//        //{
//        //    public static void Prefix(IntVec3 loc, GlowGrid __instance)
//        //    {
//        //        __instance.map.GetGlowerTracker().Notify_ChangeAt(loc);
//        //    }
//        //}

//        //[ProtonPatch(typeof(GlowGrid), nameof(GlowGrid.RecalculateAllGlow))]
//        //public static class RecalculateAllGlow_Patch
//        //{
//        //    public static bool Prefix(GlowGrid __instance)
//        //    {
//        //        if (Current.ProgramState != ProgramState.Playing)
//        //        {
//        //            return false;
//        //        }
//        //        if (__instance.initialGlowerLocs != null)
//        //        {
//        //            foreach (IntVec3 initialGlowerLoc in __instance.initialGlowerLocs)
//        //            {
//        //                __instance.MarkGlowGridDirty(initialGlowerLoc);
//        //            }
//        //            __instance.initialGlowerLocs = null;
//        //        }
//        //        __instance.map.GetGlowerTracker().RecalculateAllGlow();
//        //        return false;
//        //    }
//        //}
//    }
//}
