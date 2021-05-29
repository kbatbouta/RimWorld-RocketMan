using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RocketMan.Tabs;
using UnityEngine;
using UnityEngine.Playables;
using Verse;
using static RocketMan.Main;

namespace RocketMan
{
    public class Window_MainControls : Window
    {
        private TabHolder tabs;

        private int _errors = 0;

        public override Vector2 InitialSize => new Vector2(685, 650);

        public Window_MainControls()
        {
            draggable = true;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
            resizeable = true;
            drawShadow = true;
            doCloseButton = false;
            doCloseX = true;
            resizeLater = false;
            layer = WindowLayer.SubSuper;
            resizer = new WindowResizer();
            resizer.minWindowSize = new Vector2(InitialSize.x, 450);
            tabs = new TabHolder(new List<ITabContent>()
            {
                new TabContent_Settings(){ Selected = true },
                new TabContent_Stats(){ Selected = false },
            }, useSidebar: true);
            Main.yieldTabContent = FunctionsUtility.GetFunctions<YieldTabContent, ITabContent>().ToList();
            for (var i = 0; i < Main.yieldTabContent.Count; i++)
            {
                ITabContent tab = Main.yieldTabContent[i].Invoke();
                Log.Message($"ROCKETMAN: Found a new tab {tab.Label}");
                tabs.AddTab(tab);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            GUIUtility.StashGUIState();
            Rect rect = inRect.TopPartPixels(25);
            try
            {
                // TODO fix this mess
                // For profiling reasons...
                RocketStates.LastFrame = Time.frameCount;
                // For Stat settings reason...
                RocketMod.ReadStats();
                // Actual work                                
                GUI.color = Color.white;
                // Create the RocketMan stamp
                Text.Font = GameFont.Small;
                Text.CurFontStyle.fontStyle = FontStyle.Bold;
                Widgets.Label(rect, "RocketMan");
                // Create the version string
                rect.xMin += 90;
                rect.xMax -= 45;
                rect.y += 2;
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect.TopPartPixels(25), $"Version <color=grey>{RocketAssembliesInfo.Version}</color>");
                // Do the window content
                inRect.yMin += 25;
                tabs.DoContent(inRect);
                // Reduce the error counter
                _errors = Math.Max(_errors - 1, 0);
            }
            catch (Exception er)
            {
                if (_errors <= 60 && _errors % 2 == 0) Log.Warning($"ROCKETMAN: UI Minor error:{er}\n{er.StackTrace}\nError count:{_errors}");
                else if (_errors <= 60) Log.Warning($"ROCKETMAN: UI error:{er}\n{er.StackTrace}\nError count:{_errors}");
                else Log.Error($"ROCKETMAN: UI Major error:{er}\n{er.StackTrace}\nError count:{_errors}");
                _errors += 3;
            }
            finally
            {
                GUIUtility.RestoreGUIState();
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);
            RocketDebugPrefs.DogData = false;
        }
    }
}