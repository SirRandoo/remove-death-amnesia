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
                    Log.Message($"+ Opcode: {instruction.opcode}::{instruction.operand.ToStringSafe()}");
                    yield return instruction;
                    
                    Log.Message("Starting work settings removal...");
                    culling = true;
                    continue;
                }

                if (!culling)
                {
                    Log.Message($"+ Opcode: {instruction.opcode}::{instruction.operand.ToStringSafe()}");
                    yield return instruction;
                }
                else
                {
                    if (instruction.opcode == OpCodes.Stfld && instruction.OperandIs(RdaStatic.PawnWorkSettings))
                    {
                        Log.Message("Finished removing work settings assignment...");
                        culling = false;
                        instruction.opcode = OpCodes.Nop;
                        continue;
                    }
                    
                    instruction.opcode = OpCodes.Nop;
                    Log.Message($"- Opcode: {instruction.opcode}::{instruction.operand.ToStringSafe()}");
                }
            }
        }
    }
}
