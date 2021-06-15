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
        public override Texture2D Icon => TexTab.Graphing;

        private static Vector2 scrollPosition = Vector2.zero;

        private Listing_Collapsible standard_Content = new Listing_Collapsible();

        public override string Label => "Grapher";

        public override bool ShouldShow => RocketPrefs.Enabled && RocketDebugPrefs.Debug && RocketPrefs.TimeDilation;

        public override void DoContent(Rect rect)
        {
            GUI.color = Color.red;
            standard_Content.Begin(rect.TopPartPixels(350), "Information and controls");
            GUI.color = Color.white;
            standard_Content.CheckboxLabeled("Enable time dilation", ref RocketPrefs.TimeDilation, "Experimental.");
            standard_Content.CheckboxLabeled("Flash dilated pawns", ref RocketDebugPrefs.FlashDilatedPawns, "Experimental.");
            if (Find.Selector.selected.Count == 1 && Find.Selector.selected.First() is Pawn pawn)
            {
                standard_Content.Line(1);
                standard_Content.Label("InspectorTabsResolved are");
                foreach (var tab in pawn.def.inspectorTabsResolved)
                    standard_Content.Label($"{tab.GetType().FullName}");
                standard_Content.Line(1);
                standard_Content.Label("InspectorTabs are");
                foreach (var tab in pawn.def.inspectorTabs)
                    standard_Content.Label($"{tab.FullName}");
                standard_Content.Line(1);
                standard_Content.Label("Comps are");
                foreach (var comp in pawn.def.comps)
                    standard_Content.Label($"{comp.GetType().FullName}");
            }
            standard_Content.End(ref rect);
            DoExtras(rect.ExpandedBy(1));
        }

        private void DoExtras(Rect rect)
        {
            var anchor = GUIFont.Anchor;
            var font = GUIFont.Font;
            var style = GUIFont.CurFontStyle.fontStyle;
            Widgets.DrawMenuSection(rect.ContractedBy(1));
            if (Find.Selector.selected.Count == 0 || !(Find.Selector.selected.First() is Pawn pawn))
            {
                GUIFont.Font = GUIFontSize.Medium;
                GUIFont.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "Please select a pawn");
            }
            else DoExtras_Internal(rect.ContractedBy(3));
            GUIFont.CurFontStyle.fontStyle = style;
            GUIFont.Font = font;
            GUIFont.Anchor = anchor;
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
                    GUIFont.Font = GUIFontSize.Tiny;
                    GUIFont.CurFontStyle.fontStyle = FontStyle.Bold;
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
                    GUIFont.Font = GUIFontSize.Tiny;
                    GUIFont.CurFontStyle.fontStyle = FontStyle.Bold;
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
            RocketDebugPrefs.LogData = true;
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_Grapher();
    }
}
