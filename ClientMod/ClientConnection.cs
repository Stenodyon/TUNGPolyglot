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

        private Thread sendThread;
        private Thread receiveThread;

        private Queue<Packet> packetQueue;

        public ClientConnection(IPEndPoint endpoint)
        {
            client = new TcpClient();
            client.Connect(endpoint);
            status = Status.Connected;

            sendThread = new Thread(this.SendThread);
            sendThread.IsBackground = true;
            sendThread.Start();

            packetQueue = new Queue<Packet>();
        }

        public void Disconnect()
        {
            if (status != Status.Disconnected)
                client.Close();
            status = Status.Disconnected;
        }

        public void SendPacket(Packet packet)
        {
            packetQueue.Enqueue(packet);
        }

        private void SendThread()
        {
            while(status != Status.Disconnected)
            {
                if(packetQueue.Count > 0)
                {
                    Packet packet = packetQueue.Dequeue();
                    BinaryFormatter formatter = new BinaryFormatter();
                    try
                    {
                        formatter.Serialize(client.GetStream(), packet);
                    } catch(Exception e)
                    {
                        Console.Log(LogType.ERROR, e.ToString());
                        Disconnect();
                    }
                }
                Thread.Sleep(30);
            }
        }

        private void ReceiveThread()
        {
            while(status != Status.Disconnected)
            {
                if(client.Available > 0)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    Packet packet = formatter.Deserialize(client.GetStream()) as Packet;
                    OnPacketReceived(packet);
                }
                Thread.Sleep(30);
            }
        }

        private void OnPacketReceived(Packet packet)
        {
            Console.Log($"Received {packet.GetType().ToString()}");
        }
    }
}
