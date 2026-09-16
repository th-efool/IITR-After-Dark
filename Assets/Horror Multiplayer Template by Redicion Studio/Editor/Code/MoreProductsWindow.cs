// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEditor;
using UnityEngine;

namespace RedicionStudio.Wizard
{
    public class MoreProductsWindow : EditorWindow
    {
        private Texture2D product1Image;
        private Texture2D product2Image;
        private Vector2 scrollPos;

        private string product1Description = @"This complete multiplayer game template and its included important features are designed to create an advanced multiplayer video game.

Key Features:
- Master Server, Game Server and Sub Server System
- Strong and Secure Player Data Save System
- Player Authentication (Registration and Login) System
- Vehicles: theft, enter/exit, damage, lights, weapons, AI/Traffic system
- Items and Inventory: weapons, ammunition, consumables, outfits, companions
- Weapons and Ammunition: customizable weapons, damage, explosive weapons
- Outfits/Skins: change character appearance
- Companions/Pets: follow players
- NPCs: intelligent behaviors, vehicle interactions
- Item Shop: purchase items
- Build System and Plot/Property System: own and build on properties
- Player Health and Nutrition: health and nutrition bars";

        private string product2Description = @"The Pirate Multiplayer Game Template (MMO) is a complete AAA Unity framework featuring realistic sailing, naval combat, treasure hunting,
character and ship customization, player-owned islands, and a full MMO backend. Create your own pirate adventure with all the systems you need!";

        [MenuItem("Tools/Horror Multiplayer Template by Redicion Studio/More Products")]
        public static void ShowWindow()
        {
            MoreProductsWindow window = GetWindow<MoreProductsWindow>("More Products");
            window.minSize = new Vector2(900, 1000); // Set the initial size of the window
        }

        private void OnEnable()
        {
            // Load product images
            product1Image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/MoreProducts/AdvancedMultiplayerGameTemplate.png");
            product2Image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/MoreProducts/PirateMultiplayerGameTemplate.png");
        }

        private void OnGUI()
        {
            Color originalBackgroundColor = GUI.backgroundColor;
            Color originalContentColor = GUI.contentColor;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Space(20);

            GUI.backgroundColor = new Color(0.8f, 0.85f, 0.9f); // Light pastel blue color
            if (EditorGUIUtility.isProSkin)
                GUI.contentColor = Color.white;   // Dark Mode
            else
                GUI.contentColor = Color.black;   // Light Mode

            // Header
            DrawSection("Other Products",
        "Here you can see other products from us that are available on the official Unity Asset Store.",
        "");

            GUILayout.Space(20);

            // Product 1
            GUILayout.BeginVertical("box");
            GUILayout.Label("Advanced Multiplayer Game Template", EditorStyles.boldLabel);
            if (product1Image != null)
            {
                GUI.contentColor = originalContentColor;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(product1Image, GUILayout.Width(750), GUILayout.Height(600));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (EditorGUIUtility.isProSkin)
                    GUI.contentColor = Color.white;   // Dark Mode
                else
                    GUI.contentColor = Color.black;   // Light Mode
            }

            GUILayout.Label(product1Description);
            if (GUILayout.Button("View the Advanced Multiplayer Game Template on Asset Store", GUILayout.Height(40)))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/templates/systems/advanced-multiplayer-game-template-241288");
            }
            GUILayout.EndVertical();

            GUILayout.Space(20);

            // Product 2
            GUILayout.BeginVertical("box");
            GUILayout.Label("Pirate Multiplayer Game Template (MMO)", EditorStyles.boldLabel);
            if (product2Image != null)
            {
                GUI.contentColor = originalContentColor;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(product2Image, GUILayout.Width(750), GUILayout.Height(600));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (EditorGUIUtility.isProSkin)
                    GUI.contentColor = Color.white;   // Dark Mode
                else
                    GUI.contentColor = Color.black;   // Light Mode
            }

            GUILayout.Label(product2Description);
            if (GUILayout.Button("View the Pirate Multiplayer Game Template (MMO) on Asset Store", GUILayout.Height(40)))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/templates/systems/pirate-multiplayer-game-template-mmo-308715");
            }
            GUILayout.EndVertical();

            GUILayout.Space(20);

            GUI.backgroundColor = originalBackgroundColor;
            GUI.contentColor = originalContentColor;

            EditorGUILayout.EndScrollView();
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
    }
}