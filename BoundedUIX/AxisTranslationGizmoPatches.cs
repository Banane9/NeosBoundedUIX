using BaseX;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System.Runtime.CompilerServices;

namespace BoundedUIX
{
    [HarmonyPatch(typeof(AxisTranslationGizmo))]
    internal static class AxisTranslationGizmoPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnInteractionBegin")]
        private static void OnInteractionBeginPrefix(AxisTranslationGizmo __instance)
        {
            if (!__instance.TargetSlot.Target.TryGetMovableRectTransform(out RectTransform rectTransform))
                return;

            var originalRect = BoundedUIX.OriginalRects.GetOrCreateValue(rectTransform);
            originalRect.Update(rectTransform);
            BoundedUIX.Msg($"Set Original Rect Offsets: {originalRect.OffsetMin} and {originalRect.OffsetMax}");
        }

        [HarmonyPrefix]
        [HarmonyPatch("UpdatePoint")]
        private static bool UpdatePointPrefix(AxisTranslationGizmo __instance, float3 localPoint, float3 direction, float3 ____pointOffset, SyncRef<SegmentMesh> ____line0, SyncRef<SegmentMesh> ____line1)
        {
            var targetSlot = __instance.TargetSlot.Target;
            if (!targetSlot.TryGetMovableRectTransform(out var rectTransform))
                return true;

            var originalRect = BoundedUIX.OriginalRects.GetOrCreateValue(rectTransform);

            var newTarget = localPoint - ____pointOffset;
            var projectedTarget = MathX.Project(newTarget, __instance.LocalAxis).xy;
            var translatedTarget = __instance.PointSpace.Space.GlobalPointToLocal(__instance.Slot.LocalPointToGlobal(projectedTarget)).xy;
            var translationOffset = translatedTarget - originalRect.Position;

            //Msg($"Translating Slot {__instance.TargetSlot.Target.Name} in direction: {direction}.");
            //Msg($"Moved from {____pointOffset} to {localPoint}, making for {newTarget} or {projectedTarget} on the {__instance.LocalAxis} axis.");
            //Msg($"This translates to {translatedTarget} on the actual slot.");

            //BoundedUIX.Msg($"Moving from Original Rect Offsets: {originalRect.OffsetMin} and {originalRect.OffsetMax}");
            //BoundedUIX.Msg($"Move by: {translatedTarget}");

            if (originalRect.Local)
            {
                if (rectTransform.OffsetMin.IsWritable())
                    rectTransform.OffsetMin.Value = originalRect.OffsetMin + translationOffset;

                if (rectTransform.OffsetMax.IsWritable())
                    rectTransform.OffsetMax.Value = originalRect.OffsetMax + translationOffset;
            }
            else
            {
                if (rectTransform.AnchorMin.IsWritable())
                    rectTransform.AnchorMin.Value = originalRect.AnchorMin + translationOffset;

                if (rectTransform.AnchorMax.IsWritable())
                    rectTransform.AnchorMax.Value = originalRect.AnchorMax + translationOffset;
            }

            // Update indicator code from original method
            if (__instance.TargetValue.Target != null)
                __instance.TargetValue.Target.Value = translatedTarget.Magnitude;

            var float2 = MathX.Reject(localPoint, __instance.LocalAxis);
            ____line0.Target.PointB.Value = float2;
            ____line1.Target.PointA.Value = float2;
            ____line1.Target.PointB.Value = float3.Zero;

            return false;
        }
    }
}