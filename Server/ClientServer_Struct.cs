/******************************************************************/
/*          This file was generated, do not modify it !!!!        */
/******************************************************************/


using DarkMultiPlayerCommon;
using Lidgren.Network;
using System;

namespace Messages
{   
    public class LockEntry
    {
        public LockEntry() {}
        ~LockEntry() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            key = pPacketReader.ReadString();
            value = pPacketReader.ReadString();
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(key);
            pPacketWriter.Write(value);
        }
    
        public string key;
        public string value;
    }
    
    public class ClientServer_HeartBeatRecv : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0000;} }
        public ClientServer_HeartBeatRecv() {}
        ~ClientServer_HeartBeatRecv() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            name = pPacketReader.ReadString();
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            pPacketWriter.Write(name);
        }
    
        public string name;
    }
    
    public class ClientServer_ChatMessageRecv : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0002;} }
        public ClientServer_ChatMessageRecv() {}
        ~ClientServer_ChatMessageRecv() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            type = pPacketReader.ReadByte();
            name = pPacketReader.ReadString();
            channel = pPacketReader.ReadString();
            message = pPacketReader.ReadString();
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            pPacketWriter.Write(type);
            pPacketWriter.Write(name);
            pPacketWriter.Write(channel);
            pPacketWriter.Write(message);
        }
    
        public byte type;
        public string name;
        public string channel;
        public string message;
    }
    
    public class ClientServer_LockAcquireRecv : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0009;} }
        public ClientServer_LockAcquireRecv() {}
        ~ClientServer_LockAcquireRecv() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            name = pPacketReader.ReadString();
            force = pPacketReader.ReadBool();
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            pPacketWriter.Write(name);
            pPacketWriter.Write(force);
        }
    
        public string name;
        public bool force;
    }
    
    public class ClientServer_LockReleaseRecv : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x000A;} }
        public ClientServer_LockReleaseRecv() {}
        ~ClientServer_LockReleaseRecv() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            name = pPacketReader.ReadString();
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            pPacketWriter.Write(name);
        }
    
        public string name;
    }
    
    public class ServerClient_HeartBeatSend : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0001;} }
        public ServerClient_HeartBeatSend() {}
        ~ServerClient_HeartBeatSend() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            name = pPacketReader.ReadString();
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            pPacketWriter.Write(name);
        }
    
        public string name;
    }
    
    public class ServerClient_ChatMessageSend : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0003;} }
        public ServerClient_ChatMessageSend() {}
        ~ServerClient_ChatMessageSend() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            type = pPacketReader.ReadByte();
            name = pPacketReader.ReadString();
            channel = pPacketReader.ReadString();
            message = pPacketReader.ReadString();
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            pPacketWriter.Write(type);
            pPacketWriter.Write(name);
            pPacketWriter.Write(channel);
            pPacketWriter.Write(message);
        }
    
        public byte type;
        public string name;
        public string channel;
        public string message;
    }
    
    public class ServerClient_AdminAddSend : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0004;} }
        public ServerClient_AdminAddSend() {}
        ~ServerClient_AdminAddSend() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            name = pPacketReader.ReadString();
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            pPacketWriter.Write(name);
        }
    
        public string name;
    }
    
    public class ServerClient_AdminRemoveSend : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0005;} }
        public ServerClient_AdminRemoveSend() {}
        ~ServerClient_AdminRemoveSend() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            name = pPacketReader.ReadString();
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            pPacketWriter.Write(name);
        }
    
        public string name;
    }
    
    public class ServerClient_PlayerLeaveSend : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0006;} }
        public ServerClient_PlayerLeaveSend() {}
        ~ServerClient_PlayerLeaveSend() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            name = pPacketReader.ReadString();
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            pPacketWriter.Write(name);
        }
    
        public string name;
    }
    
    public class ServerClient_ModDataSend : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0007;} }
        public ServerClient_ModDataSend() {}
        ~ServerClient_ModDataSend() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            name = pPacketReader.ReadString();
            {
                UInt32 length = 0;
                length = pPacketReader.ReadUInt32();
                data = new byte[length];
                for(UInt32 i = 0; i < length; ++i)
                {
                    data[i] = pPacketReader.ReadByte();
                }
            }
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            pPacketWriter.Write(name);
            {
                UInt32 length = (UInt32)data.Length;
                pPacketWriter.Write(length);
                for(UInt32 i = 0; i < length; ++i)
                {
                    pPacketWriter.Write(data[i]);
                }
            }
        }
    
        public string name;
        public byte[] data = new byte[0];
    }
    
    public class ServerClient_VesselRemoveSend : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0008;} }
        public ServerClient_VesselRemoveSend() {}
        ~ServerClient_VesselRemoveSend() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            name = pPacketReader.ReadString();
            subspace = pPacketReader.ReadInt32();
            clock = pPacketReader.ReadDouble();
            unk = pPacketReader.ReadBool();
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            pPacketWriter.Write(name);
            pPacketWriter.Write(subspace);
            pPacketWriter.Write(clock);
            pPacketWriter.Write(unk);
        }
    
        public string name;
        public Int32 subspace;
        public double clock;
        public bool unk;
    }
    
    public class ServerClient_LockAcquireSend : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x000B;} }
        public ServerClient_LockAcquireSend() {}
        ~ServerClient_LockAcquireSend() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            name = pPacketReader.ReadString();
            player = pPacketReader.ReadString();
            result = pPacketReader.ReadBool();
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            pPacketWriter.Write(name);
            pPacketWriter.Write(player);
            pPacketWriter.Write(result);
        }
    
        public string name;
        public string player;
        public bool result;
    }
    
    public class ServerClient_LockReleaseSend : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x000C;} }
        public ServerClient_LockReleaseSend() {}
        ~ServerClient_LockReleaseSend() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            name = pPacketReader.ReadString();
            player = pPacketReader.ReadString();
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            pPacketWriter.Write(name);
            pPacketWriter.Write(player);
        }
    
        public string name;
        public string player;
    }
    
    public class ServerClient_LockListSend : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x000D;} }
        public ServerClient_LockListSend() {}
        ~ServerClient_LockListSend() {}
    
        public void Deserialize(NetBuffer pPacketReader)
        {
            {
                UInt32 length = 0;
                length = pPacketReader.ReadUInt32();
                locks = new LockEntry[length];
                for(UInt32 i = 0; i < length; ++i)
                {
                    locks[i].Deserialize(pPacketReader);
                }
            }
        }
        public void Serialize(NetBuffer pPacketWriter)
        {
            pPacketWriter.Write(Opcode);
            {
                UInt32 length = (UInt32)locks.Length;
                pPacketWriter.Write(length);
                for(UInt32 i = 0; i < length; ++i)
                {
                    locks[i].Serialize(pPacketWriter);
                }
            }
        }
    
        public LockEntry[] locks = new LockEntry[0];
    }
    
    public class ClientServerMessageFactory : IMessageFactory
    {
        public IMessage Create(ushort opcode)
        {
            switch(opcode)
            {
                case 0x0000:
                    return new ClientServer_HeartBeatRecv();
                case 0x0002:
                    return new ClientServer_ChatMessageRecv();
                case 0x0009:
                    return new ClientServer_LockAcquireRecv();
                case 0x000A:
                    return new ClientServer_LockReleaseRecv();
            }
            return null;
        }
    }
}

