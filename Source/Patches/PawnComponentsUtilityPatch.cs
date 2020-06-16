using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SirRandoo.RDA.Patches
{
    [HarmonyPatch(typeof(PawnComponentsUtility), "RemoveComponentsOnKilled")]
    public static class PawnComponentsUtility__RemoveComponentsOnKilled
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RemoveComponentsOnKilled(IEnumerable<CodeInstruction> instructions)
        {
            var culling = false;

            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stfld && instruction.OperandIs(RdaStatic.PawnMindState))
                {
                    yield return instruction;
                    culling = true;
                    continue;
                }

                if (!culling)
                {
                    yield return instruction;
                }
                else
                {
                    if (instruction.opcode == OpCodes.Stfld && instruction.OperandIs(RdaStatic.PawnWorkSettings))
                    {
                        culling = false;
                        instruction.opcode = OpCodes.Nop;
                        continue;
                    }
                    
                    instruction.opcode = OpCodes.Nop;
                }
            }
        }
    }
}
