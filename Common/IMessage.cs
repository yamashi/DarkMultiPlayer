using DarkMultiPlayerCommon.Events;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace DarkMultiPlayerCommon
{
    public interface IMessage : IEvent
    {
        long ConnectionId { get; set; }
        ushort Opcode { get; }

        void Serialize(NetBuffer pPacketWriter);
        void Deserialize(NetBuffer pPacketReader);
    }
}
