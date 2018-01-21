using PiTung_Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Polyglot
{
    public delegate void CommandExecutor(IEnumerable<string> arguments);

    /// <summary>
    /// Type of a log (should be self-explanatory
    /// </summary>
    public enum LogType
    {
        INFO,
        USERINPUT,
        ERROR
    }

    /// <summary>
    /// A line of log. It has a message and a log type
    /// </summary>
    internal class LogEntry
    {
        public LogType Type { get; private set; }
        public string Message { get; private set; }

        public LogEntry(LogType type, string message)
        {
            this.Type = type;
            this.Message = message;
        }

        public Color GetColor()
        {
            switch(Type)
            {
                case LogType.INFO:
                    return Color.white;
                case LogType.USERINPUT:
                    return Color.cyan;
                case LogType.ERROR:
                    return Color.red;
            }
            return Color.white;
        }
    }

    //TODO: Moving cursor within command for edition
    //TODO: Document

    /// <summary>
    /// In game console
    /// <para>This static class allows for logging and registering commands
    /// which will be executed by callbacks</para>
    /// </summary>
    public class Console
    {
        private static int maxHistory = 100;
        private static int lineHeight = 16;
        private static GUIStyle style;

        private static DropOutStack<LogEntry> cmdLog;
        private static DropOutStack<String> history;
        private static int historySelector = -1;
        private static string currentCmd = "";

        private static Dictionary<string, CommandExecutor> registry;

        public static bool show = false;

        /// <summary>
        /// Call this function before doing anything with the console
        /// </summary>
        public static void Init()
        {
            cmdLog = new DropOutStack<LogEntry>(maxHistory);
            history = new DropOutStack<string>(maxHistory);
            registry = new Dictionary<string, CommandExecutor>();
            style = new GUIStyle
            {
                font = Font.CreateDynamicFontFromOSFont("Lucida Console", 16)
            };

            RegisterCommand("lsfont", Command_lsfont);
            Log("Console initialized");
        }

        /// <summary>
        /// Call this function on Update calls
        /// </summary>
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

        /// <summary>
        /// Call this function on OnGUI calls
        /// </summary>
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
                LogEntry entry = cmdLog.Get(line);
                int y = (linecount - 2 - line) * lineHeight;
                DrawText(entry.Message, new Vector2(5, y), entry.GetColor());
            }
            int consoleY = (linecount - 1) * lineHeight;
            DrawText("> " + currentCmd + "_", new Vector2(5, consoleY), Color.green);
        }

        /// <summary>
        /// Log a message to the console (can be multiline)
        /// </summary>
        /// <param name="type">Type of log <see cref="LogType"/></param>
        /// <param name="msg">Message to log</param>
        public static void Log(LogType type, string msg)
        {
            string[] lines = msg.Split('\n');
            foreach(string line in lines)
            {
                cmdLog.Push(new LogEntry(type, line));
            }
        }

        /// <summary>
        /// Logs a message as simple info
        /// </summary>
        /// <param name="msg">Message to log</param>
        public static void Log(string msg)
        {
            Log(LogType.INFO, msg);
        }

        /// <summary>
        /// Register a command with a callback
        /// </summary>
        /// <param name="name">Name of the command. This is what is typed in the
        /// console to invoke the command</param>
        /// <param name="callback">The callback which is called when the command
        /// is invoked. Its arguments are the arguments of the command</param>
        /// <returns>True of succeeded, false otherwise</returns>
        public static bool RegisterCommand(string name, CommandExecutor callback)
        {
            if (registry.ContainsKey(name))
                return false;
            registry.Add(name, callback);
            return true;
        }

        /// <summary>
        /// Removes a command from the registry
        /// </summary>
        /// <param name="name">Name of the command to remove</param>
        /// <returns>True if a command was removed, false otherwise</returns>
        public static bool UnregisterCommand(string name)
        {
            if(registry.ContainsKey(name))
            {
                registry.Remove(name);
                return true;
            }
            return false;
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
                    Log(LogType.ERROR, e.ToString());
                }
            }
            else
            {
                Log(LogType.ERROR, $"Unrecognized command {verb}");
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
                    Log(LogType.USERINPUT, "> " + currentCmd);
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
                Log(LogType.ERROR, $"Invalid arguments: \"{extras}\"");
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
