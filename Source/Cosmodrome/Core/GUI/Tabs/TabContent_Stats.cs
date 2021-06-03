using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RocketMan.Tabs
{
    public class TabContent_Stats : ITabContent
    {
        private IEnumerable<StatDef> stats;

        private Vector2 scrollPosition = Vector2.zero;

        public override string Label => "Statistics";

        public override bool ShouldShow => RocketPrefs.Enabled;

        public TabContent_Stats()
        {
            stats = DefDatabase<StatDef>.AllDefs;
        }

        public override void DoContent(Rect rect)
        {
            if (Widgets.ButtonText(rect.TopPartPixels(20), "Select test"))
            {
                Find.WindowStack.Add(new Selector_DefSelection(DefDatabase<ThingDef>.AllDefs, (def) =>
                {
                    Log.Message(def.defName);
                }));
            }
            rect.yMin += 20;
            RocketMan.GUIUtility.ScrollView(rect, ref scrollPosition, stats,
                (stat) =>
                {
                    return 40f;
                },
                (rect, stat) =>
                {
                    Widgets.Label(rect.TopPartPixels(20), stat.label.CapitalizeFirst());
                    GUIUtility.StashGUIState();
                    Text.Anchor = TextAnchor.UpperRight;
                    Text.CurFontStyle.fontStyle = FontStyle.Italic;
                    Widgets.Label(rect, $"{RocketStates.StatExpiry[stat.index]} Ticks");
                    GUIUtility.RestoreGUIState();
                    RocketStates.StatExpiry[stat.index] = Widgets.HorizontalSlider(rect.BottomPartPixels(20), RocketStates.StatExpiry[stat.index], 0, 1024, false, null, null);
                });
        }

        public override void OnSelect()
        {
        }

        public override void OnDeselect()
        {
            RocketMod.Settings.Write();
        }
    }
}