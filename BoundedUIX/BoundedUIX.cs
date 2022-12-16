using System;
using System.Collections;
using System.Text;
using HarmonyLib;
using NeosModLoader;

namespace BoundedUIX
{
    public class BoundedUIX : NeosMod
    {
        public override string Author => "Banane9";
        public override string Link => "https://github.com/Banane9/NeosBoundedUIX";
        public override string Name => "BoundedUIX";
        public override string Version => "2.2.0";

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"{Author}.{Name}");
            harmony.PatchAll();
        }
    }
}