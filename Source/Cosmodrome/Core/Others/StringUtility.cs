using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Verse;

namespace RocketMan
{
    public static class StringUtility
    {
        private const char _A = 'A';
        private const char _Z = 'Z';

        private static readonly Dictionary<string, string> splitingCache = new Dictionary<string, string>();

        public static string SplitStringByCapitalLetters(this string inputString)
        {
            if (splitingCache.TryGetValue(inputString, out string outputString))
            {
                return outputString;
            }
            outputString = string.Empty;
            for (int i = 0; i < inputString.Length; i++)
            {
                if (inputString[i] >= _A && inputString[i] <= _Z)
                {
                    outputString += " ";
                    outputString += inputString[i];
                    i++;
                    while (i < inputString.Length && inputString[i] >= _A && inputString[i] <= _Z)
                    {
                        outputString += inputString[i];
                        i++;
                    }
                }
                outputString += inputString[i];
            }
            return splitingCache[inputString] = outputString;
        }

        private static Dictionary<Pair<string, GameFont>, string> fitCache = new Dictionary<Pair<string, GameFont>, string>();


        public static string Fit(this string text, Rect rect)
        {
            Pair<string, GameFont> key = new Pair<string, GameFont>(text, Text.Font);
            if (fitCache.TryGetValue(key, out string result))
                return result;
            float width = text.GetWidthCached();
            if (width < rect.width)
                return fitCache[key] = text;
            result = text.Substring(0, (int)Math.Max(rect.width / (width * 1.1f) * (float)text.Length - 3, Math.Min(3, text.Length)));
            return fitCache[key] = result + "...";
        }

        private static Dictionary<Pair<string, GameFont>, bool> canFitCache = new Dictionary<Pair<string, GameFont>, bool>();

        public static bool CanFit(this string text, Rect rect)
        {
            Pair<string, GameFont> key = new Pair<string, GameFont>(text, Text.Font);
            if (canFitCache.TryGetValue(key, out bool canFit))
                return canFit;
            return canFitCache[key] = CanFitInternal(text, rect);
        }

        private static bool CanFitInternal(string text, Rect rect)
        {
            return text.GetWidthCached() < rect.width;
        }

        public static string Base64Encode(this string str)
        {
            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
