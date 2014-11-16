// using System;
// using System.Collections.Generic;
// using System.Net;
// using System.Net.Sockets;
// using System.IO;
// using System.Runtime.Serialization.Formatters.Binary;
// using System.Security.Cryptography;
// using System.Threading;
// using UnityEngine;
// using DarkMultiPlayerCommon;
// using MessageStream;
// using Lidgren.Network;
// 
// namespace DarkMultiPlayer
// {
//     public class GameClient : GameNetwork
//     {
//         //Read from ConnectionWindow
//         public ClientState state
//         {
//             private set;
//             get;
//         }
// 
//         //Used for the initial sync
//         private int numberOfKerbals = 0;
//         private int numberOfKerbalsReceived = 0;
//         private int numberOfVessels = 0;
//         private int numberOfVesselsReceived = 0;
//         //Locking
//         private string serverMotd;
//         private bool displayMotd;
// 
//         public GameClient()
//         {
//             Client.Instance.UpdateEvent += this.Update;
//         }
// 
//         protected override void ProcessNetwork()
//         {
//             if (state == ClientState.CONNECTED)
//             {
//                 Client.Instance.status = "Connected";
//             }
// 
//             if (state == ClientState.HANDSHAKING)
//             {
//                 Client.Instance.status = "Handshaking";
//             }
// 
//             if (state == ClientState.AUTHENTICATED)
//             {
//                 GameClient.Instance.SendPlayerStatus(PlayerStatusWorker.Instance.myPlayerStatus);
//                 DarkLog.Debug("Sending time sync!");
//                 state = ClientState.TIME_SYNCING;
//                 Client.Instance.status = "Syncing server clock";
//                 SendTimeSync();
//             }
//             if (TimeSyncer.Instance.synced && state == ClientState.TIME_SYNCING)
//             {
//                 DarkLog.Debug("Time Synced!");
//                 state = ClientState.TIME_SYNCED;
//             }
//             if (state == ClientState.TIME_SYNCED)
//             {
//                 DarkLog.Debug("Requesting kerbals!");
//                 Client.Instance.status = "Syncing kerbals";
//                 state = ClientState.SYNCING_KERBALS;
//                 SendKerbalsRequest();
//             }
//             if (state == ClientState.VESSELS_SYNCED)
//             {
//                 DarkLog.Debug("Vessels Synced!");
//                 Client.Instance.status = "Syncing universe time";
//                 state = ClientState.TIME_LOCKING;
//                 //The subspaces are held in the wrap control messages, but the warp worker will create a new subspace if we aren't locked.
//                 //Process the messages so we get the subspaces, but don't enable the worker until the game is started.
//                 WarpWorker.Instance.ProcessWarpMessages();
//                 TimeSyncer.Instance.workerEnabled = true;
//                 ChatWorker.Instance.workerEnabled = true;
//                 PlayerColorWorker.Instance.workerEnabled = true;
//                 FlagSyncer.Instance.workerEnabled = true;
//                 FlagSyncer.Instance.SendFlagList();
//                 PlayerColorWorker.Instance.SendPlayerColorToServer();
//             }
//             if (state == ClientState.TIME_LOCKING)
//             {
//                 if (TimeSyncer.Instance.locked)
//                 {
//                     DarkLog.Debug("Time Locked!");
//                     DarkLog.Debug("Starting Game!");
//                     Client.Instance.status = "Starting game";
//                     state = ClientState.STARTING;
//                     Client.Instance.startGame = true;
//                 }
//             }
//             if ((state == ClientState.STARTING) && (HighLogic.LoadedScene == GameScenes.SPACECENTER))
//             {
//                 state = ClientState.RUNNING;
//                 Client.Instance.status = "Running";
//                 Client.Instance.gameRunning = true;
//                 AsteroidWorker.Instance.workerEnabled = true;
//                 VesselWorker.Instance.workerEnabled = true;
//                 PlayerStatusWorker.Instance.workerEnabled = true;
//                 ScenarioWorker.Instance.workerEnabled = true;
//                 DynamicTickWorker.Instance.workerEnabled = true;
//                 WarpWorker.Instance.workerEnabled = true;
//                 CraftLibraryWorker.Instance.workerEnabled = true;
//                 ScreenshotWorker.Instance.workerEnabled = true;
//                 QuickSaveLoader.Instance.workerEnabled = true;
//                 SendMotdRequest();
//             }
//             if (displayMotd && (HighLogic.LoadedScene != GameScenes.LOADING) && (Time.timeSinceLevelLoad > 2f))
//             {
//                 displayMotd = false;
//                 ScreenMessages.PostScreenMessage(serverMotd, 10f, ScreenMessageStyle.UPPER_CENTER);
//             }
//         }
// 
//         protected override NetOutgoingMessage CreateMessage()
//         {
//             return null;
//         }
// 
//         #region Connecting to server
//         //Called from main
//         public void ConnectToServer(string address, int port)
//         {
//             
//         }
// 
//         #endregion
//         #region Connection housekeeping
//     
// 
//         public void Disconnect(string reason)
//         {
//             
//         }
// 
//         #endregion
//         #region Network writers/readers
//       
// 
//         private void HandleDisconnectException(Exception e)
//         {
//             if (e.InnerException != null)
//             {
//                 DarkLog.Debug("Connection error: " + e.Message + ", " + e.InnerException);
//                 Disconnect("Connection error: " + e.Message + ", " + e.InnerException.Message);
//             }
//             else
//             {
//                 DarkLog.Debug("Connection error: " + e);
//                 Disconnect("Connection error: " + e.Message);
//             }
//         }
//         #endregion
//     }
// }
// 
