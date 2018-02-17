using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private static Byte[] typeBuffer = new Byte[sizeof(PacketType)];

        public static PacketType GetType(Byte[] data)
        {
            return (PacketType)BitConverter.ToInt32(data, 0);
        }

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

        protected static v3 ReadV3(Byte[] data, int offset)
        {
            float x = BitConverter.ToSingle(data, offset);
            offset += sizeof(int);
            float y = BitConverter.ToSingle(data, offset);
            offset += sizeof(int);
            float z = BitConverter.ToSingle(data, offset);
            offset += sizeof(int);
            return new v3(x, y, z);
        }

        protected static void WriteString(MemoryStream stream, string str)
        {
            var serstr = Encoding.Unicode.GetBytes(str);
            WriteInt(stream, serstr.Length);
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

        public static PlayerIDAttribution Deserialize(Byte[] data)
        {
#if DEBUG
            PacketType type = (PacketType)BitConverter.ToInt32(data, 0);
            Debug.Assert(type == PacketType.IDAttribution);
#endif
            int offset = sizeof(PacketType);
            int ID = BitConverter.ToInt32(data, offset);
            return new PlayerIDAttribution(ID);
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

        public static IDAttibutionACK Deserialize(Byte[] data)
        {
#if DEBUG
            PacketType type = (PacketType)BitConverter.ToInt32(data, 0);
            Debug.Assert(type == PacketType.IDAttributionAck);
#endif
            return new IDAttibutionACK();
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

        public static NewPlayer Deserialize(Byte[] data)
        {
#if DEBUG
            PacketType type = (PacketType)BitConverter.ToInt32(data, 0);
            Debug.Assert(type == PacketType.NewPlayer);
#endif
            int offset = sizeof(PacketType);
            int ID = BitConverter.ToInt32(data, offset);
            return new NewPlayer(ID);
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

        public static PlayerDisconnected Deserialize(Byte[] data)
        {
#if DEBUG
            PacketType type = (PacketType)BitConverter.ToInt32(data, 0);
            Debug.Assert(type == PacketType.PlayerDisconnected);
#endif
            int offset = sizeof(PacketType);
            int ID = BitConverter.ToInt32(data, offset);
            return new PlayerDisconnected(ID);
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

        public static PlayerPosition Deserialize(Byte[] data, int offset = 0)
        {
#if DEBUG
            PacketType type = (PacketType)BitConverter.ToInt32(data, offset);
            Debug.Assert(type == PacketType.PlayerPosition);
#endif
            offset += sizeof(PacketType);
            int PlayerID = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            v3 Pos = ReadV3(data, offset);
            offset += 3 * sizeof(float);
            v3 Angles = ReadV3(data, offset);
            return new PlayerPosition(PlayerID, Pos, Angles);
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

        public static PlayerList Deserialize(Byte[] data)
        {
#if DEBUG
            PacketType type = (PacketType)BitConverter.ToInt32(data, 0);
            Debug.Assert(type == PacketType.PlayerList);
#endif
            int _base = sizeof(PacketType);
            int length = BitConverter.ToInt32(data, _base);
            PlayerPosition[] List = new PlayerPosition[length];
            _base += sizeof(int);
            for(int index = 0; index < length; index++)
            {
                int offset = _base + index * (sizeof(int) + 6 * sizeof(float));
                List[index] = PlayerPosition.Deserialize(data, offset);
            }
            return new PlayerList(List);
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

        public static DisconnectPacket Deserialize(Byte[] data)
        {
#if DEBUG
            PacketType type = (PacketType)BitConverter.ToInt32(data, 0);
            Debug.Assert(type == PacketType.Disconnect);
#endif
            int offset = sizeof(PacketType);
            int length = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            string reason = Encoding.Unicode.GetString(data, offset, length);
            return new DisconnectPacket(reason);
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

        public static NewBoard Deserialize(Byte[] data)
        {
#if DEBUG
            PacketType type = (PacketType)BitConverter.ToInt32(data, 0);
            Debug.Assert(type == PacketType.NewBoard);
#endif
            int offset = sizeof(PacketType);
            int ID = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            int Parent = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            int Width = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            int Height = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            v3 Position = ReadV3(data, offset);
            offset += 3 * sizeof(float);
            v3 Angles = ReadV3(data, offset);
            return new NewBoard
            {
                ID = ID,
                Parent = Parent,
                Width = Width, Height = Height,
                Position = Position, Angles = Angles
            };
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

        public static GlobalIDAttribution Deserialize(Byte[] data)
        {
#if DEBUG
            PacketType type = (PacketType)BitConverter.ToInt32(data, 0);
            Debug.Assert(type == PacketType.GlobalIDAttribution);
#endif
            int offset = sizeof(PacketType);
            int LocalID = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            int GlobalID = BitConverter.ToInt32(data, offset);
            return new GlobalIDAttribution
            {
                LocalID = LocalID, GlobalID = GlobalID
            };
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

        public static DeleteBoard Deserialize(Byte[] data)
        {
#if DEBUG
            PacketType type = (PacketType)BitConverter.ToInt32(data, 0);
            Debug.Assert(type == PacketType.DeleteBoard);
#endif
            int offset = sizeof(PacketType);
            int ID = BitConverter.ToInt32(data, offset);
            return new DeleteBoard { ID = ID };
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

        public static MovedBoard Deserialize(Byte[] data)
        {
#if DEBUG
            PacketType type = (PacketType)BitConverter.ToInt32(data, 0);
            Debug.Assert(type == PacketType.MovedBoard);
#endif
            int offset = sizeof(PacketType);
            int ID = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            int Parent = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            v3 Position = ReadV3(data, offset);
            offset += 3 * sizeof(float);
            v3 Rotation = ReadV3(data, offset);
            return new MovedBoard
            {
                ID = ID,
                Parent = Parent,
                Position = Position, Rotation = Rotation
            };
        }
    }
}
