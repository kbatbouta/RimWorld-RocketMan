using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RocketMan.Optimizations
{
    public static class Text_Patch
    {
        private const int MAX_CACHE_SIZE = 10000;

        private static readonly IEqualityComparer<GUITextState> textStateEqualityComparer = new GUITextStateEqualityComparer();

        private static Dictionary<GUITextState, Vector2> cacheSize = new Dictionary<GUITextState, Vector2>(textStateEqualityComparer);

        private static Dictionary<GUITextState, float> cacheHeight = new Dictionary<GUITextState, float>(textStateEqualityComparer);

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
                CalcSize_Patch.key = new GUITextState(text);
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
                CalcHeight_Patch.key = new GUITextState(text, width: width);
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

        private readonly struct GUITextState
        {
            internal readonly string text;
            internal readonly GameFont font;
            internal readonly float width;
            internal readonly float uiScale;
            internal readonly bool wordWrap;

            internal GUITextState(string text, float width = float.MinValue)
            {
                this.text = text;
                this.width = width;
                this.font = Text.fontInt;
                this.uiScale = Prefs.UIScale;
                this.wordWrap = Text.wordWrapInt;
            }
        }

        private class GUITextStateEqualityComparer : IEqualityComparer<GUITextState>
        {
            public bool Equals(GUITextState x, GUITextState y)
            {
                if (x.text != y.text || x.width != y.width || x.font != y.font)
                {
                    return false;
                }

                return x.uiScale == y.uiScale && x.wordWrap == y.wordWrap;
            }

            public int GetHashCode(GUITextState obj)
            {
                return obj.text.GetHashCode() ^ obj.width.GetHashCode() ^ obj.font.GetHashCode() ^ obj.uiScale.GetHashCode() ^ obj.wordWrap.GetHashCode();
            }
        }
    }
}
