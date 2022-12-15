using BaseX;
using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;
using HarmonyLib;

namespace BoundedUIX
{
    [HarmonyPatch(typeof(PlaneTranslationGizmo))]
    internal static class PlaneTranslationGizmoPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnInteractionBegin")]
        private static void OnInteractionBeginPostfix(PlaneTranslationGizmo __instance)
        {
            if (!__instance.TargetSlot.Target.TryGetMovableRectTransform(out RectTransform rectTransform))
                return;

            var originalTransform = rectTransform.GetOriginal();
            originalTransform.Update(rectTransform);

            if (originalTransform.Local)
            {
                rectTransform.OffsetMin.CreateUndoPoint(true);
                rectTransform.OffsetMax.CreateUndoPoint(true);
            }
            else
            {
                rectTransform.AnchorMin.CreateUndoPoint(true);
                rectTransform.AnchorMax.CreateUndoPoint(true);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("UpdatePoint")]
        private static void UpdatePointPostfix(PlaneTranslationGizmo __instance)
        {
            var targetSlot = __instance.TargetSlot.Target;
            if (!targetSlot.TryGetMovableRectTransform(out var rectTransform))
                return;

            var originalRect = rectTransform.GetOriginal();
            var translationOffset = (targetSlot.LocalPosition - originalRect.Position).xy;

            var pxOffset = rectTransform.Canvas.UnitScale.Value * translationOffset;
            if (originalRect.Local)
            {
                if (rectTransform.OffsetMin.CanSet())
                    rectTransform.OffsetMin.Value = originalRect.OffsetMin + pxOffset;

                if (rectTransform.OffsetMax.CanSet())
                    rectTransform.OffsetMax.Value = originalRect.OffsetMax + pxOffset;
            }
            else
            {
                var anchorOffset = pxOffset / rectTransform.RectParent.ComputeGlobalComputeRect().size;

                if (rectTransform.AnchorMin.CanSet())
                    rectTransform.AnchorMin.Value = originalRect.AnchorMin + anchorOffset;

                if (rectTransform.AnchorMax.CanSet())
                    rectTransform.AnchorMax.Value = originalRect.AnchorMax + anchorOffset;
            }

            // Reset slot position
            targetSlot.LocalPosition = originalRect.Position;
        }
    }
}