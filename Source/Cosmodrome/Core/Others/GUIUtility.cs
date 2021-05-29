using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public static class GUIUtility
    {
        private struct GUIState
        {
            public GameFont font;
            public FontStyle curStyle;
            public FontStyle curTextAreaReadOnlyStyle;
            public FontStyle curTextAreaStyle;
            public FontStyle curTextFieldStyle;
            public TextAnchor anchor;
            public Color color;
            public Color contentColor;
            public Color backgroundColor;
            public bool wordWrap;
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
                contentColor = GUI.contentColor,
                backgroundColor = GUI.backgroundColor
            });
        }

        public static void RestoreGUIState()
        {
            GUIState config = stack.Last();
            stack.RemoveLast();
            Text.Font = config.font;
            Text.CurTextAreaReadOnlyStyle.fontStyle = config.curTextAreaReadOnlyStyle;
            Text.CurTextAreaStyle.fontStyle = config.curTextAreaStyle;
            Text.CurTextFieldStyle.fontStyle = config.curTextFieldStyle;
            Text.CurFontStyle.fontStyle = config.curStyle;
            GUI.color = config.color;
            GUI.contentColor = config.contentColor;
            GUI.backgroundColor = config.backgroundColor;
            Text.WordWrap = config.wordWrap;
            Text.Anchor = config.anchor;
        }

        public static Exception ExecuteSafeGUIAction(Action function, Action fallbackAction = null, bool catchExceptions = false)
        {
            StashGUIState();
            Exception exception = null;
            try
            {
                function.Invoke();
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETMAN:UI error in ExecuteSafeGUIAction {er}");
                exception = er;
            }
            finally
            {
                RestoreGUIState();
            }
            if (exception != null && !catchExceptions)
            {
                if (fallbackAction != null)
                    exception = ExecuteSafeGUIAction(
                        fallbackAction,
                        catchExceptions: false);
                if (exception != null)
                    throw exception;
            }
            return exception;
        }

        private static readonly Color _altGray = new Color(0.2f, 0.2f, 0.2f);

        public static void ScrollView<T>(Rect rect, ref Vector2 scrollPosition, IEnumerable<T> elements, Func<T, float> heightLambda, Action<Rect, T> elementLambda, Func<T, IComparable> orderByLambda = null, bool drawBackground = true, bool showScrollbars = true, bool catchExceptions = false, bool drawMouseOverHighlights = true)
        {
            Exception exception = null;
            if (drawBackground)
            {
                Widgets.DrawMenuSection(rect);
                rect = rect.ContractedBy(2);
            }
            Rect contentRect = new Rect(0, 0, showScrollbars ? rect.width - 23 : rect.width, 0);
            IEnumerable<T> elementsInt = orderByLambda == null ? elements : elements.OrderBy(orderByLambda);
            float[] heights = new float[elementsInt.Count()];
            float h = 0f;
            int j = 0;
            int k = 0;
            foreach (T element in elementsInt)
            {
                h = heightLambda.Invoke(element);
                heights[j++] = h;
                contentRect.height += Math.Max(h, 0f);
            }
            j = 0;
            Widgets.BeginScrollView(rect, ref scrollPosition, contentRect, showScrollbars: showScrollbars);
            try
            {
                Rect currentRect = new Rect(1, 0, showScrollbars ? rect.width - 16 : rect.width, 0);
                foreach (T element in elementsInt)
                {
                    if (heights[j] <= 0.05f)
                    {
                        j++;
                        continue;
                    }
                    if (drawBackground && k % 2 == 0)
                        Widgets.DrawBoxSolid(currentRect, _altGray);
                    if (drawMouseOverHighlights)
                        Widgets.DrawHighlightIfMouseover(currentRect);
                    currentRect.height = heights[j];
                    ExecuteSafeGUIAction(() =>
                    {
                        elementLambda.Invoke(currentRect, element);
                    });
                    currentRect.y += heights[j];
                    k++;
                    j++;
                }
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETMAN:UI error in ScrollView {er}");
                exception = er;
            }
            finally
            {
                Widgets.EndScrollView();
            }
            if (exception != null && !catchExceptions)
                throw exception;
        }

        public static void GridView<T>(Rect rect, int columns, List<T> elements, Action<Rect, T> cellLambda, bool drawBackground = true, bool drawVerticalDivider = false)
        {
            if (drawBackground)
            {
                Widgets.DrawMenuSection(rect);
            }
            rect = rect.ContractedBy(1);
            int rows = (int)Math.Ceiling((decimal)elements.Count / columns);
            float columnStep = rect.width / columns;
            float rowStep = rect.height / rows;
            Rect curRect = new Rect(0, 0, columnStep, rowStep);
            int k = 0;
            for (int i = 0; i < columns && k < elements.Count; i++)
            {
                curRect.x = i * columnStep + rect.x;
                for (int j = 0; j < rows && k < elements.Count; j++)
                {
                    curRect.y = j * rowStep + rect.y;
                    ExecuteSafeGUIAction(() =>
                    {
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Text.Font = GameFont.Tiny;
                        cellLambda(curRect, elements[k++]);
                    });
                }
            }
        }

        public static void Row(Rect rect, List<Action<Rect>> contentLambdas, bool drawDivider = true, bool drawBackground = false)
        {
            if (drawBackground)
            {
                Widgets.DrawMenuSection(rect);
            }
            float step = rect.width / contentLambdas.Count;
            Rect curRect = new Rect(rect.x - 5, rect.y, step - 10, rect.height);
            for (int i = 0; i < contentLambdas.Count; i++)
            {
                Action<Rect> lambda = contentLambdas[i];
                if (drawDivider && i + 1 < contentLambdas.Count)
                {
                    Vector2 start = new Vector2(curRect.xMax + 5, curRect.yMin + 1);
                    Vector2 end = new Vector2(curRect.xMax + 5, curRect.yMax - 1);
                    Widgets.DrawLine(start, end, Color.white, 1);
                }
                ExecuteSafeGUIAction(() =>
                {
                    lambda.Invoke(curRect);
                    curRect.x += step;
                });
            }
        }

        public static void ColorBoxDescription(Rect rect, Color color, string description)
        {
            Rect textRect = new Rect(rect.x + 30, rect.y, rect.width - 30, rect.height);
            Rect boxRect = new Rect(0, 0, 10, 10);
            boxRect.center = new Vector2(rect.xMin + 15, rect.yMin + rect.height / 2);
            ExecuteSafeGUIAction(() =>
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Tiny;
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                Widgets.DrawBoxSolid(boxRect, color);
                Widgets.Label(textRect, description.Fit(textRect));
            });
        }
    }
}
