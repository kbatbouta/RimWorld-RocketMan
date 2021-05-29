using System;
using HarmonyLib;
using Verse;

namespace RocketMan.Patches
{
    [RocketPatch(typeof(TickManager), nameof(TickManager.TickRateMultiplier), MethodType.Getter)]
    public class TickManager_TickRateMultiplier_Patch
    {
        public static bool Prefix(ref float __result)
        {
            if (RocketDebugPrefs.Debug150MTPS && RocketDebugPrefs.Debug)
            {
                __result = 150f;
                return false;
            }
            return true;
        }
    }

    [RocketPatch(typeof(TickManager), nameof(TickManager.Notify_GeneratedPotentiallyHostileMap))]
    public class TickManager_Notify_GeneratedPotentiallyHostileMap_Patch
    {
        public static bool Prefix()
        {
            if (Prefs.DevMode) return false;
            else return true;
        }
    }

    [RocketPatch(typeof(TickManager), nameof(TickManager.TickManagerUpdate))]
    public static class TickManager_TickManagerUpdate_Patch
    {
        public static bool Prefix(TickManager __instance)
        {
            if (!RocketStates.SingleTickIncrement)
                return true;
            if (RocketStates.SingleTickIncrement && RocketStates.SingleTickLeft > 0)
            {
                if (__instance.Paused)
                {
                    __instance.TogglePaused();
                }
                RocketStates.SingleTickLeft = Math.Max(RocketStates.SingleTickLeft - 1, 0);
                return true;
            }
            if (!__instance.Paused)
            {
                __instance.Pause();
            }
            return false;
        }
    }
}