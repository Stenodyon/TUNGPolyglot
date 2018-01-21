using System;
using PiTung_Bootstrap;
using UnityEngine;

namespace Polyglot
{
    public class Polyglot : Mod
    {
        public override string ModName => "Polyglot";
        public override string ModAuthor => "Stenodyon";
        public override Version ModVersion => new Version(0, 1, 0);

        public override void AfterPatch()
        {
            Console.Init();
            Console.Log($"Polyglot v{ModVersion.ToString()} initialized");
        }

        public override void Update()
        {
            Console.Update();
        }

        public override void OnGUI()
        {
            if(ModUtilities.IsOnMainMenu)
            {
                string text = "Polyglot Loaded (TAB for console)";
                ModUtilities.Graphics.DrawText(text, new Vector2(6, 21), Color.black);
                ModUtilities.Graphics.DrawText(text, new Vector2(5, 20), Color.white);
            }
            Console.Draw();
        }
    }
}
