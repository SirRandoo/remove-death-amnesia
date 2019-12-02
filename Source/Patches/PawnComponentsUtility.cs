
using System.Collections.Generic;
using System.Reflection.Emit;

using Harmony;

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
            var mindState = AccessTools.Field(type: typeof(Pawn), name: "mindState");
            var workSettings = AccessTools.Field(type: typeof(Pawn), name: "workSettings");
            var culling = false;

            foreach(var instruction in instructions)
            {
                if(instruction.opcode == OpCodes.Stfld && instruction.operand == mindState) culling = true;
                if(!culling) yield return instruction;
                if(instruction.opcode == OpCodes.Stfld && instruction.operand == workSettings) culling = false;

                instruction.opcode = OpCodes.Nop;
            }
        }
    }
}
