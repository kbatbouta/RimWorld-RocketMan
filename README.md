## <a href="https://steamcommunity.com/sharedfiles/filedetails/?id=2479389928&searchtext=">[BETA] RocketMan</a>:

RocketMan is a RimWorld mod that is designed to improve RimWorld performance.

##### Important note: RocketMan should be the last mod in your mod list.

### Features:

1. Time dilation: time dilation is a feature of RocketMan where it throttles the tick rate of animals and world pawns from 60 Hz to 5 Hz. This allows you to have hundreds of world pawns and animals on your map with almost no performance impact. This removes the need for RuntimeGC since world Pawns are no longer an issue. This will lead to a huge performance impact on large maps and old colonies. (+150 TPS on avg)
2. Statistics caching: RimWorld; without RocketMan, will do the same calculations for stats every tick. RocketMan allows you to cache these results thus removing 90% of the vanilla overhead. This will result in a modest performance impact. (+30 TPS on avg)
3. GlowGrid rework: RimWorld has a very bad implementation of the lighting grid. It basically recalculates the entire light grid for the entire map any time a change occurs such as a light bubble being switched on/off or fire. RocketMan makes the updates only occur in the relevant areas. This will result in a smoother game experience and generally removing light related lag spikes. This will help greatly with mods that have lighting effects in them by eliminating staturing.
4. World Reachability Checks: RocketMan when first loading the game creates a table containing the reachability data for each planet tile. Thus making the process of finding if a tile is reachable from another tile almost O(1) or one operation.
5. Pathfinding Stability: RocketMan will detect pathfinding errors and fix them to some degree.
6. Corpse Removal: RocketMan will remove dead corpses from the map when some conditions are met. RocketMan will not remove corpses that are always within your sight or near to your base to preserve the game balance. It will remove corpses near the edge of the map or outside your base in 7 days on average. This removes entirely the need for RuntimeGC since RocketMan does the same function but in a smart manner. 
7. Game Log Removal: RocketMan will automatically trim your game log to insure smoother operations further removing the need for RuntimeGC.


### Notes: 

1. RocketMan doesn't support RimThreaded and never will.
2. Bug reports with no logs will not get a response.

Github Link: https://github.com/kbatbouta/RocketMan

RocketMan is OpenSource and for anyone to use, modify, update.

### Credits: 

* The main developer: Karim (aka Krkr)
* The Thumbnail: Trisscar.
* The original concept of Stat caching: Notooshabby.

DISCLAIMER: I’m not responsible in any way for damage done by RocketMan to your saves. RocketMan has been tested for over 9 months. Any damage done to your save is probably your own fault since RocketMan has been tested for so long. 

### RocketRules (Compatibility system) (Currently not active and is being overhauled)
Now RocketMan support a new rule system to avoid compatibility issues.
This works by placing `RocketRules.xml` files in `YourModFolder/Extras/RocketRules.xml`
and following this format:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<RocketRules>
    <IgnoreMe defname="AIRobot_Hauler_III"/>
    <IgnoreMe packageId="Haplo.Miscellaneous.Robots"/>    
    <IgnoreMe defname="AIRobot_Hauler_I"/>
</RocketRules>
``` 
**Note** RocketRules can be made from any number of IgnoreMe nodes. To make RocketMan ignore a ThingDef as in this example just use the defname attribute. Or if you are lazy just use your mod packageId.
This system will work whatever the load order is and it tells rocketman to:
* Not throttle pawns/things in your mod (by providing def name).
* Not to cache stats in your mod (by providing def name).
* Not to do either to anything (by providing the packageId).
* This will be applied to every new feature in RocketMan.
This system is the new standard going forward for RocketMan. This is meant to make the compatibility process easy, simple, and seamless.  
#### Request and notification system
```xml
<?xml version="1.0" encoding="utf-8" ?>
<RocketRules>
     <Notify type="PawnDirty" packageId="krkr.RocketMan" method="ThingWithComps:Notify_Equipped"/>
</RocketRules>
```
Your mod can notify RocketMan to clear the statCache by calling a function in your code (preferably empty one). You can follow this format
* `packageId` is your mod `packageId`. This is used only to keep track of the current rules.
* `method` (formated as `YourClass:Method`) is the method that you call to notify rocketman that your mod need the cache cleared.

**Note** This work by applying a `Prefix` patch on your destination/provided empty method (in this case `ThingWithComps:Notify_Equipped`) thus every time you call `ThingWithComps:Notify_Equipped` in this example the prefix is executed and the cache is cleared.
and that prefix notify rocketman to clear the cache

**Notification types**
* `PawnDirty` The target/provided method for this need to have `Pawn pawn` as a parameter. 

**Note on notification types** For now there is only one which the above `PawnDirty`. This system is the new way for your mod to call RocketMan regardless of the load order.

#### Special Thanks goes to:

* Madman666 the entire Dubwise server for the help and testing they provided!
* Trisscar for their awesome thumbnail.
* Dubwise for hosting me on their discord server.
* Wiri for their awesome contribution.
* Bad Vlad (ModderK) for their awesome feedback.
* Brrainz for Harmony and ZombieLand which inspired my time dilation solution.

You can always ask questions on the Dubwise discord server: https://discord.gg/mKVDMqq4

## You can always support me!
<a href='https://ko-fi.com/karimbat' target='_blank'><img height='35' style='border:0px;height:46px;' src='https://az743702.vo.msecnd.net/cdn/kofi3.png?v=0' border='0' alt='Buy Me a Coffee at ko-fi.com' />
 
