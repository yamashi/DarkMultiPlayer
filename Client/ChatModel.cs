using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkMultiPlayer
{
    public class Channel
    {
        private List<string> m_messages = new List<string>();

        public List<string> Messages { get { return m_messages; } }

        public bool Highlight { get; set; }
    }

    public class ChannelManager
    {
        private Dictionary<string, Channel> m_channels = new Dictionary<string, Channel>();

        public Dictionary<string, Channel> Channels { get { return m_channels; } }

        public string Selected { get; set; }

        public List<string> GetMessages(string aChannel)
        {
            if (!m_channels.ContainsKey(aChannel))
            {
                m_channels.Add(aChannel, new Channel());
            }

            return m_channels[aChannel].Messages;
        }

        public void Add(string aChannel)
        {
            if (!m_channels.ContainsKey(aChannel))
            {
                m_channels.Add(aChannel, new Channel());
            }
        }

        public void Remove(string aChannel)
        {
            if (m_channels.ContainsKey(aChannel))
            {
                m_channels.Remove(aChannel);
            }
        }

        public bool Contains(string aChannel)
        {
            if (aChannel == null)
                return false;

            return m_channels.ContainsKey(aChannel);
        }

        public void AddMessage(string aChannel, string aMessage)
        {
            Add(aChannel);

            m_channels[aChannel].Messages.Add(aMessage);
        }

        public bool IsHighlighted(string aChanel)
        {
            if(Contains(aChanel))
            {
                return m_channels[aChanel].Highlight;
            }

            return false;
        }

        public void Focus(string aChannel)
        {
            Add(aChannel);

            m_channels[aChannel].Highlight = false;
        }

        public void Highlight(string aChannel)
        {
            Add(aChannel);

            m_channels[aChannel].Highlight = true;
        }
    }

    public class ChatModel
    {
        private ChannelManager m_channels = new ChannelManager();
        private ChannelManager m_privates = new ChannelManager();

        public ChannelManager Channels { get { return m_channels; } }
        public ChannelManager Privates { get { return m_privates; } }


        private Dictionary<string, List<string>> m_playersChannels = new Dictionary<string, List<string>>();

        public Dictionary<string, List<string>> PlayersChannels { get { return m_playersChannels; } }

        public string ConsoleId { get; set; }

        public bool IsGlobal { get { return Channels.Selected == null && Privates.Selected == null; } }

        public bool IsConsole { get { return Channels.Selected == ConsoleId; } }

        public bool Display { get; set; }

        public List<string> ActiveMessages
        {
            get
            {
                if (Channels.Selected == null && Privates.Selected == null)
                {
                    return Channels.GetMessages("");
                }
                else if (Channels.Selected != null)
                {
                    return Channels.GetMessages(Channels.Selected);
                }
                else if (Privates.Selected != null)
                {
                    return Privates.GetMessages(Privates.Selected);
                }
                return new List<string>();
            }
        }

        public void AddPlayerChannel(string aPlayer, string aChannel)
        {
            if (!m_playersChannels.ContainsKey(aPlayer))
            {
                m_playersChannels.Add(aPlayer, new List<string>());
            }
            if (!m_playersChannels[aPlayer].Contains(aChannel))
            {
                m_playersChannels[aPlayer].Add(aChannel);
            }
        }

        public void RemovePlayerChannel(string aPlayer, string aChannel)
        {
            if (m_playersChannels.ContainsKey(aPlayer))
            {
                if (m_playersChannels[aPlayer].Contains(aChannel))
                {
                    m_playersChannels[aPlayer].Remove(aChannel);
                }
                if (m_playersChannels[aPlayer].Count == 0)
                {
                    m_playersChannels.Remove(aPlayer);
                }
            }
        }

        public void RemovePlayer(string aPlayer)
        {
            if (m_playersChannels.ContainsKey(aPlayer))
            {
                m_playersChannels.Remove(aPlayer);
            }

            Privates.Remove(aPlayer);
        }

        public void JoinChannel(string aChannel)
        {
            Channels.Add(aChannel);

            Channels.Selected = aChannel;
            Privates.Selected = null;
        }
        public void LeaveChannel(string aChannel)
        {
            Channels.Remove(aChannel);

            Channels.Selected = null;
            Privates.Selected = null;
        }

        public void JoinPrivate(string aChannel)
        {
            Privates.Add(aChannel);

            Channels.Selected = null;
            Privates.Selected = aChannel;
        }

        public void LeavePrivate(string aChannel)
        {
            Privates.Remove(aChannel);

            Channels.Selected = null;
            Privates.Selected = null;
        }

        public void FocusChannel(string aChannel)
        {
            Channels.Focus(aChannel);

            Channels.Selected = aChannel;
            Privates.Selected = null;
        }

        public void FocusPrivate(string aChannel)
        {
            Privates.Focus(aChannel);

            Channels.Selected = null;
            Privates.Selected = aChannel;
        }
    }
}
