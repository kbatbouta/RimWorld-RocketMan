using System;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Soyuz.Tabs
{
    public class TabContent_WorldPawns : ITabContent
    {
        private Listing_Collapsible collapsible_statistic = new Listing_Collapsible();

        public override Texture2D Icon => TexTab.World;

        public override bool ShouldShow => RocketPrefs.Enabled;

        public override string Label => "World Pawns";

        public TabContent_WorldPawns()
        {
        }

        public override void DoContent(Rect rect)
        {
            collapsible_statistic.Begin(rect, "World Pawns Statistic");
            collapsible_statistic.Label("General information about world pawns.");
            collapsible_statistic.Gap(5);
            collapsible_statistic.Label($"<color=green>Alive</color> world pawns count: <color=orange>{Find.WorldPawns.pawnsAlive.Count}</color>");
            collapsible_statistic.Label($"<color=red>Dead</color> world pawns count: <color=orange>{Find.WorldPawns.pawnsDead.Count}</color>");
            collapsible_statistic.Label($"<color=red>Suspended</color> world pawns count: <color=orange>{Find.WorldPawns.pawnsMothballed.Count}</color>");
            collapsible_statistic.End(ref rect);
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
        }

        public override void OnSelect()
        {
            base.OnSelect();
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_WorldPawns();

    }
}
