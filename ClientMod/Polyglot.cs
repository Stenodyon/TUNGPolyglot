using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PiTung_Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polyglot
{
    public class Polyglot : Mod
    {
        public override string ModName => "Polyglot";
        public override string ModAuthor => "Stenodyon";
        public override Version ModVersion => new Version(0, 1, 0);

        private Client client;

        public override void AfterPatch()
        {
            Console.Init();
            client = new Client();
            Console.RegisterCommand(new Command_find());
            Console.RegisterCommand(new Command_findobj());
            Console.Log($"Polyglot v{ModVersion.ToString()} initialized");
        }

        public override void Update()
        {
            Console.Update();
            client.Update();
        }

        public override void OnGUI()
        {
            if (ModUtilities.IsOnMainMenu)
            {
                string text = "Polyglot Loaded (TAB for console)";
                ModUtilities.Graphics.DrawText(text, new Vector2(6, 21), Color.black);
                ModUtilities.Graphics.DrawText(text, new Vector2(5, 20), Color.white);
            }
            Console.Draw();
        }

        private class Command_find : Command
        {
            public override string Name => "find";
            public override string Usage => $"{Name} component_name";

            public override void Execute(IEnumerable<string> arguments)
            {
                if (arguments.Count() != 1)
                {
                    Console.Error("Usage: find component_name");
                    return;
                }
                Assembly asm = typeof(UnityEngine.Object).Assembly;
                Type componentType = asm.GetType($"UnityEngine.{arguments.ElementAt(0)}");
                if (componentType == null)
                {
                    Console.Error($"{arguments.ElementAt(0)} is not a type");
                    return;
                }
                if (!componentType.IsSubclassOf(typeof(Component)))
                {
                    Console.Error($"{arguments.ElementAt(0)} is not a Component");
                    return;
                }
                Component[] matches = (Component[])GameObject.FindObjectsOfType(componentType);
                foreach (Component component in matches)
                {
                    Console.Log($"\"{component.name}\" is at {component.transform.position}");
                }
            }
        }

        private class Command_findobj : Command
        {
            public override string Name => "findobj";
            public override string Usage => $"{Name} name";

            public override void Execute(IEnumerable<string> arguments)
            {
                if (arguments.Count() == 0)
                {
                    Console.Error("Usage: findobj name");
                    return;
                }
                string name = string.Join(" ", arguments.ToArray());
                GameObject found = GameObject.Find(name);
                if (found != null)
                    Console.Log($"{found.name} found at {found.transform.position}");
                else
                    Console.Error("Object not found");
            }
        }

    }
}
