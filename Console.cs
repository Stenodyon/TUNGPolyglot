using PiTung_Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Polyglot
{
    class Console
    {
        private static int maxHistory = 100;
        private static int lineHeight = 15;

        private static DropOutStack<String> cmdHistory;
        private static string currentCmd = "";

        public static void Init()
        {
            cmdHistory = new DropOutStack<string>(maxHistory);
            Log("Console initialized");
        }

        public static void Log(string msg)
        {
            cmdHistory.Push(msg);
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

    }
}
