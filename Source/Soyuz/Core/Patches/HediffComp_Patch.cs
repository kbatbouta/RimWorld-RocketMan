using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Soyuz.Patches
{
    public class HediffComp_Patch
    {
        [SoyuzPatch]
        public static class HediffComp_GenHashInterval_Replacement
        {
                public static IEnumerable<MethodBase> TargetMethods()
                {
                    MethodBase method;
                    yield return AccessTools.Method(typeof(Hediff), nameof(Hediff.Tick));
                    foreach (var type in typeof(Hediff).AllSubclassesNonAbstract())
                    {
                        method = type.GetMethod("Tick");
                        if (method != null && method.HasMethodBody()) yield return method;
                    }
                    
                    yield return AccessTools.Method(typeof(HediffComp), nameof(HediffComp.CompPostTick));
                    foreach (var type in typeof(HediffComp).AllSubclassesNonAbstract())
                    {
                        method = type.GetMethod("CompPostTick");
                        if (method != null && method.HasMethodBody()) yield return method;
                    }
                }

                public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    return instructions.MethodReplacer(
                        AccessTools.Method(typeof(Gen), nameof(Gen.IsHashIntervalTick), new[] {typeof(Thing), typeof(int)}),
                        AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsCustomTickInterval)));
                }
        }
        
        [SoyuzPatch(typeof(HediffComp_ChanceToRemove), nameof(HediffComp_ChanceToRemove.CompPostTick))]
        public static class HediffComp_ChanceToRemove_Patch
        {
            public static void Prefix(HediffComp_ChanceToRemove __instance)
            {
                if(true 
                   && __instance.parent.pawn.IsBeingThrottled()
                   && __instance.parent.pawn.IsValidWildlifeOrWorldPawn())
                    __instance.currentInterval -= __instance.parent.pawn.GetDeltaT();
            }   
        }
        
        [SoyuzPatch(typeof(HediffComp_ChangeNeed), nameof(HediffComp_ChangeNeed.CompPostTick))]
        public static class HediffComp_ChangeNeed_Patch
        {
            public static void Prefix(HediffComp_ChangeNeed __instance)
            {
                if(true 
                   && __instance.parent.pawn.IsBeingThrottled()
                   && __instance.parent.pawn.IsValidWildlifeOrWorldPawn())
                    if (__instance.Need != null)
                        __instance.Need.CurLevelPercentage += __instance.Props.percentPerDay / 60000f * (__instance.parent.pawn.GetDeltaT() - 1);
            }   
        }
        
        [SoyuzPatch(typeof(HediffComp_Disappears), nameof(HediffComp_Disappears.CompPostTick))]
        public static class HediffComp_Disappears_Patch
        {
            public static void Prefix(HediffComp_Disappears __instance)
            {
                if(true 
                   && __instance.parent.pawn.IsBeingThrottled()
                   && __instance.parent.pawn.IsValidWildlifeOrWorldPawn())
                __instance.ticksToDisappear -= (__instance.parent.pawn.GetDeltaT() - 1);
            }   
        }
        
        [SoyuzPatch(typeof(HediffComp_Discoverable), nameof(HediffComp_Discoverable.CompPostTick))]
        public static class HediffComp_Discoverable_Patch
        {
            public static bool Prefix(HediffComp_Discoverable __instance)
            {
                if(true 
                   && __instance.parent.pawn.IsBeingThrottled()
                   && __instance.parent.pawn.IsValidWildlifeOrWorldPawn())
                    if ((Find.TickManager.TicksGame + __instance.parent.pawn.thingIDNumber) % 90 == 0)
                        __instance.CheckDiscovered();
                return false;
            }   
        }
        
        [SoyuzPatch(typeof(HediffComp_HealPermanentWounds), nameof(HediffComp_HealPermanentWounds.CompPostTick))]
        public static class HediffComp_HealPermanentWounds_Patch
        {
            public static void Prefix(HediffComp_HealPermanentWounds __instance)
            {
                if(true 
                   && __instance.parent.pawn.IsBeingThrottled()
                   && __instance.parent.pawn.IsValidWildlifeOrWorldPawn())
                __instance.ticksToHeal -= (__instance.parent.pawn.GetDeltaT() - 1);
            }   
        }
        
        [SoyuzPatch(typeof(HediffComp_Infecter), nameof(HediffComp_Infecter.CompPostTick))]
        public static class HediffComp_Infecter_Patch
        {
            public static void Prefix(HediffComp_Infecter __instance)
            {
                if(true 
                   && __instance.parent.pawn.IsBeingThrottled()
                   && __instance.parent.pawn.IsValidWildlifeOrWorldPawn())
                __instance.ticksUntilInfect -= (__instance.parent.pawn.GetDeltaT() - 1);
            }   
        }
        
        [SoyuzPatch(typeof(HediffComp_SelfHeal), nameof(HediffComp_SelfHeal.CompPostTick))]
        public static class HediffComp_SelfHeal_Patch
        {
            public static void Prefix(HediffComp_SelfHeal __instance)
            { 
                if(true 
                && __instance.parent.pawn.IsBeingThrottled()
                && __instance.parent.pawn.IsValidWildlifeOrWorldPawn())
                __instance.ticksSinceHeal += (__instance.parent.pawn.GetDeltaT() - 1);
            }   
        }
        
        [SoyuzPatch(typeof(HediffComp_TendDuration), nameof(HediffComp_TendDuration.CompPostTick))]
        public static class HediffComp_TendDuration_Patch
        {
            public static void Prefix(HediffComp_TendDuration __instance)
            {
                if(true 
                   && __instance.parent.pawn.IsBeingThrottled()
                   && __instance.parent.pawn.IsValidWildlifeOrWorldPawn()
                   && __instance.TProps.TendIsPermanent == false)
                    __instance.tendTicksLeft -= (__instance.parent.pawn.GetDeltaT() - 1);
            }   
        }
    }
}