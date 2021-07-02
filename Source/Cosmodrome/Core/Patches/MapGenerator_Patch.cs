using System;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public static class MapGenerator_Patch
    {
        [RocketPatch(typeof(MapGenerator), nameof(MapGenerator.GenerateMap))]
        public static class GenerateMap_Patch
        {
            public static void Prefix(ref IntVec3 mapSize)
            {
                if (Find.Maps.Count != 0 && Faction.OfPlayerSilentFail != null && Find.Maps.Any(m => m.IsPlayerHome))
                {
                    World world = Find.World;
                    WorldInfoComponent comp = world.GetComponent<WorldInfoComponent>();
                    if (comp.useCustomMapSizes)
                    {
                        Vector3 vector = comp.IntialMapSize;
                        mapSize.x = (int)vector.x;
                        mapSize.y = (int)vector.y;
                        mapSize.z = (int)vector.z;

                        comp.useCustomMapSizes = false;
                        Log.Message($"ROCKETMAN: Applied custom map size for new settelment/map");
                    }
                }
            }
        }
    }
}
