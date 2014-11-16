using System;
using UnityEngine;

namespace DarkMultiPlayer
{
    public class IncorrectInstallWindow
    {
        public static IncorrectInstallWindow m_instance;

        private const int WINDOW_WIDTH = 600;
        private const int WINDOW_HEIGHT = 200;
        private Rect m_windowRect;
        private Rect m_moveRect;
        private bool m_initialized;
        private bool m_display;
        private GUILayoutOption[] m_layoutOptions;


        public static IncorrectInstallWindow Instance
        {
            get
            {
                return m_instance;
            }
        }

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
            if (!m_initialized)
            {
                m_initialized = true;
                InitGUI();
            }
            if (m_display)
            {
                m_windowRect = GUILayout.Window(6705 + Client.WINDOW_OFFSET, m_windowRect, DrawContent, "DarkMultiPlayer", m_layoutOptions);
            }
        }

        private void DrawContent(int windowID)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(m_moveRect);
            GUILayout.Label("DMP is not correctly installed");
            GUILayout.Label("Current location: " + Client.Instance.Assembly.AssemblyPath);
            GUILayout.Label("Correct location: " + Client.Instance.Assembly.AssemblyValidPath);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close"))
            {
                m_display = false;
            }
            GUILayout.EndVertical();
        }

        public void Enable()
        {
            if(!m_display)
            {
                m_display = true;
                Client.Instance.DrawEvent += Draw;
            }
        }
    }
}

