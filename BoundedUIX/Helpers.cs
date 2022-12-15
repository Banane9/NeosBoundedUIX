using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoundedUIX
{
    internal static class Helpers
    {
        public static bool IsWritable<T>(this IField<T> field)
        {
            return field != null && (!field.IsDriven || field.IsHooked);
        }

        public static bool TryGetMovableRectTransform(this Slot slot, out RectTransform rectTransform)
        {
            if (slot.GetComponent<RectTransform>() is RectTransform rt && slot.GetComponent<Canvas>() == null)
            {
                rectTransform = rt;
                return true;
            }

            rectTransform = null;
            return false;
        }
    }
}