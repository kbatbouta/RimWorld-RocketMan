using System;
using HarmonyLib;
using Verse;

namespace RocketMan.Patches
{
    [RocketStartupPatch(typeof(Game), nameof(Game.FinalizeInit))]
    public static class Game_FinalizeInit_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static void Postfix()
        {
            Main.WorldLoaded();
        }
    }

    [RocketStartupPatch(typeof(Game), nameof(Game.DeinitAndRemoveMap))]
    public static class Game_DeinitAndRemoveMap_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static void Postfix(Map map)
        {
            Main.MapDiscarded(map);
        }
    }
}
