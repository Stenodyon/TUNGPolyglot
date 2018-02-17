using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PolyglotCommon
{
    [Serializable]
    public abstract class Packet
    {
        public abstract Byte[] Serialize();

        protected static void WriteType(MemoryStream stream, PacketType type)
        {
            var sertype = BitConverter.GetBytes((int)type);
            stream.Write(sertype, 0, sertype.Length);
        }

        protected static void WriteInt(MemoryStream stream, int value)
        {
            var servalue = BitConverter.GetBytes(value);
            stream.Write(servalue, 0, servalue.Length);
        }

        protected static void WriteFloat(MemoryStream stream, float value)
        {
            var servalue = BitConverter.GetBytes(value);
            stream.Write(servalue, 0, servalue.Length);
        }

        protected static void WriteV3(MemoryStream stream, v3 vector)
        {
            WriteFloat(stream, vector.x);
            WriteFloat(stream, vector.y);
            WriteFloat(stream, vector.z);
        }

        protected static void WriteString(MemoryStream stream, string str)
        {
            var serstr = Encoding.Unicode.GetBytes(str);
            stream.Write(serstr, 0, serstr.Length);
        }
    }

    public enum PacketType
    {
        IDAttribution,
        IDAttributionAck,
        NewPlayer,
        PlayerDisconnected,
        PlayerPosition,
        PlayerList,
        Disconnect,
        NewBoard,
        GlobalIDAttribution,
        DeleteBoard,
        MovedBoard
    }

    [Serializable]
    public class PlayerIDAttribution : Packet
    {
        public int ID { get; set; }
        public PlayerIDAttribution(int id) { this.ID = id; }

        public override byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            WriteType(stream, PacketType.IDAttribution);
            WriteInt(stream, ID);
            return stream.ToArray();
        }
    }

    [Serializable]
    public class IDAttibutionACK : Packet
    {
        public override byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            WriteType(stream, PacketType.IDAttributionAck);
            return stream.ToArray();
        }
    }

    [Serializable]
    public class NewPlayer : Packet
    {
        public int ID { get; set; }
        public NewPlayer(int id) { this.ID = id; }

        public override byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            WriteType(stream, PacketType.NewPlayer);
            WriteInt(stream, ID);
            return stream.ToArray();
        }
    }

    [Serializable]
    public class PlayerDisconnected : Packet
    {
        public int ID { get; set; }
        public PlayerDisconnected(int id) { this.ID = id; }

        public override byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            WriteType(stream, PacketType.PlayerDisconnected);
            WriteInt(stream, ID);
            return stream.ToArray();
        }
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

        public override byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            WriteType(stream, PacketType.PlayerPosition);
            WriteInt(stream, PlayerID);
            WriteV3(stream, Pos);
            WriteV3(stream, Angles);
            return stream.ToArray();
        }
    }

    [Serializable]
    public class PlayerList : Packet
    {
        public PlayerPosition[] List { get; set; }
        public PlayerList(PlayerPosition[] list) { this.List = list; }

        public override byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            WriteType(stream, PacketType.PlayerList);
            WriteInt(stream, List.Length);
            foreach(var position in List)
            {
                WriteInt(stream, position.PlayerID);
                WriteV3(stream, position.Pos);
                WriteV3(stream, position.Angles);
            }
            return stream.ToArray();
        }
    }

    [Serializable]
    public class DisconnectPacket : Packet
    {
        public string Reason { get; set; }
        public DisconnectPacket(string reason) { this.Reason = reason; }

        public override byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            WriteType(stream, PacketType.Disconnect);
            WriteString(stream, Reason);
            return stream.ToArray();
        }
    }

    [Serializable]
    public class NewBoard : Packet
    {
        public int ID;
        public int Parent = -1;
        public int Width, Height;
        public v3 Position, Angles;

        public override byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            WriteType(stream, PacketType.NewBoard);
            WriteInt(stream, ID);
            WriteInt(stream, Parent);
            WriteInt(stream, Width);
            WriteInt(stream, Height);
            return stream.ToArray();
        }
    }

    [Serializable]
    public class GlobalIDAttribution : Packet
    {
        public int LocalID, GlobalID;

        public override byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            WriteType(stream, PacketType.GlobalIDAttribution);
            WriteInt(stream, LocalID);
            WriteInt(stream, GlobalID);
            return stream.ToArray();
        }
    }

    [Serializable]
    public class DeleteBoard : Packet
    {
        public int ID;

        public override byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            WriteType(stream, PacketType.DeleteBoard);
            WriteInt(stream, ID);
            return stream.ToArray();
        }
    }

    [Serializable]
    public class MovedBoard : Packet
    {
        public int ID;
        public int Parent = -1;
        public v3 Position, Rotation;

        public override byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            WriteType(stream, PacketType.MovedBoard);
            WriteInt(stream, ID);
            WriteInt(stream, Parent);
            WriteV3(stream, Position);
            WriteV3(stream, Rotation);
            return stream.ToArray();
        }
    }
}
