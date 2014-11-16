using DarkMultiPlayerCommon;
using DarkMultiPlayerCommon.Events;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkMultiPlayerServer
{
    public class GameServerConnectionEvent : IEvent
    {
        public long Id { get; set; }
    }
    public class GameServerDisconnectionEvent : IEvent
    {
        public long Id { get; set; }
    }
    class GameServer : GameNetwork
    {
        private NetServer m_server = null;
        private readonly Dictionary<long, NetConnection> m_connections = null;

        public enum SendType
        {
            KOrdered,
            KUnordered,
            KUnreliable
        }

        public GameServer(int aPort, IMessageFactory aFactory) : base(aFactory)
        {
            m_server = StartServer(aPort);
            m_connections = new Dictionary<long, NetConnection>();
        }

        protected override NetOutgoingMessage CreateMessage()
        {
            return m_server.CreateMessage();
        }

        protected override void ProcessNetwork()
        {
            NetIncomingMessage inc;
            while ((inc = m_server.ReadMessage()) != null)
            {
                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)inc.ReadByte();
                        switch (status)
                        {
                            case NetConnectionStatus.Connected:
                                Trigger(new GameServerConnectionEvent { Id = inc.SenderConnection.RemoteUniqueIdentifier });
                                break;
                            case NetConnectionStatus.Disconnected:
                                m_connections.Remove(inc.SenderConnection.RemoteUniqueIdentifier);
                                Trigger(new GameServerDisconnectionEvent { Id = inc.SenderConnection.RemoteUniqueIdentifier });
                                break;
                            case NetConnectionStatus.Disconnecting:
                            case NetConnectionStatus.InitiatedConnect:
                            case NetConnectionStatus.RespondedConnect:
                                break;
                        }
                        break;
                    //Check for client attempting to connect
                    case NetIncomingMessageType.ConnectionApproval:
                        HandleConnectionChallenge(inc);
                        break;
                    case NetIncomingMessageType.Data:
                        HandleData(inc);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                        DarkLog.Debug(inc.ReadString());
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        DarkLog.Normal(inc.ReadString());
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        DarkLog.Error(inc.ReadString());
                        break;
                }
                m_server.Recycle(inc);
            }
        }

        private void HandleData(NetIncomingMessage aMessage)
        { 
            if(aMessage.LengthBytes < 2)
                return;

            IMessage message = m_factory.Create(aMessage.ReadUInt16());

            message.ConnectionId = aMessage.SenderConnection.RemoteUniqueIdentifier;

            Trigger(message);
        }

        private void HandleConnectionChallenge(NetIncomingMessage aMessage)
        {

        }

        public void Broadcast(IMessage aMessage)
        {
            m_server.SendToAll(Pack(aMessage), NetDeliveryMethod.ReliableOrdered);
        }

        public void BroadcastBut(IMessage aMessage, long aId)
        {
            NetConnection connection;
            if (m_connections.TryGetValue(aId, out connection))
            {
                m_server.SendToAll(Pack(aMessage), connection, NetDeliveryMethod.ReliableOrdered, 0);
            }
            else 
            {
                Broadcast(aMessage);
            }
        }

        public void SendTo(IMessage aMessage, long aId, SendType aSendType = SendType.KOrdered, int aChannel = 0)
        {
            NetConnection connection;
            if (m_connections.TryGetValue(aId, out connection))
            {
                NetDeliveryMethod method =  aSendType == SendType.KOrdered ? NetDeliveryMethod.ReliableOrdered :
                                            aSendType == SendType.KUnordered ? NetDeliveryMethod.ReliableUnordered :
                                            NetDeliveryMethod.Unreliable;

                connection.SendMessage(Pack(aMessage), method, aChannel);
            }
        }

        public void Kick(long aId, string aReason)
        {
            NetConnection connection;
            if (m_connections.TryGetValue(aId, out connection))
            {
                connection.Disconnect(aReason);
            }
        }

        public void KickAll(string aReason)
        {
            foreach(var pair in m_connections)
            {
                pair.Value.Disconnect(aReason);
            }
        }
    }
}
