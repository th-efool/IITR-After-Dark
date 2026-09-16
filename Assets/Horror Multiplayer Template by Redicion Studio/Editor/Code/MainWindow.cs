// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEditor;
using UnityEngine;

namespace RedicionStudio.Wizard
{
    [InitializeOnLoad]
    public class AutoOpenMainWindow
    {
        static AutoOpenMainWindow()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.update += OpenMainWindow;
            }
        }

        private static void OpenMainWindow()
        {
            EditorApplication.update -= OpenMainWindow;
            MainWindow.ShowWindow();
        }
    }

    public class MainWindow : EditorWindow
    {
        private Texture2D titleImage;
        private Texture2D createItemIcon;
        private Texture2D createKillerIcon;
        private Texture2D createOutfitIcon;
        private Texture2D createPerkIcon;
        private Texture2D createMapIcon;
        private Texture2D redicionstudioIcon;

        [MenuItem("Tools/Horror Multiplayer Template by Redicion Studio/Main Window (Create Items, Killers, Outfits and Perks)")]
        public static void ShowWindow()
        {
            MainWindow window = GetWindow<MainWindow>("Main Window");
            window.minSize = new Vector2(800, 1000); // Set the initial size of the window
        }

        private void OnEnable()
        {
            titleImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/MainWindow/MainWindow.png");
            createItemIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/ItemCreation/ItemCreationIcon.png");
            createKillerIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/KillerCreation/KillerCreationIcon.png");
            createOutfitIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/OutfitCreation/OutfitCreationIcon.png");
            createPerkIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/PerkCreation/PerkCreationIcon.png");
            createMapIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/MapCreation/MapCreationIcon.png");
            redicionstudioIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/RedicionStudioLogo/RedicionStudio_Profil_Picture.png");
        }

        private void OnGUI()
        {
            Color originalBackgroundColor = GUI.backgroundColor;
            Color originalContentColor = GUI.contentColor;

            // Display the title image
            if (titleImage != null)
                DrawBanner(titleImage, 0f, 10f);

            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.8f, 0.85f, 0.9f); // Light pastel blue color
            if (EditorGUIUtility.isProSkin)
                GUI.contentColor = Color.white;   // Dark Mode
            else
                GUI.contentColor = Color.black;   // Light Mode

            /*GUILayout.Label("Main Window", EditorStyles.boldLabel);
            GUILayout.Space(10);*/

            DrawSection("Documentation",
                "See the documentation (Horror Multiplayer Template by Redicion Studio/Horror Multiplayer Template by Redicion Studio Documentation.pdf) for more information regarding the Horror Multiplayer Template by Redicion Studio.",
                "");

            DrawSection("Create New Items",
                "Use the buttons below to create new items, killers, outfits, and perks for your game.",
                "");
            GUI.contentColor = originalContentColor;
            DrawButtonWithIcon("Create Item", createItemIcon, RedicionStudio.Wizard.CreateItemWindow.ShowWindow);
            DrawButtonWithIcon("Create Killer", createKillerIcon, CreateKillerWindow.ShowWindow);
            DrawButtonWithIcon("Create Survivor", createOutfitIcon, CreateOutfitWindow.ShowWindow);
            DrawButtonWithIcon("Create Perk", createPerkIcon, CreatePerkWindow.ShowWindow);
            DrawButtonWithIcon("Create Map", createMapIcon, CreateMapWindow.ShowWindow);
            if (EditorGUIUtility.isProSkin)
                GUI.contentColor = Color.white;   // Dark Mode
            else
                GUI.contentColor = Color.black;   // Light Mode

            DrawSection("Review",
                "If you enjoy using this asset, please consider leaving a 5-star review on the asset store. We would greatly appreciate it.",
                "Write a review", "https://assetstore.unity.com/packages/templates/systems/horror-multiplayer-game-template-297297#reviews");

            DrawSection("Support",
                "If you have any questions, need help, or would like to see certain features in the next update, please contact us by e-mail: Contact@RedicionStudio.com",
                "contact@redicionstudio.com", "mailto:contact@redicionstudio.com");

            GUILayout.Space(3);
            GUI.backgroundColor = originalBackgroundColor;
            GUI.contentColor = originalContentColor;
            DrawTextWithIcon("Developed by Florian Lauka from Redicion Studio", redicionstudioIcon, RedicionStudio.Wizard.CreateItemWindow.ShowWindow);

            Rect labelRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Link);
            if (Event.current.type == EventType.MouseUp && labelRect.Contains(Event.current.mousePosition))
            {
                Application.OpenURL("https://redicionstudio.com/");
            }
        }

        private void DrawSection(string heading, string text, string linkText, string url = "")
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label(heading, EditorStyles.boldLabel);
            GUILayout.Label(text, EditorStyles.wordWrappedLabel);

            if (!string.IsNullOrEmpty(linkText))
            {
                if (GUILayout.Button(linkText, GUILayout.Height(20)))
                {
                    Application.OpenURL(url);
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(10);
        }

        private void DrawButtonWithIcon(string buttonText, Texture2D icon, System.Action onClick)
        {
            GUILayout.BeginHorizontal();
            if (icon != null)
            {
                GUILayout.Label(icon, GUILayout.Width(40), GUILayout.Height(40));
            }
            if (GUILayout.Button(buttonText, GUILayout.Height(40)))
            {
                onClick.Invoke();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawTextWithIcon(string text, Texture2D icon, System.Action onClick)
        {
            GUILayout.BeginHorizontal();
            if (icon != null)
            {
                GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
            }
            GUILayout.Label(text, EditorStyles.wordWrappedLabel);
            GUILayout.EndHorizontal();
        }

        private void DrawBanner(Texture2D tex, float horizontalPadding = 0f, float verticalPadding = 0f)
        {
            float availWidth = position.width - (horizontalPadding * 2f);

            float aspect = (float)tex.width / tex.height;
            float height = availWidth / aspect;

            Rect r = GUILayoutUtility.GetRect(availWidth, height, GUILayout.ExpandWidth(true));
            r.x += horizontalPadding;
            r.width = availWidth;
            r.y += verticalPadding;
            r.height = height;

            GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit);
        }
    }
}