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
        private static MethodInfo checkCanvasHitMethod = typeof(DevToolTipPatches).GetMethod(nameof(CheckCanvas), AccessTools.allDeclared);
        private static FieldInfo colliderField = typeof(RaycastHit).GetField("Collider", AccessTools.allDeclared);

        private static Slot CheckCanvas(RaycastHit hit)
        {
            var best = hit.Collider.Slot;

            if (best?.GetComponent<Canvas>() != null && best?.GetComponent<RectTransform>() is RectTransform rectTransform)
            {
                best = FindBestFittingRect(hit.Point, best, rectTransform.GetGlobalBounds(), out _);

                if (best.GetComponentInParents<Button>() is Button button && button.Slot.HierachyDepth > rectTransform.Slot.HierachyDepth)
                    best = button.Slot;
            }

            return best;
        }

        private static Slot FindBestFittingRect(float3 hitPoint, Slot currentRoot, BoundingBox currentBounds, out RectTransform bestRect)
        {
            var best = currentRoot;
            currentRoot.TryGetRectTransform(out bestRect);
            var bestSize = bestRect.LocalComputeRect.size.GetArea();

            foreach (var child in currentRoot.Children)
            {
                if (!child.TryGetRectTransform(out RectTransform rectTransform))
                    continue;

                var bounds = rectTransform.GetGlobalBounds();
                if (!bounds.Contains(hitPoint))
                    continue;

                var foundBest = FindBestFittingRect(hitPoint, child, child.GetComponent<Mask>() != null ? rectTransform.GetGlobalBounds() : currentBounds, out var foundBestRect);
                var foundBestSize = foundBestRect.LocalComputeRect.size.GetArea();

                if (foundBestSize < bestSize || foundBest.HierachyDepth > best.HierachyDepth)
                {
                    best = foundBest;
                    bestRect = foundBestRect;
                    bestSize = foundBestSize;
                }
            }

            return best;
        }

        [HarmonyTranspiler]
        [HarmonyPatch("TryOpenGizmo")]
        private static IEnumerable<CodeInstruction> TryOpenGizmoTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();
            var raycastValueIndex = instructions.FindIndex(instruction => instruction.LoadsField(colliderField));

            instructions.RemoveAt(raycastValueIndex);
            instructions[raycastValueIndex] = new CodeInstruction(OpCodes.Call, checkCanvasHitMethod);

            return instructions;
        }
    }
}