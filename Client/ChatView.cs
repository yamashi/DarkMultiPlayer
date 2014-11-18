using System;
using System.Collections.Generic;
using UnityEngine;
using DarkMultiPlayerCommon;
using MessageStream;

namespace DarkMultiPlayer
{
    public delegate void SendDelegate(string aMesssage);
    public delegate void LeaveDelegate();
    public class ChatView
    {
        private bool m_isWindowLocked = false;
        private bool m_initialized = false;

        private ChatModel m_model;

        public event SendDelegate OnSend;
        public event LeaveDelegate OnLeave;

        //State tracking
        public bool m_chatButtonHighlighted = false;
        private bool m_chatLocked = false;
        private bool m_ignoreChatInput = false;
        private bool m_selectTextBox = false;
        private string m_inputText = "";
        //GUILayout stuff
        private Rect m_windowRect;
        private Rect m_moveRect;
        private GUILayoutOption[] m_layoutOptions;
        private GUILayoutOption[] m_smallSizeOptions;
        private GUIStyle m_windowStyle;
        private GUIStyle m_labelStyle;
        private GUIStyle m_buttonStyle;
        private GUIStyle m_highlightStyle;
        private GUIStyle m_textAreaStyle;
        private GUIStyle m_scrollStyle;
        private Vector2 m_scrollPosition;
        private Vector2 m_playerScrollPosition;
        //window size
        private float WINDOW_HEIGHT = 300;
        private float WINDOW_WIDTH = 400;

        public float Height
        {
            get { return WINDOW_HEIGHT; }
            set { WINDOW_HEIGHT = value; m_initialized = false; }
        }

        public float Width
        {
            get { return WINDOW_HEIGHT; }
            set { WINDOW_HEIGHT = value; m_initialized = false; }
        }

        //const
        private const string DMP_CHAT_LOCK = "DMP_ChatLock";
        private const string DMP_CHAT_WINDOW_LOCK = "DMP_Chat_Window_Lock";
        public const ControlTypes BLOCK_ALL_CONTROLS = ControlTypes.ALL_SHIP_CONTROLS | ControlTypes.ACTIONS_ALL | ControlTypes.EVA_INPUT | ControlTypes.TIMEWARP | ControlTypes.MISC | ControlTypes.GROUPS_ALL | ControlTypes.CUSTOM_ACTION_GROUPS;

        public ChatView(ChatModel aModel)
        {
            m_model = aModel;
        }

        private void InitGUI()
        {
            //Setup GUI stuff
            m_windowRect = new Rect(Screen.width / 10, Screen.height / 2f - WINDOW_HEIGHT / 2f, WINDOW_WIDTH, WINDOW_HEIGHT);
            m_moveRect = new Rect(0, 0, 10000, 20);

            m_layoutOptions = new GUILayoutOption[4];
            m_layoutOptions[0] = GUILayout.MinWidth(WINDOW_WIDTH);
            m_layoutOptions[1] = GUILayout.MaxWidth(WINDOW_WIDTH);
            m_layoutOptions[2] = GUILayout.MinHeight(WINDOW_HEIGHT);
            m_layoutOptions[3] = GUILayout.MaxHeight(WINDOW_HEIGHT);

            m_smallSizeOptions = new GUILayoutOption[1];
            m_smallSizeOptions[0] = GUILayout.Width(WINDOW_WIDTH * .25f);

            m_windowStyle = new GUIStyle(GUI.skin.window);
            m_scrollStyle = new GUIStyle(GUI.skin.scrollView);

            m_scrollPosition = new Vector2(0, 0);
            m_labelStyle = new GUIStyle(GUI.skin.label);
            m_buttonStyle = new GUIStyle(GUI.skin.button);
            m_highlightStyle = new GUIStyle(GUI.skin.button);
            m_highlightStyle.normal.textColor = Color.red;
            m_highlightStyle.active.textColor = Color.red;
            m_highlightStyle.hover.textColor = Color.red;
            m_textAreaStyle = new GUIStyle(GUI.skin.textArea);
        }

        public void Update()
        {
            if (m_chatButtonHighlighted && m_model.Display)
            {
                m_chatButtonHighlighted = false;
            }
            if (m_chatLocked && !m_model.Display)
            {
                m_chatLocked = false;
                InputLockManager.RemoveControlLock(DMP_CHAT_LOCK);
            }
        }

        public void ScrollDown()
        {
            m_scrollPosition.y = float.PositiveInfinity;
        }

        public void Draw()
        {
            if (!m_initialized)
            {
                InitGUI();
                m_initialized = true;
            }
            if (m_model.Display)
            {
                bool pressedChatShortcutKey = (Event.current.type == EventType.KeyDown && Event.current.keyCode == Settings.fetch.chatKey);
                if (pressedChatShortcutKey)
                {
                    m_ignoreChatInput = true;
                    m_selectTextBox = true;
                }
                m_windowRect = GUILayout.Window(6704 + Client.WINDOW_OFFSET, m_windowRect, DrawContent, "DarkMultiPlayer Chat", m_windowStyle, m_layoutOptions);
            }
            CheckWindowLock();
        }

        private void DrawContent(int windowID)
        {
            bool pressedEnter = (Event.current.type == EventType.KeyDown && !Event.current.shift && Event.current.character == '\n');
            GUILayout.BeginVertical();
            GUI.DragWindow(m_moveRect);
            GUILayout.BeginHorizontal();
            {
                DrawRooms();

                GUILayout.FlexibleSpace();

                if ((m_model.Channels.Selected != null && !m_model.IsConsole) || m_model.Privates.Selected != null)
                {
                    if (GUILayout.Button("Leave", m_buttonStyle))
                    {
                        OnLeave();
                    }
                }

                DrawConsole();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, m_scrollStyle);

            List<string> messages = m_model.ActiveMessages;
            foreach (string channelMessage in messages)
            {
                GUILayout.Label(channelMessage, m_labelStyle);
            }

            GUILayout.EndScrollView();
            m_playerScrollPosition = GUILayout.BeginScrollView(m_playerScrollPosition, m_scrollStyle, m_smallSizeOptions);
            GUILayout.BeginVertical();
            GUILayout.Label(Settings.fetch.playerName, m_labelStyle);
            if (m_model.Privates.Selected != null)
            {
                GUILayout.Label(m_model.Privates.Selected, m_labelStyle);
            }
            else
            {
                if (m_model.Privates.Selected == null)
                {
                    //Global chat
                    foreach (PlayerStatus player in PlayerStatusWorker.fetch.playerStatusList)
                    {
                        if (m_model.Privates.Contains(player.playerName))
                        {
                            GUI.enabled = false;
                        }
                        if (GUILayout.Button(player.playerName, m_labelStyle))
                        {
                            m_model.JoinPrivate(player.playerName);
                        }
                        GUI.enabled = true;
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, List<string>> playerEntry in m_model.PlayersChannels)
                    {
                        if (playerEntry.Key != Settings.fetch.playerName)
                        {
                            if (playerEntry.Value.Contains(m_model.Channels.Selected))
                            {
                                if (m_model.Privates.Contains(playerEntry.Key))
                                {
                                    GUI.enabled = false;
                                }
                                if (GUILayout.Button(playerEntry.Key, m_labelStyle))
                                {
                                    m_model.JoinPrivate(playerEntry.Key);
                                }
                                GUI.enabled = true;
                            }
                        }
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("SendTextArea");
            string tempSendText = GUILayout.TextArea(m_inputText, m_textAreaStyle);
            //Don't add the newline to the messages, queue a send
            if (!m_ignoreChatInput)
            {
                if (pressedEnter)
                {
                    OnSend(m_inputText);
                    m_inputText = "";
                }
                else
                {
                    m_inputText = tempSendText;
                }
            }
            if (m_inputText == "")
            {
                GUI.enabled = false;
            }
            if (GUILayout.Button("Send", m_buttonStyle, m_smallSizeOptions))
            {
                OnSend(m_inputText);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            if ((GUI.GetNameOfFocusedControl() == "SendTextArea") && !m_chatLocked)
            {
                m_chatLocked = true;
                InputLockManager.SetControlLock(BLOCK_ALL_CONTROLS, DMP_CHAT_LOCK);
            }
            if ((GUI.GetNameOfFocusedControl() != "SendTextArea") && m_chatLocked)
            {
                m_chatLocked = false;
                InputLockManager.RemoveControlLock(DMP_CHAT_LOCK);
            }
            if (m_selectTextBox)
            {
                m_selectTextBox = false;
                GUI.FocusControl("SendTextArea");
            }
        }

        private void DrawConsole()
        {
            GUIStyle possibleHighlightButtonStyle = m_buttonStyle;
            if (m_model.IsConsole)
            {
                GUI.enabled = false;
            }
            if (m_model.Channels.IsHighlighted(m_model.ConsoleId))
            {
                possibleHighlightButtonStyle = m_highlightStyle;
            }
            else
            {
                possibleHighlightButtonStyle = m_buttonStyle;
            }
            if (Client.Instance.AdminSystem.IsAdmin(Settings.fetch.playerName))
            {
                if (GUILayout.Button("#" + m_model.ConsoleId, possibleHighlightButtonStyle))
                {
                    m_model.FocusChannel(m_model.ConsoleId);

                    ScrollDown();
                }
            }
            GUI.enabled = true;
        }

        private void DrawRooms()
        {
            GUIStyle possibleHighlightButtonStyle = m_buttonStyle;
            if (m_model.Channels.Selected == null && m_model.Privates.Selected == null)
            {
                GUI.enabled = false;
            }
            if (m_model.Channels.IsHighlighted(""))
            {
                possibleHighlightButtonStyle = m_highlightStyle;
            }
            else
            {
                possibleHighlightButtonStyle = m_buttonStyle;
            }
            if (GUILayout.Button("#Global", possibleHighlightButtonStyle))
            {
                m_model.FocusChannel("");
                m_model.Channels.Selected = null;

                ScrollDown();
            }
            GUI.enabled = true;
            foreach (var kvp in m_model.Channels.Channels)
            {
                if (kvp.Key == "")
                    continue;

                if (m_model.Channels.Selected == kvp.Key)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("#" + kvp.Key, kvp.Value.Highlight ? m_highlightStyle : m_buttonStyle))
                {
                    m_model.FocusChannel(kvp.Key);

                    ScrollDown();
                }
                GUI.enabled = true;
            }

            foreach (var kvp in m_model.Privates.Channels)
            {
                if (m_model.Privates.Selected == kvp.Key)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("@" + kvp.Key, kvp.Value.Highlight ? m_highlightStyle : m_buttonStyle))
                {
                    m_model.FocusPrivate(kvp.Key);

                    ScrollDown();
                }
                GUI.enabled = true;
            }
        }


        public void Resize(float aWidth, float aHeight)
        {
            Width = aWidth;
            Height = aHeight;
        }

        public bool Contains(Vector2 aPoint)
        {
            return m_windowRect.Contains(aPoint);
        }

        public void Reset()
        {
            lock (Client.eventLock)
            {
                RemoveWindowLock();
            }
        }

        private void CheckWindowLock()
        {
            if (!Client.Instance.gameRunning)
            {
                RemoveWindowLock();
                return;
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                RemoveWindowLock();
                return;
            }

            if (m_model.Display)
            {
                Vector2 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;

                bool shouldLock = m_windowRect.Contains(mousePos);

                if (shouldLock && !m_isWindowLocked)
                {
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, DMP_CHAT_WINDOW_LOCK);
                    m_isWindowLocked = true;
                }
                if (!shouldLock && m_isWindowLocked)
                {
                    RemoveWindowLock();
                }
            }

            if (!m_model.Display && m_isWindowLocked)
            {
                RemoveWindowLock();
            }
        }

        private void RemoveWindowLock()
        {
            if (m_isWindowLocked)
            {
                m_isWindowLocked = false;
                InputLockManager.RemoveControlLock(DMP_CHAT_WINDOW_LOCK);
            }
        }
    }
}

