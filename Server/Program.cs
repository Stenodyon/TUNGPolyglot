using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyglotServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(4545);
            string cmd = "";
            while(cmd != "exit" && cmd != "stop")
            {
                cmd = Console.ReadLine();
            }
            server.Stop();
        }
    }
}
