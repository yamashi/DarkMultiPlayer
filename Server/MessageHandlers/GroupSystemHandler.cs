using System;
using System.Collections.Generic;
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
                GroupMessageType messageType = (GroupMessageType)mr.Read<int>();
                string fromPlayer = mr.Read<string>();
                if (client.playerName != fromPlayer)
                {
                    ClientHandler.SendConnectionEnd(client, "Kicked for sending a group message for another player");
                    return;
                }
                switch (messageType)
                {
                    case GroupMessageType.CREATE:
                        {
                            string groupName = mr.Read<string>();
                            GroupPrivacy groupPrivacy = (GroupPrivacy)mr.Read<int>();
                            if (GroupSystem.fetch.CreateGroup(groupName, fromPlayer, groupPrivacy))
                            {
                                relayMessage = true;
                            }
                            else
                            {
                                //Notify of failure
                                ClientHandler.SendChatMessageToClient(client, "Failed to create group: Group already exists");
                            }
                        }
                        break;
                    case GroupMessageType.HANDOVER_OWNERSHIP:
                        {
                            string groupName = mr.Read<string>();
                            string toPlayer = mr.Read<string>();
                            if (GroupSystem.fetch.GroupExists(groupName))
                            {
                                if (GroupSystem.fetch.GetGroupOwner(groupName) == fromPlayer)
                                {
                                    if (GroupSystem.fetch.SetGroupOwner(groupName, toPlayer))
                                    {
                                        relayMessage = true;
                                    }
                                    else
                                    {
                                        //Notify of failure
                                        ClientHandler.SendChatMessageToClient(client, "Failed to handover group.");
                                    }
                                }
                                else
                                {
                                    ClientHandler.SendChatMessageToClient(client, "Failed to handover group: You are not the group owner");
                                }
                            }
                            else
                            {
                                ClientHandler.SendChatMessageToClient(client, "Failed to handover group: Group does not exist");
                            }
                        }
                        break;
                    case GroupMessageType.SET_PASSWORD:
                        {
                            string groupName = mr.Read<string>();
                            string groupSalt = mr.Read<string>();
                            string groupPassword = mr.Read<string>();

                            if (GroupSystem.fetch.GroupExists(groupName))
                            {
                                if (GroupSystem.fetch.GetGroupOwner(groupName) == fromPlayer)
                                {
                                    if (GroupSystem.fetch.SetGroupPassword(groupName, groupSalt, groupPassword))
                                    {
                                        relayMessage = true;
                                    }
                                    else
                                    {
                                        //Notify of failure
                                        ClientHandler.SendChatMessageToClient(client, "Failed to set password.");
                                    }
                                }
                                else
                                {
                                    ClientHandler.SendChatMessageToClient(client, "Failed to set password: You are not the group owner");
                                }
                            }
                            else
                            {
                                ClientHandler.SendChatMessageToClient(client, "Failed to set password: Group doesn't exist");
                            }
                        }
                        break;
                    case GroupMessageType.SET_PRIVACY:
                        {
                            string groupName = mr.Read<string>();
                            GroupPrivacy groupPrivacy = (GroupPrivacy)mr.Read<int>();
                            if (GroupSystem.fetch.GroupExists(groupName))
                            {
                                if (GroupSystem.fetch.GetGroupOwner(groupName) == fromPlayer)
                                {
                                    if (GroupSystem.fetch.SetGroupPrivacy(groupName, groupPrivacy))
                                    {
                                        relayMessage = true;
                                    }
                                    else
                                    {
                                        //Notify of failure
                                        ClientHandler.SendChatMessageToClient(client, "Failed to set privacy");
                                    }
                                }
                                else
                                {
                                    ClientHandler.SendChatMessageToClient(client, "Failed to set privacy: You are not the group owner");
                                }
                            }
                            else
                            {
                                ClientHandler.SendChatMessageToClient(client, "Failed to set privacy: Group doesn't exist");
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
                                    ServerMessage newMessage = new ServerMessage();
                                    newMessage.type = ServerMessageType.GROUP_SYSTEM;
                                    using (MessageWriter mw = new MessageWriter())
                                    {
                                        mw.Write<int>((int)GroupMessageType.JOIN);
                                        mw.Write<string>(fromPlayer);
                                        mw.Write<string>(groupName);
                                        newMessage.data = mw.GetMessageBytes();
                                    }
                                    ClientHandler.SendToAll(null, newMessage, true);
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
                                        ServerMessage newMessage = new ServerMessage();
                                        newMessage.type = ServerMessageType.GROUP_SYSTEM;
                                        using (MessageWriter mw = new MessageWriter())
                                        {
                                            mw.Write<int>((int)GroupMessageType.JOIN);
                                            mw.Write<string>(fromPlayer);
                                            mw.Write<string>(groupName);
                                            newMessage.data = mw.GetMessageBytes();
                                        }
                                        ClientHandler.SendToAll(null, newMessage, true);
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
                                ClientHandler.SendChatMessageToClient(client, "Failed to leave group: You are the owner of " + groupName);
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

        private void SendGroupList(ClientObject client)
        {
            List<string> groupNames = new List<string>();
            List<GroupPrivacy> groupPrivacy = new List<GroupPrivacy>();
            Dictionary<string, List<string>> groupMembers = new Dictionary<string, List<string>>();
        }
    }
}

