using UnityEngine;
using Verse;

namespace RocketMan.Tabs
{
    public class TabContent_Stats : ITabContent
    {
        public override string Label => "Statistics";
        public override bool ShouldShow => RocketDebugPrefs.Debug;

        public override void DoContent(Rect rect)
        {
            // TODO: rework this.
            RocketMod.DoStatSettings(rect);
        }

        public override void OnSelect()
        {
        }

        public override void OnDeselect()
        {
        }
    }
}