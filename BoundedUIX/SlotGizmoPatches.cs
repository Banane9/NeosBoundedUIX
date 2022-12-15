﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BaseX;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;

namespace BoundedUIX
{
    [HarmonyPatch(typeof(SlotGizmo))]
    internal static class SlotGizmoPatches
    {
        private static readonly MethodInfo boundUIXMethod = typeof(SlotGizmoPatches).GetMethod(nameof(BoundUIX), AccessTools.allDeclared);

        private static readonly MethodInfo computeBoundingBoxMethod = typeof(BoundsHelper).GetMethod("ComputeBoundingBox", AccessTools.allDeclared);

        private static BoundingBox BoundUIX(BoundingBox bounds, Slot target, Slot space)
        {
            if (!(target.GetComponent<RectTransform>() is RectTransform rect))
                return bounds;

            var area = rect.ComputeGlobalComputeRect();
            bounds.Encapsulate(space.GlobalPointToLocal(rect.Canvas.Slot.LocalPointToGlobal(area.ExtentMin)));
            bounds.Encapsulate(space.GlobalPointToLocal(rect.Canvas.Slot.LocalPointToGlobal(area.ExtentMax)));

            return bounds;
        }

        [HarmonyTranspiler]
        [HarmonyPatch("OnCommonUpdate")]
        private static IEnumerable<CodeInstruction> OnCommonUpdateTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var computeIndex = instructions.FindIndex(instruction => instruction.Calls(computeBoundingBoxMethod));

            if (computeIndex < 0)
                return instructions;

            instructions.Insert(computeIndex + 1, instructions[computeIndex - 5]);
            instructions.Insert(computeIndex + 2, instructions[computeIndex - 3]);
            instructions.Insert(computeIndex + 3, new CodeInstruction(OpCodes.Call, boundUIXMethod));

            return instructions;
        }

        [HarmonyPostfix]
        [HarmonyPatch("SwitchSpace")]
        private static void SwitchSpacePostfix(TransformRelayRef ____targetSlot, Sync<bool> ____isLocalSpace, SyncRef<Slot> ____buttonsSlot, ref bool __state)
        {
            if (!____targetSlot.Target.TryGetMovableRectTransform(out var rectTransform))
                return;

            // Restore true state to show the different icon
            ____isLocalSpace.Value = __state;
            BoundedUIX.OriginalRects.GetOrCreateValue(rectTransform).Local = __state;
            ____buttonsSlot.Target.FindInChildren("LocalSpaceIcon").ActiveSelf = __state;
            ____buttonsSlot.Target.FindInChildren("GlobalSpaceIcon").ActiveSelf = __state;
        }

        [HarmonyPrefix]
        [HarmonyPatch("SwitchSpace")]
        private static void SwitchSpacePrefix(TransformRelayRef ____targetSlot, Sync<bool> ____isLocalSpace, ref bool __state)
        {
            if (!____targetSlot.Target.TryGetMovableRectTransform(out _))
                return;

            // Always let it set local space for the translation gizmos on rect transforms
            __state = !____isLocalSpace.Value;
            ____isLocalSpace.Value = false;
        }
    }
}