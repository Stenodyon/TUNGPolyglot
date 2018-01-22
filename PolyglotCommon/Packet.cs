using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

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
        public PlayerIDAttribution(int id) { this.ID = id; }
    }

    [Serializable]
    public class IDAttibutionACK : Packet {}

    [Serializable]
    public class NewPlayer : Packet
    {
        public int ID { get; set; }
        public NewPlayer(int id) { this.ID = id; }
    }

    [Serializable]
    public class PlayerPosition : Packet
    {
        public int PlayerID { get; set; }
        public v3 Pos { get; set; }
        public v3 Angles { get; set; }

        public PlayerPosition(int id, v3 pos, v3 angles)
        {
            this.PlayerID = id;
            this.Pos = pos;
            this.Angles = angles;
        }
    }

    [Serializable]
    public class PlayerList : Packet
    {
        public PlayerPosition[] List { get; set; }
        public PlayerList(PlayerPosition[] list) { this.List = list; }
    }

    [Serializable]
    public class DisconnectPacket : Packet
    {
        public string Reason { get; set; }
        public DisconnectPacket(string reason) { this.Reason = reason; }
    }
}
