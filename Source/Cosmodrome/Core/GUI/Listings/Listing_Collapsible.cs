using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO.Ports;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using GUILambda = System.Action<UnityEngine.Rect>;

namespace RocketMan
{
    public class Listing_Collapsible
    {
        private bool expanded = false;

        private Vector2 margins = new Vector2(8, 4);

        private float inXMin = 0f;

        private float inXMax = 0f;

        private float curYMin = 0f;

        private float inYMin = 0f;

        private float inYMax = 0f;

        public Color CollapsibleBGColor = Widgets.MenuSectionBGFillColor;

        public Color CollapsibleBGBorderColor = Widgets.MenuSectionBGBorderColor;

        private struct RectSlice
        {
            public Rect inside;
            public Rect outside;

            public RectSlice(Rect inside, Rect outside)
            {
                this.outside = outside;
                this.inside = inside;
            }
        }

        private float insideWidth
        {
            get => (inXMax - inXMin) - margins.x * 2f;
        }

        public Vector4 Margins
        {
            get => this.margins;
        }

        public bool Expanded
        {
            get => this.expanded;
            set => this.expanded = value;
        }

        public Rect Rect
        {
            get => new Rect(inXMin, curYMin, inXMax - inXMin, inYMax - curYMin);
            set
            {
                this.inXMin = value.xMin;
                this.inXMax = value.xMax;
                this.curYMin = value.yMin;
                this.inYMin = value.yMin;
                this.inYMax = value.yMax;
            }
        }

        public Listing_Collapsible(bool expanded = false)
        {
            this.expanded = expanded;
        }

        public void Begin(Rect inRect, TaggedString title, bool drawInfo = true, bool drawIcon = true, bool hightlightIfMouseOver = true)
        {
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                this.inYMin = inRect.yMin;
                this.Rect = inRect;
                GUIFont.Font = GUIFontSize.Tiny;
                GUIFont.Anchor = TextAnchor.MiddleLeft;
                RectSlice slice = Slice(title.GetTextHeight(this.insideWidth - 30f));
                if (hightlightIfMouseOver)
                {
                    Widgets.DrawHighlightIfMouseover(slice.outside);
                }
                GUI.color = this.CollapsibleBGBorderColor;
                GUI.color = Color.gray;
                Rect titleRect = slice.inside;
                if (drawInfo)
                {
                    GUIFont.Font = GUIFontSize.Tiny;
                    GUIFont.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(titleRect, expanded ? KeyedResources.RocketMan_Collapsible_Hide : KeyedResources.RocketMan_Collapsible_Expand);
                }
                GUIFont.Font = GUIFontSize.Smaller;
                GUIFont.CurFontStyle.fontStyle = FontStyle.Normal;
                GUIFont.Anchor = TextAnchor.MiddleLeft;
                if (drawIcon)
                {
                    Widgets.DrawTextureFitted(titleRect.LeftPartPixels(25), expanded ? TexButton.Collapse : TexButton.Reveal, 0.65f);
                    titleRect.xMin += 35;
                }
                GUI.color = Color.white;
                Widgets.Label(titleRect, title);
                if (Widgets.ButtonInvisible(slice.outside))
                {
                    expanded = !expanded;
                }
                GUI.color = this.CollapsibleBGBorderColor;
                Widgets.DrawBox(slice.outside, 1);
            });
            this.Gap(2);
            GUIUtility.StashGUIState();
            GUIFont.Font = GUIFontSize.Tiny;
            GUIFont.CurFontStyle.fontStyle = FontStyle.Normal;
        }

        public void Label(TaggedString text, string tooltip = null, bool invert = false, bool hightlightIfMouseOver = true, GUIFontSize fontSize = GUIFontSize.Tiny, FontStyle fontStyle = FontStyle.Normal)
        {
            if (invert == this.expanded)
            {
                return;
            }
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                RectSlice slice = Slice(text.GetTextHeight(this.insideWidth));
                if (hightlightIfMouseOver)
                {
                    Widgets.DrawHighlightIfMouseover(slice.outside);
                }
                GUIFont.Font = fontSize;
                GUIFont.CurFontStyle.fontStyle = fontStyle;
                Widgets.Label(slice.inside, text);
                if (tooltip != null)
                {
                    TooltipHandler.TipRegion(slice.outside, tooltip);
                }
            });
        }

        public bool CheckboxLabeled(TaggedString text, ref bool checkOn, string tooltip = null, bool invert = false, bool disabled = false, bool hightlightIfMouseOver = true, GUIFontSize fontSize = GUIFontSize.Tiny, FontStyle fontStyle = FontStyle.Normal)
        {
            if (invert == this.expanded)
            {
                return false;
            }
            bool changed = false;
            bool checkOnInt = checkOn;
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                GUIFont.Font = fontSize;
                GUIFont.CurFontStyle.fontStyle = fontStyle;
                RectSlice slice = Slice(text.GetTextHeight(insideWidth - 23f));
                if (hightlightIfMouseOver)
                {
                    Widgets.DrawHighlightIfMouseover(slice.outside);
                }
                GUIUtility.CheckBoxLabeled(slice.inside, text, ref checkOnInt, disabled: disabled, iconWidth: 23f, drawHighlightIfMouseover: false);
                if (tooltip != null)
                {
                    TooltipHandler.TipRegion(slice.outside, tooltip);
                }
            });
            if (checkOnInt != checkOn)
            {
                checkOn = checkOnInt;
                changed = true;
            }
            return changed;
        }

        public void Columns(float height, IEnumerable<GUILambda> lambdas, float gap = 5, bool invert = false, bool useMargins = false, Action fallback = null)
        {
            if (invert == expanded)
            {
                return;
            }
            if (lambdas.Count() == 1)
            {
                Lambda(height, lambdas.First(), invert, useMargins, fallback);
                return;
            }
            Rect rect = useMargins ? Slice(height).inside : Slice(height).outside;
            Rect[] columns = rect.Columns(lambdas.Count(), gap);
            int i = 0;
            foreach (GUILambda lambda in lambdas)
            {
                GUIUtility.ExecuteSafeGUIAction(() =>
                {
                    lambda(columns[i++]);
                }, fallback);
            }
        }

        public void Lambda(float height, GUILambda contentLambda, bool invert = false, bool useMargins = false, Action fallback = null)
        {
            if (invert == expanded)
            {
                return;
            }
            RectSlice slice = Slice(height);
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                contentLambda(useMargins ? slice.inside : slice.outside);
            }, fallback);
        }

        public void Gap(float height = 9f, bool invert = false)
        {
            if (expanded != invert)
            {
                Slice(height, includeMargins: false);
            }
        }

        public void Line(float thickness, bool invert = false)
        {
            if (expanded != invert)
            {
                Gap(height: 3.5f);
                Widgets.DrawBoxSolid(this.Slice(thickness, includeMargins: false).outside, this.CollapsibleBGBorderColor);
                Gap(height: 3.5f);
            }
        }

        public void End(ref Rect inRect)
        {
            Gap(height: 5);
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                GUI.color = this.CollapsibleBGBorderColor;
                Widgets.DrawBox(new Rect(inXMin, inYMin, inXMax - inXMin, curYMin - inYMin));
            });
            GUIUtility.RestoreGUIState();
            inRect.yMin = curYMin;
        }

        private RectSlice Slice(float height, bool includeMargins = true)
        {
            Rect outside = new Rect(inXMin, curYMin, inXMax - inXMin, includeMargins ? height + margins.y : height);
            Rect inside = new Rect(outside);
            if (includeMargins)
            {
                inside.xMin += margins.x * 2;
                inside.xMax -= margins.x;
                inside.yMin += margins.y / 2f;
                inside.yMax -= margins.y / 2f;
            }
            this.curYMin += includeMargins ? height + margins.y : height;
            Widgets.DrawBoxSolid(outside, CollapsibleBGColor);
            return new RectSlice(inside, outside);
        }
    }
}
