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
        private static readonly ConditionalWeakTable<RectTransform, OriginalRect> originalRects = new ConditionalWeakTable<RectTransform, OriginalRect>();

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

        public static bool TryGetMovableRectTransform(this Slot slot, out RectTransform rectTransform)
        {
            if (slot?.GetComponent<RectTransform>() is RectTransform rt && rt.Canvas != null && rt.Slot != rt.Canvas.Slot)
            {
                rectTransform = rt;
                return true;
            }

            rectTransform = null;
            return false;
        }
    }
}