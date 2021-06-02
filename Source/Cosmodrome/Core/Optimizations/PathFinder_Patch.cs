using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RocketMan.Optimizations
{
    // [RocketPatch(typeof(PathFinder), nameof(PathFinder.FindPath), 
    //    parameters: new []{typeof(IntVec3), typeof(LocalTargetInfo),
    //    typeof(TraverseParms), typeof(PathEndMode)})]
    // public class PathFinder_FindPath_Patch
    // {
    //    private const int MAX_FAILS = 4;
    //    private static readonly Dictionary<int, Pair<int, int>> cache = new Dictionary<int, Pair<int, int>>();
    //
    //    public static void Postfix(PawnPath __result, TraverseParms traverseParms, LocalTargetInfo dest)
    //    {
    //        if (!Finder.enabled) return;
    //        if (__result != PawnPath.NotFound) return;
    //        if (traverseParms.pawn == null) return;
    //        var key = Tools.GetKey(traverseParms, dest);
    //        if (cache.TryGetValue(key, out var store) && GenTicks.TicksGame - store.second < 2500)
    //        {
    //            if (store.first > MAX_FAILS)
    //            {
    //                cache.Remove(key);
    //                var pawn = traverseParms.pawn;
    //                pawn.Map?.reachability?.cache?.ClearFor(traverseParms.pawn);
    //                pawn.Map?.reachability?.cache?.AddCachedResult(pawn.GetRoom(),
    //                    pawn.Map.regionGrid.GetValidRegionAt(dest.Cell).Room, traverseParms, false);
    //            }
    //            else
    //            {
    //                store.first += 1;
    //                store.second = GenTicks.TicksGame;
    //                cache[key] = store;
    //            }
    //
    //            return;
    //        }
    //
    //        if (store != null)
    //        {
    //            store.first = Mathf.Max(store.first - 1, 0);
    //            store.second = GenTicks.TicksGame;
    //            cache[key] = store;
    //            return;
    //        }
    //
    //        cache[key] = new Pair<int, int>(1, GenTicks.TicksGame);
    //    }
    // }
}