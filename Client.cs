using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Net;

namespace Polyglot
{
    class Client
    {
        private static int defaultPort = 4545;
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

        private void Connect(IPEndPoint host)
        {
            Console.Log($"Connecting to {host.ToString()}");
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

        private IPEndPoint Parse(string host)
        {
            string[] hostname = host.Split(':');
            if (hostname.Length != 1 && hostname.Length != 2)
                throw new ArgumentException("Invalid address");

            string domain = hostname[0];
            int port = defaultPort;
            if (hostname.Length == 2)
                port = Int32.Parse(hostname[1]);
            var dnslookup = Dns.GetHostAddresses(domain);
            return new IPEndPoint(dnslookup[0], port);
        }

        private void Command_connect(IEnumerable<string> args)
        {
            if(args.Count() != 1)
            {
                Console.Log(LogType.ERROR, "Usage: connect host[:port]");
                return;
            }
            IPEndPoint endpoint = Parse(args.ElementAt(0));
            Connect(endpoint);
        }

        private void Command_disconnect(IEnumerable<string> args)
        {
            Console.Log("Disconnecting...");
            Disconnect();
        }
    }
}
