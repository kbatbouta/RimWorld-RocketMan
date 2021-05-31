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
    }
}
