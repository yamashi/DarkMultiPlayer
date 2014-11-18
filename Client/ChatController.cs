using System;
using System.Collections.Generic;
using UnityEngine;
using DarkMultiPlayerCommon;
using MessageStream;

namespace DarkMultiPlayer
{
    public class ChatController
    {
        public bool Display { get { return m_model.Display; } set { m_model.Display = value; } }
        public string ConsoleId { get { return m_model.ConsoleId; } set { m_model.ConsoleId = value; } }

        private bool m_registered = false;

        public bool workerEnabled = false;

        private ChatView m_view;
        private ChatModel m_model;

        //State tracking
        public bool ChatActive {get;set;}
        //chat command register
        private Dictionary<string, ChatCommand> registeredChatCommands = new Dictionary<string, ChatCommand>();
        //const
        private const string DMP_CHAT_LOCK = "DMP_ChatLock";
        private const string DMP_CHAT_WINDOW_LOCK = "DMP_Chat_Window_Lock";
        public const ControlTypes BLOCK_ALL_CONTROLS = ControlTypes.ALL_SHIP_CONTROLS | ControlTypes.ACTIONS_ALL | ControlTypes.EVA_INPUT | ControlTypes.TIMEWARP | ControlTypes.MISC | ControlTypes.GROUPS_ALL | ControlTypes.CUSTOM_ACTION_GROUPS;

        public ChatController()
        {
            m_model = new ChatModel();

            m_view = new ChatView(m_model);
            m_view.OnLeave += this.OnLeave;
            m_view.OnSend += this.HandleChatInput;

            RegisterChatCommand("help", DisplayHelpCommand, "Displays this help");
            RegisterChatCommand("join", JoinChannelCommand, "Joins a new chat channel");
            RegisterChatCommand("query", StartQueryCommand, "Starts a query");
            RegisterChatCommand("leave", LeaveChannelCommand, "Leaves the current channel");
            RegisterChatCommand("part", LeaveChannelCommand, "Leaves the current channel");
            RegisterChatCommand("ping", ServerPingCommand, "Pings the server");
            RegisterChatCommand("motd", ServerMOTDCommand, "Gets the current Message of the Day");
            RegisterChatCommand("resize", ResizeChatCommand, "Resized the chat window");
            RegisterChatCommand("version", DisplayVersionCommand, "Displays the current version of DMP");
        }

        private void PrintToSelectedChannel(string text)
        {
            if (m_model.Channels.Selected == null && m_model.Privates.Selected == null)
            {
                AddChannelMessage(Settings.fetch.playerName, "", text);
            }
            if (m_model.Channels.Selected != null && !m_model.IsConsole)
            {
                AddChannelMessage(Settings.fetch.playerName, m_model.Channels.Selected, text);
            }
            if (m_model.IsConsole)
            {
                AddSystemMessage(text);
            }
            if (m_model.Privates.Selected != null)
            {
                AddPrivateMessage(Settings.fetch.playerName, m_model.Privates.Selected, text);
            }
        }

        private void DisplayHelpCommand(string commandArgs)
        {
            List<ChatCommand> commands = new List<ChatCommand>();
            int longestName = 0;
            foreach (ChatCommand cmd in registeredChatCommands.Values)
            {
                commands.Add(cmd);
                if (cmd.name.Length > longestName)
                {
                    longestName = cmd.name.Length;
                }
            }
            commands.Sort();
            foreach (ChatCommand cmd in commands)
            {
                string helpText = cmd.name.PadRight(longestName) + " - " + cmd.description;
                PrintToSelectedChannel(helpText);
            }
        }

        private void DisplayVersionCommand(string commandArgs)
        {
            string versionMessage = (Common.PROGRAM_VERSION.Length == 40) ? "DarkMultiPlayer development build " + Common.PROGRAM_VERSION.Substring(0, 7) : "DarkMultiPlayer " + Common.PROGRAM_VERSION;
            PrintToSelectedChannel(versionMessage);
        }

        private void JoinChannelCommand(string commandArgs)
        {
            if (commandArgs != "" && commandArgs != "Global" && commandArgs != m_model.ConsoleId)
            {
                DarkLog.Debug("Joining channel " + commandArgs);
                
                m_model.JoinChannel(commandArgs);
                
                using (MessageWriter mw = new MessageWriter())
                {
                    mw.Write<int>((int)ChatMessageType.JOIN);
                    mw.Write<string>(Settings.fetch.playerName);
                    mw.Write<string>(commandArgs);
                    NetworkWorker.fetch.SendChatMessage(mw.GetMessageBytes());
                }
            }
            else
            {
                ScreenMessages.PostScreenMessage("Couln't join '" + commandArgs + "', channel name not valid!");
            }
        }

        private void LeaveChannelCommand(string commandArgs)
        {
            m_model.LeaveChannel(commandArgs);
        }

        private void StartQueryCommand(string commandArgs)
        {
            bool playerFound = false;
            if (commandArgs != m_model.ConsoleId)
            {
                foreach (PlayerStatus ps in PlayerStatusWorker.fetch.playerStatusList)
                {
                    if (ps.playerName == commandArgs)
                    {
                        playerFound = true;
                    }
                }
            }
            else
            {
                //Make sure we can always query the server.
                playerFound = true;
            }
            if (playerFound)
            {
                DarkLog.Debug("Starting query with " + commandArgs);

                m_model.JoinPrivate(commandArgs);
            }
            else
            {
                DarkLog.Debug("Couln't start query with '" + commandArgs + "', player not found!");
            }
        }

        private void ServerPingCommand(string commandArgs)
        {
            NetworkWorker.fetch.SendPingRequest();
        }

        private void ServerMOTDCommand(string commandArgs)
        {
            NetworkWorker.fetch.SendMotdRequest();
        }

        private void ResizeChatCommand(string commandArgs)
        {
            string func = "";
            float size = 0;

            func = commandArgs;
            if (commandArgs.Contains(" "))
            {
                func = commandArgs.Substring(0, commandArgs.IndexOf(" "));
                if (commandArgs.Substring(func.Length).Contains(" "))
                {
                    try
                    {
                        size = Convert.ToSingle(commandArgs.Substring(func.Length + 1));
                    }
                    catch (FormatException)
                    {
                        PrintToSelectedChannel("Error: " + size + " is not a valid number");
                        size = 400f;
                    }
                }
            }
            
            switch (func)
            {
                default:
                    PrintToSelectedChannel("Undefined function. Usage: /resize [default|medium|large], /resize [x|y] size, or /resize show");
                    PrintToSelectedChannel("Chat window size is currently: " + m_view.Width + "x" + m_view.Height);
                    break;
                case "x":
                    if (size <= 800 && size >= 300)
                    {
                        m_view.Width = size;

                        PrintToSelectedChannel("New window size is: " + m_view.Width + "x" + m_view.Height);
                    }
                    else
                    {
                        PrintToSelectedChannel("Size is out of range.");
                    }
                    break;
                case "y":
                    if (size <= 800 && size >= 300)
                    {
                        m_view.Height = size;

                        PrintToSelectedChannel("New window size is: " + m_view.Width + "x" + m_view.Height);
                    }
                    else
                    {
                        PrintToSelectedChannel("Size is out of range.");
                    }
                    break;
                case "default":
                    m_view.Height = 300;
                    m_view.Width = 400;
                    PrintToSelectedChannel("New window size is: " + m_view.Width + "x" + m_view.Height);
                    break;
                case "medium":
                    m_view.Height = 600;
                    m_view.Width = 600;
                    PrintToSelectedChannel("New window size is: " + m_view.Width + "x" + m_view.Height);
                    break;
                case "large":
                    m_view.Height = 800;
                    m_view.Width = 800;
                    PrintToSelectedChannel("New window size is: " + m_view.Width + "x" + m_view.Height);
                    break;
                case "show":
                    PrintToSelectedChannel("New window size is: " + m_view.Width + "x" + m_view.Height);
                    break;
            }
        }

        public void JoinChannel(string playerName, string channelName)
        {
            m_model.AddPlayerChannel(playerName, channelName);
        }

        public void LeaveChannel(string playerName, string channelName)
        {
            m_model.RemovePlayerChannel(playerName, channelName);
        }

        public void AddChannelMessage(string fromPlayer, string channelName, string channelMessage)
        {
            if ((m_model.Channels.Selected == null && m_model.Privates.Selected == null && channelName != "") ||
                (m_model.Channels.Selected != channelName))
            {
                m_model.Channels.Highlight(channelName);
            }

            if(m_model.Privates.Selected == null)
            {
                if((m_model.Channels.Selected == null && channelName == "") ||
                   (m_model.Channels.Selected == channelName))
                {
                    m_view.ScrollDown();
                }
            }

            m_model.Channels.AddMessage(channelName, fromPlayer + ": " + channelMessage);

            if (!m_model.Display)
            {
                ChatActive = true;
                if (channelName != "")
                {
                    ScreenMessages.PostScreenMessage(fromPlayer + " -> #" + channelName + ": " + channelMessage, 5f, ScreenMessageStyle.UPPER_LEFT);
                }
                else
                {
                    ScreenMessages.PostScreenMessage(fromPlayer + " -> #Global : " + channelMessage, 5f, ScreenMessageStyle.UPPER_LEFT);
                }
            }
        }

        public void AddPrivateMessage(string fromPlayer, string toPlayer, string privateMessage)
        {
            if (fromPlayer != Settings.fetch.playerName)
            {
                m_model.Privates.AddMessage(fromPlayer, fromPlayer + ": " + privateMessage);

                if (m_model.Privates.Selected != fromPlayer)
                {
                    m_model.Privates.Highlight(fromPlayer);
                }
            }
            //Move the bar to the bottom on a new message
            if (m_model.Privates.Selected != null && m_model.Channels.Selected == null && m_model.Privates.Selected == fromPlayer)
            {
                m_view.ScrollDown();
            }

            if (!m_model.Display)
            {
                ChatActive = true;
                if (fromPlayer != Settings.fetch.playerName)
                {
                    ScreenMessages.PostScreenMessage(fromPlayer + " -> @" + toPlayer + ": " + privateMessage, 5f, ScreenMessageStyle.UPPER_LEFT);
                }
            }
        }

        public void RemovePlayer(string playerName)
        {
            m_model.RemovePlayer(playerName);
        }

        public void SendServerMessage(string message)
        {
            using (MessageWriter mw = new MessageWriter())
            {
                mw.Write<int>((int)ChatMessageType.PRIVATE_MESSAGE);
                mw.Write<string>(Settings.fetch.playerName);
                mw.Write<string>(m_model.ConsoleId);
                mw.Write<string>(message);
                NetworkWorker.fetch.SendChatMessage(mw.GetMessageBytes());
            }
        }

        public void AddSystemMessage(string message)
        {
            m_model.Channels.Highlight(m_model.ConsoleId);
            //Move the bar to the bottom on a new message
            if(m_model.IsConsole)
            {
                m_view.ScrollDown();
            }

            m_model.Channels.AddMessage(m_model.ConsoleId, message);
        }

        public void RegisterChatCommand(string command, Action<string> func, string description)
        {
            ChatCommand cmd = new ChatCommand(command, func, description);
            if (!registeredChatCommands.ContainsKey(command))
            {
                registeredChatCommands.Add(command, cmd);
            }
        }

        private void HandleChatInput(string input)
        {
            if (!input.StartsWith("/") || input.StartsWith("//"))
            {
                //Handle chat messages
                if (input.StartsWith("//"))
                {
                    input = input.Substring(1);
                }

                if (m_model.IsGlobal)
                {
                    //Sending a global chat message
                    using (MessageWriter mw = new MessageWriter())
                    {
                        mw.Write<int>((int)ChatMessageType.CHANNEL_MESSAGE);
                        mw.Write<string>(Settings.fetch.playerName);
                        //Global channel name is empty string.
                        mw.Write<string>("");
                        mw.Write<string>(input);
                        NetworkWorker.fetch.SendChatMessage(mw.GetMessageBytes());
                    }
                }
                if (m_model.Channels.Selected != null && !m_model.IsConsole)
                {
                    using (MessageWriter mw = new MessageWriter())
                    {
                        mw.Write<int>((int)ChatMessageType.CHANNEL_MESSAGE);
                        mw.Write<string>(Settings.fetch.playerName);
                        mw.Write<string>(m_model.Channels.Selected);
                        mw.Write<string>(input);
                        NetworkWorker.fetch.SendChatMessage(mw.GetMessageBytes());
                    }
                }
                if (m_model.IsConsole)
                {
                    using (MessageWriter mw = new MessageWriter())
                    {
                        mw.Write<int>((int)ChatMessageType.CONSOLE_MESSAGE);
                        mw.Write<string>(Settings.fetch.playerName);
                        mw.Write<string>(input);
                        NetworkWorker.fetch.SendChatMessage(mw.GetMessageBytes());
                        DarkLog.Debug("Server Command: " + input);
                    }
                }
                if (m_model.Privates.Selected != null)
                {
                    using (MessageWriter mw = new MessageWriter())
                    {
                        mw.Write<int>((int)ChatMessageType.PRIVATE_MESSAGE);
                        mw.Write<string>(Settings.fetch.playerName);
                        mw.Write<string>(m_model.Privates.Selected);
                        mw.Write<string>(input);
                        NetworkWorker.fetch.SendChatMessage(mw.GetMessageBytes());
                    }
                }
            }
            else
            {
                string commandPart = input.Substring(1);
                string argumentPart = "";
                if (commandPart.Contains(" "))
                {
                    if (commandPart.Length > commandPart.IndexOf(' ') + 1)
                    {
                        argumentPart = commandPart.Substring(commandPart.IndexOf(' ') + 1);
                    }
                    commandPart = commandPart.Substring(0, commandPart.IndexOf(' '));
                }
                if (commandPart.Length > 0)
                {
                    if (registeredChatCommands.ContainsKey(commandPart))
                    {
                        try
                        {
                            DarkLog.Debug("Chat Command: " + input.Substring(1));
                            registeredChatCommands[commandPart].func(argumentPart);
                        }
                        catch (Exception e)
                        {
                            DarkLog.Debug("Error handling chat command " + commandPart + ", Exception " + e);
                            PrintToSelectedChannel("Error handling chat command: " + commandPart);
                        }
                    }
                    else
                    {
                        PrintToSelectedChannel("Unknown chat command: " + commandPart);
                    }
                }
            }
        }

        private void OnLeave()
        {
            if (m_model.Channels.Selected != null && !m_model.IsConsole)
            {
                using (MessageWriter mw = new MessageWriter())
                {
                    mw.Write<int>((int)ChatMessageType.LEAVE);
                    mw.Write<string>(Settings.fetch.playerName);
                    mw.Write<string>(m_model.Channels.Selected);
                    NetworkWorker.fetch.SendChatMessage(mw.GetMessageBytes());
                }

                m_model.LeaveChannel(m_model.Channels.Selected);
            }
            else if(m_model.Privates.Selected != null)
            {
                m_model.LeavePrivate(m_model.Privates.Selected);
            }
        }


        private void Update()
        {
            m_view.Update();
        }

        public void Draw()
        {
            m_view.Draw();
        }
        public void Reset()
        {
            lock (Client.eventLock)
            {
                workerEnabled = false;

                m_view.Reset();

                if(m_registered == false)
                {
                    m_registered = true;
                    Client.updateEvent.Add(this.Update);
                    Client.drawEvent.Add(this.Draw);
                }

            }
        }

        private class ChatCommand : IComparable
        {
            public string name;
            public Action<string> func;
            public string description;

            public ChatCommand(string name, Action<string> func, string description)
            {
                this.name = name;
                this.func = func;
                this.description = description;
            }

            public int CompareTo(object obj)
            {
                var cmd = obj as ChatCommand;
                return this.name.CompareTo(cmd.name);
            }
        }

    }
}

