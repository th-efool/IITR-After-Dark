// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using RedicionStudio.InventorySystem;
using System.Text.RegularExpressions;

namespace RedicionStudio.Wizard
{
    public class CreatePerkWindow : EditorWindow
    {
        public string perkName;
        public Sprite perkSprite;
        public string tooltipText;
        public ItemSO.Rarity perkRarity;
        public int perkPrice;
        public int perkSellPrice;
        public List<PerkSO.PerkEffect> perkEffects = new List<PerkSO.PerkEffect>();
        public string perkNote;

        private bool showEffects = true;
        private Texture2D titleImage;
        private Texture2D redicionstudioIcon;
        private Vector2 scrollPos;

        //[MenuItem("Horror Multiplayer Template by Redicion Studio/Create Perk Window")]
        public static void ShowWindow()
        {
            CreatePerkWindow window = GetWindow<CreatePerkWindow>("Create Perk");
            window.minSize = new Vector2(400, 600); // Set the initial size of the window
        }

        private void OnEnable()
        {
            titleImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/PerkCreation/PerkCreation.png");
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
            GUILayout.Label("Create New Perk", EditorStyles.boldLabel);
            RedicionStudio.Wizard.HelpBoxWithLink.ShowHelpBoxWithLink("You can find a tutorial video on creating perks at this URL:", "https://www.youtube.com/watch?v=dHFsenwn0_E", MessageType.Info);

            GUI.backgroundColor = Color.white;
            EditorGUILayout.BeginVertical("box");

            // Perk Name
            EditorGUILayout.LabelField("Perk Name");
            perkName = EditorGUILayout.TextField(perkName);
            if (!string.IsNullOrEmpty(perkName) && !Regex.IsMatch(perkName, @"^[a-zA-Z0-9]+$"))
            {
                EditorGUILayout.HelpBox("Perk Name must contain only letters and numbers with no spaces.", MessageType.Error);
            }

            GUILayout.Space(10);

            // Perk Sprite
            EditorGUILayout.LabelField("Perk Sprite");
            perkSprite = (Sprite)EditorGUILayout.ObjectField(perkSprite, typeof(Sprite), false);

            GUILayout.Space(10);

            // Tooltip Text
            EditorGUILayout.LabelField("Tooltip Text");
            tooltipText = EditorGUILayout.TextArea(tooltipText);

            GUILayout.Space(10);

            // Perk Rarity
            EditorGUILayout.LabelField("Perk Rarity");
            perkRarity = (ItemSO.Rarity)EditorGUILayout.EnumPopup(perkRarity);

            GUILayout.Space(10);

            // Perk Price
            EditorGUILayout.LabelField("Perk Price");
            perkPrice = EditorGUILayout.IntField(perkPrice);

            GUILayout.Space(10);

            // Perk Sell Price
            EditorGUILayout.LabelField("Perk Sell Price");
            perkSellPrice = EditorGUILayout.IntField(perkSellPrice);

            GUILayout.Space(10);

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            // Perk Effects
            showEffects = EditorGUILayout.Foldout(showEffects, "Perk Effects");
            if (showEffects)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUI.indentLevel++;
                if (GUILayout.Button("Add Effect"))
                {
                    perkEffects.Add(new PerkSO.PerkEffect());
                }

                for (int i = 0; i < perkEffects.Count; i++)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Effect {i + 1}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Remove"))
                    {
                        perkEffects.RemoveAt(i);
                        EditorGUILayout.EndHorizontal(); // Close the layout group before modifying the list
                        EditorGUILayout.EndVertical();
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    perkEffects[i].name = EditorGUILayout.TextField("Name", perkEffects[i].name);
                    perkEffects[i].type = (PerkEffectType)EditorGUILayout.EnumPopup("Type", perkEffects[i].type);
                    perkEffects[i].status = (PerkStatus)EditorGUILayout.EnumPopup("Status", perkEffects[i].status);
                    perkEffects[i].value = EditorGUILayout.FloatField("Value", perkEffects[i].value);

                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(10);

            // Perk Note
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Perk Note");
            perkNote = EditorGUILayout.TextField(perkNote);
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            if (IsValidPerk())
            {
                if (GUILayout.Button("Create Perk", GUILayout.Height(40)))
                {
                    CreatePerk();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please fill in all fields and add at least one perk effect before creating the perk.", MessageType.Warning);
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

        private bool IsValidPerk()
        {
            if (string.IsNullOrEmpty(perkName) || !Regex.IsMatch(perkName, @"^[a-zA-Z0-9]+$"))
            {
                return false;
            }

            if (perkSprite == null || string.IsNullOrEmpty(tooltipText) || perkRarity == ItemSO.Rarity.None ||
                perkPrice <= 0 || perkSellPrice < 0 || perkEffects.Count == 0)
            {
                return false;
            }

            foreach (var effect in perkEffects)
            {
                if (string.IsNullOrEmpty(effect.name) || effect.value <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void CreatePerk()
        {
            // Create Perk ScriptableObject
            PerkSO perk = ScriptableObject.CreateInstance<PerkSO>();
            perk.uniqueName = perkName;
            perk.sprite = perkSprite;
            perk.tooltipText = tooltipText;
            perk.rarity = perkRarity;
            perk.price = perkPrice;
            perk.sellPrice = perkSellPrice;
            perk.perkEffects = perkEffects;
            perk.perkNote = perkNote;

            // Set Item Type to Perk
            perk.itemType = ItemSO.ItemType.Perk;

            // Save Perk ScriptableObject
            string assetPath = "Assets/Horror Multiplayer Template by Redicion Studio/InventorySystem/Resources/ItemSOs/Perks/" + perkName + ".asset";
            AssetDatabase.CreateAsset(perk, assetPath);
            AssetDatabase.SaveAssets();

            // Create and save model prefab
            GameObject model = new GameObject(perkName + "_Model");
            string prefabPath = "Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/" + perkName + "_Model.prefab";
            GameObject modelPrefab = PrefabUtility.SaveAsPrefabAsset(model, prefabPath);
            perk.modelPrefab = modelPrefab;
            DestroyImmediate(model);

            // Update Perk ScriptableObject with model prefab
            EditorUtility.SetDirty(perk);
            AssetDatabase.SaveAssets();

            // Add perk to ItemShop
            AddPerkToItemShop(perk);

            Debug.Log("Perk created successfully at " + assetPath + " with model prefab at " + prefabPath);
        }

        private void AddPerkToItemShop(ItemSO perk)
        {
            // Load the ItemShop prefab
            GameObject itemShopPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/ItemShop.prefab");
            if (itemShopPrefab != null)
            {
                ItemShop itemShop = itemShopPrefab.GetComponent<ItemShop>();
                if (itemShop != null)
                {
                    List<ItemSO> itemsList = new List<ItemSO>(itemShop.items);
                    itemsList.Add(perk);
                    itemShop.items = itemsList.ToArray();

                    // Mark the prefab as dirty to save changes
                    EditorUtility.SetDirty(itemShop);
                    AssetDatabase.SaveAssets();

                    Debug.Log("Perk added to ItemShop successfully.");
                }
                else
                {
                    Debug.LogError("ItemShop component not found on the ItemShop prefab.");
                }
            }
            else
            {
                Debug.LogError("ItemShop prefab not found at the specified path.");
            }
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