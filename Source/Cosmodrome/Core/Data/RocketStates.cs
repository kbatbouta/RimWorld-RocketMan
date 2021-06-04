using System;
namespace RocketMan
{
    public static class RocketStates
    {
        public static int LastFrame;

        public static int TicksSinceStarted = 0;

        public static bool DefsLoaded = false;

        public static bool SingleTickIncrement = false;

        public static int SingleTickLeft = 0;

        public static float[] StatExpiry = new float[ushort.MaxValue];

        public static bool[] DilatedDefs = new bool[ushort.MaxValue];

        public static object LOCKER = new object();
    }
}
