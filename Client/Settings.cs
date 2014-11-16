using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;

namespace DarkMultiPlayer
{
    public class Settings
    {
        //Settings
        private string m_playerName;
        private string m_publicKey;
        private string m_privateKey;
        private int cacheSize;
        private int m_disclaimerAccepted;
        private List<ServerEntry> servers;
        private Color playerColor;
        private KeyCode screenshotKey;
        private KeyCode chatKey;
        private string m_flag;
        private const string DEFAULT_PLAYER_NAME = "Player";
        private const string SETTINGS_FILE = "servers.xml";
        private const string PUBLIC_KEY_FILE = "publickey.txt";
        private const string PRIVATE_KEY_FILE = "privatekey.txt";
        private const int DEFAULT_CACHE_SIZE = 100;
        private string dataLocation;
        private string settingsFile;
        private string backupSettingsFile;
        private string publicKeyFile;
        private string privateKeyFile;
        private string backupPublicKeyFile;
        private string backupPrivateKeyFile;

        public string PlayerName
        {
            get { return m_playerName; }
        }

        public string PublicKey
        {
            get { return m_publicKey; }
        }

        public string PrivateKey
        {
            get { return m_privateKey; }
        }

        public int DisclaimerAccepted
        {
            get { return m_disclaimerAccepted; }
            set { m_disclaimerAccepted = value; }
        }
        
        public string Flag
        {
            get { return m_flag; }
        }


        public Settings()
        {
            string darkMultiPlayerDataDirectory = Path.Combine(Path.Combine(Path.Combine(Path.Combine(KSPUtil.ApplicationRootPath, "GameData"), "DarkMultiPlayer"), "Plugins"), "Data");
            if (!Directory.Exists(darkMultiPlayerDataDirectory))
            {
                Directory.CreateDirectory(darkMultiPlayerDataDirectory);
            }
            string darkMultiPlayerSavesDirectory = Path.Combine(Path.Combine(KSPUtil.ApplicationRootPath, "saves"), "DarkMultiPlayer");
            if (!Directory.Exists(darkMultiPlayerSavesDirectory))
            {
                Directory.CreateDirectory(darkMultiPlayerSavesDirectory);
            }
            dataLocation = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Data");
            settingsFile = Path.Combine(dataLocation, SETTINGS_FILE);
            backupSettingsFile = Path.Combine(darkMultiPlayerSavesDirectory, SETTINGS_FILE);
            publicKeyFile = Path.Combine(dataLocation, PUBLIC_KEY_FILE);
            backupPublicKeyFile = Path.Combine(darkMultiPlayerSavesDirectory, PUBLIC_KEY_FILE);
            privateKeyFile = Path.Combine(dataLocation, PRIVATE_KEY_FILE);
            backupPrivateKeyFile = Path.Combine(darkMultiPlayerSavesDirectory, PRIVATE_KEY_FILE);
            LoadSettings();
        }

        public void LoadSettings()
        {

            //Read XML settings
            try
            {
                bool saveXMLAfterLoad = false;
                XmlDocument xmlDoc = new XmlDocument();
                if (File.Exists(backupSettingsFile) && !File.Exists(settingsFile))
                {
                    DarkLog.Debug("Restoring player settings file!");
                    File.Copy(backupSettingsFile, settingsFile);
                }
                if (!File.Exists(settingsFile))
                {
                    xmlDoc.LoadXml(NewXMLString());
                    m_playerName = DEFAULT_PLAYER_NAME;
                    xmlDoc.Save(settingsFile);
                }
                if (!File.Exists(backupSettingsFile))
                {
                    DarkLog.Debug("Backing up player token and settings file!");
                    File.Copy(settingsFile, backupSettingsFile);
                }
                xmlDoc.Load(settingsFile);
                m_playerName = xmlDoc.SelectSingleNode("/settings/global/@username").Value;
                try
                {
                    cacheSize = Int32.Parse(xmlDoc.SelectSingleNode("/settings/global/@cache-size").Value);
                }
                catch
                {
                    DarkLog.Debug("Adding cache size to settings file");
                    saveXMLAfterLoad = true;
                    cacheSize = DEFAULT_CACHE_SIZE;
                }
                try
                {
                    m_disclaimerAccepted = Int32.Parse(xmlDoc.SelectSingleNode("/settings/global/@disclaimer").Value);
                }
                catch
                {
                    DarkLog.Debug("Adding disclaimer to settings file");
                    saveXMLAfterLoad = true;
                }
                try
                {
                    string floatArrayString = xmlDoc.SelectSingleNode("/settings/global/@player-color").Value;
                    string[] floatArrayStringSplit = floatArrayString.Split(',');
                    float redColor = float.Parse(floatArrayStringSplit[0].Trim());
                    float greenColor = float.Parse(floatArrayStringSplit[1].Trim());
                    float blueColor = float.Parse(floatArrayStringSplit[2].Trim());
                    //Bounds checking - Gotta check up on those players :)
                    if (redColor < 0f)
                    {
                        redColor = 0f;
                    }
                    if (redColor > 1f)
                    {
                        redColor = 1f;
                    }
                    if (greenColor < 0f)
                    {
                        greenColor = 0f;
                    }
                    if (greenColor > 1f)
                    {
                        greenColor = 1f;
                    }
                    if (blueColor < 0f)
                    {
                        blueColor = 0f;
                    }
                    if (blueColor > 1f)
                    {
                        blueColor = 1f;
                    }
                    playerColor = new Color(redColor, greenColor, blueColor, 1f);
/*                    OptionsWindow.Instance.loadEventHandled = false;*/
                }
                catch
                {
                    DarkLog.Debug("Adding player color to settings file");
                    saveXMLAfterLoad = true;
//                     playerColor = PlayerColorWorker.GenerateRandomColor();
//                     OptionsWindow.Instance.loadEventHandled = false;
                }
                try
                {
                    chatKey = (KeyCode)Int32.Parse(xmlDoc.SelectSingleNode("/settings/global/@chat-key").Value);
                }
                catch
                {
                    DarkLog.Debug("Adding chat key to settings file");
                    saveXMLAfterLoad = true;
                    chatKey = KeyCode.BackQuote;
                }
                try
                {
                    screenshotKey = (KeyCode)Int32.Parse(xmlDoc.SelectSingleNode("/settings/global/@screenshot-key").Value);
                }
                catch
                {
                    DarkLog.Debug("Adding screenshot key to settings file");
                    saveXMLAfterLoad = true;
                    chatKey = KeyCode.F8;
                }
                try
                {
                    m_flag = xmlDoc.SelectSingleNode("/settings/global/@selected-flag").Value;
                }
                catch
                {
                    DarkLog.Debug("Adding selected flag to settings file");
                    saveXMLAfterLoad = true;
                    m_flag = "Squad/Flags/default";
                }
                XmlNodeList serverNodeList = xmlDoc.GetElementsByTagName("server");
                servers = new List<ServerEntry>();
                foreach (XmlNode xmlNode in serverNodeList)
                {
                    ServerEntry newServer = new ServerEntry();
                    newServer.name = xmlNode.Attributes["name"].Value;
                    newServer.address = xmlNode.Attributes["address"].Value;
                    Int32.TryParse(xmlNode.Attributes["port"].Value, out newServer.port);
                    servers.Add(newServer);
                }
                if (saveXMLAfterLoad)
                {
                    SaveSettings();
                }
            }
            catch (Exception e)
            {
                DarkLog.Debug("XML Exception: " + e);
            }

            //Read player token
            try
            {
                //Restore backup if needed
                if (File.Exists(backupPublicKeyFile) && File.Exists(backupPrivateKeyFile) && (!File.Exists(publicKeyFile) || !File.Exists(privateKeyFile)))
                {
                    DarkLog.Debug("Restoring backed up keypair!");
                    File.Copy(backupPublicKeyFile, publicKeyFile, true);
                    File.Copy(backupPrivateKeyFile, privateKeyFile, true);
                }
                //Load or create token file
                if (File.Exists(privateKeyFile) && File.Exists(publicKeyFile))
                {
                    m_publicKey = File.ReadAllText(publicKeyFile);
                    m_privateKey = File.ReadAllText(privateKeyFile);
                }
                else
                {
                    DarkLog.Debug("Creating new keypair!");
                    GenerateNewKeypair();
                }
                //Save backup token file if needed
                if (!File.Exists(backupPublicKeyFile) || !File.Exists(backupPrivateKeyFile))
                {
                    DarkLog.Debug("Backing up keypair");
                    File.Copy(publicKeyFile, backupPublicKeyFile, true);
                    File.Copy(privateKeyFile, backupPrivateKeyFile, true);
                }
            }
            catch
            {
                DarkLog.Debug("Error processing keypair, creating new keypair");
                GenerateNewKeypair();
                DarkLog.Debug("Backing up keypair");
                File.Copy(publicKeyFile, backupPublicKeyFile, true);
                File.Copy(privateKeyFile, backupPrivateKeyFile, true);
            }
        }

        private void GenerateNewKeypair()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024))
            {
                try
                {
                    m_publicKey = rsa.ToXmlString(false);
                    m_privateKey = rsa.ToXmlString(true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e);
                }
                finally
                {
                    //Don't save the key in the machine store.
                    rsa.PersistKeyInCsp = false;
                }
            }
            File.WriteAllText(publicKeyFile, m_publicKey);
            File.WriteAllText(privateKeyFile, m_privateKey);
        }

        public void SaveSettings()
        {
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(settingsFile))
            {
                xmlDoc.Load(settingsFile);
            }
            else
            {
                xmlDoc.LoadXml(NewXMLString());
            }
            xmlDoc.SelectSingleNode("/settings/global/@username").Value = m_playerName;
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@cache-size").Value = cacheSize.ToString();
            }
            catch
            {
                XmlAttribute cacheAttribute = xmlDoc.CreateAttribute("cache-size");
                cacheAttribute.Value = DEFAULT_CACHE_SIZE.ToString();
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(cacheAttribute);
            }
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@disclaimer").Value = m_disclaimerAccepted.ToString();
            }
            catch
            {
                XmlAttribute disclaimerAttribute = xmlDoc.CreateAttribute("disclaimer");
                disclaimerAttribute.Value = "0";
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(disclaimerAttribute);
            }
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@player-color").Value = playerColor.r.ToString() + ", " + playerColor.g.ToString() + ", " + playerColor.b.ToString();
            }
            catch
            {
                XmlAttribute colorAttribute = xmlDoc.CreateAttribute("player-color");
                colorAttribute.Value = playerColor.r.ToString() + ", " + playerColor.g.ToString() + ", " + playerColor.b.ToString();
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(colorAttribute);
            }
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@chat-key").Value = ((int)chatKey).ToString();
            }
            catch
            {
                XmlAttribute chatKeyAttribute = xmlDoc.CreateAttribute("chat-key");
                chatKeyAttribute.Value = ((int)chatKey).ToString();
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(chatKeyAttribute);
            }
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@screenshot-key").Value = ((int)screenshotKey).ToString();
            }
            catch
            {
                XmlAttribute screenshotKeyAttribute = xmlDoc.CreateAttribute("screenshot-key");
                screenshotKeyAttribute.Value = ((int)screenshotKey).ToString();
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(screenshotKeyAttribute);
            }
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@selected-flag").Value = m_flag;
            }
            catch
            {
                XmlAttribute selectedFlagAttribute = xmlDoc.CreateAttribute("selected-flag");
                selectedFlagAttribute.Value = m_flag;
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(selectedFlagAttribute);
            }
            XmlNode serverNodeList = xmlDoc.SelectSingleNode("/settings/servers");
            serverNodeList.RemoveAll();
            foreach (ServerEntry server in servers)
            {
                XmlElement serverElement = xmlDoc.CreateElement("server");
                serverElement.SetAttribute("name", server.name);
                serverElement.SetAttribute("address", server.address);
                serverElement.SetAttribute("port", server.port.ToString());
                serverNodeList.AppendChild(serverElement);
            }
            xmlDoc.Save(settingsFile);
            File.Copy(settingsFile, backupSettingsFile, true);
        }

        private string NewXMLString()
        {
            return String.Format("<?xml version=\"1.0\"?><settings><global username=\"{0}\" cache-size=\"{1}\"/><servers></servers></settings>", DEFAULT_PLAYER_NAME, DEFAULT_CACHE_SIZE);
        }
    }

    public class ServerEntry
    {
        public string name;
        public string address;
        public int port;
    }
}

