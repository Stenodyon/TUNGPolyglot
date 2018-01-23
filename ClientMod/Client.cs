﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Net;
using PiTung_Bootstrap.Console;

namespace Polyglot
{
    class Client
    {
        private const int defaultPort = 4545;
        private static string savesPath;

        private static ClientConnection connection;

        public Client()
        {
            IGConsole.RegisterCommand(new Command_connect());
            IGConsole.RegisterCommand(new Command_disconnect());
            savesPath = Application.persistentDataPath + "/saves/multiplayer";
        }

        public void Update()
        {
            if(connection != null)
                connection.HandlePackets();
        }

        private static void DeleteSave()
        {
            string saveName = savesPath + "/_";
            if (File.Exists(saveName))
                File.Delete(saveName);
        }

        private static void Connect(string address, int port)
        {
            if(connection != null && connection.Status == Status.Connected)
            {
                IGConsole.Error("Already connected, disconnect first");
                return;
            }
            IGConsole.Log($"Connecting to {address}:{port}");
            connection = new ClientConnection(address, port);
            if (!Directory.Exists(savesPath))
                Directory.CreateDirectory(savesPath);
            DeleteSave();
            SaveManager.SaveName = "multiplayer/_";
            SceneManager.LoadScene("gameplay");
        }

        private static void Disconnect()
        {
            if(connection != null)
                connection.Disconnect();
        }

        private class Command_connect : Command
        {
            public override string Name => "connect";
            public override string Usage => $"{Name} host[:port]";

            public override bool Execute(IEnumerable<string> arguments)
            {
                if (arguments.Count() != 1)
                {
                    IGConsole.Error("Usage: connect host[:port]");
                    return false;
                }
                string[] parts = arguments.ElementAt(0).Split(':');
                int port = 4545;
                if (parts.Length > 1)
                    port = Int32.Parse(parts[1]);
                Connect(parts[0], port);
                return true;
            }
        }

        private class Command_disconnect : Command
        {
            public override string Name => "disconnect";
            public override string Usage => $"{Name}";

            public override bool Execute(IEnumerable<string> arguments)
            {
                IGConsole.Log("Disconnecting...");
                Disconnect();
                if (SceneManager.GetActiveScene().name == "gameplay")
                {
                    UIManager.UnlockMouseAndDisableFirstPersonLooking();
                    SceneManager.LoadScene("main menu");
                }
                else
                {
                    IGConsole.Log("Not currently connected");
                    return false;
                }
                return true;
            }
        }
    }
}
