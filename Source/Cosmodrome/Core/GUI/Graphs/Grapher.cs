using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public class Grapher
    {
        private struct Point
        {
            public float t;
            public float y;
            public Color color;

            public Point(float t, float y, Color color)
            {
                this.t = t;
                this.y = y;
                this.color = color;
            }
        }

        private Listing_Collapsible collapsible = new Listing_Collapsible(scrollViewOnOverflow: false);

        private List<Point> points = new List<Point>();

        private float MinY
        {
            get => points.Min(p => p.y);
        }

        private float MaxY
        {
            get => points.Max(p => p.y);
        }

        private float MinT
        {
            get => points.First().t;
        }

        private float MaxT
        {
            get => points.Last().t;
        }

        private bool mouseIsOver = false;

        private Point mouseIsOverPoint = new Point(0, 0, Color.white);

        public string description = string.Empty;

        public string title = string.Empty;

        public int maxRecords = 60;

        public Grapher(string title, string description = null)
        {
            this.title = title;
            this.description = description ?? string.Empty;
        }

        public float this[float t]
        {
            set => Add(t, value);
        }

        public void Add(float t, float y)
        {
            this.Add(t, y, Color.cyan);
        }

        public void Add(float t, float y, Color color)
        {
            if (points.Count > 10)
            {
                Point last = points.Last();
                if (Mathf.Abs(last.y - y) < 1e-4)
                {
                    points[points.Count - 1] = new Point(t, y, color);
                    return;
                }
            }
            if (points.Count >= maxRecords)
            {
                points.RemoveAt(0);
            }
            points.Add(new Point(t, y, color));
        }

        public void Plot(ref Rect inRect, int lastT = -1)
        {
            if (points.Count <= 2)
            {
                return;
            }
            collapsible.Begin(inRect, this.title);
            if (mouseIsOver)
            {
            }
            collapsible.Lambda(100, (rect) =>
            {
                DrawGraph(rect);
            });
            if (description != null)
            {
                collapsible.Line(1);
            }
            collapsible.End(ref inRect);
        }

        private void DrawGraph(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, Color.black);
            GUI.color = Color.white;
            GUIFont.Font = GUIFontSize.Tiny;
            GUIFont.Anchor = TextAnchor.MiddleLeft;
            rect = rect.ContractedBy(3);
            float minT = MinT;
            float minY = MinY;
            float rangeT = MaxT - minT;
            float rangeY = MaxY - minY;
            float width = rect.width;
            float height = rect.height;

            Rect textRect = new Rect(Vector2.zero, GUIFont.CalcSize("0.000"));
            float offset = textRect.width + 5;

            float x0 = rect.xMin;
            float x1 = rect.xMax;

            for (int i = 1; i < 4; i++)
            {
                float y = rect.height / 4f * (float)i;
                textRect.x = x0;
                textRect.y = rect.yMax - y - 15;
                Widgets.DrawLine(new Vector2(x0 + 2 + offset, rect.yMax - y), new Vector2(x1 - 2, rect.yMax - y), Color.gray, 1);
                Widgets.Label(textRect, $"{ Math.Round(rangeY / 4f * i, 2) }");
            }
            width -= offset;
            rect.xMin += offset;

            this.mouseIsOver = false;

            Vector2 v0 = new Vector2();

            v0.x = rect.xMin;
            v0.y = rect.yMax - (points.First().y - minY) / rangeY * height;

            Rect hoverRect = new Rect(v0.x, rect.y + 2, 0, rect.height - 2);

            foreach (Point p in points)
            {
                Vector2 v1 = new Vector2();

                v1.x = v0.x + (p.t - minT) / rangeT * width;
                v1.y = rect.yMax - (p.y - minY) / rangeY * height;

                hoverRect.xMin = v0.x;
                hoverRect.xMax = v1.x;

                Widgets.DrawLine(v0, v1, p.color, 1);

                if (Mouse.IsOver(hoverRect))
                {
                    Widgets.DrawBoxSolid(hoverRect.RightPartPixels(1), Color.gray);
                    mouseIsOverPoint = p;
                    mouseIsOver = true;
                }
                v0 = v1;
            }
        }
    }
}
