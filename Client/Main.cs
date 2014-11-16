using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DarkMultiPlayerCommon;
using System.Reflection;

namespace DarkMultiPlayer
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Client : MonoBehaviour
    {
        private static Client m_instance;

        private bool m_showUI;
        private bool m_disableMod;

        public bool DisableMod
        { 
            get { return m_disableMod; }
            set { m_disableMod = value; }
        }

        public delegate void UpdateDelegate();
        public delegate void FixedUpdateDelegate();
        public delegate void ResetDelegate();
        public delegate void DrawDelegate();

        public event UpdateDelegate UpdateEvent;
        public event FixedUpdateDelegate FixedUpdateEvent;
        public event ResetDelegate ResetEvent;
        public event DrawDelegate DrawEvent;

        private KerbalAssembly m_assembly;
        private Settings m_settings;
        private ModWorker m_modWorker;

        // UI
        private IncorrectInstallWindow m_incorrectInstallView;
        private ModWindow m_modView;
        private DisclaimerWindow m_disclaimerWindow;

        public ModWindow ModWindow
        {
            get { return m_modView; }
        }

        public KerbalAssembly Assembly
        {
            get { return m_assembly; }
        }

        public Settings Settings
        {
            get { return m_settings; }
        }

        //Chosen by a 2147483647 sided dice roll. Guaranteed to be random.
        public const int WINDOW_OFFSET = 1664952404;
        //Hack gravity fix.
        private Dictionary<CelestialBody, double> bodiesGees = new Dictionary<CelestialBody,double>();
        //Command line connect
        public static ServerEntry commandLineConnect;

        public Client()
        {
            m_instance = this;

            m_modWorker = new ModWorker();
        }

        public static Client Instance
        {
            get
            {
                return m_instance;
            }
        }

        public void Awake()
        {
            Profiler.DMPReferenceTime.Start();
            GameObject.DontDestroyOnLoad(this);

            UnityEngine.Debug.Log("KSP installed at " + Assembly.AssemblyPath);
            UnityEngine.Debug.Log("DMP installed at " + Assembly.AssemblyPath);
            
            if (!Assembly.IsValid)
            {
                UnityEngine.Debug.LogError("DMP is installed at '" + Assembly.AssemblyPath + "', It should be installed at '" + Assembly.AssemblyValidPath + "'");
                return;
            }

            m_assembly = new KerbalAssembly();
            m_settings = new Settings();

            m_modWorker.BuildModuleList();

            // UI
            m_incorrectInstallView = null;
            m_modView = new ModWindow();
            m_disclaimerWindow = null;

            if (Settings.DisclaimerAccepted != 1 && m_disclaimerWindow == null)
            {
                m_disableMod = true;
                m_disclaimerWindow = new DisclaimerWindow();
                m_disclaimerWindow.Enable();
            }

            SetupDirectoriesIfNeeded();

            GameEvents.onHideUI.Add(() =>
            {
                m_showUI = false;
            });
            GameEvents.onShowUI.Add(() =>
            {
                m_showUI = true;
            });
            
            ResetEvent();

            HandleCommandLineArgs();

            DarkLog.Debug("DarkMultiPlayer " + Common.PROGRAM_VERSION + ", protocol " + Common.PROTOCOL_VERSION + " Initialized!");
        }

        private void HandleCommandLineArgs()
        {
            bool nextLineIsAddress = false;
            bool valid = false;
            string address = null;
            int port = 0;
            foreach (string commandLineArg in Environment.GetCommandLineArgs())
            {
                //Supporting IPv6 is FUN!
                if (nextLineIsAddress)
                {
                    valid = CommandLineParser.ParseIp(commandLineArg, out address, out port);
                    if(port == 0)
                    {
                        port = 6702;
                    }
                    nextLineIsAddress = false;
                }

                if (commandLineArg == "-dmp")
                {
                    nextLineIsAddress = true;
                }
            }
            if (valid)
            {
                commandLineConnect = new ServerEntry();
                commandLineConnect.address = address;
                commandLineConnect.port = port;
                DarkLog.Debug("Connecting via command line to: " + address + ", port: " + port);
            }
            else
            {
                DarkLog.Debug("Command line address is invalid: " + address + ", port: " + port);
            }
        }

        public void Update()
        {
            long startClock = Profiler.DMPReferenceTime.ElapsedTicks;
            DarkLog.Update();
            if (m_disableMod)
            {
                return;
            }
            if (!Assembly.IsValid)
            {
                if (m_incorrectInstallView == null)
                {
                    m_incorrectInstallView = new IncorrectInstallWindow();
                    m_incorrectInstallView.Enable();
                }
                return;
            }
            try
            {
                if (HighLogic.LoadedScene == GameScenes.MAINMENU)
                {
//                     if (!dmpSaveChecked)
//                     {
//                         dmpSaveChecked = true;
//                         SetupBlankGameIfNeeded();
//                     }
                }

                //Handle GUI events
//                 if (!PlayerStatusWindow.Instance.disconnectEventHandled)
//                 {
//                     PlayerStatusWindow.Instance.disconnectEventHandled = true;
//                     forceQuit = true;
//                     GameClient.Instance.SendDisconnect("Quit");
//                 }
//                 if (!ConnectionWindow.Instance.renameEventHandled)
//                 {
//                     PlayerStatusWorker.Instance.myPlayerStatus.playerName = Settings.Instance.playerName;
//                     Settings.Instance.SaveSettings();
//                     ConnectionWindow.Instance.renameEventHandled = true;
//                 }
//                 if (!ConnectionWindow.Instance.addEventHandled)
//                 {
//                     Settings.Instance.servers.Add(ConnectionWindow.Instance.addEntry);
//                     ConnectionWindow.Instance.addEntry = null;
//                     Settings.Instance.SaveSettings();
//                     ConnectionWindow.Instance.addingServer = false;
//                     ConnectionWindow.Instance.addEventHandled = true;
//                 }
//                 if (!ConnectionWindow.Instance.editEventHandled)
//                 {
//                     Settings.servers[ConnectionWindow.Instance.selected].name = ConnectionWindow.Instance.editEntry.name;
//                     Settings.Instance.servers[ConnectionWindow.Instance.selected].address = ConnectionWindow.Instance.editEntry.address;
//                     Settings.Instance.servers[ConnectionWindow.Instance.selected].port = ConnectionWindow.Instance.editEntry.port;
//                     ConnectionWindow.Instance.editEntry = null;
//                     Settings.Instance.SaveSettings();
//                     ConnectionWindow.Instance.addingServer = false;
//                     ConnectionWindow.Instance.editEventHandled = true;
//                 }
//                 if (!ConnectionWindow.Instance.removeEventHandled)
//                 {
//                     Settings.Instance.servers.RemoveAt(ConnectionWindow.Instance.selected);
//                     ConnectionWindow.Instance.selected = -1;
//                     Settings.Instance.SaveSettings();
//                     ConnectionWindow.Instance.removeEventHandled = true;
//                 }
//                 if (!ConnectionWindow.Instance.connectEventHandled)
//                 {
//                     ConnectionWindow.Instance.connectEventHandled = true;
//                     GameClient.Instance.ConnectToServer(Settings.Instance.servers[ConnectionWindow.Instance.selected].address, Settings.Instance.servers[ConnectionWindow.Instance.selected].port);
//                 }
//                 if (commandLineConnect != null && HighLogic.LoadedScene == GameScenes.MAINMENU && Time.timeSinceLevelLoad > 1f)
//                 {
//                     GameClient.Instance.ConnectToServer(commandLineConnect.address, commandLineConnect.port);
//                     commandLineConnect = null;
//                 }
// 
//                 if (!ConnectionWindow.Instance.disconnectEventHandled)
//                 {
//                     ConnectionWindow.Instance.disconnectEventHandled = true;
//                     gameRunning = false;
//                     fireReset = true;
//                     if (GameClient.Instance.state == ClientState.CONNECTING)
//                     {
//                         GameClient.Instance.Disconnect("Cancelled connection to server");
//                     }
//                     else
//                     {
//                         GameClient.Instance.SendDisconnect("Quit during initial sync");
//                     }
//                 }

                UpdateEvent();

                //Force quit
//                 if (forceQuit)
//                 {
//                     forceQuit = false;
//                     gameRunning = false;
//                     fireReset = true;
//                     StopGame();
//                 }

//                 if (displayDisconnectMessage)
//                 {
//                     if (HighLogic.LoadedScene != GameScenes.MAINMENU)
//                     {
//                         if ((UnityEngine.Time.realtimeSinceStartup - lastDisconnectMessageCheck) > 1f)
//                         {
//                             lastDisconnectMessageCheck = UnityEngine.Time.realtimeSinceStartup;
//                             if (disconnectMessage != null)
//                             {
//                                 disconnectMessage.duration = 0;
//                             }
//                             disconnectMessage = ScreenMessages.PostScreenMessage("You have been disconnected!", 2f, ScreenMessageStyle.UPPER_CENTER);
//                         }
//                     }
//                     else
//                     {
//                         displayDisconnectMessage = false;
//                     }
//                 }

                //Normal quit
//                 if (gameRunning)
//                 {
//                     if (HighLogic.LoadedScene == GameScenes.MAINMENU)
//                     {
//                         gameRunning = false;
//                         fireReset = true;
//                         GameClient.Instance.SendDisconnect("Quit to main menu");
//                     }
// 
//                     if (ScreenshotWorker.Instance.uploadScreenshot)
//                     {
//                         ScreenshotWorker.Instance.uploadScreenshot = false;
//                         StartCoroutine(UploadScreenshot());
//                     }
// 
//                     if (HighLogic.CurrentGame.flagURL != Settings.Instance.selectedFlag)
//                     {
//                         DarkLog.Debug("Saving selected flag");
//                         Settings.Instance.selectedFlag = HighLogic.CurrentGame.flagURL;
//                         Settings.Instance.SaveSettings();
//                         FlagSyncer.Instance.flagChangeEvent = true;
//                     }
// 
//                     // save every GeeASL from each body in FlightGlobals
//                     if (HighLogic.LoadedScene == GameScenes.FLIGHT && bodiesGees.Count == 0)
//                     {
//                         foreach (CelestialBody body in FlightGlobals.fetch.bodies)
//                         {
//                             bodiesGees.Add(body, body.GeeASL);
//                         }
//                     }
// 
//                     //handle use of cheats
//                     if (!serverAllowCheats)
//                     {
//                         CheatOptions.InfiniteFuel = false;
//                         CheatOptions.InfiniteEVAFuel = false;
//                         CheatOptions.InfiniteRCS = false;
//                         CheatOptions.NoCrashDamage = false;
// 
//                         foreach (KeyValuePair<CelestialBody, double> gravityEntry in bodiesGees)
//                         {
//                             gravityEntry.Key.GeeASL = gravityEntry.Value;
//                         }
//                     }
// 
//                     if (HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ready)
//                     {
//                         HighLogic.CurrentGame.Parameters.Flight.CanLeaveToSpaceCenter = (PauseMenu.canSaveAndExit == ClearToSaveStatus.CLEAR);
//                     }
//                     else
//                     {
//                         HighLogic.CurrentGame.Parameters.Flight.CanLeaveToSpaceCenter = true;
//                     }
//                 }
// 
//                 if (fireReset)
//                 {
//                     fireReset = false;
//                     FireResetEvent();
//                 }
// 
//                 if (startGame)
//                 {
//                     startGame = false;
//                     StartGame();
//                 }
            }
            catch (Exception e)
            {
//                 DarkLog.Debug("Threw in Update, state " + GameClient.Instance.state.ToString() + ", exception" + e);
//                 if (GameClient.Instance.state != ClientState.RUNNING)
//                 {
//                     if (GameClient.Instance.state != ClientState.DISCONNECTED)
//                     {
//                         GameClient.Instance.SendDisconnect("Unhandled error while syncing!");
//                     }
//                     else
//                     {
//                         GameClient.Instance.Disconnect("Unhandled error while syncing!");
//                     }
//                 }
            }
            Profiler.updateData.ReportTime(startClock);
        }

        public IEnumerator<WaitForEndOfFrame> UploadScreenshot()
        {
            yield return new WaitForEndOfFrame();
//             ScreenshotWorker.Instance.SendScreenshot();
//             ScreenshotWorker.Instance.screenshotTaken = true;
        }

        public void FixedUpdate()
        {
            long startClock = Profiler.DMPReferenceTime.ElapsedTicks;
            if (m_disableMod)
            {
                return;
            }

            FixedUpdateEvent();

            Profiler.fixedUpdateData.ReportTime(startClock);
        }

        public void OnGUI()
        {
            //Window ID's - Doesn't include "random" offset.
            //Connection window: 6702
            //Status window: 6703
            //Chat window: 6704
            //Debug window: 6705
            //Mod windw: 6706
            //Craft library window: 6707
            //Craft upload window: 6708
            //Screenshot window: 6710
            //Options window: 6711
            //Converter window: 6712
            //Disclaimer window: 6713
            long startClock = Profiler.DMPReferenceTime.ElapsedTicks;
            if (m_showUI)
            {
                DrawEvent();
            }
            Profiler.guiData.ReportTime(startClock);
        }

        private void StartGame()
        {
            //Create new game object for our DMP session.
            HighLogic.CurrentGame = CreateBlankGame();

            //Set the game mode
            SetGameMode();

//             //Found in KSP's files. Makes a crapton of sense :)
//             if (HighLogic.CurrentGame.Mode != Game.Modes.SANDBOX)
//             {
//                 HighLogic.CurrentGame.Parameters.Difficulty.AllowStockVessels = false;
//             }
//             HighLogic.CurrentGame.flightState.universalTime = TimeSyncer.Instance.GetUniverseTime();
// 
//             //Load DMP stuff
//             VesselWorker.Instance.LoadKerbalsIntoGame();
//             VesselWorker.Instance.LoadVesselsIntoGame();
// 
//             //Load the scenarios from the server
//             ScenarioWorker.Instance.LoadScenarioDataIntoGame();
// 
//             //Load the missing scenarios as well (Eg, Contracts and stuff for career mode
//             ScenarioWorker.Instance.LoadMissingScenarioDataIntoGame();
// 
//             //This only makes KSP complain
//             HighLogic.CurrentGame.CrewRoster.ValidateAssignments(HighLogic.CurrentGame);
//             DarkLog.Debug("Starting " + gameMode + " game...");
// 
//             //Control locks will bug out the space centre sceen, so remove them before starting.
//             DeleteAllTheControlLocksSoTheSpaceCentreBugGoesAway();
// 
//             //.Start() seems to stupidly .Load() somewhere - Let's overwrite it so it loads correctly.
//             GamePersistence.SaveGame(HighLogic.CurrentGame, "persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
//             HighLogic.CurrentGame.Start();
//             ChatWorker.Instance.display = true;

            throw new NotImplementedException();
            DarkLog.Debug("Started!");
        }


        private void DeleteAllTheControlLocksSoTheSpaceCentreBugGoesAway()
        {
            DarkLog.Debug("Clearing " + InputLockManager.lockStack.Count + " control locks");
            InputLockManager.ClearControlLocks();
        }

        private void StopGame()
        {
            HighLogic.SaveFolder = "DarkMultiPlayer";
            if (HighLogic.LoadedScene != GameScenes.MAINMENU)
            {
                HighLogic.LoadScene(GameScenes.MAINMENU);
            }
            HighLogic.CurrentGame = null;
            bodiesGees.Clear();
        }

        private void SetGameMode()
        {
            throw new NotImplementedException();
//             switch (gameMode)
//             {
//                 case GameMode.CAREER:
//                     HighLogic.CurrentGame.Mode = Game.Modes.CAREER;
//                     break;
//                 case GameMode.SANDBOX:
//                     HighLogic.CurrentGame.Mode = Game.Modes.SANDBOX;
//                     break;
//                 case GameMode.SCIENCE:
//                     HighLogic.CurrentGame.Mode = Game.Modes.SCIENCE_SANDBOX;
//                     break;
//             }
        }

        private void SetupDirectoriesIfNeeded()
        {
            string darkMultiPlayerSavesDirectory = Path.Combine(Path.Combine(KSPUtil.ApplicationRootPath, "saves"), "DarkMultiPlayer");
            CreateIfNeeded(darkMultiPlayerSavesDirectory);
            CreateIfNeeded(Path.Combine(darkMultiPlayerSavesDirectory, "Ships"));
            CreateIfNeeded(Path.Combine(darkMultiPlayerSavesDirectory, Path.Combine("Ships", "VAB")));
            CreateIfNeeded(Path.Combine(darkMultiPlayerSavesDirectory, Path.Combine("Ships", "SPH")));
            CreateIfNeeded(Path.Combine(darkMultiPlayerSavesDirectory, "Subassemblies"));
            string darkMultiPlayerCacheDirectory = Path.Combine(Path.Combine(Path.Combine(KSPUtil.ApplicationRootPath, "GameData"), "DarkMultiPlayer"), "Cache");
            CreateIfNeeded(darkMultiPlayerCacheDirectory);
            string darkMultiPlayerFlagsDirectory = Path.Combine(Path.Combine(Path.Combine(KSPUtil.ApplicationRootPath, "GameData"), "DarkMultiPlayer"), "Flags");
            CreateIfNeeded(darkMultiPlayerFlagsDirectory);
        }

        private void SetupBlankGameIfNeeded()
        {
            string persistentFile = Path.Combine(Path.Combine(Path.Combine(KSPUtil.ApplicationRootPath, "saves"), "DarkMultiPlayer"), "persistent.sfs");
            if (!File.Exists(persistentFile))
            {
                DarkLog.Debug("Creating new blank persistent.sfs file");
                Game blankGame = CreateBlankGame();
                HighLogic.SaveFolder = "DarkMultiPlayer";
                GamePersistence.SaveGame(blankGame, "persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
            }
        }

        private Game CreateBlankGame()
        {
            Game returnGame = new Game();
            //KSP complains about a missing message system if we don't do this.
            returnGame.additionalSystems = new ConfigNode();
            returnGame.additionalSystems.AddNode("MESSAGESYSTEM");

            //Flightstate is null on new Game();
            returnGame.flightState = new FlightState();

            //DMP stuff
            returnGame.startScene = GameScenes.SPACECENTER;
            returnGame.flagURL = Settings.Flag;
            returnGame.Title = "DarkMultiPlayer";
            returnGame.Parameters.Flight.CanQuickLoad = false;
            returnGame.Parameters.Flight.CanRestart = false;
            returnGame.Parameters.Flight.CanLeaveToEditor = false;
            HighLogic.SaveFolder = "DarkMultiPlayer";

            return returnGame;
        }

        private void CreateIfNeeded(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}

