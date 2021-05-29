using System;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public abstract class ISelector : Window
    {
        protected readonly Action closeAction;
        protected bool integrated;

        public Vector2 scrollPosition = Vector2.zero;
        public string searchString = "";
        public bool useSearchBar = false;

        public ISelector(bool integrated = false, Action closeAction = null)
        {
            this.integrated = integrated;
            this.closeAction = closeAction;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            integrated = false;
            inRect.height -= 30;
            FillContents(inRect);
            inRect.height += 30;
            if (Widgets.ButtonText(inRect.BottomPartPixels(30), "ColonyManager2Close".Translate())) Close();

            Text.Font = font;
            Text.Anchor = anchor;
        }

        public void DoIntegratedContents(Rect inRect)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;

            integrated = true;
            FillContents(inRect);

            Text.Font = font;
            Text.Anchor = anchor;
        }

        public abstract void FillContents(Rect inRect);

        public override void Close(bool doCloseSound = true)
        {
            if (!integrated)
                base.Close(doCloseSound);
            else
                closeAction.Invoke();
        }
    }
}