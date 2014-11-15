using System;
using DarkMultiPlayerCommon;
using MessageStream;
using UnityEngine;

namespace DarkMultiPlayer
{
    public class QuickSaveLoader
    {
        public bool workerEnabled;
        private static QuickSaveLoader singleton;
        private ConfigNode savedVessel;
        private Subspace savedSubspace;
        private double lastLoadTime;

        public static QuickSaveLoader Instance
        {
            get
            {
                return singleton;  
            }
        }

        public void Save()
        {
            if ((HighLogic.LoadedScene == GameScenes.FLIGHT) && (FlightGlobals.fetch.activeVessel != null))
            {
                if (FlightGlobals.fetch.activeVessel.loaded && !FlightGlobals.fetch.activeVessel.packed)
                {
                    if (FlightGlobals.fetch.activeVessel.situation != Vessel.Situations.FLYING)
                    {
                        savedVessel = new ConfigNode();
                        ProtoVessel tempVessel = new ProtoVessel(FlightGlobals.fetch.activeVessel);
                        tempVessel.Save(savedVessel);
                        savedSubspace = new Subspace();
                        savedSubspace.planetTime = Planetarium.GetUniversalTime();
                        savedSubspace.serverClock = TimeSyncer.Instance.GetServerClock();
                        savedSubspace.subspaceSpeed = 1f;
                        ScreenMessages.PostScreenMessage("Quicksaved!", 3f, ScreenMessageStyle.UPPER_CENTER);
                    }
                    else
                    {
                        ScreenMessages.PostScreenMessage("Cannot quicksave - Active vessel is in flight!", 3f, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
                else
                {
                    ScreenMessages.PostScreenMessage("Cannot quicksave - Active vessel is not loaded!", 3f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
            else
            {
                ScreenMessages.PostScreenMessage("Cannot quicksave - Not in flight!", 3f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        public void Load()
        {
            if (savedVessel != null && savedSubspace != null)
            {
                if ((UnityEngine.Time.realtimeSinceStartup - lastLoadTime) > 5f)
                {
                    lastLoadTime = UnityEngine.Time.realtimeSinceStartup;
                    TimeSyncer.Instance.UnlockSubspace();
                    long serverClock = TimeSyncer.Instance.GetServerClock();
                    int newSubspace = TimeSyncer.Instance.LockNewSubspace(serverClock, savedSubspace.planetTime, savedSubspace.subspaceSpeed);
                    using (MessageWriter mw = new MessageWriter())
                    {
                        mw.Write<int>((int)WarpMessageType.NEW_SUBSPACE);
                        mw.Write<string>(Settings.Instance.playerName);
                        mw.Write<int>(newSubspace);
                        mw.Write<long>(serverClock);
                        mw.Write<double>(savedSubspace.planetTime);
                        mw.Write<float>(savedSubspace.subspaceSpeed);
                        NetworkWorker.Instance.SendWarpMessage(mw.GetMessageBytes());
                    }
                    TimeSyncer.Instance.LockSubspace(newSubspace);
                    VesselWorker.Instance.LoadVessel(savedVessel);
                    ScreenMessages.PostScreenMessage("Quickloaded!", 3f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
            else
            {
                ScreenMessages.PostScreenMessage("No current quicksave to load from!", 3f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        private void Update()
        {
            if (workerEnabled)
            {
                if (Input.GetKey(KeyCode.F5))
                {
                    Save();
                }
                if (Input.GetKey(KeyCode.F9))
                {
                    Load();
                }
            }
        }

        public static void Reset()
        {
            lock (Client.eventLock)
            {
                if (singleton != null)
                {
                    singleton.workerEnabled = false;
                    Client.updateEvent.Remove(singleton.Update);
                }
                singleton = new QuickSaveLoader();
                Client.updateEvent.Add(singleton.Update);
            }
        }
    }
}

