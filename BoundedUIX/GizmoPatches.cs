using BaseX;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoundedUIX
{
    [HarmonyPatch(typeof(Gizmo))]
    internal static class GizmoPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Gizmo.PositionAtTarget))]
        private static bool PositionAtTargetPrefix(Gizmo __instance)
        {
            if (!__instance.TargetSlot.Target.TryGetMovableRectTransform(out var rectTransform))
                return true;

            var center = rectTransform.GetGlobalBounds().Center;
            rectTransform.GetOriginal().Center = center;

            __instance.Slot.GlobalPosition = center - BoundedUIX.GizmoOffset * rectTransform.Canvas.Slot.Forward;
            __instance.Slot.GlobalRotation = rectTransform.Canvas.Slot.GlobalRotation;

            return false;
        }
    }
}