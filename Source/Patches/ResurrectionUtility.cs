using System.Collections.Generic;
using System.Reflection.Emit;

using Harmony;

using RimWorld;

namespace SirRandoo.RDA.Patches
{
    [HarmonyPatch(typeof(ResurrectionUtility), "Resurrect")]
    public static class ResurrectionUtility__Resurrect
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Resurrect(IEnumerable<CodeInstruction> instructions)
        {
            var enableAndInit = AccessTools.Method(type: typeof(Pawn_WorkSettings), name: nameof(Pawn_WorkSettings.EnableAndInitialize));
            var enableAndInitIf = AccessTools.Method(type: typeof(Pawn_WorkSettings), name: nameof(Pawn_WorkSettings.EnableAndInitializeIfNotAlreadyInitialized));

            foreach(var instruction in instructions)
            {
                if(instruction.opcode == OpCodes.Callvirt && instruction.operand == enableAndInit)
                {
                    yield return new CodeInstruction(opcode: OpCodes.Callvirt, operand: enableAndInitIf);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
