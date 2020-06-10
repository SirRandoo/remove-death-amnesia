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
            if (!MemoryThingComp.ShouldRemember(___pawn))
            {
                return;
            }

            var cache = ___pawn.TryGetComp<MemoryThingComp>()?.GetWorkSettings();

            if (cache == null)
            {
                return;
            }

            foreach (var work in cache)
            {
                ___pawn.workSettings.SetPriority(work.Key, work.Value);
            }
        }
    }
}
