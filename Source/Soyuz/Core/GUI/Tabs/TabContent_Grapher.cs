using System;
using System.Linq;
using System.Runtime.InteropServices;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Soyuz.Tabs
{
    public class TabContent_Grapher : ITabContent
    {
        private static Vector2 scrollPosition = Vector2.zero;
        private Listing_Standard standard_Content = new Listing_Standard();

        public override string Label => "Grapher";
        public override bool ShouldShow => RocketDebugPrefs.Debug;

        public override void DoContent(Rect rect)
        {
            standard_Content.Begin(rect);
            GUI.color = Color.red;
            Text.CurFontStyle.fontStyle = FontStyle.Bold;

            standard_Content.Label("Soyuz grapher");
            Text.CurFontStyle.fontStyle = FontStyle.Normal;
            GUI.color = Color.white;
            var font = Text.Font;
            Text.Font = GameFont.Tiny;
            standard_Content.CheckboxLabeled("Enable time dilation", ref RocketPrefs.TimeDilation, "Experimental.");
            standard_Content.CheckboxLabeled("Flash dilated pawns", ref RocketDebugPrefs.FlashDilatedPawns, "Experimental.");
            Text.Font = font;
            standard_Content.End();
            rect.yMin += 75;
            DoExtras(rect);
        }

        private void DoExtras(Rect rect)
        {
            var anchor = Text.Anchor;
            var font = Text.Font;
            var style = Text.CurFontStyle.fontStyle;
            Widgets.DrawMenuSection(rect.ContractedBy(1));
            if (Find.Selector.selected.Count == 0 || !(Find.Selector.selected.First() is Pawn pawn))
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "Please select a pawn");
            }
            else DoExtras_Internal(rect.ContractedBy(3));
            Text.CurFontStyle.fontStyle = style;
            Text.Font = font;
            Text.Anchor = anchor;
        }

        private void DoExtras_Internal(Rect rect)
        {
            var pawn = Find.Selector.selected.First() as Pawn;
            var needs = pawn.needs.needs;
            var hediffs = pawn.health.hediffSet.hediffs;
            var curRect = rect;
            curRect.ContractedBy(5);
            curRect.yMin += 5;
            curRect.width -= 5;
            Widgets.BeginScrollView(curRect, ref scrollPosition, new Rect(Vector2.zero, new Vector2(rect.width - 15, (120 + 20) * (hediffs.Count + needs.Count()))));
            var elementRect = new Rect(5, 5, rect.width - 25, 87);
            var needsModel = pawn.GetNeedModels();
            foreach (var need in needs)
            {
                if (needsModel.TryGetValue(need.GetType(), out var model))
                {
                    model.DrawGraph(elementRect.BottomPartPixels(70));
                    Text.Font = GameFont.Tiny;
                    Text.CurFontStyle.fontStyle = FontStyle.Bold;
                    Widgets.Label(elementRect.TopPartPixels(14), GenText.CapitalizeFirst(need.def.label));
                    elementRect.y += elementRect.height + 20;
                }
            }
            var hediffsModel = pawn.GetHediffModels();
            foreach (var hediff in hediffs)
            {
                if (hediffsModel.TryGetValue(hediff, out var model))
                {
                    model.DrawGraph(elementRect.BottomPartPixels(70));
                    Text.Font = GameFont.Tiny;
                    Text.CurFontStyle.fontStyle = FontStyle.Bold;
                    Widgets.Label(elementRect.TopPartPixels(14), GenText.CapitalizeFirst(hediff.def.label));
                    elementRect.y += elementRect.height;
                }
            }
            Widgets.EndScrollView();
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
        }

        public override void OnSelect()
        {
            base.OnSelect();
            RocketDebugPrefs.DogData = true;
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_Grapher();
    }
}
