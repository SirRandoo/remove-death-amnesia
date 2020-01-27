using System.Collections.Generic;
using System.Reflection.Emit;

using Harmony;

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
            var billMarker = AccessTools.Method(type: typeof(BillUtility), name: nameof(BillUtility.Notify_ColonistUnavailable));
            var wrapper = AccessTools.Method(type: typeof(Pawn__Kill), name: nameof(Notify_ColonistUnavailable));

            foreach(var instruction in instructions)
            {
                if(instruction.opcode == OpCodes.Call && instruction.operand == billMarker)
                {
                    instruction.operand = wrapper;
                }

                yield return instruction;
            }
        }

        public static void Notify_ColonistUnavailable(Pawn pawn)
        {
            if(Settings.Bills) return;

            BillUtility.Notify_ColonistUnavailable(pawn);
        }
    }
}
