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

        public ISelector_GenericSelection(IEnumerable<T> defs, Action<T> selectionAction, bool integrated = false,
            Action closeAction = null) : base(integrated, closeAction)
        {
            items = defs;
            this.selectionAction = selectionAction;
        }

        public virtual float RowHeight => 54f;

        protected abstract void DoSingleItem(Rect rect, T item);
        protected abstract bool ItemMatchSearchString(T item);

        public override void DoContent(Rect inRect)
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
                GUIUtility.ScrollView(inRect, ref scrollPosition, items,
                    heightLambda: (item) => !searchString.NullOrEmpty() ? (ItemMatchSearchString(item) ? -1f : RowHeight) : RowHeight,
                    elementLambda: (rect, item) =>
                    {
                        DoSingleItem(rect, item);
                        if (Widgets.ButtonInvisible(rect))
                        {
                            selectionAction.Invoke(item);
                            if (!integrated) Close();
                        }
                    });
            }
            catch (Exception er)
            {
                Log.Error(er.ToString());
            }
        }
    }
}