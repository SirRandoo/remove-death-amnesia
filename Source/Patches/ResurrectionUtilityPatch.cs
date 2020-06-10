using HarmonyLib;
using RimWorld;
using Verse;

namespace SirRandoo.RDA.Patches
{
    [HarmonyPatch(typeof(ResurrectionUtility), "Resurrect")]
    public static class ResurrectionUtility__Resurrect
    {
        [HarmonyPostfix]
        public static void ResurrectPostfix(Pawn pawn)
        {
            if (!MemoryThingComp.ShouldRemember(pawn))
            {
                return;
            }

            pawn.TryGetComp<MemoryThingComp>()?.TryRestoreMemory();
        }
    }
}
