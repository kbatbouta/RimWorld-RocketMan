using System;
using RimWorld;
using Verse;

namespace RocketMan
{
    public static class DebugMenuActions
    {
        [DebugAction("RocketMan", "Make random faction leader null", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void MakeRandomFactionLeaderNull()
        {
            foreach (Faction faction in Find.FactionManager.AllFactions)
            {
                if (faction.IsPlayer)
                    continue;
                if (faction.leader == null)
                    continue;
                if (faction.leader.Spawned)
                    continue;
                if (!faction.ShouldHaveLeader)
                    continue;
                if (Rand.Chance(0.9f))
                    continue;
                faction.leader.Destroy();
                faction.leader = null;
                break;
            }
        }

        public static int t1;

        [Main.OnTickRarer]
        public static void TickRare1()
        {
            //Log.Message($"ROCKETMAN: Ticked TickRare1 after {GenTicks.TicksGame - t1}");
            t1 = GenTicks.TicksGame;
        }

        public static int t2;

        [Main.OnTickRare]
        public static void TickRare2()
        {
            //Log.Message($"ROCKETMAN: Ticked TickRare2 after {GenTicks.TicksGame - t2}");
            t2 = GenTicks.TicksGame;
        }

        public static int t3;

        [Main.OnTickRare]
        public static void TickRare3()
        {
            //Log.Message($"ROCKETMAN: Ticked TickRarer3 after {GenTicks.TicksGame - t3}");
            t3 = GenTicks.TicksGame;
        }
    }
}
