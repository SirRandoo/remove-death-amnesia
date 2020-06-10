using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
// ReSharper disable InconsistentNaming

namespace SirRandoo.RDA.Patches
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class PawnKillPatch
    {
        private static readonly MethodInfo BillColonistUnavailable = AccessTools.Method(typeof(BillUtility), nameof(BillUtility.Notify_ColonistUnavailable));
        private static readonly MethodInfo NotifyColonistUnavailable = AccessTools.Method(typeof(PawnKillPatch), nameof(Notify__ColonistUnavailable));
        
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PreserveBillsOnDeath(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && instruction.operand as MethodInfo == BillColonistUnavailable)
                {
                    instruction.operand = NotifyColonistUnavailable;
                }

                yield return instruction;
            }
        }

        [HarmonyPrefix]
        public static void StoreMemoriesOnDeath(Pawn __instance)
        {
            if (!MemoryThingComp.ShouldRemember(__instance))
            {
                return;
            }

            __instance.TryGetComp<MemoryThingComp>()?.TryStoreMemory();
        }

        private static void Notify__ColonistUnavailable(Pawn pawn)
        {
            if (Settings.Bills)
            {
                return;
            }

            BillUtility.Notify_ColonistUnavailable(pawn);
        }
    }

    [HarmonyPatch(typeof(Pawn), "SetFaction")]
    public static class PawnSetFactionPatch
    {
        [HarmonyPostfix]
        public static void RestorePlayerColonist(Pawn __instance, Faction newFaction)
        {
            if (!MemoryThingComp.ShouldRemember(__instance))
            {
                return;
            }

            if (newFaction != Faction.OfPlayer)
            {
                return;
            }

            __instance?.TryGetComp<MemoryThingComp>()?.TryRestoreMemory();
        }
    }

    [HarmonyPatch(typeof(Pawn), "ChangeKind")]
    public static class PawnChangeKindPatch
    {
        [HarmonyPrefix]
        public static void WildManCheck(Pawn __instance, PawnKindDef newKindDef)
        {
            if (!MemoryThingComp.ShouldRemember(__instance))
            {
                return;
            }

            if (newKindDef != PawnKindDefOf.WildMan)
            {
                return;
            }
            
            __instance?.TryGetComp<MemoryThingComp>()?.TryStoreMemory();
        }
    }
}
