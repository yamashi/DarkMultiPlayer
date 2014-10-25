using System;
using DarkMultiPlayerCommon;
using MessageStream;

namespace DarkMultiPlayerServer.MessageHandler
{
    public class GroupSystemHandler
    {
        public static void HandleMessage(ClientObject client, byte[] messageData)
        {
            bool relayMessage = false;
            using (MessageReader mr = new MessageReader(messageData, false))
            {
                int messageType = mr.Read<int>();
                string fromPlayer = mr.Read<string>();
                if (client.playerName != fromPlayer)
                {
                    ClientHandler.SendConnectionEnd(client, "Kicked for sending a group message for another player");
                    return;
                }
                switch ((GroupMessageType)messageType)
                {
                    case GroupMessageType.CREATE:
                        {
                            string groupName = mr.Read<string>();
                            if (GroupSystem.fetch.CreateGroup(groupName, fromPlayer, GroupPrivacy.PUBLIC))
                            {
                                ServerMessage newMessage = new ServerMessage();
                                newMessage.type = ServerMessageType.GROUP_SYSTEM;
                                newMessage.data = messageData;
                                //Relay it back to everyone to confirm
                                ClientHandler.SendToAll(null, newMessage, true);
                            }
                            else
                            {
                                //Notify of failure
                                ClientHandler.SendChatMessageToClient(client, "Failed to create group: Group already exists");
                            }
                        }
                        break;
                    case GroupMessageType.DISBAND:
                        {
                            string groupName = mr.Read<string>();
                            if (GroupSystem.fetch.GroupExists(groupName) && (GroupSystem.fetch.GetGroupOwner(groupName) == client.playerName))
                            {
                                if (GroupSystem.fetch.RemoveGroup(groupName))
                                {
                                    relayMessage = true;
                                }
                                else
                                {
                                    ClientHandler.SendChatMessageToClient(client, "Failed to delete group");
                                }
                            }
                            else
                            {
                                //Notify of failure
                                ClientHandler.SendChatMessageToClient(client, "Failed to delete group: You aren't the owner");
                            }
                        }
                        break;
                    case GroupMessageType.JOIN:
                        {
                            string groupName = mr.Read<string>();

                            
                            if (GroupSystem.fetch.GetGroupPrivacy(groupName) == GroupPrivacy.PUBLIC)
                            {
                                //Public group
                                if (GroupSystem.fetch.JoinGroup(groupName, fromPlayer))
                                {
                                    relayMessage = true;
                                }
                                else
                                {
                                    ClientHandler.SendChatMessageToClient(client, "Failed to join group");
                                }
                            }
                            else
                            {
                                //Private group
                                string groupPassword = mr.Read<string>();
                                if (GroupSystem.fetch.CheckGroupPassword(groupName, groupPassword))
                                {
                                    if (GroupSystem.fetch.JoinGroup(groupName, fromPlayer))
                                    {
                                        relayMessage = true;
                                    }
                                    else
                                    {
                                        ClientHandler.SendChatMessageToClient(client, "Failed to join group");
                                    }
                                }
                                else
                                {
                                    //Notify of failure
                                    ClientHandler.SendChatMessageToClient(client, "Failed to join group: Incorrect password");
                                }
                            }
                        }
                        break;
                    case GroupMessageType.LEAVE:
                        {
                            string groupName = mr.Read<string>();

                            if (GroupSystem.fetch.GetGroupOwner(groupName) != fromPlayer)
                            {
                                if (GroupSystem.fetch.GetPlayerGroup(fromPlayer) == groupName)
                                {
                                    if (GroupSystem.fetch.LeaveGroup(fromPlayer))
                                    {
                                        relayMessage = true;
                                    }
                                    else
                                    {
                                        ClientHandler.SendChatMessageToClient(client, "Failed to leave group");
                                    }
                                }
                                else
                                {
                                    ClientHandler.SendChatMessageToClient(client, "Failed to leave group: You aren't a member of " + groupName);
                                }

                            }
                            else
                            {
                            }
                        }
                        break;
                }
            }
            if (relayMessage)
            {
                ServerMessage newMessage = new ServerMessage();
                newMessage.type = ServerMessageType.GROUP_SYSTEM;
                newMessage.data = messageData;
                ClientHandler.SendToAll(null, newMessage, true);
            }
        }
    }
}

