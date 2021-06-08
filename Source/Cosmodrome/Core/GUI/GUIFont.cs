using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RocketMan
{
    public enum GUIFontSize
    {
        Tiny = 11, Smaller = 13, Small = 17, Medium = 22,
    }

    public enum GUIFontStyle
    {
        Normal = 1, Bold = 2, Italic = 3, BoldAndItalic = 4,
    }

    public static partial class GUIFont
    {
        public static GameFont Font
        {
            get => Text.CurFontStyle.fontSize != ((int)GUIFontSize.Tiny) ? Text.Font : GameFont.Tiny;
            set
            {
                switch (value)
                {
                    case GameFont.Tiny:
                        size = GUIFontSize.Tiny;
                        break;
                    case GameFont.Small:
                        size = GUIFontSize.Small;
                        break;
                    case GameFont.Medium:
                        size = GUIFontSize.Medium;
                        break;
                }
            }
        }

        public static FontStyle FontStyle
        {
            get => Text.CurFontStyle.fontStyle;
            set
            {
                switch (value)
                {
                    case FontStyle.Normal:
                        style = GUIFontStyle.Normal;
                        break;
                    case FontStyle.Bold:
                        style = GUIFontStyle.Bold;
                        break;
                    case FontStyle.Italic:
                        style = GUIFontStyle.Italic;
                        break;
                    case FontStyle.BoldAndItalic:
                        style = GUIFontStyle.BoldAndItalic;
                        break;
                }
            }
        }

        public static GUIFontSize size
        {
            set
            {
                switch (value)
                {
                    case GUIFontSize.Tiny:
                        Text.Font = GameFont.Small;
                        break;
                    case GUIFontSize.Smaller:
                        Text.Font = GameFont.Small;
                        break;
                    case GUIFontSize.Small:
                        Text.Font = GameFont.Small;
                        break;
                    default:
                        Text.Font = GameFont.Medium;
                        break;
                }
                SetSize((int)value);
            }
        }

        public static GUIFontStyle style
        {
            set
            {
                FontStyle style = FontStyle.Normal;
                switch (value)
                {
                    case GUIFontStyle.Normal:
                        break;
                    case GUIFontStyle.Bold:
                        style = FontStyle.Bold;
                        break;
                    case GUIFontStyle.Italic:
                        style = FontStyle.Italic;
                        break;
                    case GUIFontStyle.BoldAndItalic:
                        style = FontStyle.BoldAndItalic;
                        break;
                }
                SetStyle(style);
            }
        }

        private static void SetSize(int size)
        {
            Text.CurFontStyle.fontSize = size;
            Text.CurTextAreaReadOnlyStyle.fontSize = size;
            Text.CurTextAreaStyle.fontSize = size;
            Text.CurTextFieldStyle.fontSize = size;
        }

        private static void SetStyle(FontStyle style)
        {
            Text.CurFontStyle.fontStyle = style;
            Text.CurTextAreaReadOnlyStyle.fontStyle = style;
            Text.CurTextAreaStyle.fontStyle = style;
            Text.CurTextFieldStyle.fontStyle = style;
        }
    }
}
