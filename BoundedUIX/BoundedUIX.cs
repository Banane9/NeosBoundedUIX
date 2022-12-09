using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
        public override string Author => "Banane9";
        public override string Link => "https://github.com/Banane9/NeosBoundedUIX";
        public override string Name => "BoundedUIX";
        public override string Version => "1.0.0";

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"{Author}.{Name}");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(SlotGizmo))]
        private static class SlotGizmoPatches
        {
            private static readonly MethodInfo boundUIXMethod = typeof(SlotGizmoPatches).GetMethod(nameof(BoundUIX), AccessTools.allDeclared);

            private static readonly MethodInfo computeBoundingBoxMethod = typeof(BoundsHelper).GetMethod("ComputeBoundingBox", AccessTools.allDeclared);

            private static BoundingBox BoundUIX(BoundingBox bounds, Slot target, Slot space)
            {
                if (!(target.GetComponent<RectTransform>() is RectTransform rect))
                    return bounds;

                var area = rect.ComputeGlobalComputeRect();
                bounds.Encapsulate(space.GlobalPointToLocal(rect.Canvas.Slot.LocalPointToGlobal(area.ExtentMin)));
                bounds.Encapsulate(space.GlobalPointToLocal(rect.Canvas.Slot.LocalPointToGlobal(area.ExtentMax)));

                return bounds;
            }

            [HarmonyTranspiler]
            [HarmonyPatch("OnCommonUpdate")]
            private static IEnumerable<CodeInstruction> OnCommonUpdateTranspiler(IEnumerable<CodeInstruction> codeInstructions)
            {
                var instructions = codeInstructions.ToList();

                var computeIndex = instructions.FindIndex(instruction => instruction.Calls(computeBoundingBoxMethod));

                if (computeIndex < 0)
                    return instructions;

                instructions.Insert(computeIndex + 1, instructions[computeIndex - 5]);
                instructions.Insert(computeIndex + 2, instructions[computeIndex - 3]);
                instructions.Insert(computeIndex + 3, new CodeInstruction(OpCodes.Call, boundUIXMethod));

                return instructions;
            }
        }
    }
}