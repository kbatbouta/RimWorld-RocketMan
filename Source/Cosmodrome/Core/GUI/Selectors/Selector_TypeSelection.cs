using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public class Selector_TypeSelection : ISelector_GenericSelection<Type>
    {
        private static readonly Dictionary<Type, string> cache = new Dictionary<Type, string>();
        private readonly int count;
        private readonly Type[] types;
        private Rect viewRect = Rect.zero;

        public Selector_TypeSelection(Type t, Action<Type> selectionAction, bool integrated = false,
            Action closeAction = null) : base(t.AllSubclassesNonAbstract(), selectionAction, integrated, closeAction)
        {
            types = t.AllSubclassesNonAbstract().ToArray();
            count = types.Length;
        }

        public override float RowHeight => 24f;

        public override void FillContents(Rect inRect)
        {
            FillTypeContent(inRect);
        }

        protected void FillTypeContent(Rect inRect)
        {
            try
            {
                viewRect = inRect.AtZero();
                viewRect.height = count * RowHeight;
                viewRect.width -= 20;
                Widgets.DrawMenuSection(inRect);
                Widgets.BeginScrollView(inRect.ContractedBy(2), ref scrollPosition, viewRect);
                Text.Font = GameFont.Tiny;
                var curRect = viewRect.TopPartPixels(RowHeight);
                foreach (var item in types)
                {
                    if (DoSingleItem(curRect, item))
                    {
                        selectionAction.Invoke(item);
                        if (!integrated) Close();
                    }
                    curRect.y += RowHeight;
                }
                Widgets.EndScrollView();
            }
            catch (Exception er)
            {
                Log.Error(er.ToString());
            }
        }

        protected override bool DoSingleItem(Rect rect, Type item)
        {
            string name;
            if (!cache.TryGetValue(item, out name))
                name = cache[item] = item.Name.Translate();
            Widgets.DrawHighlightIfMouseover(rect);
            Widgets.Label(rect, name);
            if (Widgets.ButtonInvisible(rect))
                return true;
            return false;
        }

        protected override bool ItemMatchSearchString(Type item)
        {
            return true;
        }
    }
}