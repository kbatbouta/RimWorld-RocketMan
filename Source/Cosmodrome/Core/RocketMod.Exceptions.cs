using RimWorld;
using Verse;

namespace RocketMan
{
    public partial class RocketMod
    {
        [Main.OnTickLong]
        [Main.OnDefsLoaded]
        public static void UpdateExceptions()
        {
            //
            // DefDatabase<StatDef>.ResolveAllReferences();
            if (StatDefOf.MarketValue != null && StatDefOf.MarketValueIgnoreHp != null)
            {
                RocketStates.StatExpiry[StatDefOf.MarketValue.index] = 0;
                RocketStates.StatExpiry[StatDefOf.MarketValueIgnoreHp.index] = 0;
            }
        }
    }
}