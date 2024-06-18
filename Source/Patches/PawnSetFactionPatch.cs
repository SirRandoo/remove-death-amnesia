// MIT License
//
// Copyright (c) 2024 SirRandoo
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace SirRandoo.RDA.Patches;

[PublicAPI]
[HarmonyPatch]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class PawnSetFactionPatch
{
    private static readonly MethodBase SetFactionMethod = AccessTools.Method(typeof(Pawn), nameof(Pawn.SetFaction));

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return SetFactionMethod;
    }

    public static void Postfix(Pawn __instance, Faction newFaction)
    {
        if (!MemoryThingComp.ShouldRemember(__instance))
        {
            return;
        }

        if (newFaction != Faction.OfPlayer)
        {
            return;
        }

        __instance?.TryGetComp<MemoryThingComp>()?.TryRestoreMemory(false);
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
