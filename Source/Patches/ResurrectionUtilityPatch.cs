using HarmonyLib;
using RimWorld;
using Verse;

namespace SirRandoo.RDA.Patches
{
    [HarmonyPatch(typeof(ResurrectionUtility), "Resurrect")]
    public static class ResurrectPatch
    {
        [HarmonyPostfix]
        public static void RestoreOnResurrect(Pawn pawn)
        {
            if (!MemoryThingComp.ShouldRemember(pawn))
            {
                return;
            }

            pawn.TryGetComp<MemoryThingComp>()?.TryRestoreMemory();
        }
    }
}
