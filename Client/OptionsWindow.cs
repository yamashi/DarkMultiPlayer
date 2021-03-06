// using System;
// using UnityEngine;
// 
// namespace DarkMultiPlayer
// {
//     public class OptionsWindow
//     {
//         public bool loadEventHandled = true;
//         public bool display;
//         private bool isWindowLocked = false;
//         private bool safeDisplay;
//         private bool initialized;
//         //GUI Layout
//         private Rect windowRect;
//         private Rect moveRect;
//         private GUILayoutOption[] layoutOptions;
//         //Styles
//         private GUIStyle windowStyle;
//         private GUIStyle buttonStyle;
//         //const
//         private const float WINDOW_HEIGHT = 400;
//         private const float WINDOW_WIDTH = 300;
//         //TempColour
//         private Color tempColor = new Color(1f, 1f, 1f, 1f);
//         private GUIStyle tempColorLabelStyle;
//         //Cache size
//         private string newCacheSize = "";
//         //Keybindings
//         private bool settingChat;
//         private bool settingScreenshot;
// 
//         private UniverseConverterWindow m_universeConverterWindow;
// 
//         public OptionsWindow()
//         {
//             m_universeConverterWindow = new UniverseConverterWindow();
// 
//             Client.Instance.UpdateEvent += this.Update;
//             Client.Instance.DrawEvent += this.Draw;
//         }
// 
//         private void InitGUI()
//         {
//             //Setup GUI stuff
//             windowRect = new Rect(Screen.width / 2f - WINDOW_WIDTH / 2f, Screen.height / 2f - WINDOW_HEIGHT / 2f, WINDOW_WIDTH, WINDOW_HEIGHT);
//             moveRect = new Rect(0, 0, 10000, 20);
// 
//             windowStyle = new GUIStyle(GUI.skin.window);
//             buttonStyle = new GUIStyle(GUI.skin.button);
// 
//             layoutOptions = new GUILayoutOption[4];
//             layoutOptions[0] = GUILayout.Width(WINDOW_WIDTH);
//             layoutOptions[1] = GUILayout.Height(WINDOW_HEIGHT);
//             layoutOptions[2] = GUILayout.ExpandWidth(true);
//             layoutOptions[3] = GUILayout.ExpandHeight(true);
// 
//             tempColor = new Color();
//             tempColorLabelStyle = new GUIStyle(GUI.skin.label);
//         }
// 
//         private void Update()
//         {
//             safeDisplay = display;
//         }
// 
//         private void Draw()
//         {
//             if (!initialized)
//             {
//                 initialized = true;
//                 InitGUI();
//             }
//             if (safeDisplay)
//             {
//                 windowRect = GUILayout.Window(6711 + Client.WINDOW_OFFSET, windowRect, DrawContent, "DarkMultiPlayer - Options", windowStyle, layoutOptions);
//             }
//             CheckWindowLock();
//         }
// 
//         private void DrawContent(int windowID)
//         {
//             if (!loadEventHandled)
//             {
//                 loadEventHandled = true;
//                 tempColor = Settings.Instance.playerColor;
//                 newCacheSize = Settings.Instance.cacheSize.ToString();
//             }
//             //Player color
//             GUILayout.BeginVertical();
//             GUI.DragWindow(moveRect);
//             GUILayout.BeginHorizontal();
//             GUILayout.Label("Player name color: ");
//             GUILayout.Label(Settings.Instance.playerName, tempColorLabelStyle);
//             GUILayout.EndHorizontal();
//             GUILayout.BeginHorizontal();
//             GUILayout.Label("R: ");
//             tempColor.r = GUILayout.HorizontalScrollbar(tempColor.r, 0, 0, 1);
//             GUILayout.EndHorizontal();
//             GUILayout.BeginHorizontal();
//             GUILayout.Label("G: ");
//             tempColor.g = GUILayout.HorizontalScrollbar(tempColor.g, 0, 0, 1);
//             GUILayout.EndHorizontal();
//             GUILayout.BeginHorizontal();
//             GUILayout.Label("B: ");
//             tempColor.b = GUILayout.HorizontalScrollbar(tempColor.b, 0, 0, 1);
//             GUILayout.EndHorizontal();
//             tempColorLabelStyle.active.textColor = tempColor;
//             tempColorLabelStyle.normal.textColor = tempColor;
//             GUILayout.BeginHorizontal();
//             if (GUILayout.Button("Random", buttonStyle))
//             {
//                 tempColor = PlayerColorWorker.GenerateRandomColor();
//             }
//             if (GUILayout.Button("Set", buttonStyle))
//             {
//                 PlayerStatusWindow.Instance.colorEventHandled = false;
//                 Settings.Instance.playerColor = tempColor;
//                 Settings.Instance.SaveSettings();
//                 if (GameClient.Instance.state == DarkMultiPlayerCommon.ClientState.RUNNING)
//                 {
//                     PlayerColorWorker.Instance.SendPlayerColorToServer();
//                 }
//             }
//             GUILayout.EndHorizontal();
//             GUILayout.Space(10);
//             //Cache
//             GUILayout.Label("Cache size");
//             GUILayout.Label("Current size: " + Math.Round((UniverseSyncCache.Instance.currentCacheSize / (float)(1024 * 1024)), 3) + "MB.");
//             GUILayout.Label("Max size: " + Settings.Instance.cacheSize + "MB.");
//             newCacheSize = GUILayout.TextArea(newCacheSize);
//             GUILayout.BeginHorizontal();
//             if (GUILayout.Button("Set", buttonStyle))
//             {
//                 int tempCacheSize;
//                 if (Int32.TryParse(newCacheSize, out tempCacheSize))
//                 {
//                     if (tempCacheSize < 1)
//                     {
//                         tempCacheSize = 1;
//                         newCacheSize = tempCacheSize.ToString();
//                     }
//                     if (tempCacheSize > 1000)
//                     {
//                         tempCacheSize = 1000;
//                         newCacheSize = tempCacheSize.ToString();
//                     }
//                     Settings.Instance.cacheSize = tempCacheSize;
//                     Settings.Instance.SaveSettings();
//                 }
//                 else
//                 {
//                     newCacheSize = Settings.Instance.cacheSize.ToString();
//                 }
//             }
//             if (GUILayout.Button("Expire cache"))
//             {
//                 UniverseSyncCache.Instance.ExpireCache();
//             }
//             if (GUILayout.Button("Delete cache"))
//             {
//                 UniverseSyncCache.Instance.DeleteCache();
//             }
//             GUILayout.EndHorizontal();
//             //Key bindings
//             GUILayout.Space(10);
//             string chatDescription = "Set chat key (current: " + Settings.Instance.chatKey + ")";
//             if (settingChat)
//             {
//                 chatDescription = "Setting chat key (click to cancel)...";
//                 if (Event.current.isKey)
//                 {
//                     if (Event.current.keyCode != KeyCode.Escape)
//                     {
//                         Settings.Instance.chatKey = Event.current.keyCode;
//                         Settings.Instance.SaveSettings();
//                         settingChat = false;
//                     }
//                     else
//                     {
//                         settingChat = false;
//                     }
//                 }
//             }
//             if (GUILayout.Button(chatDescription))
//             {
//                 settingChat = !settingChat;
//             }
//             string screenshotDescription = "Set screenshot key (current: " + Settings.Instance.screenshotKey.ToString() + ")";
//             if (settingScreenshot)
//             {
//                 screenshotDescription = "Setting screenshot key (click to cancel)...";
//                 if (Event.current.isKey)
//                 {
//                     if (Event.current.keyCode != KeyCode.Escape)
//                     {
//                         Settings.Instance.screenshotKey = Event.current.keyCode;
//                         Settings.Instance.SaveSettings();
//                         settingScreenshot = false;
//                     }
//                     else
//                     {
//                         settingScreenshot = false;
//                     }
//                 }
//             }
//             if (GUILayout.Button(screenshotDescription))
//             {
//                 settingScreenshot = !settingScreenshot;
//             }
//             GUILayout.Space(10);
//             GUILayout.Label("Generate a server DMPModControl:");
//             if (GUILayout.Button("Generate blacklist DMPModControl.txt"))
//             {
//                 ModWorker.Instance.GenerateModControlFile(false);
//             }
//             if (GUILayout.Button("Generate whitelist DMPModControl.txt"))
//             {
//                 ModWorker.Instance.GenerateModControlFile(true);
//             }
// 
//             m_universeConverterWindow.Display = GUILayout.Toggle(m_universeConverterWindow.Display, "Generate Universe from saved game", buttonStyle);
// 
//             if (GUILayout.Button("Reset disclaimer"))
//             {
//                 Client.Instance.Settings.DisclaimerAccepted = 0;
//                 Settings.Instance.SaveSettings();
//             }
//             GUILayout.FlexibleSpace();
//             if (GUILayout.Button("Close", buttonStyle))
//             {
//                 display = false;
//             }
//             GUILayout.EndVertical();
//         }
// 
//         private void CheckWindowLock()
//         {
//             if (!Client.Instance.gameRunning)
//             {
//                 RemoveWindowLock();
//                 return;
//             }
// 
//             if (HighLogic.LoadedSceneIsFlight)
//             {
//                 RemoveWindowLock();
//                 return;
//             }
// 
//             if (safeDisplay)
//             {
//                 Vector2 mousePos = Input.mousePosition;
//                 mousePos.y = Screen.height - mousePos.y;
// 
//                 bool shouldLock = windowRect.Contains(mousePos);
// 
//                 if (shouldLock && !isWindowLocked)
//                 {
//                     InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "DMP_OptionsLock");
//                     isWindowLocked = true;
//                 }
//                 if (!shouldLock && isWindowLocked)
//                 {
//                     RemoveWindowLock();
//                 }
//             }
// 
//             if (!safeDisplay && isWindowLocked)
//             {
//                 RemoveWindowLock();
//             }
//         }
// 
//         private void RemoveWindowLock()
//         {
//             if (isWindowLocked)
//             {
//                 isWindowLocked = false;
//                 InputLockManager.RemoveControlLock("DMP_OptionsLock");
//             }
//         }
//     }
// }
// 
