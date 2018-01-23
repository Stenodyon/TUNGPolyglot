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

    /// <summary>
    /// Represents a command that can be invoked from the console
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Used to invoke the command (e.g. "help")
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// How to use the command (e.g. "{Name} argument [optional_argument]")
        /// </summary>
        public abstract string Usage { get; }

        /// <summary>
        /// Short description of what the command does, preferably on 1 line
        /// </summary>
        public virtual string Description { get; } = "";

        /// <summary>
        /// Called when the command is invoked
        /// </summary>
        /// <param name="arguments">The arguments given to the command</param>
        public abstract void Execute(IEnumerable<string> arguments);
    }

    //TODO: ICommand interface
    //TODO: Prevent player movement when console open
    //TODO: Add verbosity levels

    /// <summary>
    /// In game console
    /// <para>This static class allows for logging and registering commands
    /// which will be executed by callbacks</para>
    /// </summary>
    public class Console
    {
        private const int maxHistory = 100;
        private const int lineHeight = 16;
        private const string prompt = "> ";
        private const string cursor = "_";

        private static GUIStyle style;
        private static DropOutStack<LogEntry> cmdLog;
        private static DropOutStack<String> history;
        private static int editLocation = 0;
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

            RegisterCommand("help", Command_help);
            RegisterCommand("lsmod", Command_lsmod);
            RegisterCommand("lsfont", Command_lsfont);
            Log("Console initialized");
        }

        /// <summary>
        /// Call this function on Update calls
        /// </summary>
        public static void Update()
        {
            // Toggle console with TAB
            if (Input.GetKeyDown(KeyCode.Tab))
                show = !show;

            if (show)
            {
                // Handling history
                if (Input.GetKeyDown(KeyCode.UpArrow) && historySelector < history.Count - 1)
                {
                    historySelector += 1;
                    currentCmd = history.Get(historySelector);
                    editLocation = currentCmd.Length;
                }
                if (Input.GetKeyDown(KeyCode.DownArrow) && historySelector > -1)
                {
                    historySelector -= 1;
                    if (historySelector == -1)
                        currentCmd = "";
                    else
                        currentCmd = history.Get(historySelector);
                    editLocation = currentCmd.Length;
                }
                // Handle editing
                if (Input.GetKeyDown(KeyCode.LeftArrow) && editLocation > 0)
                    editLocation--;
                if (Input.GetKeyDown(KeyCode.RightArrow) && editLocation < currentCmd.Length)
                    editLocation++;

                ReadInput(); // Read text input
            }
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
            // Background rectangle
            ModUtilities.Graphics.DrawRect(new Rect(0, 0, width, linecount * lineHeight + 5), background);
            for(int line = 0; line < Math.Min(linecount - 1, cmdLog.Count); line++)
            {
                LogEntry entry = cmdLog.Get(line);
                int y = (linecount - 2 - line) * lineHeight;
                DrawText(entry.Message, new Vector2(5, y), entry.GetColor());
            }
            int consoleY = (linecount - 1) * lineHeight;
            try
            {
                DrawText(prompt + currentCmd, new Vector2(5, consoleY), Color.green);
                float x = Width(prompt) + Width(currentCmd.Substring(0, editLocation));
                DrawText(cursor, new Vector2(5 + x, consoleY), Color.green);
            } catch(Exception e)
            {
                Error($"currentCmd: \"{currentCmd}\"\neditLocation: {editLocation}");
                Error(e.ToString());
                currentCmd = "";
                editLocation = 0;
            }
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
        /// Logs an error message
        /// </summary>
        /// <param name="msg">Message to log</param>
        public static void Error(string msg)
        {
            Log(LogType.ERROR, msg);
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
                        string firstHalf = currentCmd.Substring(0, editLocation - 1);
                        string secondHalf = currentCmd.Substring(editLocation, currentCmd.Length - editLocation);
                        currentCmd = firstHalf + secondHalf;
                        editLocation--;
                    }
                }
                else if ((c == '\n') || (c == '\r')) // enter/return
                {
                    Log(LogType.USERINPUT, "> " + currentCmd);
                    history.Push(currentCmd);
                    ExecuteCommand(currentCmd);
                    currentCmd = "";
                    editLocation = 0;
                }
                else
                {
                    currentCmd = currentCmd.Insert(editLocation, c.ToString());
                    editLocation++;
                }
            }
        }

        private static float Width(string text)
        {
            return style.CalcSize(new GUIContent(text)).x;
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

        static void Command_lsmod(IEnumerable<string> args)
        {
            Log(LogType.ERROR, "Not implemented");
        }

        static void Command_help(IEnumerable<string> args)
        {
            Log("Here is a list of available commands:");
            foreach(string name in registry.Keys)
                Log(name);
        }
    }
}
