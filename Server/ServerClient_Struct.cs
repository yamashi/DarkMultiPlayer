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
            }
            return null;
        }
    }
}

