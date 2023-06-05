using BaseX;
using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BoundedUIX
{
    internal static class Helpers
    {
        private static readonly ConditionalWeakTable<RectTransform, OriginalRect> originalRects = new();

        public static bool Contains(this BoundingBox2D boundingBox, float2 point)
            => (point >= boundingBox.Min).All() && (point <= boundingBox.Max).All();

        public static BoundingBox2D GetCanvasBounds(this RectTransform rectTransform)
        {
            var area = rectTransform.ComputeGlobalComputeRect();

            var bounds = BoundingBox2D.Empty();
            bounds.Encapsulate(area.ExtentMin / rectTransform.Canvas.UnitScale);
            bounds.Encapsulate(area.ExtentMax / rectTransform.Canvas.UnitScale);

            return bounds;
        }

        public static BoundingBox GetGlobalBounds(this RectTransform rectTransform)
        {
            var area = rectTransform.ComputeGlobalComputeRect();

            var bounds = BoundingBox.Empty();
            bounds.Encapsulate(rectTransform.Canvas.Slot.LocalPointToGlobal(area.ExtentMin / rectTransform.Canvas.UnitScale));
            bounds.Encapsulate(rectTransform.Canvas.Slot.LocalPointToGlobal(area.ExtentMax / rectTransform.Canvas.UnitScale));

            return bounds;
        }

        public static OriginalRect GetOriginal(this RectTransform rectTransform)
                    => originalRects.GetOrCreateValue(rectTransform);

        public static void ResetTransform(this RectTransform rectTransform)
        {
            rectTransform.AnchorMin.Value = float2.Zero;
            rectTransform.AnchorMax.Value = float2.One;
            rectTransform.OffsetMin.Value = float2.Zero;
            rectTransform.OffsetMax.Value = float2.Zero;
            rectTransform.Pivot.Value = new(.5f, .5f);
        }

        public static bool TryGetMovableRectTransform(this Slot slot, out RectTransform rectTransform)
            => slot.TryGetRectTransform(out rectTransform) && rectTransform.Slot != rectTransform.Canvas.Slot;

        public static bool TryGetRectTransform(this Slot slot, out RectTransform rectTransform)
        {
            if (slot?.GetComponent<RectTransform>() is RectTransform rt && rt.Canvas != null)
            {
                rectTransform = rt;
                return true;
            }

            rectTransform = null;
            return false;
        }
    }
}