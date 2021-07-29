using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using HarmonyLib;
using UnityEngine;
using Verse;
using GUITextState = System.Tuple<string, Verse.GameFont, float, float, bool>;

namespace RocketMan.Optimizations
{
    public static class Text_Patch
    {
        private const int MAX_CACHE_SIZE = 10000;

        private static Dictionary<GUITextState, Vector2> cacheSize = new Dictionary<GUITextState, Vector2>();

        private static Dictionary<GUITextState, float> cacheHeight = new Dictionary<GUITextState, float>();

        [Main.OnTickLonger]
        private static void Cleanup()
        {
            if (cacheSize.Count > MAX_CACHE_SIZE) cacheHeight.Clear();
            if (cacheHeight.Count > MAX_CACHE_SIZE) cacheHeight.Clear();
        }

        [RocketPatch(typeof(Text), nameof(Text.CalcSize))]
        public static class CalcSize_Patch
        {
            private static bool shouldCache = false;

            private static GUITextState key;

            [HarmonyPriority(int.MaxValue)]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool Prefix(string text, ref Vector2 __result)
            {
                if (!RocketPrefs.TranslationCaching || !RocketPrefs.Enabled)
                {
                    return !(shouldCache = false);
                }
                CalcSize_Patch.key = GetGUIState(text);
                if (cacheSize.TryGetValue(key, out __result))
                {
                    return shouldCache = false;
                }
                return shouldCache = true;
            }

            [HarmonyPriority(int.MinValue)]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Postfix(Vector2 __result)
            {
                if (shouldCache)
                {
                    cacheSize[key] = __result;
                }
                shouldCache = false;
            }
        }

        [RocketPatch(typeof(Text), nameof(Text.CalcHeight))]
        public static class CalcHeight_Patch
        {
            private static bool shouldCache = false;

            private static GUITextState key;

            [HarmonyPriority(int.MaxValue)]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool Prefix(string text, float width, ref float __result)
            {
                if (!RocketPrefs.TranslationCaching || !RocketPrefs.Enabled)
                {
                    return !(shouldCache = false);
                }
                CalcHeight_Patch.key = GetGUIState(text, width: width);
                if (cacheHeight.TryGetValue(key, out __result))
                {
                    return shouldCache = false;
                }
                return shouldCache = true;
            }

            [HarmonyPriority(int.MinValue)]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Postfix(float __result)
            {
                if (shouldCache)
                {
                    cacheHeight[key] = __result;
                }
                shouldCache = false;
            }
        }

        [Main.OnTickLonger]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
        private static void ClearCache()
        {
            // TODO redo this
            // cacheSize.Clear();
            cacheHeight.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static GUITextState GetGUIState(string text, float width = float.MinValue)
        {
            return new GUITextState(text, Text.fontInt, width, Prefs.UIScale, Text.wordWrapInt);
        }
    }
}
