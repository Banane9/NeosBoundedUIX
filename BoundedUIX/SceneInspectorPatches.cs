using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoundedUIX
{
    [HarmonyPatch(typeof(SceneInspector))]
    internal static class SceneInspectorPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnAddChildPressed")]
        private static bool OnAddChildPressedPrefix(SceneInspector __instance)
        {
            if (!(__instance.ComponentView.Target is Slot target))
                return false;

            var slot = target.AddSlot(target.Name + " - Child", true);

            if (target.TryGetRectTransform(out _))
            {
                slot.Name = "Panel";
                slot.AttachComponent<RectTransform>();
            }

            slot.CreateSpawnUndoPoint(null);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnInsertParentPressed")]
        private static bool OnInsertParentPressedPrefix(SceneInspector __instance)
        {
            if (!(__instance.ComponentView.Target is Slot target))
                return false;

            __instance.World.BeginUndoBatch(__instance.GetLocalized("Undo.InsertParent", null, "name", target.Name));

            var parent = target.Parent.AddSlot(target.Name + " - Parent", true);
            parent.CopyTransform(target);
            parent.CreateSpawnUndoPoint(null);

            if (target.TryGetMovableRectTransform(out _))
            {
                parent.Name = "Panel";
                parent.AttachComponent<RectTransform>();
            }

            target.CreateTransformUndoState(true, true, true, true);
            target.SetParent(parent, true);
            target.SetIdentityTransform();

            __instance.World.EndUndoBatch();

            return false;
        }
    }
}