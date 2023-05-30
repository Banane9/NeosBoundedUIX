using BaseX;
using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;
using HarmonyLib;

namespace BoundedUIX
{
    [HarmonyPatch(typeof(AxisTranslationGizmo))]
    internal static class AxisTranslationGizmoPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(AxisTranslationGizmo.OnInteractionBegin))]
        private static void OnInteractionBeginPostfix(AxisTranslationGizmo __instance)
        {
            if (!__instance.TargetSlot.Target.TryGetMovableRectTransform(out RectTransform rectTransform))
                return;

            var originalTransform = rectTransform.GetOriginal();
            originalTransform.Update(rectTransform);

            __instance.World.BeginUndoBatch("Undo.TranslateAlongAxis".AsLocaleKey());

            if (!originalTransform.Local)
            {
                rectTransform.OffsetMin.CreateUndoPoint(true);
                rectTransform.OffsetMax.CreateUndoPoint(true);
            }
            else
            {
                rectTransform.AnchorMin.CreateUndoPoint(true);
                rectTransform.AnchorMax.CreateUndoPoint(true);
            }

            __instance.World.EndUndoBatch();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(AxisTranslationGizmo.UpdatePoint))]
        private static bool UpdatePointPrefix(AxisTranslationGizmo __instance, float3 localPoint)
        {
            var targetSlot = __instance.TargetSlot.Target;
            if (!targetSlot.TryGetMovableRectTransform(out var rectTransform))
                return true;

            var offsetPoint = localPoint - __instance._pointOffset;
            var projectedPoint = MathX.Project(offsetPoint, __instance.LocalAxis);
            projectedPoint = __instance.Slot.LocalPointToGlobal(projectedPoint);
            projectedPoint = __instance.PointSpace.Space.GlobalPointToLocal(projectedPoint);
            var originalRect = rectTransform.GetOriginal();
            var translationOffset = (projectedPoint - __instance.PointSpace.Space.GlobalPointToLocal(originalRect.Center)).xy;

            if (__instance.TargetValue.Target != null)
            {
                __instance.TargetValue.Target.Value = translationOffset.Magnitude;
            }

            var pxOffset = rectTransform.Canvas.UnitScale.Value * translationOffset;
            if (!originalRect.Local)
            {
                if (rectTransform.OffsetMin.CanSet())
                    rectTransform.OffsetMin.Value += pxOffset;

                if (rectTransform.OffsetMax.CanSet())
                    rectTransform.OffsetMax.Value += pxOffset;
            }
            else
            {
                var anchorOffset = pxOffset / rectTransform.RectParent.ComputeGlobalComputeRect().size;

                if (rectTransform.AnchorMin.CanSet())
                    rectTransform.AnchorMin.Value += anchorOffset;

                if (rectTransform.AnchorMax.CanSet())
                    rectTransform.AnchorMax.Value += anchorOffset;
            }

            var line = MathX.Reject(localPoint, __instance.LocalAxis);
            __instance._line0.Target.PointB.Value = line;
            __instance._line1.Target.PointA.Value = line;
            __instance._line1.Target.PointB.Value = float3.Zero;

            return false;
        }
    }
}