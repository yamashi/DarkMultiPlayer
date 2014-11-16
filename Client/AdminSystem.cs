using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DarkMultiPlayerCommon;
using MessageStream;

namespace DarkMultiPlayer
{
    public class AdminSystem
    {
        private List<string> m_serverAdmins = new List<string>();

        public void HandleAdminMessage(byte[] messageData)
        {
            throw new NotImplementedException();
        }

        private void RegisterServerAdmin(string adminName)
        {
            if (!m_serverAdmins.Contains(adminName))
            {
                m_serverAdmins.Add(adminName);
            }
            
        }

        private void UnregisterServerAdmin(string adminName)
        {
            if (m_serverAdmins.Contains(adminName))
            {
                m_serverAdmins.Remove(adminName);
            } 
        }

        /// <summary>
        /// Check wether the current player is an admin on the server
        /// </summary>
        /// <returns><c>true</c> if the current player is admin; otherwise, <c>false</c>.</returns>
        public bool IsAdmin()
        {
            return IsAdmin(Client.Instance.Settings.PlayerName);
        }

        /// <summary>
        /// Check wether the specified player is an admin on the server
        /// </summary>
        /// <returns><c>true</c> if the specified player is admin; otherwise, <c>false</c>.</returns>
        /// <param name="playerName">Player name to check for admin.</param>
        public bool IsAdmin(string playerName)
        {
            return m_serverAdmins.Contains(playerName);
        }

        public void Reset()
        {
            m_serverAdmins = new List<string>();
        }
    }
}

