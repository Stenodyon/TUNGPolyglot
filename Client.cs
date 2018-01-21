using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Polyglot
{
    class Client
    {
        private string savesPath;

        public Client()
        {
            Console.RegisterCommand("connect", this.Command_connect);
            Console.RegisterCommand("disconnect", this.Command_disconnect);
            savesPath = Application.persistentDataPath + "/saves/multiplayer";
        }

        private void DeleteSave()
        {
            string saveName = savesPath + "/_";
            if (File.Exists(saveName))
                File.Delete(saveName);
        }

        private void Connect()
        {
            if (!Directory.Exists(savesPath))
                Directory.CreateDirectory(savesPath);
            DeleteSave();
            SaveManager.SaveName = "multiplayer/_";
            SceneManager.LoadScene("gameplay");
        }

        private void Disconnect()
        {
            UIManager.UnlockMouseAndDisableFirstPersonLooking();
            SceneManager.LoadScene("main menu");
        }

        private void Command_connect(IEnumerable<string> args)
        {
            Console.Log("Connecting...");
            Connect();
        }

        private void Command_disconnect(IEnumerable<string> args)
        {
            Console.Log("Disconnecting...");
            Disconnect();
        }
    }
}
