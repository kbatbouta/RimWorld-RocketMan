using RimWorld;
using UnityEngine;
using Verse;

namespace RocketMan
{
    internal class MainButton_Toggle : MainButtonWorker
    {
        public override bool Disabled => !RocketPrefs.MainButtonToggle ? true : (Find.CurrentMap == null && (!def.validWithoutMap || def == MainButtonDefOf.World) || Find.WorldRoutePlanner.Active && Find.WorldRoutePlanner.FormingCaravan && (!def.validWithoutMap || def == MainButtonDefOf.World));

        public override float ButtonBarPercent => RocketPrefs.MainButtonToggle ? base.ButtonBarPercent : 0f;

        public override void Activate()
        {
            if (Event.current.button == 0)
            {
                if (Find.WindowStack.WindowOfType<Window_MainControls>() != null)
                {
                    Find.WindowStack.RemoveWindowsOfType(typeof(Window_MainControls));
                }
                else
                {
                    Find.WindowStack.Add(new Window_MainControls());
                }
            }
            else
            {
                if (Find.WindowStack.WindowOfType<Window_MainControls>() == null) Find.WindowStack.Add(new Window_MainControls());
            }
        }
    }
}