using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public abstract class ISelector_GenericSelection<T> : ISelector
    {
        public IEnumerable<T> items;
        public Action<T> selectionAction;
        private Rect viewRect = Rect.zero;

        public ISelector_GenericSelection(IEnumerable<T> defs, Action<T> selectionAction, bool integrated = false,
            Action closeAction = null) : base(integrated, closeAction)
        {
            items = defs;
            this.selectionAction = selectionAction;
        }

        public virtual float RowHeight => 54f;

        protected abstract bool DoSingleItem(Rect rect, T item);
        protected abstract bool ItemMatchSearchString(T item);

        public override void FillContents(Rect inRect)
        {
            if (useSearchBar)
            {
                var rect = inRect.TopPartPixels(30);
                Text.Font = GameFont.Tiny;
                var searchRect = rect.TopPartPixels(20);
                if (Widgets.ButtonImage(searchRect.LeftPartPixels(20), TexButton.OpenInspector))
                {
                }

                searchRect.xMin += 25;
                searchString = Widgets.TextField(searchRect, searchString).ToLower();
                inRect.y += 25;
                inRect.height -= 25;
            }

            try
            {
                viewRect = inRect.AtZero();
                viewRect.height = items.Count() * RowHeight;
                viewRect.width -= 20;
                Widgets.DrawMenuSection(inRect);
                Widgets.BeginScrollView(inRect.ContractedBy(2), ref scrollPosition, viewRect);
                Text.Font = GameFont.Tiny;
                var curRect = viewRect.TopPartPixels(RowHeight);
                foreach (var item in items)
                {
                    if (useSearchBar && !ItemMatchSearchString(item))
                        continue;
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
    }
}