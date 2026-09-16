// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEditor;
using UnityEngine;
using RedicionStudio.InventorySystem;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RedicionStudio.Wizard
{
    public class CreateOutfitWindow : EditorWindow
    {
        public string outfitName;
        public string outfitStyle;
        public int outfitID = 1;
        public GameObject previewModelPrefab;
        public Sprite outfitSprite;
        public string tooltipText;
        public ItemSO.Rarity outfitRarity;
        public int outfitPrice;
        public int outfitSellPrice;
        public bool isDefaultOutfit;
        public string outfitModelName;

        private Texture2D titleImage;
        private Texture2D redicionstudioIcon;
        private Vector2 scrollPos;

        //[MenuItem("Horror Multiplayer Template by Redicion Studio/Create Survivor Window")]
        public static void ShowWindow()
        {
            CreateOutfitWindow window = GetWindow<CreateOutfitWindow>("Create Survivor");
            window.minSize = new Vector2(400, 600); // Set the initial size of the window
        }

        private void OnEnable()
        {
            titleImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/OutfitCreation/OutfitCreation.png");
            redicionstudioIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/RedicionStudioLogo/RedicionStudio_Profil_Picture.png");
        }

        private void OnGUI()
        {
            Color originalBackgroundColor = GUI.backgroundColor;
            Color originalContentColor = GUI.contentColor;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Titelbild anzeigen
            if (titleImage != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(titleImage);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            // Set custom background and content colors
            GUI.backgroundColor = new Color(0.8f, 0.85f, 0.9f); // Light pastel blue color
            if (EditorGUIUtility.isProSkin)
                GUI.contentColor = Color.white;   // Dark Mode
            else
                GUI.contentColor = Color.black;   // Light Mode

            GUILayout.Space(10);
            GUILayout.Label("Create New Survivor", EditorStyles.boldLabel);
            RedicionStudio.Wizard.HelpBoxWithLink.ShowHelpBoxWithLink("You can find a tutorial video on creating survivors at this URL:", "https://www.youtube.com/watch?v=JLr_n8qSZOA", MessageType.Info);

            GUI.backgroundColor = Color.white;
            EditorGUILayout.BeginVertical("box");

            // Outfit Name
            EditorGUILayout.LabelField("Survivor Name");
            EditorGUILayout.HelpBox("The Survivor Name must be unique, ensuring that no other survivor use the same name. Only letters and numbers with no spaces are allowed.", MessageType.Info);
            outfitName = EditorGUILayout.TextField(outfitName);
            if (!string.IsNullOrEmpty(outfitName) && !Regex.IsMatch(outfitName, @"^[a-zA-Z0-9]+$"))
            {
                EditorGUILayout.HelpBox("Survivor Name must contain only letters and numbers with no spaces.", MessageType.Error);
            }

            GUILayout.Space(10);

            // Outfit Style
            EditorGUILayout.LabelField("Survivor Note");
            outfitStyle = EditorGUILayout.TextField(outfitStyle);

            GUILayout.Space(10);

            // Outfit ID
            EditorGUILayout.LabelField("Survivor ID");
            EditorGUILayout.HelpBox("The survivor's ID must be unique, ensuring that no other survivor use the same ID.", MessageType.Info);
            outfitID = EditorGUILayout.IntField(outfitID);

            GUILayout.Space(10);

            // Preview Model Prefab
            EditorGUILayout.LabelField("Preview Model Prefab");
            previewModelPrefab = (GameObject)EditorGUILayout.ObjectField(previewModelPrefab, typeof(GameObject), false);

            GUILayout.Space(10);

            // Outfit Sprite
            EditorGUILayout.LabelField("Survivor Sprite");
            outfitSprite = (Sprite)EditorGUILayout.ObjectField(outfitSprite, typeof(Sprite), false);

            GUILayout.Space(10);

            // Tooltip Text
            EditorGUILayout.LabelField("Survivor Description");
            tooltipText = EditorGUILayout.TextField(tooltipText);

            GUILayout.Space(10);

            // Outfit Rarity
            EditorGUILayout.LabelField("Survivor Rarity");
            outfitRarity = (ItemSO.Rarity)EditorGUILayout.EnumPopup(outfitRarity);

            GUILayout.Space(10);

            // Outfit Price
            EditorGUILayout.LabelField("Survivor Price");
            outfitPrice = EditorGUILayout.IntField(outfitPrice);

            GUILayout.Space(10);

            // Outfit Sell Price
            EditorGUILayout.LabelField("Survivor Sell Price");
            outfitSellPrice = EditorGUILayout.IntField(outfitSellPrice);

            GUILayout.Space(10);

            // Outfit Model Name
            EditorGUILayout.LabelField("Survivor Model Name");
            EditorGUILayout.HelpBox("Please enter the name of the game object that contains the mesh for the new survivor to be activated in the Player.prefab when selected.", MessageType.Info);
            outfitModelName = EditorGUILayout.TextField(outfitModelName);

            GUILayout.Space(10);

            // Is Default Outfit
            EditorGUILayout.LabelField("Is Default Survivor");
            isDefaultOutfit = EditorGUILayout.Toggle(isDefaultOutfit);

            GUILayout.Space(10);

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            if (IsValidOutfit())
            {
                if (GUILayout.Button("Create Survivor", GUILayout.Height(40)))
                {
                    CreateOutfit();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please fill in all fields before creating the survivor.", MessageType.Warning);
            }

            GUILayout.Space(10);
            GUI.backgroundColor = originalBackgroundColor;
            GUI.contentColor = originalContentColor;
            DrawTextWithIcon("Developed by Florian Lauka from Redicion Studio", redicionstudioIcon, RedicionStudio.Wizard.CreateItemWindow.ShowWindow);

            Rect labelRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Link);
            if (Event.current.type == EventType.MouseUp && labelRect.Contains(Event.current.mousePosition))
            {
                Application.OpenURL("https://redicionstudio.com/");
            }

            EditorGUILayout.EndScrollView();

            // Reset the GUI background and content colors
            GUI.backgroundColor = originalBackgroundColor;
            GUI.contentColor = originalContentColor;
        }

        private bool IsValidOutfit()
        {
            if (string.IsNullOrEmpty(outfitName) || !Regex.IsMatch(outfitName, @"^[a-zA-Z0-9]+$"))
            {
                return false;
            }

            if (previewModelPrefab == null || outfitSprite == null || string.IsNullOrEmpty(tooltipText) || outfitRarity == ItemSO.Rarity.None ||
                outfitPrice <= 0 || outfitSellPrice < 0 || string.IsNullOrEmpty(outfitStyle) || string.IsNullOrEmpty(outfitModelName))
            {
                return false;
            }

            return true;
        }

        private void CreateOutfit()
        {
            // Create Outfit ScriptableObject
            OutfitItemSO outfit = ScriptableObject.CreateInstance<OutfitItemSO>();
            outfit.uniqueName = outfitName;
            outfit.outfitStyle = outfitStyle;
            outfit.outfitID = outfitID;
            outfit.previewModelPrefab = previewModelPrefab;
            outfit.sprite = outfitSprite;
            outfit.tooltipText = tooltipText;
            outfit.rarity = outfitRarity;
            outfit.price = outfitPrice;
            outfit.sellPrice = outfitSellPrice;

            // Save Outfit ScriptableObject
            string assetPath = "Assets/Horror Multiplayer Template by Redicion Studio/InventorySystem/Resources/ItemSOs/Outfits/" + outfitName + ".asset";
            AssetDatabase.CreateAsset(outfit, assetPath);
            AssetDatabase.SaveAssets();

            // Create and save model prefab
            GameObject model = new GameObject(outfitName + "_Model");
            string prefabPath = "Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/" + outfitName + "_Model.prefab";
            GameObject modelPrefab = PrefabUtility.SaveAsPrefabAsset(model, prefabPath);
            outfit.modelPrefab = modelPrefab;
            DestroyImmediate(model);

            // Update Outfit ScriptableObject with model prefab
            EditorUtility.SetDirty(outfit);
            AssetDatabase.SaveAssets();

            // Add outfit to OutfitManager
            AddOutfitToOutfitManager(outfit);

            Debug.Log("Outfit created successfully at " + assetPath + " with model prefab at " + prefabPath);
        }

        private void AddOutfitToOutfitManager(OutfitItemSO outfit)
        {
            // Load the Player prefab
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/Player.prefab");
            if (playerPrefab != null)
            {
                OutfitManager outfitManager = playerPrefab.GetComponent<OutfitManager>();
                if (outfitManager != null)
                {
                    List<ItemSO> outfitItemList = new List<ItemSO>(outfitManager.outfitItemSOs);
                    outfitItemList.Add(outfit);
                    outfitManager.outfitItemSOs = outfitItemList.ToArray();

                    if (isDefaultOutfit)
                    {
                        outfitManager.defaultOutfitSO = outfit;
                        outfitManager.defaultOutfitId = outfit.outfitID;
                    }

                    // Find the outfit model by name
                    Transform outfitModelTransform = FindChildByName(playerPrefab.transform, outfitModelName);
                    if (outfitModelTransform != null)
                    {
                        OutfitItem newOutfitItem = new OutfitItem
                        {
                            name = outfitName,
                            outfitID = outfitID,
                            outfitModel = outfitModelTransform.gameObject,
                            outfitImage = outfitSprite
                        };

                        List<OutfitItem> outfitsList = new List<OutfitItem>(outfitManager.outfits);
                        outfitsList.Add(newOutfitItem);
                        outfitManager.outfits = outfitsList.ToArray();
                    }
                    else
                    {
                        Debug.LogError("Outfit model with the specified name not found in the Player prefab.");
                    }

                    // Mark the prefab as dirty to save changes
                    EditorUtility.SetDirty(outfitManager);
                    AssetDatabase.SaveAssets();

                    Debug.Log("Outfit added to OutfitManager successfully.");
                }
                else
                {
                    Debug.LogError("OutfitManager component not found on the Player prefab.");
                }
            }
            else
            {
                Debug.LogError("Player prefab not found at the specified path.");
            }
        }

        private Transform FindChildByName(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                var result = FindChildByName(child, name);
                if (result != null)
                    return result;
            }
            return null;
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
    }
}
