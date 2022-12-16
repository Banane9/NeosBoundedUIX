using BaseX;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BoundedUIX
{
    [HarmonyPatch(typeof(DevToolTip))]
    internal static class DevToolTipPatches
    {
        private static MethodInfo checkCanvasHitMethod = typeof(DevToolTipPatches).GetMethod(nameof(CheckCanvas), AccessTools.allDeclared);
        private static FieldInfo colliderField = typeof(RaycastHit).GetField("Collider", AccessTools.allDeclared);
        private static MethodInfo getValueMethod = typeof(RaycastHit?).GetProperty("Value", AccessTools.allDeclared).GetMethod;

        private static Slot CheckCanvas(RaycastHit hit)
        {
            var originalSlot = hit.Collider.Slot;
            var slot = originalSlot;

            if (slot?.GetComponent<Canvas>() is Canvas canvas)
                foreach (var rectTransform in canvas.Slot.GetComponentsInChildren<RectTransform>())
                {
                    var bounds = rectTransform.GetGlobalBounds();
                    var enlargedBounds = bounds;
                    enlargedBounds.Encapsulate(hit.Point);

                    if (MathX.Approximately(bounds.Size, enlargedBounds.Size, .001f) && rectTransform.Slot.HierachyDepth > slot.HierachyDepth)
                        slot = rectTransform.Slot;
                }

            if (slot.GetComponentInParents<Button>() is Button button && button.Slot.HierachyDepth > originalSlot.HierachyDepth)
                slot = button.Slot;

            return slot;
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