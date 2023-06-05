using BaseX;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BoundedUIX
{
    [HarmonyPatch(typeof(DevToolTip))]
    internal static class DevToolTipPatches
    {
        private static Slot CheckCanvas(RaycastHit hit)
        {
            var bestSlot = hit.Collider.Slot;

            if (BoundedUIX.EnableUIXSelection && bestSlot.TryGetRectTransform(out var rectTransform) && rectTransform.Canvas.Slot == bestSlot)
            {
                bestSlot = FindBestRect(bestSlot.GlobalPointToLocal(hit.Point).xy, bestSlot);

                if (bestSlot.GetComponentInParents<Button>() is Button button && button.Slot.HierachyDepth > rectTransform.Slot.HierachyDepth)
                    bestSlot = button.Slot;
            }

            return bestSlot;
        }

        private static Slot FindBestRect(float2 hitPoint, Slot best)
        {
            var prioritizeDepth = BoundedUIX.PrioritizeHierarchyDepth;
            var ignoreSelected = BoundedUIX.IgnoreAlreadySelected;

            var traversal = new Stack<Slot>();
            traversal.Push(best);

            while (traversal.Count > 0)
            {
                var current = traversal.Pop();

                if (!current.TryGetRectTransform(out var rectTransform))
                    continue;

                var isHit = rectTransform.GetCanvasBounds().Contains(hitPoint);
                var hasGraphic = rectTransform.Graphic != null;

                if (isHit && hasGraphic && (!rectTransform.IsMask || rectTransform.IsMaskVisible)   // Has anything possibly visible in the bounds
                 && (!ignoreSelected || current.TryGetGizmo<SlotGizmo>() == null)                   // Not already selected - to help when something's in the way
                 && (!prioritizeDepth || best.HierachyDepth <= current.HierachyDepth))              // Hierarchy depth at least as deep when prioritizing
                    best = current;

                if (rectTransform.IsMask && (!isHit || !hasGraphic))
                    continue;

                foreach (var child in current.Children.Where(child => child.ActiveSelf).Reverse())
                    traversal.Push(child);
            }

            return best;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(DevToolTip.TryOpenGizmo))]
        private static IEnumerable<CodeInstruction> TryOpenGizmoTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var checkCanvasHitMethod = typeof(DevToolTipPatches).GetMethod(nameof(CheckCanvas), AccessTools.allDeclared);
            var colliderField = typeof(RaycastHit).GetField(nameof(RaycastHit.Collider), AccessTools.allDeclared);

            var instructions = codeInstructions.ToList();
            var raycastValueIndex = instructions.FindIndex(instruction => instruction.LoadsField(colliderField));

            instructions.RemoveAt(raycastValueIndex);
            instructions[raycastValueIndex] = new CodeInstruction(OpCodes.Call, checkCanvasHitMethod);

            return instructions;
        }
    }
}