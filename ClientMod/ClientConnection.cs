using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PolyglotCommon;
using System.Runtime.Serialization.Formatters.Binary;

namespace Polyglot
{
    internal enum Status
    {
        Disconnected,
        Connected
    }

    class ClientConnection
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

        public ClientConnection(string address, int port)
        {
            client = new TcpClient();
            client.Connect(address, port);
            status = Status.Connected;
            sendQueue = new Queue<Packet>();
            receiveQueue = new Queue<Packet>();

            sendThread = new Thread(this.SendThread);
            sendThread.IsBackground = true;
            sendThread.Start();

            receiveThread = new Thread(this.ReceiveThread);
            receiveThread.IsBackground = true;
            receiveThread.Start();
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
            Console.Log("Starting sender thread");
            while(status != Status.Disconnected)
            {
                if(sendQueue.Count > 0)
                {
                    Packet packet = sendQueue.Dequeue();
                    BinaryFormatter formatter = new BinaryFormatter();
                    try
                    {
                        formatter.Serialize(client.GetStream(), packet);
                        Console.Log($"Sent {packet.GetType().ToString()}");
                    } catch(Exception e)
                    {
                        Console.Log(LogType.ERROR, e.ToString());
                        Disconnect();
                    }
                }
                Thread.Sleep(30);
            }
            Console.Log("Sender thread stopped");
        }

        private void ReceiveThread()
        {
            Console.Log("Starting receiver thread");
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
                        Console.Error(e.ToString());
                    }
                }
                Thread.Sleep(30);
            }
            Console.Log("Receiver thread stopped");
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
                    Console.Error(e.ToString());
                }
            }
        }

        private void OnPacketReceived(Packet packet)
        {
            Console.Log($"Received {packet.GetType().ToString()}");
            if(object.Equals(packet.GetType(), typeof(PlayerIDAttribution)))
                OnPlayerIDAttribution((PlayerIDAttribution)packet);
        }

        private void OnPlayerIDAttribution(PlayerIDAttribution packet)
        {
            if(ID.HasValue)
            {
                Console.Error("Received unexpected IDAttribution");
                return;
            }
            ID = packet.ID;
            Console.Log($"ID set to {ID.Value}");
            SendPacket(new IDAttibutionACK());
        }
    }
}
