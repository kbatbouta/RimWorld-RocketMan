using System;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;

namespace Soyuz.Tabs
{
    public class TabContent_WorldPawnsUtility : ITabContent
    {
        public TabContent_WorldPawnsUtility()
        {
        }

        public override bool ShouldShow => RocketPrefs.Enabled && RocketDebugPrefs.Debug;

        public override string Label => "World Pawns";

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
        public static ITabContent YieldTab() => new TabContent_WorldPawnsUtility();

    }
}
