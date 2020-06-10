using HarmonyLib;
using Verse;
using Verse.AI;

namespace SirRandoo.RDA.Patches
{
    [HarmonyPatch(typeof(MentalBreakWorker_RunWild), "TryStart")]
    public static class RunWildPatch
    {
        [HarmonyPrefix]
        public static void StoreOnRunWild(Pawn pawn)
        {
            if (!MemoryThingComp.ShouldRemember(pawn))
            {
                return;
            }

            pawn?.TryGetComp<MemoryThingComp>()?.TryStoreMemory();
        }
    }
}
