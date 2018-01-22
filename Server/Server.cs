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

        private Dictionary<int, Player> awaitingID;
        private Dictionary<int, Player> players;
        private Thread acceptThread;

        public Server(int port)
        {
            IPAddress address = IPAddress.Parse("0.0.0.0");
            server = new TcpListener(address, port);
            server.Start();
            started = true;
            awaitingID = new Dictionary<int, Player>();
            players = new Dictionary<int, Player>();

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
                Player newPlayer = new Player(client, this);
                awaitingID.Add(newPlayer.ID, newPlayer);
                Thread.Sleep(30);
            }
        }

        public void Broadcast(Packet packet)
        {
            foreach(Player player in players.Values)
                player.SendPacket(packet);
        }

        public void Stop()
        {
            started = false;
            foreach (Player player in awaitingID.Values)
                player.DisconnectNoNotify();
            foreach (Player player in players.Values)
                player.DisconnectNoNotify();
            server.Stop();
        }

        private PlayerList MakePlayerList()
        {
            PlayerPosition[] positions = new PlayerPosition[players.Count];
            int index = 0;
            foreach(Player player in players.Values)
            {
                positions[index] = new PlayerPosition(player.ID, player.Position, player.Angles);
                index++;
            }
            return new PlayerList(positions);
        }

        public void OnPlayerDisconnect(Player player)
        {
            players.Remove(player.ID);
        }

        public void OnReceivedPacket(Player player, Packet packet)
        {
            var packetSwitch = new Dictionary<Type, Action>
            {
                {typeof(IDAttibutionACK),
                    () => OnIDAttributionACK(player, (IDAttibutionACK)packet) },
                {typeof(PlayerPosition),
                    () => OnPlayerPosition(player, (PlayerPosition)packet) },
            };
            Action action;
            if(!packetSwitch.TryGetValue(packet.GetType(), out action))
                action = () => OnUnhandledPacket(player, packet);
            action();
        }

        private void OnUnhandledPacket(Player player, Packet packet)
        {
            Console.WriteLine($"Unhandled packet: {packet.GetType().ToString()}");
        }

        private void OnIDAttributionACK(Player player, IDAttibutionACK packet)
        {
            if(!awaitingID.ContainsKey(player.ID))
            {
                Console.WriteLine("Unexpected IDAttributionACK: Player not awaiting ID");
                return;
            }
            if(players.ContainsKey(player.ID))
            {
                Console.WriteLine("Unexpected IDAttributionACK: player already connected");
                return;
            }
            Broadcast(new NewPlayer(player.ID));
            awaitingID.Remove(player.ID);
            players.Add(player.ID, player);
            player.SendPacket(MakePlayerList());
        }

        private void OnPlayerPosition(Player player, PlayerPosition packet)
        {
            player.Position = packet.Pos;
            player.Angles = packet.Angles;
            foreach(Player otherPlayer in players.Values)
            {
                if (otherPlayer.ID == player.ID)
                    continue;
                otherPlayer.SendPacket(packet);
            }
        }
    }
}
