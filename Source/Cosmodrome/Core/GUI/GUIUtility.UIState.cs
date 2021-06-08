using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RocketMan
{
    public static partial class GUIUtility
    {
        private struct GUIState
        {
            public GameFont font;
            public FontStyle curStyle;
            public FontStyle curTextAreaReadOnlyStyle;
            public FontStyle curTextAreaStyle;
            public FontStyle curTextFieldStyle;
            public int curFontStyle_fontSize;
            public int curTextAreaReadOnlyStyle_fontSize;
            public int curTextAreaStyle_fontSize;
            public int curTextFieldStyle_fontSize;
            public TextAnchor anchor;
            public Color color;
            public Color contentColor;
            public Color backgroundColor;
            public bool wordWrap;
            public RectOffset margin;
        }

        private readonly static List<GUIState> stack = new List<GUIState>();

        public static void StashGUIState()
        {
            stack.Add(new GUIState()
            {
                font = Text.Font,
                curStyle = Text.CurFontStyle.fontStyle,
                curTextAreaReadOnlyStyle = Text.CurTextAreaReadOnlyStyle.fontStyle,
                curTextAreaStyle = Text.CurTextAreaStyle.fontStyle,
                curTextFieldStyle = Text.CurTextFieldStyle.fontStyle,
                anchor = Text.Anchor,
                color = GUI.color,
                wordWrap = Text.WordWrap,
                margin = Text.CurFontStyle.margin,
                curFontStyle_fontSize = Text.CurFontStyle.fontSize,
                curTextAreaReadOnlyStyle_fontSize = Text.CurTextAreaReadOnlyStyle.fontSize,
                curTextFieldStyle_fontSize = Text.CurTextFieldStyle.fontSize,
                curTextAreaStyle_fontSize = Text.CurTextAreaStyle.fontSize,
                contentColor = GUI.contentColor,
                backgroundColor = GUI.backgroundColor,
            });
        }

        public static void RestoreGUIState()
        {
            GUIState config = stack.Last();
            stack.RemoveLast();
            Restore(config);
            // TODO
            // FIX THIS SHIT!
            Restore(config);
        }

        public static void ClearGUIState()
        {
            if (stack.Count > 0)
            {
                Log.Message("ROCKETMAN: GUI state should be clear at exit");
                Restore(stack[0]);
            }
            stack.Clear();
        }

        private static void Restore(GUIState config)
        {
            Text.CurFontStyle.margin = config.margin;
            Text.CurTextAreaReadOnlyStyle.fontStyle = config.curTextAreaReadOnlyStyle;
            Text.CurTextAreaStyle.fontStyle = config.curTextAreaStyle;
            Text.CurTextFieldStyle.fontStyle = config.curTextFieldStyle;
            Text.CurFontStyle.fontStyle = config.curStyle;
            Text.CurFontStyle.fontSize = config.curFontStyle_fontSize;
            Text.CurTextAreaReadOnlyStyle.fontSize = config.curTextAreaReadOnlyStyle_fontSize;
            Text.CurTextAreaStyle.fontSize = config.curTextAreaStyle_fontSize;
            Text.CurTextFieldStyle.fontSize = config.curTextFieldStyle_fontSize;
            GUI.color = config.color;
            GUI.contentColor = config.contentColor;
            GUI.backgroundColor = config.backgroundColor;
            Text.WordWrap = config.wordWrap;
            Text.Anchor = config.anchor;
            Text.Font = config.font;
        }
    }
}
