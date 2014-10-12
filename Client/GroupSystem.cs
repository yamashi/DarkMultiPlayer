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
    }
}

