using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace SirRandoo.RDA.Patches;

[PublicAPI]
[HarmonyPatch]
internal static class ResurrectPatch
{
    private static readonly MethodBase ResurrectMethod = AccessTools.Method(typeof(ResurrectionUtility), nameof(ResurrectionUtility.TryResurrect));

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return ResurrectMethod;
    }

    private static void Postfix(Pawn pawn)
    {
        if (!MemoryThingComp.ShouldRemember(pawn))
        {
            return;
        }

        pawn.TryGetComp<MemoryThingComp>()?.TryRestoreMemory();
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
