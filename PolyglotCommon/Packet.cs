using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PolyglotCommon
{
    [Serializable]
    public class Packet
    {
    }

    [Serializable]
    public class PlayerIDAttribution : Packet
    {
        public int ID { get; set; }
    }

    [Serializable]
    public class NewPlayer : Packet
    {
        public int ID { get; set; }
    }

    [Serializable]
    public class PlayerPosition : Packet
    {
        public int PlayerID { get; set; }
        public Vector3 Pos { get; set; }
    }
}
