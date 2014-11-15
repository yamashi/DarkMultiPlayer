using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace DarkMultiPlayerServer
{
    public class BanList
    {
        private static string banlistFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "DMPPlayerBans.txt");
        private static string ipBanlistFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "DMPIPBans.txt");
        private static string publicKeyBanlistFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "DMPKeyBans.txt");

        private List<string> m_bannedNames;
        private List<IPAddress> m_bannedIps;
        private List<string> m_bannedPublicKeys;
        public BanList()
        {
            DarkLog.Debug("Loading bans");

            m_bannedNames = new List<string>();
            m_bannedIps = new List<IPAddress>();
            m_bannedPublicKeys = new List<string>();

            if (File.Exists(banlistFile))
            {
                foreach (string line in File.ReadAllLines(banlistFile))
                {
                    m_bannedNames.Add(line);
                }
            }
            else
            {
                File.Create(banlistFile);
            }

            if (File.Exists(ipBanlistFile))
            {
                foreach (string line in File.ReadAllLines(ipBanlistFile))
                {
                    IPAddress banIPAddr = null;
                    if (IPAddress.TryParse(line, out banIPAddr))
                    {
                        m_bannedIps.Add(banIPAddr);
                    }
                    else
                    {
                        DarkLog.Error("Error in IP ban list file, " + line + " is not an IP address");
                    }
                }
            }
            else
            {
                File.Create(ipBanlistFile);
            }

            if (File.Exists(publicKeyBanlistFile))
            {
                foreach (string bannedPublicKey in File.ReadAllLines(publicKeyBanlistFile))
                {
                    m_bannedPublicKeys.Add(bannedPublicKey);
                }
            }
            else
            {
                File.Create(publicKeyBanlistFile);
            }
        }
    }
}
