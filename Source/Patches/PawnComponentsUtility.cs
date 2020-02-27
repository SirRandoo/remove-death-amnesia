using HarmonyLib;

using RimWorld;

using Verse;

namespace SirRandoo.RDA.Patches
{
    [HarmonyPatch(typeof(PawnComponentsUtility), "RemoveComponentsOnKilled")]
    public static class PawnComponentsUtility__RemoveComponentsOnKilled
    {
        [HarmonyPostfix]
        public static void RemoveComponentsOnKilled__Postfix(Pawn pawn, ref Pawn_WorkSettings __state)
        {
            if(Settings.Priorities)
            {
                pawn.workSettings = __state;
            }
        }

        [HarmonyPrefix]
        public static void RemoveComponentsOnKilled__Prefix(Pawn pawn, ref Pawn_WorkSettings __state) => __state = pawn.workSettings;
    }
}
