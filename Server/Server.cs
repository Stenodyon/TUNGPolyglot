using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using PolyglotCommon;

namespace PolyglotServer
{
    class Server
    {
        private TcpListener server;
        private bool started = false;

        private List<Player> players;
        private Thread acceptThread;

        public Server(int port)
        {
            IPAddress address = IPAddress.Parse("0.0.0.0");
            server = new TcpListener(address, port);
            server.Start();
            started = true;
            players = new List<Player>();

            acceptThread = new Thread(this.Accept);
            acceptThread.IsBackground = true;
            acceptThread.Start();

            Console.WriteLine($"Server listening on port {port}");
            Console.WriteLine("Type 'exit' or 'stop' to stop the server");
        }

        private void Accept()
        {
            while(started)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine($"Connection received from {client.Client.RemoteEndPoint}");
                players.Add(new Player(client, this));
                Thread.Sleep(30);
            }
        }

        public void OnPlayerDisconnect(Player player)
        {
            players.Remove(player);
        }

        public void OnReceivedPacket(Player player, Packet packet)
        {
            Console.WriteLine($"Received {packet.GetType().ToString()}");
        }

        public void Stop()
        {
            started = false;
            foreach (Player player in players)
                player.DisconnectNoNotify();
            server.Stop();
        }
    }
}
