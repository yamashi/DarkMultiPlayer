using DarkMultiPlayerCommon;
using DarkMultiPlayerCommon.Events;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkMultiPlayerCommon
{
    public abstract class GameNetwork : EventAggregator
    {
        protected IMessageFactory m_factory;

        public GameNetwork(IMessageFactory aFactory) : base()
        {
            m_factory = aFactory;
        }

        protected NetServer StartServer(int aPort)
        {
            NetPeerConfiguration npConfig = new NetPeerConfiguration("DMP");
            npConfig.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            npConfig.EnableMessageType(NetIncomingMessageType.StatusChanged);
            npConfig.EnableMessageType(NetIncomingMessageType.Data);
            npConfig.Port = aPort;

            NetServer server = new NetServer(npConfig);
            server.Start();

            return server;
        }

        public void Update()
        {
            ProcessNetwork();

            Run();
        }

        protected abstract void ProcessNetwork();

        protected abstract NetOutgoingMessage CreateMessage();

        protected NetOutgoingMessage Pack(IMessage aMessage)
        {
            NetOutgoingMessage msg = CreateMessage();
 
            aMessage.Serialize(msg);

            return msg;
        }
    }
}
