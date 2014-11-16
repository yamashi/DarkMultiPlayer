using System;
using System.Threading;
using System.Collections.Generic;
using DarkMultiPlayerCommon;

namespace DarkMultiPlayerServer
{
    public class CommandHandler
    {
        private Dictionary<string, Command> m_commands = new Dictionary<string, Command>();

        public CommandHandler()
        {
            AsyncConsoleReader.Initialize();

            RegisterCommand("help", DisplayHelp, "Displays this help");
            RegisterCommand("say", Say, "Broadcasts a message to clients");
            RegisterCommand("dekessler", Dekessler.RunDekessler, "Clears out debris from the server");
            RegisterCommand("nukeksc", NukeKSC.RunNukeKSC, "Clears ALL vessels from KSC and the Runway");
            RegisterCommand("listclients", ListClients, "Lists connected clients");
            RegisterCommand("countclients", CountClients, "Counts connected clients");
        }

        public void Run()
        {
            string input = AsyncConsoleReader.ReadLine();
            while (input != "")
            {
                try
                {
                    DarkLog.Normal("Command input: " + input);
                    if (input.StartsWith("/"))
                    {
                        HandleServerInput(input.Substring(1));
                    }
                    else
                    {
                        if (input != "")
                        {
                            m_commands["say"].func(input);
                        }
                    }

                    input = AsyncConsoleReader.ReadLine();

                }
                catch (Exception e)
                {
                    if (Server.serverRunning)
                    {
                        DarkLog.Fatal("Error in command handler thread, Exception: " + e);
                        throw;
                    }
                }
            }

        }

        public void HandleServerInput(string input)
        {
            string commandPart = input;
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
                if (m_commands.ContainsKey(commandPart))
                {
                    try
                    {
                        m_commands[commandPart].func(argumentPart);
                    }
                    catch (Exception e)
                    {
                        DarkLog.Error("Error handling server command " + commandPart + ", Exception " + e);
                    }
                }
                else
                {
                    DarkLog.Normal("Unknown server command: " + commandPart);
                }
            }
        }

        public void RegisterCommand(string command, Action<string> func, string description)
        {
            Command cmd = new Command(command, func, description);
            if (!m_commands.ContainsKey(command))
            {
                m_commands.Add(command, cmd);
            }
        }

        private void DisplayHelp(string commandArgs)
        {
            List<Command> commands = new List<Command>();
            int longestName = 0;
            foreach (Command cmd in m_commands.Values)
            {
                commands.Add(cmd);
                if (cmd.name.Length > longestName)
                {
                    longestName = cmd.name.Length;
                }
            }
            commands.Sort();
            foreach (Command cmd in commands)
            {
                DarkLog.Normal(cmd.name.PadRight(longestName) + " - " + cmd.description);
            }
        }

        private static void Say(string sayText)
        {
            DarkLog.Normal("Broadcasting " + sayText);

            Messages.ServerClient_ChatMessageSend msg = new Messages.ServerClient_ChatMessageSend();
            msg.message = sayText;
            msg.name = Settings.settingsStore.consoleIdentifier;
            msg.channel = "";
            msg.type = (byte)ChatMessageType.CHANNEL_MESSAGE;

            WorldManager.Instance.Broadcast(msg);
        }

        private static void ListClients(string commandArgs)
        {
            if (Server.players != "")
            {
                DarkLog.Normal("Online players: " + Server.players);
            }
            else
            {
                DarkLog.Normal("No clients connected");
            }
        }

        private static void CountClients(string commandArgs)
        {
            DarkLog.Normal("Online players: " + Server.playerCount);
        }

        private class Command : IComparable
        {
            public string name;
            public Action<string> func;
            public string description;

            public Command(string name, Action<string> func, string description)
            {
                this.name = name;
                this.func = func;
                this.description = description;
            }

            public int CompareTo(object obj)
            {
                var cmd = obj as Command;
                return this.name.CompareTo(cmd.name);
            }
        }
    }
}

