using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using CodeX;
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
        internal static readonly ConditionalWeakTable<RectTransform, OriginalRect> OriginalRects = new ConditionalWeakTable<RectTransform, OriginalRect>();

        public override string Author => "Banane9";
        public override string Link => "https://github.com/Banane9/NeosBoundedUIX";
        public override string Name => "BoundedUIX";
        public override string Version => "1.0.0";

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"{Author}.{Name}");
            harmony.PatchAll();
        }
    }
}