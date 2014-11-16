/******************************************************************/
/*          This file was generated, do not modify it !!!!        */
/******************************************************************/


using DarkMultiPlayerCommon;
using Lidgren.Network;
using System;

namespace Messages
{   
    public class ServerClient_HeartBeatRecv : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0001;} }
        public ServerClient_HeartBeatRecv() {}
        ~ServerClient_HeartBeatRecv() {}
    
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
    
    public class ServerClient_ChatMessageRecv : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0003;} }
        public ServerClient_ChatMessageRecv() {}
        ~ServerClient_ChatMessageRecv() {}
    
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
    
    public class ServerClient_AdminAddRecv : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0004;} }
        public ServerClient_AdminAddRecv() {}
        ~ServerClient_AdminAddRecv() {}
    
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
    
    public class ServerClient_AdminRemoveRecv : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0005;} }
        public ServerClient_AdminRemoveRecv() {}
        ~ServerClient_AdminRemoveRecv() {}
    
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
    
    public class ServerClient_PlayerLeaveRecv : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0006;} }
        public ServerClient_PlayerLeaveRecv() {}
        ~ServerClient_PlayerLeaveRecv() {}
    
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
    
    public class ServerClient_ModDataRecv : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0007;} }
        public ServerClient_ModDataRecv() {}
        ~ServerClient_ModDataRecv() {}
    
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
    
    public class ServerClient_VesselRemoveRecv : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0008;} }
        public ServerClient_VesselRemoveRecv() {}
        ~ServerClient_VesselRemoveRecv() {}
    
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
    
    public class ClientServer_HeartBeatSend : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0000;} }
        public ClientServer_HeartBeatSend() {}
        ~ClientServer_HeartBeatSend() {}
    
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
    
    public class ClientServer_ChatMessageSend : IMessage
    {
        public long ConnectionId { get; set; }
        public ushort Opcode { get{ return 0x0002;} }
        public ClientServer_ChatMessageSend() {}
        ~ClientServer_ChatMessageSend() {}
    
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
    
    public class ServerClientMessageFactory : IMessageFactory
    {
        public IMessage Create(ushort opcode)
        {
            switch(opcode)
            {
                case 0x0001:
                    return new ServerClient_HeartBeatRecv();
                case 0x0003:
                    return new ServerClient_ChatMessageRecv();
                case 0x0004:
                    return new ServerClient_AdminAddRecv();
                case 0x0005:
                    return new ServerClient_AdminRemoveRecv();
                case 0x0006:
                    return new ServerClient_PlayerLeaveRecv();
                case 0x0007:
                    return new ServerClient_ModDataRecv();
                case 0x0008:
                    return new ServerClient_VesselRemoveRecv();
            }
            return null;
        }
    }
}

