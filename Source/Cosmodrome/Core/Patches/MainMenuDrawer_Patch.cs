using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace RocketMan
{
    [RocketPatch(typeof(MainMenuDrawer), nameof(MainMenuDrawer.DoMainMenuControls))]
    public static class MainMenuDrawer_DoMainMenuControls_Patch
    {
        private static bool started = false;
        private static bool finished = false;
        private static Stopwatch timer = new Stopwatch();

        public static bool Prefix()
        {
            if (Current.ProgramState != ProgramState.Playing)
            {
                if (finished || !RocketEnvironmentInfo.IncompatibilityUnresolved) return true;
                if (timer.IsRunning && timer.Elapsed.Seconds > 60)
                {
                    finished = true;
                    timer.Stop();
                    return true;
                }
                if (!started && Find.WindowStack.WindowOfType<Window_IncompatibilityWarning>() == null)
                {
                    timer.Start();
                    started = true;
                    Find.WindowStack.Add(new Window_IncompatibilityWarning(() =>
                    {
                        finished = true;
                    }));
                }
                if (started && Find.WindowStack.WindowOfType<Window_IncompatibilityWarning>() == null)
                {
                    if (timer.IsRunning) timer.Stop();
                    finished = true;
                    return true;
                }
                return false;
            }
            else return true;
        }
    }
}
