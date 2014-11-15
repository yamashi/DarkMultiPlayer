/******************************************************************/
/*          This file was generated, do not modify it !!!!        */
/******************************************************************/


using DarkMultiPlayerCommon;
using Lidgren.Network;
using System;

namespace Messages
{   
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
            }
            return null;
        }
    }
}

