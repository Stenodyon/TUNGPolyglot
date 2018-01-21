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

        protected override KeyCode[] ModKeys => new[] { KeyCode.Tab };

        private bool showConsole = false;

        public override void AfterPatch()
        {
            Console.Init();
            Console.Log("Polyglot initialized");
        }

        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                showConsole = !showConsole;
            if (showConsole)
                Console.ReadInput();
        }

        public void ExecuteCmd(string cmd)
        {
        }

        public override void OnGUI()
        {
            if(ModUtilities.IsOnMainMenu)
            {
                string text = "Polyglot Loaded (TAB for console)";
                ModUtilities.Graphics.DrawText(text, new Vector2(6, 16), Color.black);
                ModUtilities.Graphics.DrawText(text, new Vector2(5, 15), Color.white);
            }
            if (showConsole)
                Console.Draw();
        }

        public override void OnKeyDown(KeyCode key)
        {
        }
    }
}
