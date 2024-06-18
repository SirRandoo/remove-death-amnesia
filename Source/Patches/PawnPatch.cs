using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming

namespace SirRandoo.RDA.Patches;

[PublicAPI]
[HarmonyPatch]
internal static class PawnKillPatch
{
    private static readonly MethodBase KillMethod = AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill));
    private static readonly MethodInfo BillColonistUnavailable = AccessTools.Method(typeof(BillUtility), nameof(BillUtility.Notify_ColonistUnavailable));
    private static readonly MethodInfo NotifyColonistUnavailable = AccessTools.Method(typeof(PawnKillPatch), nameof(Notify__ColonistUnavailable));

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return KillMethod;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Call && instruction.operand as MethodInfo == BillColonistUnavailable)
            {
                instruction.operand = NotifyColonistUnavailable;
            }

            yield return instruction;
        }
    }

    private static void Prefix(Pawn __instance)
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

[HarmonyPatch(typeof(Pawn), "ChangeKind")]
public static class PawnChangeKindPatch
{
    [HarmonyPrefix]
    public static void WildManCheck(Pawn __instance, PawnKindDef newKindDef)
    {
        if (!MemoryThingComp.ShouldRemember(__instance))
        {
            return;
        }

        if (newKindDef != PawnKindDefOf.WildMan)
        {
            return;
        }

        __instance?.TryGetComp<MemoryThingComp>()?.TryStoreMemory();
    }
}
