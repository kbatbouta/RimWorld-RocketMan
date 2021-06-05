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

        public override string Label => "Soyuz.Tab".Translate();

        public static readonly Color ignoredColor = new Color(1f, 0.913f, 0.541f, 0.2f);

        public static readonly Color disabledColor = new Color(0.972f, 0.070f, 0.137f, 0.2f);

        private Vector2 scrollPosition = Vector2.zero;

        private RaceSettings curSettings;

        private string searchString = string.Empty;

        public static List<Pair<Color, string>> descriptionBoxes;

        public TabContent_DilationSettings()
        {
            if (descriptionBoxes == null)
            {
                descriptionBoxes = new List<Pair<Color, string>>();
                descriptionBoxes.Add(new Pair<Color, string>(Color.red,
                    "Soyuz.Colored.Disabled".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.yellow,
                    "Soyuz.Colored.IgnoredAndDisabled".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.blue,
                    "Soyuz.Colored.IsFastMoving".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.cyan,
                    "Soyuz.Colored.IgnorePlayerFaction".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.magenta,
                    "Soyuz.Colored.IgnoreAllFaction".Translate()));
            }
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
        }

        public override void OnSelect()
        {
            base.OnSelect();
        }

        public override void DoContent(Rect rect)
        {
            RocketMan.GUIUtility.StashGUIState();
            Listing_Standard standard = new Listing_Standard(GameFont.Tiny);
            standard.Begin(rect.TopPartPixels(115 + (RocketDebugPrefs.Debug ? 100 : 0)));
            standard.CheckboxLabeled("Soyuz.Current.EnableTimeDilation".Translate(), ref RocketPrefs.TimeDilation);
            standard.GapLine();
            standard.CheckboxLabeled("Soyuz.Current.EnableTimeWildlife".Translate(), ref RocketPrefs.TimeDilationWildlife);
            standard.CheckboxLabeled("Soyuz.Current.EnableTimeDilationVisitors".Translate(), ref RocketPrefs.TimeDilationVisitors);
            standard.CheckboxLabeled("Soyuz.Current.EnableTimeWorldPawns".Translate(), ref RocketPrefs.TimeDilationWorldPawns);
            standard.CheckboxLabeled("Soyuz.Current.EnableTimeColonnyAnimals".Translate(), ref RocketPrefs.TimeDilationColonyAnimals);
            if (RocketDebugPrefs.Debug)
            {
                standard.GapLine();
                standard.CheckboxLabeled("Soyuz.EnableDataLogging".Translate(), ref RocketDebugPrefs.DogData, "For debugging only.");
                standard.CheckboxLabeled("Soyuz.Debug150MTPS".Translate(), ref RocketDebugPrefs.Debug150MTPS, "Dangerous!");
                standard.CheckboxLabeled("Soyuz.FlashPawns".Translate(),
                    ref RocketDebugPrefs.FlashDilatedPawns);
                standard.CheckboxLabeled("Soyuz.AlwaysDilate".Translate(), ref RocketDebugPrefs.AlwaysDilating);
            }
            standard.End();
            rect.yMin += 120 + (RocketDebugPrefs.Debug ? 100 : 0);

            if (RocketPrefs.TimeDilation)
            {
                DoExtras(rect);
            }
            else RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Medium;
                Widgets.DrawMenuSection(rect);
                Widgets.Label(rect, "Soyuz.DilationDisabled".Translate());
            });
            RocketMan.GUIUtility.RestoreGUIState();
        }

        private void DoExtras(Rect inRect)
        {
            string oldString = searchString;
            searchString = Widgets.TextField(inRect.TopPartPixels(25), searchString).ToLower();
            if (oldString != searchString)
                scrollPosition = Vector2.zero;
            inRect.yMin += 30;
            if (curSettings != null)
            {
                DoRaceSettings(inRect);
                inRect.yMin += 95;
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
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Rect tempRect = inRect.TopPartPixels(25);
                Widgets.DrawMenuSection(tempRect);
                tempRect.xMin += 10 + 50;
                tempRect.xMax -= 25;
                RocketMan.GUIUtility.GridView<Action<Rect>>(tempRect.TopPartPixels(25), 2,
                        new List<Action<Rect>>()
                        {
                        (curRect) =>
                        {
                            Widgets.Label(curRect, KeyedResources.Soyuz_RaceName);
                        },
                        (curRect) =>
                        {
                            Widgets.Label(curRect, KeyedResources.Soyuz_PackageId);
                        }
                        }, (rect, action) => { action.Invoke(rect); }, drawBackground: false);
            });
            inRect.yMin += 25;
            RocketMan.GUIUtility.ScrollView<RaceSettings>(inRect, ref scrollPosition, Context.Settings.AllRaceSettings,
                heightLambda: (raceSettings) =>
                {
                    if (searchString?.Trim().NullOrEmpty() ?? true)
                        return raceSettings.def != null ? 35 : -0.1f;
                    return raceSettings.def != null ?
                    (raceSettings.def.label != null && raceSettings.def.label.ToLower().Contains(searchString) ? 45f : -0.1f)
                    : -0.1f;
                },
                elementLambda: (rect, raceSettings) =>
                {
                    if (Widgets.ButtonInvisible(rect))
                        curSettings = raceSettings;
                    bool ignored = IgnoreMeDatabase.ShouldIgnore(raceSettings.def);
                    DoColorBars(ref rect, raceSettings, ignored);
                    Rect iconRect = new Rect(rect.x, rect.y, rect.height, rect.height);
                    iconRect = iconRect.ContractedBy(4);
                    iconRect = iconRect.CenteredOnYIn(rect);
                    rect.xMin += rect.height + 5;
                    Widgets.DefIcon(iconRect, raceSettings.def, null, 0.90f);
                    RocketMan.GUIUtility.GridView(rect, 2,
                        elements: new List<Action<Rect>>()
                        {
                            (r) =>
                            {
                                Widgets.Label(r, raceSettings.def.label?.CapitalizeFirst() ?? raceSettings.def.defName);
                            },
                            (r) =>
                            {
                                 Widgets.Label(r, raceSettings.def.modContentPack?.PackageIdPlayerFacing ?? "Unknown");
                            }
                        },
                        cellLambda: (r, f) => f(r), false, false);
                }
            );
        }

        private void DoRaceSettings(Rect inRect)
        {
            if (curSettings != null)
            {
                RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Rect curRect = inRect.TopPartPixels(90);
                    Widgets.DrawMenuSection(curRect);
                    curRect.xMax -= 2;
                    Rect closeRect = curRect.TopPartPixels(20).RightPartPixels(20);
                    closeRect.x -= 3;
                    closeRect.y += 3;
                    if (Widgets.ButtonImage(closeRect, TexButton.CloseXSmall, true))
                    {
                        curSettings = null;
                        return;
                    }
                    curRect.xMin += 5;
                    RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
                    {
                        Text.Font = GameFont.Tiny;
                        Text.CurFontStyle.fontStyle = FontStyle.Bold;
                        Widgets.Label(curRect.TopPartPixels(25), $"{curSettings.def.label?.CapitalizeFirst() ?? curSettings.def.defName}");
                    });
                    curRect.yMin += 25;
                    bool enabled = curSettings.enabled;
                    string color = enabled ? "white" : "red";
                    if (!IgnoreMeDatabase.ShouldIgnore(curSettings.def))
                    {
                        Text.Anchor = TextAnchor.MiddleLeft;
                        RocketMan.GUIUtility.CheckBoxLabeled(curRect.TopPartPixels(20), "Soyuz.Current.Enable".Translate(),
                            ref curSettings.enabled);
                        curRect.yMin += 20;
                        RocketMan.GUIUtility.CheckBoxLabeled(curRect.TopPartPixels(20), "Soyuz.Current.IgnoreAllFactions".Translate(),
                            ref curSettings.ignoreFactions);
                        curRect.yMin += 20;
                        RocketMan.GUIUtility.CheckBoxLabeled(curRect.TopPartPixels(20), "Soyuz.Current.IgnorePlayerFaction".Translate(),
                            ref curSettings.ignorePlayerFaction);
                        curRect.yMin += 20;
                    }
                    else if (curSettings.isFastMoving)
                    {
                        Widgets.Label(curRect.TopPartPixels(20), "<color=yellow>" + "Soyuz.Current.FastPawn".Translate() + "</color>");
                        curRect.yMin += 20;
                        Widgets.Label(curRect.TopPartPixels(20), "Soyuz.Current.MoveSpeed".Translate().Formatted(curSettings.def.GetStatValueAbstract(StatDefOf.MoveSpeed)));
                        curRect.yMin += 20;
                    }
                    else
                    {
                        Widgets.Label(curRect.TopPartPixels(20), "<color=yellow>" + "Soyuz.Current.Ignored".Translate() + "</color>");
                        curRect.yMin += 20;
                        Widgets.Label(curRect.TopPartPixels(20), IgnoreMeDatabase.Report(curSettings.def));
                        curRect.yMin += 20;
                    }
                });
            }
        }

        private static void DoColorBars(ref Rect inRect, RaceSettings settings, bool ignored = false)
        {
            Widgets.DrawBoxSolid(inRect.LeftPartPixels(3), Color.grey);

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
