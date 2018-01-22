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
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            Console.RegisterCommand("playerpos", Command_playerpos);
            Console.RegisterCommand("find", Command_find);
            Console.RegisterCommand("spawnmesh", Command_spawnmesh);
            Console.RegisterCommand("lsshader", Command_lsshader);
            Console.RegisterCommand("findplayer", Command_findplayer);
            Console.RegisterCommand("findobj", Command_findobj);
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

        public static void Command_playerpos(IEnumerable<string> args)
        {
            Scene currentScene = SceneManager.GetActiveScene();
            if(currentScene.name != "gameplay")
            {
                Console.Log(LogType.ERROR, "Not currently in gameplay");
                return;
            }
            Camera[] cameras = (Camera[])UnityEngine.Object.FindObjectsOfType(typeof(Camera));
            foreach(Camera cam in cameras)
            {
                Vector3 pos = cam.transform.position;
                Console.Log($"Camera {cam.name} is at {pos}");
            }
        }

        private void Command_findplayer(IEnumerable<string> args)
        {
            GameObject player = GameObject.Find("FirstPersonController");
            if (player == null)
                Console.Error("Could not find player");
            else
                Console.Log("Found player");
        }

        public static void Command_find(IEnumerable<string> args)
        {
            if(args.Count() != 1)
            {
                Console.Error("Usage: find component_name");
                return;
            }
            Assembly asm = typeof(UnityEngine.Object).Assembly;
            Type componentType = asm.GetType($"UnityEngine.{args.ElementAt(0)}");
            if(componentType == null)
            {
                Console.Error($"{args.ElementAt(0)} is not a type");
                return;
            }
            if(!componentType.IsSubclassOf(typeof(Component)))
            {
                Console.Error($"{args.ElementAt(0)} is not a Component");
                return;
            }
            Component[] matches = (Component[])GameObject.FindObjectsOfType(componentType);
            foreach(Component component in matches)
            {
                Console.Log($"\"{component.name}\" is at {component.transform.position}");
            }
        }

        private static void Command_findobj(IEnumerable<string> args)
        {
            if(args.Count() == 0)
            {
                Console.Error("Usage: findobj name");
                return;
            }
            string name = string.Join(" ", args.ToArray());
            GameObject found = GameObject.Find(name);
            if (found != null)
                Console.Log($"{found.name} found at {found.transform.position}");
            else
                Console.Error("Object not found");
        }

        public static void Command_lsshader(IEnumerable<string> args)
        {
            if(args.Count() != 1)
            {
                Console.Error("Usage: lsshader name");
                return;
            }
            Shader shader = Shader.Find(args.ElementAt(0));
            if (shader == null)
                Console.Log("Shader not found");
            else
                Console.Log("Shader exists!");
        }

        public void Command_spawnmesh(IEnumerable<string> args)
        {
            if(SceneManager.GetActiveScene().name != "gameplay")
            {
                Console.Error("Can only spawn mesh in gameplay");
                return;
            }
            if(testObj != null)
            {
                Console.Error("Object already instantiated");
                return;
            }
            testObj = RemotePlayer.MakePlayerModel();
            testObj.transform.position = new Vector3(0.0f, 1.0f, 0.0f);
        }

        public GameObject testObj;

        public void SceneManager_activeSceneChanged(Scene arg0, Scene newScene)
        {
            if (newScene.name != "gameplay")
                testObj = null;
        }
    }
}
