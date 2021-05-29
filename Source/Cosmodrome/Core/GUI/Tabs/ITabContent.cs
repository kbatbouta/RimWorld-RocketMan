using UnityEngine;
using Verse;

namespace RocketMan.Tabs
{
    public abstract class ITabContent
    {
        private bool selected;

        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                if (value) OnSelect();
                else OnDeselect();
            }
        }

        public abstract bool ShouldShow { get; }

        public virtual float LabelWidth => Text.CalcSize(Label).x;
        public abstract string Label { get; }

        public abstract void DoContent(Rect rect);
        public abstract void OnSelect();
        public abstract void OnDeselect();
    }
}