using System;
using System.Collections.Generic;
using UnityEngine;

namespace DarkMultiPlayer
{
    public class AsteroidManager
    {
        private const float ASTEROID_CHECK_INTERVAL = 5f;

        //How many asteroids to spawn into the server
        public int MaxUntrackedAsteroids { get; set; }
        //private state variables
        private float                       m_lastAsteroidCheck;
        
        ScenarioDiscoverableObjects         m_scenarioController;
        private List<string>                m_serverAsteroids = new List<string>();
        private Dictionary<string,string>   m_asteroidTrackStatus = new Dictionary<string,string>();
        private readonly object             m_asteroidListLock = new object();

        public int Count
        {
            get
            {
                List<Guid> seenAsteroids = new List<Guid>();
                foreach (Vessel checkAsteroid in Asteroids)
                {
                    if (!seenAsteroids.Contains(checkAsteroid.id))
                    {
                        seenAsteroids.Add(checkAsteroid.id);
                    }
                }
                foreach (ProtoVessel checkAsteroid in HighLogic.CurrentGame.flightState.protoVessels)
                {
                    if (VesselIsAsteroid(checkAsteroid))
                    {
                        if (!seenAsteroids.Contains(checkAsteroid.vesselID))
                        {
                            seenAsteroids.Add(checkAsteroid.vesselID);
                        }
                    }
                }
                return seenAsteroids.Count;
            }
        }

        public Vessel[] Asteroids
        {
            get
            {
                List<Vessel> currentAsteroids = new List<Vessel>();
                foreach (Vessel checkVessel in FlightGlobals.fetch.vessels)
                {
                    if (VesselIsAsteroid(checkVessel))
                    {
                        currentAsteroids.Add(checkVessel);
                    }
                }
                return currentAsteroids.ToArray();
            }
        }

        public void Enable()
        {
            Client.updateEvent.Add(this.Update);
            GameEvents.onVesselCreate.Add(this.OnVesselCreate);
        }

        public void Disable()
        {
            GameEvents.onVesselCreate.Remove(this.OnVesselCreate);
            Client.updateEvent.Remove(this.Update);
        }

        private void Update()
        {
            if (m_scenarioController == null)
            {
                foreach (ProtoScenarioModule psm in HighLogic.CurrentGame.scenarios)
                {
                    if (psm != null)
                    {
                        if (psm.moduleName == "ScenarioDiscoverableObjects")
                        {
                            if (psm.moduleRef != null)
                            {
                                m_scenarioController = (ScenarioDiscoverableObjects)psm.moduleRef;
                                m_scenarioController.spawnInterval = float.MaxValue;
                            }
                        }
                    }
                }
            }
            
            if (m_scenarioController != null)
            {
                if ((UnityEngine.Time.realtimeSinceStartup - m_lastAsteroidCheck) > ASTEROID_CHECK_INTERVAL)
                {
                    m_lastAsteroidCheck = UnityEngine.Time.realtimeSinceStartup;
                    //Try to acquire the asteroid-spawning lock if nobody else has it.
                    if (!LockSystem.fetch.LockExists("asteroid-spawning"))
                    {
                        LockSystem.fetch.AcquireLock("asteroid-spawning", false);
                    }

                    //We have the spawn lock, lets do stuff.
                    if (LockSystem.fetch.LockIsOurs("asteroid-spawning"))
                    {
                        if ((HighLogic.CurrentGame.flightState.protoVessels != null) && (FlightGlobals.fetch.vessels != null))
                        {
                            if ((HighLogic.CurrentGame.flightState.protoVessels.Count == 0) || (FlightGlobals.fetch.vessels.Count > 0))
                            {
                                int beforeSpawn = Count;
                                int asteroidsToSpawn = MaxUntrackedAsteroids - beforeSpawn;
                                for (int asteroidsSpawned = 0; asteroidsSpawned < asteroidsToSpawn; asteroidsSpawned++)
                                {
                                    DarkLog.Debug("Spawning asteroid, have " + (beforeSpawn + asteroidsSpawned) + ", need " + MaxUntrackedAsteroids);
                                    m_scenarioController.SpawnAsteroid();
                                }
                            }
                        }
                    }

                    //Check for changes to tracking
                    foreach (Vessel asteroid in Asteroids)
                    {
                        if (asteroid.state != Vessel.State.DEAD)
                        {
                            if (!m_asteroidTrackStatus.ContainsKey(asteroid.id.ToString()))
                            {
                                m_asteroidTrackStatus.Add(asteroid.id.ToString(), asteroid.DiscoveryInfo.trackingStatus.Value);
                            }
                            else
                            {
                                if (asteroid.DiscoveryInfo.trackingStatus.Value != m_asteroidTrackStatus[asteroid.id.ToString()])
                                {
                                    ProtoVessel pv = asteroid.BackupVessel();
                                    DarkLog.Debug("Sending changed asteroid, new state: " + asteroid.DiscoveryInfo.trackingStatus.Value + "!");
                                    m_asteroidTrackStatus[asteroid.id.ToString()] = asteroid.DiscoveryInfo.trackingStatus.Value;
                                    NetworkWorker.fetch.SendVesselProtoMessage(pv, false, false);
                                }
                            }
                        }
                    }
                }
            }
            
        }

        private void OnVesselCreate(Vessel checkVessel)
        {
            if (VesselIsAsteroid(checkVessel))
            {
                lock (m_asteroidListLock)
                {
                    if (LockSystem.fetch.LockIsOurs("asteroid-spawning"))
                    {
                        if (!m_serverAsteroids.Contains(checkVessel.id.ToString()))
                        {
                            if (Count <= MaxUntrackedAsteroids)
                            {
                                DarkLog.Debug("Spawned in new server asteroid!");
                                m_serverAsteroids.Add(checkVessel.id.ToString());
                                VesselWorker.fetch.RegisterServerVessel(checkVessel.id.ToString());
                                NetworkWorker.fetch.SendVesselProtoMessage(checkVessel.protoVessel, false, false);
                            }
                            else
                            {
                                DarkLog.Debug("Killing non-server asteroid " + checkVessel.id);
                                checkVessel.Die();
                            }
                        }
                    }
                    else
                    {
                        if (!m_serverAsteroids.Contains(checkVessel.id.ToString()))
                        {
                            DarkLog.Debug("Killing non-server asteroid " + checkVessel.id + ", we don't own the asteroid-spawning lock");
                            checkVessel.Die();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the vessel is an asteroid.
        /// </summary>
        /// <returns><c>true</c> if the vessel is an asteroid, <c>false</c> otherwise.</returns>
        /// <param name="checkVessel">The vessel to check</param>
        public bool VesselIsAsteroid(Vessel checkVessel)
        {
            if (checkVessel != null)
            {
                if (!checkVessel.loaded)
                {
                    return VesselIsAsteroid(checkVessel.protoVessel);
                }
                //Check the vessel has exactly one part.
                if (checkVessel.parts != null ? (checkVessel.parts.Count == 1) : false)
                {
                    if (checkVessel.parts[0].partName == "PotatoRoid")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the vessel is an asteroid.
        /// </summary>
        /// <returns><c>true</c> if the vessel is an asteroid, <c>false</c> otherwise.</returns>
        /// <param name="checkVessel">The vessel to check</param>
        public bool VesselIsAsteroid(ProtoVessel checkVessel)
        {
            if (checkVessel != null)
            {
                if (checkVessel.protoPartSnapshots != null ? (checkVessel.protoPartSnapshots.Count == 1) : false)
                {
                    if (checkVessel.protoPartSnapshots[0].partName == "PotatoRoid")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Registers the server asteroid - Prevents DMP from deleting it.
        /// </summary>
        /// <param name="asteroidID">Asteroid to register</param>
        public void RegisterServerAsteroid(string asteroidID)
        {
            lock (m_asteroidListLock)
            {
                if (!m_serverAsteroids.Contains(asteroidID))
                {
                    m_serverAsteroids.Add(asteroidID);
                }
                //This will ignore status changes so we don't resend the asteroid.
                if (m_asteroidTrackStatus.ContainsKey(asteroidID))
                {
                    m_asteroidTrackStatus.Remove(asteroidID);
                }
            }
        }

        private void OnGameSceneLoadRequested(GameScenes scene)
        {
            //Force the worker to find the scenario module again.
            m_scenarioController = null;
        }

        public void Reset()
        {
            lock (Client.eventLock)
            {
                m_lastAsteroidCheck = 0;
                m_scenarioController = null;
                m_serverAsteroids.Clear();
                m_asteroidTrackStatus.Clear();

                GameEvents.onGameSceneLoadRequested.Add(this.OnGameSceneLoadRequested);
            }
        }
    }
}

