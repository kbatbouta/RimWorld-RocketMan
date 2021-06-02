using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Soyuz.Tabs
{
    public class TabContent_DilationSettings : ITabContent
    {
        public override bool ShouldShow => RocketPrefs.Enabled;

        public override string Label => "Time Dilation Controls";

        public static readonly Color ignoredColor = new Color(1f, 0.913f, 0.541f, 0.2f);

        public static readonly Color disabledColor = new Color(0.972f, 0.070f, 0.137f, 0.2f);

        private Vector2 scrollPosition = Vector2.zero;

        private RaceSettings currentSettings;

        private string searchString = string.Empty;

        public static List<Pair<Color, string>> descriptionBoxes;

        public TabContent_DilationSettings()
        {
            if (descriptionBoxes == null)
            {
                descriptionBoxes = new List<Pair<Color, string>>();
                descriptionBoxes.Add(new Pair<Color, string>(Color.red,
                    "Soyuz.TabContent_DilationSettings.Colored.Disabled".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.yellow,
                    "Soyuz.TabContent_DilationSettings.Colored.IgnoredAndDisabled".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.blue,
                    "Soyuz.TabContent_DilationSettings.Colored.IsFastMoving".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.cyan,
                    "Soyuz.TabContent_DilationSettings.Colored.IgnorePlayerFaction".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.magenta,
                    "Soyuz.TabContent_DilationSettings.Colored.IgnoreAllFaction".Translate()));
            }
        }

        public override void OnDeselect()
        {
        }

        public override void OnSelect()
        {
        }

        public override void DoContent(Rect rect)
        {
            Listing_Standard standard = new Listing_Standard(GameFont.Tiny);
            standard.Begin(rect.TopPartPixels(95 + (RocketDebugPrefs.Debug ? 100 : 0)));
            standard.CheckboxLabeled("Enable time dilation", ref RocketPrefs.TimeDilation, "Experimental.");
            standard.GapLine();
            standard.CheckboxLabeled("Enable time dilation for visitor pawns.", ref RocketPrefs.TimeDilationVisitors, "Experimental: Can cause a lot of bugs.");
            standard.CheckboxLabeled("Enable time dilation for world pawns", ref RocketPrefs.TimeDilationWorldPawns, "Throttle ticking for world pawns.");
            if (RocketDebugPrefs.Debug)
            {
                standard.GapLine();
                standard.CheckboxLabeled("Enable data logging", ref RocketDebugPrefs.DogData, "For debugging only.");
                standard.CheckboxLabeled("Set tick multiplier to 150", ref RocketDebugPrefs.Debug150MTPS, "Dangerous!");
                standard.CheckboxLabeled("Enable flashing dilated pawns",
                    ref RocketDebugPrefs.FlashDilatedPawns);
                standard.CheckboxLabeled("Simulate offscreen behavior", ref RocketDebugPrefs.AlwaysDilating);
            }
            standard.End();
            rect.yMin += 85 + (RocketDebugPrefs.Debug ? 100 : 0);

            if (RocketPrefs.TimeDilation)
            {
                DoExtras(rect);
            }
            else RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Medium;
                Widgets.DrawMenuSection(rect);
                Widgets.Label(rect, "Time dilation is disabled!");
            });
        }

        private void DoExtras(Rect inRect)
        {
            string oldString = searchString;
            searchString = Widgets.TextField(inRect.TopPartPixels(25), searchString).ToLower();
            if (oldString != searchString)
                scrollPosition = Vector2.zero;
            inRect.yMin += 30;
            if (currentSettings != null)
            {
                DoRaceSettings(inRect, currentSettings);
                inRect.yMin += 115;
            }
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Rect curRect = inRect.TopPartPixels(60);
                Widgets.DrawMenuSection(curRect);
                Text.Font = GameFont.Tiny;
                RocketMan.GUIUtility.GridView<Pair<Color, string>>(curRect, 2, descriptionBoxes, (rect, pair) =>
                {
                    RocketMan.GUIUtility.ColorBoxDescription(rect, pair.first, pair.second);
                }, drawBackground: false);
            });
            inRect.yMin += 60;
            Text.Font = GameFont.Tiny;
            Text.CurFontStyle.fontStyle = FontStyle.Normal;
            RocketMan.GUIUtility.ScrollView<RaceSettings>(inRect, ref scrollPosition, Context.Settings.raceSettings,
                heightLambda: (raceSettings) =>
                {
                    if (searchString?.Trim().NullOrEmpty() ?? true)
                        return raceSettings.pawnDef != null ? 45f : -0.1f;
                    return raceSettings.pawnDef != null ?
                    (raceSettings.pawnDef.label != null && raceSettings.pawnDef.label.ToLower().Contains(searchString) ? 45f : -0.1f)
                    : -0.1f;
                },
                elementLambda: (rect, raceSettings) =>
                {
                    if (Widgets.ButtonInvisible(rect))
                        currentSettings = raceSettings;
                    bool ignored = IgnoreMeDatabase.ShouldIgnore(raceSettings.pawnDef);
                    DoColorBars(ref rect, raceSettings, ignored);
                    Widgets.DefLabelWithIcon(rect, iconMargin: 4, def: raceSettings.pawnDef);
                }
            );
        }

        private void DoRaceSettings(Rect inRect, RaceSettings settings)
        {
            Rect rect = inRect.TopPartPixels(110);
            Rect closeButtonRect = new Rect(inRect.xMax - 20, inRect.yMin + 5, 15, 15);
            Widgets.DrawMenuSection(rect);
            if (Widgets.ButtonImage(closeButtonRect, TexButton.CloseXSmall))
            {
                currentSettings = null;
                return;
            }
            rect = rect.ContractedBy(5);
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Text.Font = GameFont.Small;
                Text.CurFontStyle.fontStyle = FontStyle.Bold;
                Text.Anchor = TextAnchor.MiddleLeft;
                Rect titleRect = rect.TopPartPixels(27);
                Widgets.Label(titleRect, (settings.pawnDef.label.CapitalizeFirst() ?? settings.pawnDef.defName));
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleRight;
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                titleRect.xMax -= 25;
                Widgets.Label(titleRect.RightPart(0.7f), "<color=gray >[" + (settings.pawnDef.modContentPack?.Name ?? "UNKNOWN") + "]</color>");
            });
            rect.yMin += 30;
            Listing_Standard standard = new Listing_Standard(GameFont.Tiny);
            Text.Font = GameFont.Tiny;
            Text.CurFontStyle.fontStyle = FontStyle.Normal;
            standard.Begin(rect);
            if (!IgnoreMeDatabase.ShouldIgnore(currentSettings.pawnDef))
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                standard.CheckboxLabeled("Soyuz.TabContent_DilationSettings.Current.Enable".Translate(),
                    ref currentSettings.enabled);
                standard.CheckboxLabeled("Soyuz.TabContent_DilationSettings.Current.IgnoreAllFactions".Translate(),
                    ref currentSettings.ignoreFactions);
                standard.CheckboxLabeled("Soyuz.TabContent_DilationSettings.Current.IgnorePlayerFaction".Translate(),
                    ref currentSettings.ignorePlayerFaction);
            }
            else if (currentSettings.isFastMoving)
            {
                standard.Label("<color=yellow>" + "Soyuz.TabContent_DilationSettings.Current.FastPawn".Translate() + "</color>");
                standard.Label($"This kinda move at an average speed of {currentSettings.pawnDef.GetStatValueAbstract(StatDefOf.MoveSpeed)} cell/seconds");
            }
            else
            {
                standard.Label("<color=yellow>" + "Soyuz.TabContent_DilationSettings.Current.Ignored".Translate() + "</color>");
                standard.Label(IgnoreMeDatabase.Report(currentSettings.pawnDef));
            }
            standard.End();

        }

        private static void DoColorBars(ref Rect inRect, RaceSettings settings, bool ignored = false)
        {
            Rect cRect = inRect.LeftPartPixels(2);
            if (ignored == true)
                Widgets.DrawBoxSolid(inRect, ignoredColor);

            if (!settings.enabled)
                Widgets.DrawBoxSolid(cRect, Color.red);
            cRect.x += 2;

            if (settings.isFastMoving)
                Widgets.DrawBoxSolid(cRect, Color.blue);
            cRect.x += 2;

            if (settings.ignorePlayerFaction)
                Widgets.DrawBoxSolid(cRect, Color.cyan);
            cRect.x += 2;

            if (settings.ignoreFactions)
                Widgets.DrawBoxSolid(cRect, Color.magenta);
            cRect.x += 2;

            inRect.xMin += 8;
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_DilationSettings();

    }
}
