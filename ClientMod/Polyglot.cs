using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PiTung_Bootstrap;
using PiTung_Bootstrap.Console;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polyglot
{
    public class Polyglot : Mod
    {
        private static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;

        public override string Name => "Polyglot";
        public override string Author => "Stenodyon";
        public override Version ModVersion => Version;
        public override Version FrameworkVersion => PiTung.FrameworkVersion;

        private bool initialized = false;

        private Client client;
        public Placer placer { get; private set; }

        public void Init()
        {
            client = new Client();
            placer = new Placer();
            IGConsole.RegisterCommand(new Command_find());
            IGConsole.RegisterCommand(new Command_findobj());
            IGConsole.Log($"Polyglot v{ModVersion.ToString()} initialized");
        }

        public override void Update()
        {
            if(!initialized)
            {
                Init();
                initialized = true;
            }
            client.Update();
        }

        private class Command_find : Command
        {
            public override string Name => "find";
            public override string Usage => $"{Name} component_name";

            public override bool Execute(IEnumerable<string> arguments)
            {
                if (arguments.Count() != 1)
                {
                    IGConsole.Error("Usage: find component_name");
                    return false;
                }
                Assembly asm = typeof(UnityEngine.Object).Assembly;
                Type componentType = asm.GetType($"UnityEngine.{arguments.ElementAt(0)}");
                if (componentType == null)
                {
                    IGConsole.Error($"{arguments.ElementAt(0)} is not a type");
                    return false;
                }
                if (!componentType.IsSubclassOf(typeof(Component)))
                {
                    IGConsole.Error($"{arguments.ElementAt(0)} is not a Component");
                    return false;
                }
                Component[] matches = (Component[])GameObject.FindObjectsOfType(componentType);
                foreach (Component component in matches)
                {
                    IGConsole.Log($"\"{component.name}\" is at {component.transform.position}");
                }
                return true;
            }
        }

        private class Command_findobj : Command
        {
            public override string Name => "findobj";
            public override string Usage => $"{Name} name";

            public override bool Execute(IEnumerable<string> arguments)
            {
                if (arguments.Count() == 0)
                {
                    IGConsole.Error("Usage: findobj name");
                    return false;
                }
                string name = string.Join(" ", arguments.ToArray());
                GameObject found = GameObject.Find(name);
                if (found != null)
                    IGConsole.Log($"{found.name} found at {found.transform.position}");
                else
                    IGConsole.Error("Object not found");
                return true;
            }
        }

    }
}
