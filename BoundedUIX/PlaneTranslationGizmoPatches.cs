using BaseX;
using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;
using HarmonyLib;
using System.Runtime.CompilerServices;

namespace BoundedUIX
{
    [HarmonyPatch(typeof(PlaneTranslationGizmo))]
    internal static class PlaneTranslationGizmoPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnInteractionBegin")]
        private static void OnInteractionBeginPostfix(PlaneTranslationGizmo __instance)
        {
            if (!__instance.TargetSlot.Target.TryGetMovableRectTransform(out var rectTransform))
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
        [HarmonyPatch("UpdatePoint")]
        private static bool UpdatePointPrefix(PlaneTranslationGizmo __instance, float3 localPoint, float3 ____pointOffset, SyncRef<SegmentMesh> ____line0, SyncRef<SegmentMesh> ____line1)
        {
            var targetSlot = __instance.TargetSlot.Target;
            if (!targetSlot.TryGetMovableRectTransform(out var rectTransform))
                return true;

            var offsetPoint = localPoint - ____pointOffset;
            var projectedPoint = MathX.Reject(offsetPoint, __instance.LocalNormal);
            projectedPoint = __instance.Slot.LocalPointToGlobal(projectedPoint);
            projectedPoint = __instance.PointSpace.Space.GlobalPointToLocal(projectedPoint);
            var originalRect = rectTransform.GetOriginal();
            var translationOffset = (projectedPoint - __instance.PointSpace.Space.GlobalPointToLocal(originalRect.Center)).xy;

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

            var line = MathX.Project(localPoint, __instance.LocalNormal);
            ____line0.Target.PointB.Value = line;
            ____line1.Target.PointA.Value = line;
            ____line1.Target.PointB.Value = float3.Zero;

            return false;
        }
    }
}