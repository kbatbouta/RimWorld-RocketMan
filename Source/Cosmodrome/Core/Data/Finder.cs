using System;
using HarmonyLib;
using Verse;

namespace RocketMan
{
    public static class Finder
    {
        public static readonly string HarmonyID = "Krkr.RocketMan";

        public static RocketMod Mod;

        public static ModContentPack ModContentPack;

        public static Harmony Harmony = new Harmony(HarmonyID);

        public static RocketShip.SkipperPatcher Rocket = new RocketShip.SkipperPatcher(HarmonyID);

        public static Window_Main RocketManWindow;

        public static StatSettingsGroup StatSettingsGroup;

        public static RocketPluginsLoader PluginsLoader;
    }
}
