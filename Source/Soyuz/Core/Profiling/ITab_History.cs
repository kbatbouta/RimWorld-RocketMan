using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RocketMan;
using UnityEngine;
using Verse;

namespace Soyuz.Profiling
{
    public class ITab_History : ITab
    {
        private const int graphHeight = 95;

        private Vector2 scrollView = Vector2.zero;
            
        public override bool IsVisible => RocketDebugPrefs.LogData && RocketDebugPrefs.Debug && Prefs.LogVerbose && Prefs.DevMode;

        public override void UpdateSize()
        {
            base.UpdateSize();
            this.size = new Vector2(450,  350);
        }

        public override void FillTab()
        {
            var pawn = SelPawn;
            var needs = pawn.needs.needs.Select(n => n.GetType());
            var hediffs = pawn.health.hediffSet.hediffs;
            var curRect = new Rect(Vector2.zero, this.size);
            curRect.ContractedBy(5);
            curRect.yMin += 20;
            curRect = curRect.AtZero();
            curRect.yMin += 20;
            curRect.width -= 5;
            Widgets.BeginScrollView(curRect, ref scrollView, new Rect(Vector2.zero, new Vector2(430, (120 + 20) * (hediffs.Count + needs.Count()))));
            var elementRect = new Rect(0, 0, 425, graphHeight);
            var needsModel = pawn.GetNeedModels();
            foreach (var type in needs)
            {
                if (needsModel.TryGetValue(type, out var model))
                {
                    model.DrawGraph(elementRect.BottomPartPixels(75));
                    Widgets.Label(elementRect.TopPartPixels(20), type.Name);
                    elementRect.y += elementRect.height + 20;
                }
            }
            var hediffsModel = pawn.GetHediffModels();
            foreach (var hediff in hediffs)
            {
                if (hediffsModel.TryGetValue(hediff, out var model))
                {
                    model.DrawGraph(elementRect.BottomPartPixels(75));
                    Widgets.Label(elementRect.TopPartPixels(20), hediff.def.label);
                    elementRect.y += elementRect.height + 20;
                }
            }
            Widgets.EndScrollView();
        }
    }
}