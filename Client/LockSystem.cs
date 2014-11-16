using System;
using System.Collections.Generic;
using DarkMultiPlayerCommon;
using MessageStream;
using DarkMultiPlayerCommon.Events;

namespace DarkMultiPlayer
{
    public delegate void AcquireDelegate(string playerName,string lockName,bool lockResult);
    public delegate void ReleaseDelegate(string playerName,string lockName);
    public class LockSystem : IRegisterable
    {
        private Dictionary<string, string> m_serverLocks = new Dictionary<string, string>();
        private Dictionary<string, double> m_lastAcquireTime = new Dictionary<string, double>();

        public event AcquireDelegate AcquireEvent;
        public event ReleaseDelegate ReleaseEvent;

        #region Packet handlers
        public void Register(IEventAggregator aggregator)
        {
            aggregator.Register<Messages.ServerClient_LockAcquireRecv>(HandleLockAcquire);
            aggregator.Register<Messages.ServerClient_LockReleaseRecv>(HandleLockRelease);
            aggregator.Register<Messages.ServerClient_LockListRecv>(HandleLockList);
        }

        public void HandleLockAcquire(Messages.ServerClient_LockAcquireRecv msg)
        {
            if (msg.result)
            {
                m_serverLocks[msg.name] = msg.player;
            }
             AcquireEvent(msg.player, msg.name, msg.result);
        }

        public void HandleLockRelease(Messages.ServerClient_LockReleaseRecv msg)
        {
            if (m_serverLocks.ContainsKey(msg.name))
            {
                m_serverLocks.Remove(msg.name);
            }

            ReleaseEvent(msg.player, msg.name);
        }

        public void HandleLockList(Messages.ServerClient_LockListRecv msg)
        {
            m_serverLocks.Clear();
            foreach(var entry in msg.locks)
            {
                m_serverLocks.Add(entry.key, entry.value);
            }
        }
        #endregion

        public void ThrottledAcquireLock(string lockname)
        {
            if (m_lastAcquireTime.ContainsKey(lockname) ? ((UnityEngine.Time.realtimeSinceStartup - m_lastAcquireTime[lockname]) > 5f) : true)
            {
                m_lastAcquireTime[lockname] = UnityEngine.Time.realtimeSinceStartup;
                AcquireLock(lockname, false);
            }
        }

        public void AcquireLock(string lockName, bool force)
        {
            Messages.ClientServer_LockAcquireSend msg = new Messages.ClientServer_LockAcquireSend
            {
                name = lockName,
                force = force
            };
            
            throw new NotImplementedException();
        }

        public void ReleaseLock(string lockName)
        {
            Messages.ClientServer_LockReleaseSend msg = new Messages.ClientServer_LockReleaseSend
            {
                name = lockName
            };

            if (LockIsOurs(lockName))
            {
                m_serverLocks.Remove(lockName);
            }
            
            throw new NotImplementedException();
        }

        public void ReleasePlayerLocks(string playerName)
        {
            List<string> removeList = new List<string>();
            foreach (KeyValuePair<string,string> kvp in m_serverLocks)
            {
                if (kvp.Value == playerName)
                {
                    removeList.Add(kvp.Key);
                }
            }
            foreach (string removeValue in removeList)
            {
                m_serverLocks.Remove(removeValue);
                ReleaseEvent(playerName, removeValue);
            }
        }

        public void ReleasePlayerLocksWithPrefix(string playerName, string prefix)
        {
            DarkLog.Debug("Releasing lock with prefix " + prefix + " for " + playerName);
            List<string> removeList = new List<string>();
            foreach (KeyValuePair<string,string> kvp in m_serverLocks)
            {
                if (kvp.Key.StartsWith(prefix) && kvp.Value == playerName)
                {
                    removeList.Add(kvp.Key);
                }
            }
            foreach (string removeValue in removeList)
            {
                if (playerName == Client.Instance.Settings.PlayerName)
                {
                    DarkLog.Debug("Releasing lock " + removeValue);
                    ReleaseLock(removeValue);
                }
                else
                {
                    m_serverLocks.Remove(removeValue);
                    ReleaseEvent(playerName, removeValue);
                }
            }
        }

        public bool LockIsOurs(string lockName)
        {
            if (m_serverLocks.ContainsKey(lockName))
            {
                if (m_serverLocks[lockName] == Client.Instance.Settings.PlayerName)
                {
                    return true;
                }
            }
            return false;
        }

        public bool LockExists(string lockName)
        {
            return m_serverLocks.ContainsKey(lockName);
        }

        public string LockOwner(string lockName)
        {
            if (m_serverLocks.ContainsKey(lockName))
            {
                return m_serverLocks[lockName];
            }
            return "";
            
        }

        public void Reset()
        {
            m_serverLocks.Clear();
        }
    }
}

