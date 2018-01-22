using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Net;

namespace Polyglot
{
    class Client
    {
        private static int defaultPort = 4545;
        private string savesPath;

        private ClientConnection connection;

        public Client()
        {
            Console.RegisterCommand("connect", this.Command_connect);
            Console.RegisterCommand("disconnect", this.Command_disconnect);
            savesPath = Application.persistentDataPath + "/saves/multiplayer";
        }

        public void Update()
        {
            if(connection != null)
                connection.HandlePackets();
        }

        private void DeleteSave()
        {
            string saveName = savesPath + "/_";
            if (File.Exists(saveName))
                File.Delete(saveName);
        }

        private void Connect(string address, int port)
        {
            if(connection != null && connection.Status == Status.Connected)
            {
                Console.Error("Already connected, disconnect first");
                return;
            }
            Console.Log($"Connecting to {address}:{port}");
            connection = new ClientConnection(address, port);
            if (!Directory.Exists(savesPath))
                Directory.CreateDirectory(savesPath);
            DeleteSave();
            SaveManager.SaveName = "multiplayer/_";
            SceneManager.LoadScene("gameplay");
        }

        private void Disconnect()
        {
            if(connection != null)
                connection.Disconnect();
            if (SceneManager.GetActiveScene().name == "gameplay")
            {
                UIManager.UnlockMouseAndDisableFirstPersonLooking();
                SceneManager.LoadScene("main menu");
            }
            else
            {
                Console.Log("Not currently connected");
            }
        }

        private void Command_connect(IEnumerable<string> args)
        {
            if(args.Count() != 1)
            {
                Console.Log(LogType.ERROR, "Usage: connect host[:port]");
                return;
            }
            string[] parts = args.ElementAt(0).Split(':');
            int port = 4545;
            if (parts.Length > 1)
                port = Int32.Parse(parts[1]);
            Connect(parts[0], port);
        }

        private void Command_disconnect(IEnumerable<string> args)
        {
            Console.Log("Disconnecting...");
            Disconnect();
        }
    }
}
