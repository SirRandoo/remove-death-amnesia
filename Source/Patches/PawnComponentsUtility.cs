
using Harmony;

using RimWorld;

using Verse;

namespace SirRandoo.RDA.Patches
{
    [HarmonyPatch(typeof(PawnComponentsUtility), "RemoveComponentsOnKilled")]
    public static class PawnComponentsUtility__RemoveComponentsOnKilled
    {
        [HarmonyPrefix]
        public static bool RemoveComponentsOnKilled(Pawn pawn)
        {
            if (pawn != null && pawn.RaceProps.Humanlike)
            {
                pawn.carryTracker = null;
                pawn.needs = null;
                pawn.mindState = null;
                pawn.trader = null;
            }

            return false;
        }
    }
}
