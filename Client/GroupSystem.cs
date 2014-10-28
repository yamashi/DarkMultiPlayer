using System;
using System.Collections.Generic;
using DarkMultiPlayerCommon;
using MessageStream;

namespace DarkMultiPlayer
{
    public class GroupSystem
    {
        private Dictionary<string, GroupObject> groupInfo = new Dictionary<string, GroupObject>();
        //playerName, groupName
        private Dictionary<string, string> playerGroup = new Dictionary<string, string>();
        private object lockObject = new object();

        public void HandleLockMessage(byte[] messageData)
        {
            lock (lockObject)
            {
                using (MessageReader mr = new MessageReader(messageData, false))
                {
                    GroupMessageType messageType = (GroupMessageType)mr.Read<int>();
                    string fromPlayer = mr.Read<string>();
                    switch (messageType)
                    {
                        case GroupMessageType.LIST:
                            {

                            }
                            break;
                        case GroupMessageType.CREATE:
                            {
                                string groupName = mr.Read<string>();
                                GroupObject newGroup = new GroupObject(fromPlayer, GroupPrivacy.PUBLIC);
                                newGroup.groupOwner = fromPlayer;
                                groupInfo[groupName] = newGroup;
                                playerGroup[fromPlayer] = groupName;
                            }
                            break;
                        case GroupMessageType.DISBAND:
                            {
                                string groupName = mr.Read<string>();
                                if (playerGroup.ContainsKey(fromPlayer))
                                {
                                    playerGroup.Remove(fromPlayer);
                                }
                                if (groupInfo.ContainsKey(groupName))
                                {
                                    groupInfo.Remove(groupName);
                                }
                            }
                            break;
                        case GroupMessageType.JOIN:
                            {
                                string groupName = mr.Read<string>();
                                playerGroup[fromPlayer] = groupName;
                            }
                            break;
                        case GroupMessageType.LEAVE:
                            {
                                if (playerGroup.ContainsKey(fromPlayer))
                                {
                                    playerGroup.Remove(fromPlayer);
                                }
                            }
                            break;
                
                    }
                }
            }
        }


    }
}

