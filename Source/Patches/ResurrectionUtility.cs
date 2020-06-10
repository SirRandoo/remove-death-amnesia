using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SirRandoo.RDA.Patches
{
    [HarmonyPatch(typeof(ResurrectionUtility), "Resurrect")]
    public static class ResurrectionUtility__Resurrect
    {
        // [HarmonyTranspiler]
        // public static IEnumerable<CodeInstruction> Resurrect(IEnumerable<CodeInstruction> instructions)
        // {
        //     var enableAndInit = AccessTools.Method(
        //         typeof(Pawn_WorkSettings),
        //         nameof(Pawn_WorkSettings.EnableAndInitialize)
        //     );
        //     var enableAndInitIf = AccessTools.Method(
        //         typeof(Pawn_WorkSettings),
        //         nameof(Pawn_WorkSettings.EnableAndInitializeIfNotAlreadyInitialized)
        //     );
        //
        //     foreach (var instruction in instructions)
        //     {
        //         if (instruction.opcode == OpCodes.Callvirt && instruction.operand as MethodInfo == enableAndInit)
        //         {
        //             yield return new CodeInstruction(OpCodes.Callvirt, enableAndInitIf);
        //         }
        //         else
        //         {
        //             yield return instruction;
        //         }
        //     }
        // }

        [HarmonyPostfix]
        public static void ResurrectPostfix(Pawn pawn)
        {
            if (!MemoryThingComp.ShouldRemember(pawn))
            {
                return;
            }

            pawn.TryGetComp<MemoryThingComp>()?.TryRestoreMemory();
        }
    }
}
