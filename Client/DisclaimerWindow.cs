using System;
using UnityEngine;

namespace DarkMultiPlayer
{
    //This disclaimer exists because I was contacted by a moderator pointing me to the addon posting rules.
    public class DisclaimerWindow
    {
        private const int WINDOW_WIDTH = 500;
        private const int WINDOW_HEIGHT = 300;
        private Rect m_windowRect;
        private Rect m_moveRect;
        private bool m_display;
        private GUILayoutOption[] m_layoutOptions;

        private void InitGUI()
        {
            //Setup GUI stuff
            m_windowRect = new Rect((Screen.width / 2f) - (WINDOW_WIDTH / 2), (Screen.height / 2f) - (WINDOW_HEIGHT / 2f), WINDOW_WIDTH, WINDOW_HEIGHT);
            m_moveRect = new Rect(0, 0, 10000, 20);

            m_layoutOptions = new GUILayoutOption[2];
            m_layoutOptions[0] = GUILayout.ExpandWidth(true);
            m_layoutOptions[1] = GUILayout.ExpandHeight(true);
        }

        private void Draw()
        {
            if (m_display)
            {
                m_windowRect = GUILayout.Window(6713 + Client.WINDOW_OFFSET, m_windowRect, DrawContent, "DarkMultiPlayer - Disclaimer", m_layoutOptions);
            }
        }

        private void DrawContent(int windowID)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(m_moveRect);
            string disclaimerText = "DarkMultiPlayer shares the following possibly personally identifiable information with any server you connect to.\n";
            disclaimerText += "a) Your player name you connect with.\n";
            disclaimerText += "b) Your player token (A randomly generated string to authenticate you).\n";
            disclaimerText += "c) Your IP address is logged on the server console.\n";
            disclaimerText += "\n";
            disclaimerText += "DMP does not contact any other computer than the server you are connecting to.\n";
            disclaimerText += "In order to use DarkMultiPlayer, you must allow DMP to use this info\n";
            disclaimerText += "\n";
            disclaimerText += "For more information - see the KSP addon rules\n";
            GUILayout.Label(disclaimerText);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open the KSP Addon rules in the browser"))
            {
                Application.OpenURL("http://forum.kerbalspaceprogram.com/threads/87841-Add-on-Posting-Rules-July-24th-2014-going-into-effect-August-21st-2014!");
            }
            if (GUILayout.Button("I accept - Enable DarkMultiPlayer"))
            {
                DarkLog.Debug("User accepted disclaimer - Enabling DarkMultiPlayer");
                m_display = false;
                Client.Instance.Settings.DisclaimerAccepted = 1;
                Client.Instance.DisableMod = false;
                Client.Instance.Settings.SaveSettings();
            }
            if (GUILayout.Button("I decline - Disable DarkMultiPlayer"))
            {
                DarkLog.Debug("User declined disclaimer - Disabling DarkMultiPlayer");
                m_display = false;
            }
            GUILayout.EndVertical();
        }

        public void Enable()
        {
            Client.Instance.DrawEvent += this.Draw;
            InitGUI();
            m_display = true;
        }
    }
}

