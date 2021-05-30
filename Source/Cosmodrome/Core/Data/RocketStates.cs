using System;
namespace RocketMan
{
    public static class RocketStates
    {
        public static int LastFrame;

        public static int TicksSinceStarted = 0;

        public static bool SingleTickIncrement = false;

        public static int SingleTickLeft = 0;

        public static byte[] StatExpiry = new byte[ushort.MaxValue];

        public static bool[] DilatedDefs = new bool[ushort.MaxValue];

        public static object LOCKER = new object();
    }
}
