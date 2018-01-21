using PiTung_Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Polyglot
{
    delegate void CommandExecutor(IEnumerable<string> arguments);

    class Console
    {
        private static int maxHistory = 100;
        private static int lineHeight = 16;
        private static GUIStyle style;

        private static DropOutStack<String> cmdLog;
        private static DropOutStack<String> history;
        private static int historySelector = -1;
        private static string currentCmd = "";

        private static Dictionary<string, CommandExecutor> registry;

        public static bool show = false;

        public static void Init()
        {
            cmdLog = new DropOutStack<string>(maxHistory);
            history = new DropOutStack<string>(maxHistory);
            registry = new Dictionary<string, CommandExecutor>();
            style = new GUIStyle
            {
                font = Font.CreateDynamicFontFromOSFont("Lucida Console", 16)
            };

            RegisterCommand("lsfont", Command_lsfont);
            Log("Console initialized");
        }

        public static void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                show = !show;

            if(Input.GetKeyDown(KeyCode.UpArrow) && historySelector < history.Count - 1)
            {
                historySelector += 1;
                currentCmd = history.Get(historySelector);
            }
            if(Input.GetKeyDown(KeyCode.DownArrow) && historySelector > -1)
            {
                historySelector -= 1;
                if (historySelector == -1)
                    currentCmd = "";
                else
                    currentCmd = history.Get(historySelector);
            }

            if (show)
                ReadInput();
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

        public static void Log(string msg)
        {
            string[] lines = msg.Split('\n');
            foreach(string line in lines)
            {
                cmdLog.Push(line);
            }
        }

        public static bool RegisterCommand(string name, CommandExecutor callback)
        {
            if (registry.ContainsKey(name))
                return false;
            registry.Add(name, callback);
            return true;
        }

        private static void ExecuteCommand(string cmd)
        {
            string[] words = cmd.Split(' ');
            if (words.Length == 0)
                return;
            string verb = words[0];
            if (registry.ContainsKey(verb))
            {
                CommandExecutor executor = registry[verb];
                try
                {
                    executor(words.Skip(1));
                } catch(Exception e)
                {
                    Log(e.ToString());
                }
            }
            else
            {
                Log($"Unrecognized command {verb}");
            }
        }

        private static void ReadInput()
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
                    history.Push(currentCmd);
                    ExecuteCommand(currentCmd);
                    currentCmd = "";
                }
                else
                {
                    currentCmd += c;
                }
            }
        }

        static void DrawText(string text, Vector2 pos, Color color)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.normal.textColor = color;
            Vector2 size = style.CalcSize(new GUIContent(text));
            Rect rect = new Rect(pos, size);

            GUI.Label(rect, text, newStyle);
        }

        static void Command_lsfont(IEnumerable<string> args)
        {
            if (args.Count() > 1)
            {
                string extras = String.Join(" ", args.Skip(1).ToArray());
                Log($"Invalid arguments: \"{extras}\"");
                return;
            }
            string contained = "";
            if (args.Count() == 1)
                contained = args.ElementAt(0);
            foreach (string name in Font.GetOSInstalledFontNames())
            {
                if (name.ToLower().Contains(contained))
                    Log(name);
            }
        }

    }
}
