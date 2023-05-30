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
    [HarmonyPatch(typeof(TranslationGizmo))]
    internal static class TranslationGizmoPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(TranslationGizmo.SetTarget))]
        private static void SetTargetPostfix(TranslationGizmo __instance, Slot slot)
        {
            var moveableRect = slot.TryGetMovableRectTransform(out _);

            foreach (var child in __instance.Slot.Children)
                child.ActiveSelf = !moveableRect || !child.Name.Contains("Z");
        }
    }
}