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
        private static readonly MethodInfo checkCanvasHitMethod = typeof(DevToolTipPatches).GetMethod(nameof(CheckCanvas), AccessTools.allDeclared);
        private static readonly FieldInfo colliderField = typeof(RaycastHit).GetField("Collider", AccessTools.allDeclared);

        private static readonly FieldInfo graphicField = typeof(RectTransform).GetField("_graphic", AccessTools.allDeclared);

        private static Slot CheckCanvas(RaycastHit hit)
        {
            var best = hit.Collider.Slot;

            if (best?.GetComponent<Canvas>() != null && best?.GetComponent<RectTransform>() is RectTransform rectTransform)
            {
                best = FindBestRect(hit.Point, best);

                if (best.GetComponentInParents<Button>() is Button button && button.Slot.HierachyDepth > rectTransform.Slot.HierachyDepth)
                    best = button.Slot;
            }

            return best;
        }

        private static Slot FindBestRect(float3 hitPoint, Slot root)
        {
            var traversal = new Stack<Slot>();
            traversal.Push(root);

            Slot best = null;
            while (traversal.Count > 0)
            {
                var current = traversal.Pop();

                if (!current.TryGetRectTransform(out var rectTransform))
                    continue;

                var isHit = rectTransform.GetGlobalBounds().Contains(hitPoint);
                var hasGraphic = graphicField.GetValue(rectTransform) != null;

                if (isHit && hasGraphic && (!rectTransform.IsMask || rectTransform.IsMaskVisible))
                    best = current;

                if (rectTransform.IsMask && (!isHit || !hasGraphic))
                    continue;

                foreach (var child in current.Children.Where(child => child.ActiveSelf).Reverse())
                    traversal.Push(child);
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