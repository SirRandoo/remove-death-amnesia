using System.Collections.Generic;
using System.Reflection.Emit;
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

            pawn.TryGetComp<MemoryThingComp>()?.TryRestoreMemory(false);
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Resurrect(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Callvirt && instruction.OperandIs(RdaStatic.EnableAndInit))
                {
                    yield return new CodeInstruction(OpCodes.Callvirt, RdaStatic.EnableAndInitIf);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
