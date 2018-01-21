using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace PolyglotServer
{
    class Server
    {
        private TcpListener server;
        private bool started = false;

        public Server(int port)
        {
            IPAddress address = IPAddress.Parse("0.0.0.0");
            server = new TcpListener(address, port);
            server.Start();
            started = true;
        }

        private void Accept()
        {
            while(started)
            {
                Socket socket = server.AcceptSocket();
                Console.WriteLine($"Connection received from {socket.RemoteEndPoint}");
                (new Thread(() => this.Serve(socket))).Start();
            }
        }

        private void Serve(Socket socket)
        {
        }

        public void Stop()
        {
            started = false;
            server.Stop();
        }
    }
}
