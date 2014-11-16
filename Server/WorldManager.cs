using System;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Cryptography;
using MessageStream;
using System.IO;
using DarkMultiPlayerCommon;
using Lidgren.Network;
using DarkMultiPlayerCommon.Events;

namespace DarkMultiPlayerServer
{
    public class WorldManager : EventAggregator
    {
        private static string subspaceFile = Path.Combine(Server.universeDirectory, "subspace.txt");
        private static string adminListFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "DMPAdmins.txt");
        private static string whitelistFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "DMPWhitelist.txt");

        private static GameServer m_server;

        private static WorldManager m_instance = new WorldManager();
        public static WorldManager Instance { get { return m_instance; } }

        private SafeList<ClientObject> m_clients;
        private Dictionary<int, Subspace> subspaces;
        private Dictionary<string, int> playerSubspace;
        private Dictionary<string, List<string>> playerChatChannels;
        private List<string> serverAdmins;
        private List<string> serverWhitelist;
        private Dictionary<string, int> playerUploadedScreenshotIndex;
        private Dictionary<string, Dictionary<string,int>> playerDownloadedScreenshotIndex;
        private Dictionary<string, string> playerWatchScreenshot;
        private LockSystem lockSystem;

        private BanList m_banList = null;

        private int ActiveClientCount
        {
            get
            {
                int authenticatedCount = 0;
                m_clients.Iterate((x) => { if (x.authenticated) authenticatedCount++; });
                return authenticatedCount;
            }
        }

        private string ActivePlayerNames
        {
            get
            {
                string playerString = "";
                m_clients.Iterate(x => { if (x.authenticated) playerString += x.playerName + ", ";  });
                if(playerString.Length > 0)
                {
                    playerString = playerString.Substring(0, playerString.Length - 2);
                }
                return playerString;
            }
        }

        #region Main loop

        public void Update()
        {
            try
            {
                m_clients.Flush();
                // TODO: Save subspace if player leaves and count == 0
                m_server.Run();

                // Dispatch events
                Run();
                //Check timers
                NukeKSC.CheckTimer();
                Dekessler.CheckTimer();
                //Run plugin update
                DMPPluginHandler.FireOnUpdate();

            }
            catch (Exception e)
            {
                DarkLog.Error("Fatal error thrown, exception: " + e);
                Server.ShutDown("Crashed!");
            }

        }
        #endregion

        public WorldManager()
        {
            m_server = new GameServer(Settings.settingsStore.port, new Messages.ClientServerMessageFactory());

            m_clients = new SafeList<ClientObject>();

            subspaces = new Dictionary<int, Subspace>();
            playerSubspace = new Dictionary<string, int>();
            playerChatChannels = new Dictionary<string, List<string>>();
            serverAdmins = new List<string>();
            serverWhitelist = new List<string>();
            playerUploadedScreenshotIndex = new Dictionary<string, int>();
            playerDownloadedScreenshotIndex = new Dictionary<string, Dictionary<string, int>>();
            playerWatchScreenshot = new Dictionary<string, string>();
            lockSystem = new LockSystem();

            m_banList = new BanList();

            LoadSavedSubspace();
            Server.serverRunning = true;
        }

        #region Server setup
        private void LoadSavedSubspace()
        {
            try
            {
                using (StreamReader sr = new StreamReader(subspaceFile))
                {
                    //Ignore the comment line.
                    string firstLine = "";
                    while (firstLine.StartsWith("#") || String.IsNullOrEmpty(firstLine))
                    {
                        firstLine = sr.ReadLine().Trim();
                    }
                    Subspace savedSubspace = new Subspace();
                    int subspaceID = Int32.Parse(firstLine);
                    savedSubspace.serverClock = Int64.Parse(sr.ReadLine().Trim());
                    savedSubspace.planetTime = Double.Parse(sr.ReadLine().Trim());
                    savedSubspace.subspaceSpeed = Single.Parse(sr.ReadLine().Trim());

                    subspaces.Add(subspaceID, savedSubspace);
                }
            }
            catch
            {
                DarkLog.Debug("Creating new subspace lock file");
                Subspace newSubspace = new Subspace();
                newSubspace.serverClock = DateTime.UtcNow.Ticks;
                newSubspace.planetTime = 100d;
                newSubspace.subspaceSpeed = 1f;
                subspaces.Add(0, newSubspace);
                SaveSubspace(0, newSubspace);
            }
        }

        private int GetLatestSubspace()
        {
            int latestID = 0;
            double latestPlanetTime = 0;
            long currentTime = DateTime.UtcNow.Ticks;
            foreach (KeyValuePair<int,Subspace> subspace in subspaces)
            {
                double currentPlanetTime = subspace.Value.planetTime + (((currentTime - subspace.Value.serverClock) / 10000000) * subspace.Value.subspaceSpeed);
                if (currentPlanetTime > latestPlanetTime)
                {
                    latestID = subspace.Key;
                }
            }
            return latestID;
        }

        private void SaveLatestSubspace()
        {
            int latestID = GetLatestSubspace();
            SaveSubspace(latestID, subspaces[latestID]);
        }

        private void UpdateSubspace(int subspaceID)
        {
            //New time = Old time + (seconds since lock * subspace rate)
            long newServerClockTime = DateTime.UtcNow.Ticks;
            float timeSinceLock = (DateTime.UtcNow.Ticks - subspaces[subspaceID].serverClock) / 10000000f;
            double newPlanetariumTime = subspaces[subspaceID].planetTime + (timeSinceLock * subspaces[subspaceID].subspaceSpeed);
            subspaces[subspaceID].serverClock = newServerClockTime;
            subspaces[subspaceID].planetTime = newPlanetariumTime;
        }

        private void SaveSubspace(int subspaceID, Subspace subspace)
        {
            string subspaceFile = Path.Combine(Server.universeDirectory, "subspace.txt");
            using (StreamWriter sw = new StreamWriter(subspaceFile))
            {
                sw.WriteLine("#Incorrectly editing this file will cause weirdness. If there is any errors, the universe time will be reset.");
                sw.WriteLine("#This file can only be edited if the server is stopped.");
                sw.WriteLine("#Each variable is on a new line. They are subspaceID, server clock (from DateTime.UtcNow.Ticks), universe time, and subspace speed.");
                sw.WriteLine(subspaceID);
                sw.WriteLine(subspace.serverClock);
                sw.WriteLine(subspace.planetTime);
                sw.WriteLine(subspace.subspaceSpeed);
            }
        }

        #endregion
        #region Network related methods
        
        private void DisconnectClient(ClientObject client, string aReason)
        {
            if (client.connectionStatus != ConnectionStatus.DISCONNECTED)
            {
                DMPPluginHandler.FireOnClientDisconnect(client);
                if (client.playerName != null)
                {
                    if (playerChatChannels.ContainsKey(client.playerName))
                    {
                        playerChatChannels.Remove(client.playerName);
                    }
                    if (playerDownloadedScreenshotIndex.ContainsKey(client.playerName))
                    {
                        playerDownloadedScreenshotIndex.Remove(client.playerName);
                    }
                    if (playerUploadedScreenshotIndex.ContainsKey(client.playerName))
                    {
                        playerUploadedScreenshotIndex.Remove(client.playerName);
                    }
                    if (playerWatchScreenshot.ContainsKey(client.playerName))
                    {
                        playerWatchScreenshot.Remove(client.playerName);
                    }
                }
                client.connectionStatus = ConnectionStatus.DISCONNECTED;
                if (client.authenticated)
                {
                    Messages.ServerClient_PlayerLeaveSend msg = new Messages.ServerClient_PlayerLeaveSend();
                    msg.name = client.playerName;
                    BroadcastBut(client, msg);

                    lockSystem.ReleasePlayerLocks(client.playerName);
                }

                m_clients.Remove(client);

                try
                {
                    m_server.Kick(client.Id, aReason);
                }
                catch (Exception e)
                {
                    DarkLog.Debug("Error closing client connection: " + e.Message);
                }
                Server.lastPlayerActivity = Server.serverClock.ElapsedMilliseconds;
            }
          
        }
        #endregion
        #region Message handling
        private void HandleMessage(IMessage aMessage)
        {
            if (DMPPluginHandler.FireOnMessageReceived(aMessage))
                return;

            try
            {
                Trigger(aMessage);
            }
            catch
            {
                //DarkLog.Debug("Error handling " + typeof(aMessage).ToString() + " from " + client.playerName + ", exception: " + e);
                m_server.Kick(aMessage.ConnectionId, "Server failed to process " + aMessage.GetType().ToString() + " message");
            }
        }

        #endregion
        #region Message sending

        public void BroadcastBut(ClientObject aExcept, IMessage aMessage)
        {
            if (aExcept != null)
                m_server.BroadcastBut(aMessage, aExcept.Id);
            else
                Broadcast(aMessage);
        }

        public void Broadcast(IMessage aMessage)
        {
            m_server.Broadcast(aMessage);
        }

        // Use this function for fat packets so that it won't affect normal traffic
        public void SendLarge(ClientObject client, IMessage aMessage)
        {
            m_server.SendTo(aMessage, client.Id, GameServer.SendType.KUnordered, 1);
        }

        public void SendOrdered(ClientObject client, IMessage aMessage)
        {
            m_server.SendTo(aMessage, client.Id, GameServer.SendType.KOrdered);
        }

        public void SendUnordered(ClientObject client, IMessage aMessage)
        {
            m_server.SendTo(aMessage, client.Id, GameServer.SendType.KUnordered);
        }

        public void SendUnreliable(ClientObject client, IMessage aMessage)
        {
            m_server.SendTo(aMessage, client.Id, GameServer.SendType.KUnreliable);
        }

        public ClientObject[] Clients
        {
            get
            {
                return m_clients.ToArray();
            }
        }

        public ClientObject GetClientByName(string playerName)
        {
            ClientObject findClient = null;
            m_clients.Iterate(x =>
            {
                if (x.authenticated && x.playerName == playerName)
                {
                    findClient = x;
                }
            });
            return findClient;
        }

        public ClientObject GetClientByIP(IPAddress ipAddress)
        {
            ClientObject findClient = null;
            m_clients.Iterate(x =>
            {
                if (x.authenticated && x.ipAddress == ipAddress)
                {
                    findClient = x;
                }
            });
            return findClient;
        }

        public ClientObject GetClientByPublicKey(string publicKey)
        {
            ClientObject findClient = null;
            m_clients.Iterate(x =>
            {
                if (x.authenticated && x.publicKey == publicKey)
                {
                    findClient = x;
                }
            });
            return findClient;
        }

        #endregion
        #region Server commands
        public void KickPlayer(string commandArgs)
        {
            string playerName = commandArgs;
            string reason = "";
            if (commandArgs.Contains(" "))
            {
                playerName = commandArgs.Substring(0, commandArgs.IndexOf(" "));
                reason = commandArgs.Substring(commandArgs.IndexOf(" ") + 1);
            }
            ClientObject player = null;

            if (playerName != "")
            {
                player = GetClientByName(playerName);
                if (player != null)
                {
                    DarkLog.Normal("Kicking " + playerName + " from the server");
                    if (reason == "")
                    {
                        reason = "no reason specified";
                    }
                    m_server.Kick(player.Id, "You are banned from the server : " + reason);
                }
            }
            else
            {
                DarkLog.Error("Syntax error. Usage: /kick playername [reason]");
            }
        }

        public void KickAll(string aReason)
        {
            m_server.KickAll(aReason);
        }

        public void BanPlayer(string commandArgs)
        {
            string playerName = commandArgs;
            string reason = "";

            if (commandArgs.Contains(" "))
            {
                playerName = commandArgs.Substring(0, commandArgs.IndexOf(" "));
                reason = commandArgs.Substring(commandArgs.IndexOf(" ") + 1);
            }

            if (playerName != "")
            {

                ClientObject player = GetClientByName(playerName);

                if (reason == "")
                {
                    reason = "no reason specified";
                }

                if (player != null)
                {
                    m_server.Kick(player.Id, "You are banned from the server : " + reason);
                }

                DarkLog.Normal("Player '" + playerName + "' was banned from the server: " + reason);
                m_banList.AddName(playerName);
            }

        }

        public void BanIP(string commandArgs)
        {
            string ip = commandArgs;
            string reason = "";

            if (commandArgs.Contains(" "))
            {
                ip = commandArgs.Substring(0, commandArgs.IndexOf(" "));
                reason = commandArgs.Substring(commandArgs.IndexOf(" ") + 1);
            }

            IPAddress ipAddress;
            if (IPAddress.TryParse(ip, out ipAddress))
            {

                ClientObject player = GetClientByIP(ipAddress);

                if (reason == "")
                {
                    reason = "no reason specified";
                }

                if (player != null)
                {
                    m_server.Kick(player.Id, "You are banned from the server : " + reason);
                }
                m_banList.AddIp(ipAddress);

                DarkLog.Normal("IP Address '" + ip + "' was banned from the server: " + reason);
            }
            else
            {
                DarkLog.Normal(ip + " is not a valid IP address");
            }

        }

        public void BanPublicKey(string commandArgs)
        {
            string publicKey = commandArgs;
            string reason = "";

            if (commandArgs.Contains(" "))
            {
                publicKey = commandArgs.Substring(0, commandArgs.IndexOf(" "));
                reason = commandArgs.Substring(commandArgs.IndexOf(" ") + 1);
            }

            ClientObject player = GetClientByPublicKey(publicKey);

            if (reason == "")
            {
                reason = "no reason specified";
            }

            if (player != null)
            {
                DisconnectClient(player, "You were banned from the server!");
            }

            //m_banList.BanKey(publicKey);

            DarkLog.Normal("Public key '" + publicKey + "' was banned from the server: " + reason);

        }

        public void PMCommand(string commandArgs)
        {
            ClientObject pmPlayer = null;
            int matchedLength = 0;
            m_clients.Iterate(x =>
            {
                //Only search authenticated players
                if (x.authenticated)
                {
                    //Try to match the longest player name
                    if (commandArgs.StartsWith(x.playerName) && x.playerName.Length > matchedLength)
                    {
                        //Double check there is a space after the player name
                        if ((commandArgs.Length > (x.playerName.Length + 1)) ? commandArgs[x.playerName.Length] == ' ' : false)
                        {
                            pmPlayer = x;
                            matchedLength = x.playerName.Length;
                        }
                    }
                }
            });

            if (pmPlayer != null)
            {
                string messageText = commandArgs.Substring(pmPlayer.playerName.Length + 1);
                Messages.ServerClient_ChatMessageSend msg = new Messages.ServerClient_ChatMessageSend();
                msg.type = (byte)ChatMessageType.PRIVATE_MESSAGE;
                msg.name = Settings.settingsStore.consoleIdentifier;
                msg.channel = pmPlayer.playerName;
                msg.message = messageText;

                SendUnordered(pmPlayer, msg);
            }
            else
            {
                DarkLog.Normal("Player not found!");
            }
        }

        public void AdminCommand(string commandArgs)
        {
            string func = "";
            string playerName = "";

            func = commandArgs;
            if (commandArgs.Contains(" "))
            {
                func = commandArgs.Substring(0, commandArgs.IndexOf(" "));
                if (commandArgs.Substring(func.Length).Contains(" "))
                {
                    playerName = commandArgs.Substring(func.Length + 1);
                }
            }

            switch (func)
            {
                default:
                    DarkLog.Normal("Undefined function. Usage: /admin [add|del] playername or /admin show");
                    break;
                case "add":
                    if (File.Exists(Path.Combine(Server.universeDirectory, "Players", playerName + ".txt")))
                    {
                        if (!serverAdmins.Contains(playerName))
                        {
                            DarkLog.Debug("Added '" + playerName + "' to admin list.");
                            serverAdmins.Add(playerName);
                            //Notify all players an admin has been added

                            Messages.ServerClient_AdminAddSend msg = new Messages.ServerClient_AdminAddSend();
                            msg.name = playerName;
                            Broadcast(msg);
                        }
                        else
                        {
                            DarkLog.Normal("'" + playerName + "' is already an admin.");
                        }

                    }
                    else
                    {
                        DarkLog.Normal("'" + playerName + "' does not exist.");
                    }
                    break;
                case "del":
                    if (serverAdmins.Contains(playerName))
                    {
                        DarkLog.Normal("Removed '" + playerName + "' from the admin list.");
                        serverAdmins.Remove(playerName);
                        //Notify all players an admin has been removed
                        Messages.ServerClient_AdminRemoveSend msg = new Messages.ServerClient_AdminRemoveSend();
                        msg.name = playerName;
                        Broadcast(msg);

                    }
                    else
                    {
                        DarkLog.Normal("'" + playerName + "' is not an admin.");
                    }
                    break;
                case "show":
                    foreach (string player in serverAdmins)
                    {
                        DarkLog.Normal(player);
                    }
                    break;
            }
        }

        public void WhitelistCommand(string commandArgs)
        {
            string func = "";
            string playerName = "";

            func = commandArgs;
            if (commandArgs.Contains(" "))
            {
                func = commandArgs.Substring(0, commandArgs.IndexOf(" "));
                if (commandArgs.Substring(func.Length).Contains(" "))
                {
                    playerName = commandArgs.Substring(func.Length + 1);
                }
            }

            switch (func)
            {
                default:
                    DarkLog.Debug("Undefined function. Usage: /whitelist [add|del] playername or /whitelist show");
                    break;
                case "add":
                    if (!serverWhitelist.Contains(playerName))
                    {
                        DarkLog.Normal("Added '" + playerName + "' to whitelist.");
                        serverWhitelist.Add(playerName);
                    }
                    else
                    {
                        DarkLog.Normal("'" + playerName + "' is already on the whitelist.");
                    }
                    break;
                case "del":
                    if (serverWhitelist.Contains(playerName))
                    {
                        DarkLog.Normal("Removed '" + playerName + "' from the whitelist.");
                        serverWhitelist.Remove(playerName);
                    }
                    else
                    {
                        DarkLog.Normal("'" + playerName + "' is not on the whitelist.");
                    }
                    break;
                case "show":
                    foreach (string player in serverWhitelist)
                    {
                        DarkLog.Normal(player);
                    }
                    break;
            }
        }
        #endregion
    }

    public class ClientObject
    {
        public long Id { get; set; }

        public bool authenticated;
        public byte[] challenge;
        public string playerName = "Unknown";
        public string clientVersion;
        public bool isBanned;
        public IPAddress ipAddress;
        public string publicKey;
        //subspace tracking
        public int subspace = -1;
        public float subspaceRate = 1f;
        //vessel tracking
        public string activeVessel = "";
        //connection
        public string endpoint;
        //State tracking
        public ConnectionStatus connectionStatus;
        public PlayerStatus playerStatus;
        public float[] playerColor;
        //Network traffic tracking
        public long bytesQueuedOut = 0;
        public long bytesSent = 0;
        public long bytesReceived = 0;
        public long lastQueueOptimizeTime = 0;
    }
}

