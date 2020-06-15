using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SirRandoo.RDA.Patches
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn__Kill
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Kill(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.OperandIs(RuntimeChecker.RimBillNotify))
                {
                    instruction.operand = RuntimeChecker.RdaBillNotify;
                }

                yield return instruction;
            }
        }

        public static void Notify_ColonistUnavailable(Pawn pawn)
        {
            if (Settings.Bills)
            {
                return;
            }

            BillUtility.Notify_ColonistUnavailable(pawn);
        }
    }

    [HarmonyPatch(typeof(Pawn), "SetFaction")]
    public static class Pawn__SetFaction
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetFaction(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Callvirt && instruction.operand == RuntimeChecker.EnableAndInit)
                {
                    yield return new CodeInstruction(OpCodes.Callvirt, RuntimeChecker.EnableAndInitIf);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
