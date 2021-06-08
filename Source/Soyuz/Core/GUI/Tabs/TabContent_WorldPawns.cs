using System;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Soyuz.Tabs
{
    public class TabContent_WorldPawns : ITabContent
    {
        public override Texture2D Icon => TexTab.World;

        public override bool ShouldShow => false;

        public override string Label => "World Pawns";

        public TabContent_WorldPawns()
        {
        }

        public override void DoContent(Rect rect)
        {
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
