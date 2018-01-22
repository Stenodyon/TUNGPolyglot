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

namespace PolyglotServer
{
    internal enum Status
    {
        Disconnected,
        Connected
    }

    class Player
    {
        private static int GlobalID = 0;

        public int ID { get; private set; }

        private Server server;
        private TcpClient client;
        private Status status = Status.Connected;
        private Queue<Packet> packetQueue;

        private Thread sendThread;
        private Thread receiveThread;

        public Player(TcpClient client, Server server)
        {
            this.server = server;
            this.ID = GlobalID;
            GlobalID++;
            this.client = client;
            this.packetQueue = new Queue<Packet>();

            sendThread = new Thread(this.SendThread);
            sendThread.IsBackground = true;
            sendThread.Start();

            receiveThread = new Thread(this.ReceiveThread);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            SendPacket(new PlayerIDAttribution(ID));
        }

        public void DisconnectNoNotify()
        {
            client.Close();
            status = Status.Disconnected;
        }

        public void Disconnect()
        {
            DisconnectNoNotify();
            server.OnPlayerDisconnect(this);
        }

        public void SendPacket(Packet packet)
        {
            this.packetQueue.Enqueue(packet);
        }

        private void SendThread()
        {
            Console.WriteLine("Started sender thread for player");
            while(status != Status.Disconnected)
            {
                if(packetQueue.Count > 0)
                {
                    Packet packet = packetQueue.Dequeue();
                    BinaryFormatter formatter = new BinaryFormatter();
                    try
                    {
                        formatter.Serialize(client.GetStream(), packet);
                        Console.WriteLine($"Sent {packet.GetType().ToString()}");
                    } catch(Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Disconnect();
                    }
                }
                Thread.Sleep(30);
            }
        }

        private void ReceiveThread()
        {
            Console.WriteLine("Started receiver thread for player");
            while(status != Status.Disconnected)
            {
                if(client.Available > 0)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    Packet packet = formatter.Deserialize(client.GetStream()) as Packet;
                    server.OnReceivedPacket(this, packet);
                }
                Thread.Sleep(30);
            }
        }
    }
}
