﻿using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using RimWorld;

using Verse;
using Verse.AI;

namespace SirRandoo.RDA.Patches
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn__Kill
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Kill(IEnumerable<CodeInstruction> instructions)
        {
            var billMarker = AccessTools.Method(typeof(BillUtility), nameof(BillUtility.Notify_ColonistUnavailable));
            var wrapper = AccessTools.Method(typeof(Pawn__Kill), nameof(Notify__ColonistUnavailable));

            foreach(var instruction in instructions)
            {
                if(instruction.opcode == OpCodes.Call && instruction.operand as MethodInfo == billMarker)
                {
                    instruction.operand = wrapper;
                }

                yield return instruction;
            }
        }

        [HarmonyPrefix]
        public static void KillPrefix(Pawn __instance)
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
        public static void Postfix(Pawn __instance, Faction newFaction)
        {
            if (!MemoryThingComp.ShouldRemember(__instance))
            {
                return;
            }
            
            if (newFaction != Faction.OfPlayer)
            {
                return;
            }
                
            // __instance?.TryGetComp<MemoryThingComp>()?.Notify_WildManTamed();
            __instance?.TryGetComp<MemoryThingComp>()?.TryRestoreMemory();
        }
    }
}
