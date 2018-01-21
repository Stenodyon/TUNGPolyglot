using System;
using System.Collections.Generic;
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
            Console.RegisterCommand("camcount", Command_camcount);
            client = new Client();
            Console.Log($"Polyglot v{ModVersion.ToString()} initialized");
        }

        public override void Update()
        {
            Console.Update();

            if(Input.GetKeyDown(KeyCode.M))
            {
                UnityEngine.Object[] objs = GameObject.FindObjectsOfType(typeof(DummyComponent));
                int count = objs.Length;
                Console.Log($"There are {count} dummycomponents in this scene");
                Command_camcount(new string[0]);
            }
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

        private static int CountCameras(Scene scene)
        {
            int count = 0;
            GameObject[] rootObjs = scene.GetRootGameObjects();
            foreach(GameObject obj in rootObjs)
            {
                Camera cam = obj.GetComponent<Camera>();
                if (cam != null)
                    count += 1;
            }
            return count;
        }

        private static void Command_camcount(IEnumerable<string> args)
        {
            int count = CountCameras(SceneManager.GetActiveScene());
            Console.Log($"There are {count} cameras in this scene");
        }
    }
}
