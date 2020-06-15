using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace SirRandoo.RDA.Patches
{
    [HarmonyPatch(typeof(ResurrectionUtility), "Resurrect")]
    public static class ResurrectionUtility__Resurrect
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Resurrect(IEnumerable<CodeInstruction> instructions)
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
