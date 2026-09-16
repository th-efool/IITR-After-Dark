using UnityEditor;
using UnityEngine;

namespace RedicionStudio.Wizard
{
    public class ControllerSupportWindow : EditorWindow
    {
        private static readonly string PlayerPrefKey = "ControllerSupportWindowShown";

        private Texture2D controllerImage;
        private Vector2 scrollPos;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefKey))
            {
                EditorApplication.update += ShowOnProjectOpen;
            }
        }

        private static void ShowOnProjectOpen()
        {
            EditorApplication.update -= ShowOnProjectOpen;
            ShowWindow();
        }

        public static void ShowWindow()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefKey))
            {
                ControllerSupportWindow window = GetWindow<ControllerSupportWindow>("Controller Support");
                window.minSize = new Vector2(1000, 600);
                window.maxSize = new Vector2(1000, 600);
            }
        }

        private void OnEnable()
        {
            controllerImage = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/ControllerSupportWindow/ControllerButtons.png"
            );
        }

        private void OnGUI()
        {
            Color originalBackgroundColor = GUI.backgroundColor;
            Color originalContentColor = GUI.contentColor;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Space(10);
            GUILayout.Label("Controller Support", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(0.8f, 0.85f, 0.9f);
            if (EditorGUIUtility.isProSkin)
                GUI.contentColor = Color.white;   // Dark Mode
            else
                GUI.contentColor = Color.black;   // Light Mode

            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Version 1.02 introduces comprehensive controller support, enabling seamless navigation and interaction using any compatible game controller.",
                MessageType.Info
            );

            GUI.backgroundColor = originalBackgroundColor;
            GUI.contentColor = originalContentColor;

            if (controllerImage != null)
            {
                float aspectRatio = (float)controllerImage.width / controllerImage.height;
                float maxHeight = 500;
                float adjustedWidth = maxHeight * aspectRatio;

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(controllerImage, GUILayout.Width(adjustedWidth), GUILayout.Height(maxHeight));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUI.backgroundColor = new Color(0.8f, 0.85f, 0.9f);
            if (EditorGUIUtility.isProSkin)
                GUI.contentColor = Color.white;   // Dark Mode
            else
                GUI.contentColor = Color.black;   // Light Modek;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("OK", GUILayout.Width(960)))
            {
                PlayerPrefs.SetInt(PlayerPrefKey, 1);
                PlayerPrefs.Save();
                Close();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.EndScrollView();
        }
    }
}