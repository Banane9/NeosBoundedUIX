using System;
using System.Collections;
using System.Text;
using HarmonyLib;
using NeosModLoader;

namespace BoundedUIX
{
    public class BoundedUIX : NeosMod
    {
        public static ModConfiguration Config;

        internal const string TargetSlotNamePlaceholder = "{TargetName}";

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<string> childSlotNameKey = new("ChildSlotName", "Default name for child Slots in UIX hierarchies. Use {TargetName} to get the parent's name.", () => "Panel", valueValidator: name => !string.IsNullOrWhiteSpace(name));

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> enableUIXGizmosKey = new("EnableUIXGizmos", "Enable modifying Slot Gizmo behavior in UIX hierarchies to move the elements instead.", () => true);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> enableUIXSelectionKey = new("EnableUIXSelection", "Enable selecting elements in UIX hierarchies directly with the Developer ToolTip.", () => true);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<float> gizmoOffsetKey = new("GizmoOffset", "Distance to raise the movement gizmos from the surface of the canvas. World scale.", () => 0.02f, valueValidator: v => v >= 0);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> moveTransformToParentKey = new("MoveTransformToParent", "Move RectTransform values up when creating a parent (analog to Slot transforms).", () => true);

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<string> parentSlotNameKey = new("ParentSlotName", "Default name for parent Slots in UIX hierarchies. Use {TargetName} to get the child-to-be's name.", () => "{TargetName} Space", valueValidator: name => !string.IsNullOrWhiteSpace(name));

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<string> pivotSlotNameKey = new("PivotSlotName", "Default name for pivot Slots in UIX hierarchies. Use {TargetName} to get the child-to-be's name.", () => "{TargetName} Space", valueValidator: name => !string.IsNullOrWhiteSpace(name));

        public static string ChildSlotName => Config.GetValue(childSlotNameKey);
        public static bool EnableUIXGizmos => Config.GetValue(enableUIXGizmosKey);
        public static bool EnableUIXSelection => Config.GetValue(enableUIXSelectionKey);
        public static float GizmoOffset => Config.GetValue(gizmoOffsetKey);
        public static bool MoveTransformToParent => Config.GetValue(moveTransformToParentKey);
        public static string ParentSlotName => Config.GetValue(parentSlotNameKey);
        public static string PivotSlotName => Config.GetValue(pivotSlotNameKey);

        public override string Author => "Banane9";
        public override string Link => "https://github.com/Banane9/NeosBoundedUIX";
        public override string Name => "BoundedUIX";
        public override string Version => "2.2.2";

        public override void OnEngineInit()
        {
            var harmony = new Harmony($"{Author}.{Name}");
            Config = GetConfiguration();
            Config.Save(true);
            harmony.PatchAll();
        }
    }
}