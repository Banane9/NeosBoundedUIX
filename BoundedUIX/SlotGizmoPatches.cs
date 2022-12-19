using System;
using System.Collections.Generic;
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

        private static readonly MethodInfo getGlobalPositionMethod = typeof(Slot).GetProperty(nameof(Slot.GlobalPosition), AccessTools.allDeclared).GetMethod;

        private static readonly Type RotationGizmoType = typeof(RotationGizmo);
        private static readonly MethodInfo uixBoundCenterMethod = typeof(SlotGizmoPatches).GetMethod(nameof(UIXBoundCenter), AccessTools.allDeclared);

        private static BoundingBox BoundUIX(BoundingBox bounds, Slot target, Slot space)
        {
            if (!target.TryGetMovableRectTransform(out var rectTransform))
                return bounds;

            var area = rectTransform.ComputeGlobalComputeRect();
            bounds.Encapsulate(space.GlobalPointToLocal(rectTransform.Canvas.Slot.LocalPointToGlobal(area.ExtentMin / rectTransform.Canvas.UnitScale)));
            bounds.Encapsulate(space.GlobalPointToLocal(rectTransform.Canvas.Slot.LocalPointToGlobal(area.ExtentMax / rectTransform.Canvas.UnitScale)));

            return bounds;
        }

        [HarmonyTranspiler]
        [HarmonyPatch("OnCommonUpdate")]
        private static IEnumerable<CodeInstruction> OnCommonUpdateTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var globalPositionIndex = instructions.FindIndex(instruction => instruction.Calls(getGlobalPositionMethod));

            if (globalPositionIndex < 0)
                return instructions;

            instructions[globalPositionIndex] = new CodeInstruction(OpCodes.Call, uixBoundCenterMethod);

            var computeIndex = instructions.FindIndex(globalPositionIndex, instruction => instruction.Calls(computeBoundingBoxMethod));

            if (computeIndex < 0)
                return instructions;

            instructions.Insert(computeIndex + 1, instructions[computeIndex - 5]);
            instructions.Insert(computeIndex + 2, instructions[computeIndex - 3]);
            instructions.Insert(computeIndex + 3, new CodeInstruction(OpCodes.Call, boundUIXMethod));

            return instructions;
        }

        [HarmonyPostfix]
        [HarmonyPatch("RegenerateButtons")]
        private static void RegenerateButtonsPostfix(SlotGizmo __instance, SyncRef<Slot> ____buttonsSlot)
        {
            var moveableRect = __instance.TargetSlot.TryGetMovableRectTransform(out _);

            if (____buttonsSlot.Target.GetComponentInChildren<SlotGizmoButton>(button => ((SyncRef<Worker>)button.TryGetField("_worker")).Target?.GetType() == RotationGizmoType) is SlotGizmoButton sgb)
                sgb.Slot.ActiveSelf = !moveableRect;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Setup")]
        private static void SetupPostfix(TransformRelayRef ____targetSlot, Sync<bool> ____isLocalSpace, SyncRef<ScaleGizmo> ____scaleGizmo)
        {
            var moveableRect = ____targetSlot.Target.TryGetMovableRectTransform(out var rectTransform);

            if (moveableRect)
                rectTransform.GetOriginal().Local = ____isLocalSpace.Value;

            if (____scaleGizmo.Target.TryGetField("_zSlot") is SyncRef<Slot> zSlot)
                zSlot.Target.ActiveSelf = !moveableRect;

            // Hide blue z line of the gizmo
            if (____scaleGizmo.Target.Slot.GetComponent<MeshRenderer>(r => r.Materials[0] is OverlayFresnelMaterial material && material.FrontNearColor == color.Blue) is MeshRenderer renderer)
                renderer.Enabled = !moveableRect;
        }

        [HarmonyPostfix]
        [HarmonyPatch("SwitchSpace")]
        private static void SwitchSpacePostfix(TransformRelayRef ____targetSlot, Sync<bool> ____isLocalSpace, SyncRef<Slot> ____buttonsSlot, ref bool __state)
        {
            if (!____targetSlot.Target.TryGetMovableRectTransform(out var rectTransform))
                return;

            // Restore true state to show the different icon
            ____isLocalSpace.Value = __state;
            rectTransform.GetOriginal().Local = __state;
            ____buttonsSlot.Target.FindInChildren("LocalSpaceIcon").ActiveSelf = __state;
            ____buttonsSlot.Target.FindInChildren("GlobalSpaceIcon").ActiveSelf = !__state;
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

        private static float3 UIXBoundCenter(Slot target)
        {
            if (!target.TryGetMovableRectTransform(out var rectTransform))
                return target.GlobalPosition;

            return rectTransform.GetGlobalBounds().Center - BoundedUIX.GizmoOffset * rectTransform.Canvas.Slot.Forward;
        }
    }
}