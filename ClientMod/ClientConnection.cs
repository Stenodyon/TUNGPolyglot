using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PolyglotCommon;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using PiTung_Bootstrap.Console;

namespace Polyglot
{
    public enum Status
    {
        Disconnected,
        Connected
    }

    public class ClientConnection
    {
        private TcpClient client;
        private Status status = Status.Disconnected;
        public Status Status { get { return this.status; } }

        private Thread sendThread;
        private Thread receiveThread;

        // Player ID, Player
        private Dictionary<int, RemotePlayer> players;
        public Nullable<int> ID { get; private set; }

        private Queue<Packet> sendQueue;
        private Queue<Packet> receiveQueue;

        private int lastSyncTime;
        private static int SyncDelay = 1000/10;

        private Transform player;
        private bool inGameplay = false;

        private BoardManager boardManager;

        public ClientConnection(string address, int port)
        {
            client = new TcpClient();
            client.Connect(address, port);
            status = Status.Connected;
            sendQueue = new Queue<Packet>();
            receiveQueue = new Queue<Packet>();
            players = new Dictionary<int, RemotePlayer>();

            sendThread = new Thread(this.SendThread);
            sendThread.IsBackground = true;
            sendThread.Start();

            receiveThread = new Thread(this.ReceiveThread);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            lastSyncTime = 0;
            SceneManager.activeSceneChanged += GameplayInit;
        }

        private void GameplayInit(Scene arg0, Scene arg1)
        {
            if (SceneManager.GetActiveScene().name != "gameplay")
                return;
            SceneManager.activeSceneChanged -= GameplayInit;
            inGameplay = true;
            GameObject playerObject = GameObject.Find("FirstPersonCharacter");
            if(playerObject == null)
            {
                IGConsole.Error("Could not find player object");
                return;
            }
            player = playerObject.transform;
            boardManager = new BoardManager(this);
            SceneManager.activeSceneChanged += DisconnectOnLeave;
        }

        private void DisconnectOnLeave(Scene arg0, Scene arg1)
        {
            if (SceneManager.GetActiveScene().name == "gameplay")
                return;
            SceneManager.activeSceneChanged -= DisconnectOnLeave;
            Disconnect();
        }

        public void Disconnect()
        {
            if (status != Status.Disconnected)
                client.Close();
            status = Status.Disconnected;
        }

        public void SendPacket(Packet packet)
        {
            sendQueue.Enqueue(packet);
        }

        private void SendThread()
        {
            IGConsole.Log("Starting sender thread");
            while(status != Status.Disconnected)
            {
                if(sendQueue.Count > 0)
                {
                    Packet packet = sendQueue.Dequeue();
                    BinaryFormatter formatter = new BinaryFormatter();
                    try
                    {
                        formatter.Serialize(client.GetStream(), packet);
#if DEBUG
                        if(packet.GetType() != typeof(PlayerPosition))
                            IGConsole.Log($"Sent {packet.GetType().ToString()}");
#endif
                    } catch(Exception e)
                    {
                        IGConsole.Error(e.ToString());
                        Disconnect();
                    }
                }
                Thread.Sleep(30);
            }
            IGConsole.Log("Sender thread stopped");
        }

        private void ReceiveThread()
        {
            IGConsole.Log("Starting receiver thread");
            while(status != Status.Disconnected)
            {
                if(client.Available > 0)
                {
                    try
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        Packet packet = formatter.Deserialize(client.GetStream()) as Packet;
                        receiveQueue.Enqueue(packet);
                    } catch (Exception e)
                    {
                        IGConsole.Error(e.ToString());
                    }
                }
                Thread.Sleep(30);
            }
            IGConsole.Log("Receiver thread stopped");
        }

        private void SyncPlayerPos()
        {
            int time = (int)(Time.time * 1000);
            if (time - lastSyncTime > SyncDelay)
            {
                lastSyncTime = time;
                SendPacket(new PlayerPosition(ID.Value, player.position, player.eulerAngles));
            }
        }

        public void HandlePackets()
        {
            while(receiveQueue.Count > 0)
            {
                Packet packet = receiveQueue.Dequeue();
                try
                {
                    OnPacketReceived(packet);
                } catch(Exception e)
                {
                    IGConsole.Error(e.ToString());
                }
            }
            if (inGameplay)
            {
                SyncPlayerPos();
            }
        }

        private void OnPacketReceived(Packet packet)
        {
#if DEBUG
            if (packet.GetType() != typeof(PlayerPosition))
                IGConsole.Log($"Received {packet.GetType()}");
#endif
            var packetSwitch = new Dictionary<Type, Action>
            {
                {typeof(PlayerIDAttribution),
                    () => OnPlayerIDAttribution((PlayerIDAttribution)packet) },
                {typeof(PlayerList),
                    () => OnPlayerList((PlayerList)packet) },
                {typeof(NewPlayer),
                    () => OnNewPlayer((NewPlayer)packet) },
                {typeof(PlayerPosition),
                    () => OnPlayerPosition((PlayerPosition)packet) },
                {typeof(PlayerDisconnected),
                    () => OnPlayerDisconnected((PlayerDisconnected)packet) },
                {typeof(GlobalIDAttribution),
                    () => OnGlobalIDAttribution((GlobalIDAttribution)packet) },
                {typeof(NewBoard),
                    () => OnNewBoard((NewBoard)packet) },
                {typeof(DeleteBoard),
                    () => OnDeleteBoard((DeleteBoard)packet) },
                {typeof(MovedBoard),
                    () => OnMovedBoard((MovedBoard)packet) },
            };
            Action action;
            if(!packetSwitch.TryGetValue(packet.GetType(), out action))
                action = () => OnUnhandledPacket(packet);
            action();
        }

        private void OnUnhandledPacket(Packet packet)
        {
            IGConsole.Error($"Unhandled packet type {packet.GetType().ToString()}");
        }

        private void OnPlayerIDAttribution(PlayerIDAttribution packet)
        {
            if(ID.HasValue)
            {
                IGConsole.Error("Received unexpected IDAttribution");
                return;
            }
            ID = packet.ID;
            IGConsole.Log($"ID set to {ID.Value}");
            SendPacket(new IDAttibutionACK());
        }

        private void OnNewPlayer(NewPlayer packet)
        {
            if (packet.ID == ID)
                return;
            if (!players.ContainsKey(packet.ID))
                players.Add(packet.ID, new RemotePlayer(packet.ID));
        }

        private void OnPlayerList(PlayerList packet)
        {
            var currentPlayers = new HashSet<int>();
            // Add new players and update existing
            foreach(PlayerPosition player in packet.List)
            {
                if (player.PlayerID == ID)
                    continue;
                currentPlayers.Add(player.PlayerID);
                RemotePlayer remotePlayer;
                if(!players.ContainsKey(player.PlayerID))
                {
                    remotePlayer = new RemotePlayer(player.PlayerID);
                    players.Add(player.PlayerID, remotePlayer);
                }
                else
                {
                    remotePlayer = players[player.PlayerID];
                }
                remotePlayer.Position = player.Pos;
                remotePlayer.Angles = player.Angles;
            }
            // Remove absent players
            List<int> toRemove = new List<int>();
            foreach(RemotePlayer player in players.Values)
            {
                if (!currentPlayers.Contains(player.ID))
                    toRemove.Add(player.ID);
            }
            foreach(int id in toRemove)
                players.Remove(id);
        }

        private void OnPlayerPosition(PlayerPosition packet)
        {
            RemotePlayer player;
            if(players.TryGetValue(packet.PlayerID, out player))
            {
                player.Position = packet.Pos;
                player.Angles = packet.Angles;
            }
        }

        private void OnPlayerDisconnected(PlayerDisconnected packet)
        {
            RemotePlayer player;
            if(players.TryGetValue(packet.ID, out player))
            {
                player.Destroy();
                players.Remove(packet.ID);
            }
        }

        private void OnGlobalIDAttribution(GlobalIDAttribution packet)
        {
            boardManager.GlobalIDAttribution(packet.LocalID, packet.GlobalID);
        }

        private void OnNewBoard(NewBoard packet)
        {
            boardManager.OnNewRemoteBoard(packet);
        }

        private void OnDeleteBoard(DeleteBoard packet)
        {
            boardManager.DeleteRemoteBoard(packet.ID);
        }

        private void OnMovedBoard(MovedBoard packet)
        {
            boardManager.OnRemoteMovedBoard(packet);
        }
    }
}
