using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BaseX;
using CodeX;
using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.LogiX.Data;
using FrooxEngine.LogiX.ProgramFlow;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;

namespace BoundedUIX
{
    public class BoundedUIX : NeosMod
    {
        public static ModConfiguration Config;

        //[AutoRegisterConfigKey]
        //private static ModConfigurationKey<bool> EnableLinkedVariablesList = new ModConfigurationKey<bool>("EnableLinkedVariablesList", "Allow generating a list of dynamic variable definitions for a space.", () => true);

        //[AutoRegisterConfigKey]
        //private static ModConfigurationKey<bool> EnableVariableHierarchy = new ModConfigurationKey<bool>("EnableVariableHierarchy", "Allow generating a hierarchy of dynamic variable components for a space.", () => true);

        public override string Author => "Banane9";
        public override string Link => "https://github.com/Banane9/NeosBoundedUIX";
        public override string Name => "BoundedUIX";
        public override string Version => "1.0.0";

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"{Author}.{Name}");
            //Config = GetConfiguration();
            //Config.Save(true);
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(BoundsHelper))]
        private static class BoundsHelperPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("EncapsulateBoundingBox")]
            private static void EncapsulateBoundingBoxPostfix(Slot slot, ref BoundingBox bounds, bool includeInactive, Slot space, Predicate<Slot> searchBlock)
            {
                if ((!includeInactive && !slot.IsActive)
                 || (searchBlock != null && !searchBlock(slot))
                 || !(slot.GetComponent<RectTransform>() is RectTransform rect))
                    return;

                var offset = rect.Canvas.Size.Value / 2f;
                var min = slot.LocalPointToGlobal(rect.LocalComputeRect.ExtentMin - offset);
                var max = slot.LocalPointToGlobal(rect.LocalComputeRect.ExtentMax - offset);

                if (space != null)
                {
                    min = space.GlobalPointToLocal(min);
                    max = space.GlobalPointToLocal(max);
                }

                bounds.Encapsulate(min);
                bounds.Encapsulate(max);
            }
        }
    }
}