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

        private int maxHistory = 100;
        private DropOutStack<String> cmdHistory;
        private bool showConsole = false;

        private int lineHeight = 15;

        public override void AfterPatch()
        {
            cmdHistory = new DropOutStack<string>(maxHistory);
            Log("Polyglot initialized");
        }

        private string currentCmd = "";

        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                showConsole = !showConsole;
            if (showConsole)
                ReadCmd();
        }

        public void ReadCmd()
        {
            foreach(char c in Input.inputString)
            {
                if (c == '\b') // has backspace/delete been pressed?
                {
                    if (currentCmd.Length != 0)
                    {
                        currentCmd = currentCmd.Substring(0, currentCmd.Length - 1);
                    }
                }
                else if ((c == '\n') || (c == '\r')) // enter/return
                {
                    Log("> " + currentCmd);
                    currentCmd = "";
                }
                else
                {
                    currentCmd += c;
                }
            }
        }

        public void ExecuteCmd(string cmd)
        {
        }

        public void DrawConsole()
        {
            Color background = Color.black;
            background.a = 0.5f;
            int height = Screen.height / 2;
            int width = Screen.width;
            int linecount = height / lineHeight;
            ModUtilities.Graphics.DrawRect(new Rect(0, 0, width, linecount * lineHeight), background);
            for(int line = 0; line < Math.Min(linecount - 1, cmdHistory.Count); line++)
            {
                int y = (linecount - 2 - line) * lineHeight;
                ModUtilities.Graphics.DrawText(cmdHistory.Get(line), new Vector2(5, y), Color.white);
            }
            int consoleY = (linecount - 1) * lineHeight;
            ModUtilities.Graphics.DrawText("> " + currentCmd + "_", new Vector2(5, consoleY), Color.green);
        }

        public override void OnGUI()
        {
            if(ModUtilities.IsOnMainMenu)
            {
                ModUtilities.Graphics.DrawText("Polyglot loaded", new Vector2(6, 16), Color.black);
                ModUtilities.Graphics.DrawText("Polyglot loaded", new Vector2(5, 15), Color.white);
            }
            if (showConsole)
                DrawConsole();
        }

        public override void OnKeyDown(KeyCode key)
        {
        }

        public void Log(String msg)
        {
            cmdHistory.Push(msg);
        }
    }
}
