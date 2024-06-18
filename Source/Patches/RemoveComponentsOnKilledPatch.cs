using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;

namespace SirRandoo.RDA.Patches;

[PublicAPI]
[HarmonyPatch]
public static class RemoveComponentsOnKilledPatch
{
    private static readonly MethodBase RemoveComponentsOnKilledMethod = AccessTools.Method(
        typeof(PawnComponentsUtility),
        nameof(PawnComponentsUtility.RemoveComponentsOnKilled)
    );

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return RemoveComponentsOnKilledMethod;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var culling = false;

        foreach (CodeInstruction instruction in instructions)
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
