using HarmonyLib;
using RimWorld;
using Verse;

namespace SirRandoo.RDA.Patches
{
    [HarmonyPatch(typeof(Pawn_WorkSettings), "EnableAndInitialize")]
    public static class WorkSettingsPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn ___pawn)
        {
            if (!Settings.Priorities || !MemoryThingComp.ShouldRemember(___pawn))
            {
                return;
            }

            ___pawn.TryGetComp<MemoryThingComp>()?.TryRestoreWorkPriorities();
        }
    }
}
