using System;
using DarkMultiPlayerCommon;
using MessageStream;

namespace DarkMultiPlayerServer.MessageHandler
{
    public class GroupSystemHandler
    {
        public static void HandleMessage(ClientObject client, byte[] messageData)
        {
            using (MessageReader mr = new MessageReader(messageData, false))
            {
                int messageType = mr.Read<int>();
                string fromPlayer = mr.Read<string>();
                switch ((GroupMessageType)messageType)
                {
                    case GroupMessageType.CREATE:
                        break;
                    case GroupMessageType.DISBAND:
                        break;
                    case GroupMessageType.JOIN:
                        break;
                    case GroupMessageType.LEAVE:
                        break;
                }
            }
        }
    }
}

