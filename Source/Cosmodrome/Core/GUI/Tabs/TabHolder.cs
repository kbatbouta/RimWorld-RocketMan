using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RocketMan.Tabs
{
    public class TabHolder
    {
        public ITabContent curTab;
        public List<ITabContent> tabs;

        private int curTabIndex;
        private Vector2 scrollPosition = Vector2.zero;
        private Rect tabBarRect;

        private readonly List<TabRecord> tabsRecord;
        private readonly bool useSidebar = true;

        public TabHolder(List<ITabContent> tabs, bool useSidebar = false)
        {
            this.useSidebar = useSidebar;
            this.tabs = tabs;
            if (tabs.Any(i => i.Selected))
            {
                curTab = tabs.First(i => i.Selected);
                curTab.Selected = true;
            }
            else
            {
                curTab = tabs[0];
                curTab.Selected = true;
            }
            tabsRecord = new List<TabRecord>();
            MakeRecords();
        }

        public void DoContent(Rect inRect)
        {
            var selectedFound = false;
            var counter = 0;

            foreach (var tab in tabs)
            {
                if (tab.Selected && tab.ShouldShow)
                {
                    selectedFound = true;
                    curTabIndex = counter;
                    continue;
                }
                if ((tab.Selected && selectedFound) || !tab.ShouldShow)
                    tab.Selected = false;
                counter++;
            }
            if (selectedFound == false)
            {
                curTabIndex = 0;
                tabs[0].Selected = true;
            }
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                curTab = tabs[curTabIndex];
                if (useSidebar)
                {
                    var tabsRect = inRect.LeftPartPixels(170);
                    var contentRect = new Rect(inRect);
                    contentRect.xMin += 180;
                    GUIUtility.ExecuteSafeGUIAction(() =>
                    {
                        DoSidebar(tabsRect);
                    });
                    GUIUtility.ExecuteSafeGUIAction(() =>
                    {
                        Text.Font = GameFont.Medium;
                        Text.CurFontStyle.fontStyle = FontStyle.Bold;
                        GUI.Label(contentRect.TopPartPixels(25), curTab.Label);
                    });
                    contentRect.yMin += 30;
                    GUIUtility.ExecuteSafeGUIAction(() =>
                    {
                        curTab.DoContent(contentRect);
                    });
                }
                else
                {
                    // TODO fix this API                
                    // inRect.yMin += 40;
                    // var tabRect = new Rect(inRect);
                    // tabRect.height = 0;                
                    // MakeRecords();
                    // TabDrawer.DrawTabs(tabRect, tabsRecord);
                    // curTab.DoContent(inRect);
                    // -----------------------
                    throw new InvalidOperationException("ROCKETMAN: this is an outdated API!");
                }
            }, fallbackAction: null, catchExceptions: false);
        }

        public void AddTab(ITabContent newTab)
        {
            tabs.Add(newTab);
            tabsRecord.Add(new TabRecord(newTab.Label, () => { curTabIndex = tabs.Count; }, false));
        }

        public void RemoveTab(ITabContent tab)
        {
            if (tab.Selected)
            {
                tab.Selected = false;
                tabs.RemoveAll(t => t == tab);
                curTabIndex = 0;
                tabs.First().Selected = true;
            }
            else
            {
                tabs.RemoveAll(t => t == tab);
                for (var i = 0; i < tabs.Count; i++)
                    if (tabs[i].Selected)
                        curTabIndex = i;
            }
        }

        private void MakeRecords()
        {
            tabsRecord.Clear();
            var counter = 0;
            foreach (var tab in tabs)
            {
                var localTab = tab;
                var localCounter = counter;
                tabsRecord.Add(new TabRecord(tab.Label, () =>
                {
                    tab.Selected = true;
                    curTabIndex = localCounter;
                    curTab.Selected = false;
                    curTab = localTab;
                }, tab.Selected));
                counter++;
            }
        }

        private void DoSidebar(Rect rect)
        {
            tabBarRect = rect;
            tabBarRect.width -= 2;
            tabBarRect.height = 30 * tabs.Count(t => t.ShouldShow);
            Widgets.DrawMenuSection(rect);
            Widgets.BeginScrollView(rect, ref scrollPosition, tabBarRect);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;
            var curRect = new Rect(rect.xMin + 5, rect.yMin + 5, 160, 30);
            var counter = 0;
            foreach (var tab in tabs)
            {
                if (!tab.ShouldShow)
                {
                    counter++;
                    continue;
                }
                if (tab.Selected)
                {
                    Widgets.DrawWindowBackgroundTutor(curRect);
                }
                Widgets.DrawHighlightIfMouseover(curRect);
                var textRect = new Rect(curRect);
                textRect.xMin += 10;
                Widgets.Label(textRect, tab.Label);
                var localTab = tab;
                var localCounter = counter;
                if (!tab.Selected && Widgets.ButtonInvisible(curRect))
                {
                    localTab.Selected = true;
                    curTab.Selected = false;
                    curTab = localTab;
                    curTabIndex = localCounter;
                }

                curRect.y += 30;
                counter++;
            }
            Widgets.EndScrollView();
        }
    }
}