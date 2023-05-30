using FrooxEngine.UIX;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseX;

namespace BoundedUIX
{
    [HarmonyPatch(typeof(RectTransform))]
    internal static class RectTransformPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(RectTransform.BuildInspectorUI))]
        private static void BuildInspectorUIPostfix(RectTransform __instance, UIBuilder ui)
        {
            var button = ui.Button("Visualize Preferred Area");
            var valueField = button.Slot.AttachComponent<ValueField<bool>>().Value;

            var toggle = button.Slot.AttachComponent<ButtonToggle>();
            toggle.TargetValue.Target = valueField;

            valueField.OnValueChange += field =>
            {
                button.Enabled = false;

                __instance.StartTask(async () =>
                {
                    while (!button.IsRemoved && !__instance.IsRemoved && (!__instance?.Canvas.IsRemoved ?? false))
                    {
                        var horizontal = __instance.GetHorizontalMetrics().preferred;
                        var vertical = __instance.GetVerticalMetrics().preferred;
                        var area = __instance.ComputeGlobalComputeRect();

                        var pos = __instance.Canvas.Slot.LocalPointToGlobal(new float3(area.Center / __instance.Canvas.UnitScale));
                        var size = __instance.Canvas.Slot.LocalScaleToGlobal(new float3(horizontal, vertical) / __instance.Canvas.UnitScale);

                        __instance.World.Debug.Box(pos, size, color.Blue.SetA(0.25f), __instance.Canvas.Slot.GlobalRotation);

                        await default(NextUpdate);
                    }
                });
            };
        }
    }
}