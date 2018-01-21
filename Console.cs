using PiTung_Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Polyglot
{
    delegate void CommandExecutor(string command);

    class Console
    {
        private static int maxHistory = 100;
        private static int lineHeight = 16;
        private static GUIStyle style;

        private static DropOutStack<String> cmdLog;
        private static DropOutStack<String> history;
        private static int historySelector = -1;
        private static string currentCmd = "";

        public static bool show = false;

        public static CommandExecutor executors = delegate { };

        static void ConsoleCommands(string cmd)
        {
            if(cmd.StartsWith("lsfont"))
            {
                string[] words = cmd.Split(' ');
                string contained = "";
                if (words.Length == 2)
                    contained = words[1];
                foreach(string name in Font.GetOSInstalledFontNames())
                {
                    if (name.ToLower().Contains(contained))
                        Log(name);
                }
            }
            else
            {
                Log($"{cmd} is not recognized");
            }
        }

        public static void Init()
        {
            executors += ConsoleCommands;
            cmdLog = new DropOutStack<string>(maxHistory);
            history = new DropOutStack<string>(maxHistory);
            style = new GUIStyle
            {
                font = Font.CreateDynamicFontFromOSFont("Lucida Console", 16)
            };
            Log("Console initialized");
        }

        public static void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                show = !show;

            if (show)
                ReadInput();
        }

        public static void Log(string msg)
        {
            cmdLog.Push(msg);
        }

        public static void ReadInput()
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
                    executors(currentCmd);
                    history.Push(currentCmd);
                    currentCmd = "";
                }
                else
                {
                    currentCmd += c;
                }
            }
        }

        public static void Draw()
        {
            if (!show)
                return;
            Color background = Color.black;
            background.a = 0.5f;
            int height = Screen.height / 2;
            int width = Screen.width;
            int linecount = height / lineHeight;
            ModUtilities.Graphics.DrawRect(new Rect(0, 0, width, linecount * lineHeight + 5), background);
            for(int line = 0; line < Math.Min(linecount - 1, cmdLog.Count); line++)
            {
                int y = (linecount - 2 - line) * lineHeight;
                DrawText(cmdLog.Get(line), new Vector2(5, y), Color.white);
            }
            int consoleY = (linecount - 1) * lineHeight;
            DrawText("> " + currentCmd + "_", new Vector2(5, consoleY), Color.green);
        }

        static void DrawText(string text, Vector2 pos, Color color)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.normal.textColor = color;
            Vector2 size = style.CalcSize(new GUIContent(text));
            Rect rect = new Rect(pos, size);

            GUI.Label(rect, text, newStyle);
        }

    }
}
