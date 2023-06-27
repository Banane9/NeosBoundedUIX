using BaseX;
using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BoundedUIX
{
    internal static class SlotAddingPatches
    {
        private static IEnumerable<CodeInstruction> PostfixToAddSlot(this IEnumerable<CodeInstruction> codeInstructions, IEnumerable<CodeInstruction> targetSlotLoadInstructions, AddSlotPostfix postfix)
        {
            var addSlotMethod = typeof(Slot).GetMethod(nameof(Slot.AddSlot));

            foreach (var code in codeInstructions)
            {
                yield return code;

                if (code.Calls(addSlotMethod))
                {
                    foreach (var loadInstruction in targetSlotLoadInstructions)
                        yield return loadInstruction;

                    yield return new CodeInstruction(OpCodes.Call, postfix.Method);
                }
            }
        }

        private delegate Slot AddSlotPostfix(Slot newSlot, Slot targetSlot);

        [HarmonyPatch(typeof(SceneInspector))]
        private static class SceneInspectorPatches
        {
            private static IEnumerable<CodeInstruction> LoadFromSceneInspector
            {
                get
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, typeof(SceneInspector).GetField(nameof(SceneInspector.ComponentView), AccessTools.all));
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(SyncRef<Slot>).GetProperty(nameof(SyncRef<Slot>.Target)).GetMethod);
                }
            }

            private static Slot OnAddChildPostfix(Slot newSlot, Slot targetSlot)
            {
                if (targetSlot.TryGetRectTransform(out _))
                {
                    newSlot.Name = BoundedUIX.ChildSlotName.Replace(BoundedUIX.TargetSlotNamePlaceholder, targetSlot.Name);
                    newSlot.AttachComponent<RectTransform>();
                }

                return newSlot;
            }

            [HarmonyTranspiler]
            [HarmonyPatch(nameof(SceneInspector.OnAddChildPressed))]
            private static IEnumerable<CodeInstruction> OnAddChildPressedTranspiler(IEnumerable<CodeInstruction> codeInstructions)
            {
                return codeInstructions.PostfixToAddSlot(LoadFromSceneInspector, OnAddChildPostfix);
            }

            private static Slot OnInsertParentPostfix(Slot newSlot, Slot targetSlot)
            {
                if (targetSlot.TryGetMovableRectTransform(out var originalTransform))
                {
                    newSlot.Name = BoundedUIX.ParentSlotName.Replace(BoundedUIX.TargetSlotNamePlaceholder, targetSlot.Name);
                    var newTransform = newSlot.AttachComponent<RectTransform>();

                    if (BoundedUIX.MoveTransformToParent)
                    {
                        newTransform.CopyValues(originalTransform);
                        originalTransform.ResetTransform();
                    }
                }

                return newSlot;
            }

            [HarmonyTranspiler]
            [HarmonyPatch(nameof(SceneInspector.OnInsertParentPressed))]
            private static IEnumerable<CodeInstruction> OnInsertParentPressedTranspiler(IEnumerable<CodeInstruction> codeInstructions)
            {
                return codeInstructions.PostfixToAddSlot(LoadFromSceneInspector, OnInsertParentPostfix);
            }
        }

        [HarmonyPatch(typeof(SlotPositioning), nameof(SlotPositioning.CreatePivotAtCenter), new[] { typeof(Slot), typeof(BoundingBox), typeof(bool) }, new[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        private static class SlotPositioningCreatePivotAtCenterPatch
        {
            private static Slot CreatePivotAddPostfix(Slot newSlot, Slot targetSlot)
            {
                if (targetSlot.TryGetMovableRectTransform(out var originalTransform))
                {
                    newSlot.Name = BoundedUIX.PivotSlotName.Replace(BoundedUIX.TargetSlotNamePlaceholder, targetSlot.Name);
                    newSlot.AttachComponent<RectTransform>();
                }

                return newSlot;
            }

            private static void Postfix(Slot slot, ref Slot __result)
            {
                // Can't be root slot when this gives true
                if (!slot.TryGetMovableRectTransform(out var originalTransform)
                 || !__result.TryGetMovableRectTransform(out var pivotTransform))
                    return;

                if (slot == __result)
                {
                    __result = slot.Parent.AddSlot(BoundedUIX.PivotSlotName.Replace(BoundedUIX.TargetSlotNamePlaceholder, slot.Name));
                    pivotTransform = __result.AttachComponent<RectTransform>();

                    slot.SetParent(__result);
                }

                var originalArea = originalTransform.ComputeGlobalComputeRect();
                var parentArea = originalTransform.RectParent.ComputeGlobalComputeRect();

                var pivotAnchor = (originalArea.Center - parentArea.ExtentMin) / parentArea.size;
                var pivotOffset = originalArea.size / 2f;

                pivotTransform.AnchorMin.Value = pivotAnchor;
                pivotTransform.AnchorMax.Value = pivotAnchor;
                pivotTransform.OffsetMin.Value = -pivotOffset;
                pivotTransform.OffsetMax.Value = pivotOffset;

                originalTransform.ResetTransform();
            }

            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
            {
                return codeInstructions.PostfixToAddSlot(new[] { new CodeInstruction(OpCodes.Ldarg_0) }, CreatePivotAddPostfix);
            }
        }
    }
}