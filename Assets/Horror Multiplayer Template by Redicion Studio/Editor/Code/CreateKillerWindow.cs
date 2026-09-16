// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEditor;
using UnityEngine;
using RedicionStudio.InventorySystem;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static RedicionStudio.HunterAbilities;

namespace RedicionStudio.Wizard
{
    public class CreateKillerWindow : EditorWindow
    {
        public string killerName;
        public int killerID = 1;
        public GameObject previewModelPrefab;
        public Sprite killerSprite;
        public string tooltipText;
        public ItemSO.Rarity killerRarity;
        public int killerPrice;
        public int killerSellPrice;
        public bool isDefaultKiller;

        // Additional variables
        public string killerModelName;
        public Avatar killerAvatar;
        public float MoveSpeed = 1f;
        public float SprintSpeed = 4.6f;
        public float MovementMultiplier = 1.9f;
        public bool canSprint = true;
        public AudioClip killerChaseMusic;
        public List<HunterSpecialAttack> killerSpecialAttacks = new List<HunterSpecialAttack>();
        public GameObject killerWeaponItem;
        public string itemParentName = "ItemParent";
        public AnimatorOverrideController killerAnimator;

        private Texture2D titleImage;
        private Texture2D redicionstudioIcon;
        private Vector2 scrollPos;

        private static readonly string[] SpecialAttackSpritePaths = {
        "Assets/Horror Multiplayer Template by Redicion Studio/Textures/UI/Special_Attack_Blackout_Strike.png",
        "Assets/Horror Multiplayer Template by Redicion Studio/Textures/UI/Special_Attack_Finisher.png",
        "Assets/Horror Multiplayer Template by Redicion Studio/Textures/UI/Special_Attack_Hunters_Instinct.png",
        "Assets/Horror Multiplayer Template by Redicion Studio/Textures/UI/Special_Attack_Hunters_Vision.png",
        "Assets/Horror Multiplayer Template by Redicion Studio/Textures/UI/Special_Attack_Rapid_Rush.png"
    };

        //[MenuItem("Horror Multiplayer Template by Redicion Studio/Create Killer Window")]
        public static void ShowWindow()
        {
            CreateKillerWindow window = GetWindow<CreateKillerWindow>("Create Killer");
            window.minSize = new Vector2(400, 600); // Set the initial size of the window
        }

        private void OnEnable()
        {
            titleImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/KillerCreation/KillerCreation.png");
            redicionstudioIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/RedicionStudioLogo/RedicionStudio_Profil_Picture.png");
            killerChaseMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Horror Multiplayer Template by Redicion Studio/Audio/Music/Chase/Hunter01/Hunter01Chase02Loop.wav");

            // Initialize default values for all special attacks
            InitializeDefaultSpecialAttacks();
        }

        private void InitializeDefaultSpecialAttacks()
        {
            killerSpecialAttacks = new List<HunterSpecialAttack>
        {
            new HunterSpecialAttack
            {
                specialAttackType = HunterSpecialAttack.SpecialAttackType.BlackoutStrike,
                specialAttackImage = AssetDatabase.LoadAssetAtPath<Sprite>(SpecialAttackSpritePaths[(int)HunterSpecialAttack.SpecialAttackType.BlackoutStrike]),
                Cooldown = 60f,
                BlackoutStrikeDuration = 24.65f,
                BlackoutStrikeAudioClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Horror Multiplayer Template by Redicion Studio/Audio/StreetLight/StreetLightBreakSound.wav")
            },
            new HunterSpecialAttack
            {
                specialAttackType = HunterSpecialAttack.SpecialAttackType.Finisher,
                specialAttackImage = AssetDatabase.LoadAssetAtPath<Sprite>(SpecialAttackSpritePaths[(int)HunterSpecialAttack.SpecialAttackType.Finisher]),
                Cooldown = 60f,
                FinisherDuration = 3.24f,
                FightDuration = 7f,
                VictimFightAnimatorTriggerName = "Kill01SurvivorFight",
                HunterFightAnimatorTriggerName = "Kill01HunterFight",
                HunterKillAnimatorTriggerName = "Kill01HunterKill",
                VictimDeathAnimatorTriggerName = "Kill01SurvivorDeath",
                HunterDisabledAnimatorTriggerName = "Kill01HunterFailed",
                VictimEscapedAnimatorTriggerName = "Kill01SurvivorEscaped",
                HunterDisabledDuration = 5.16f,
                VictimEscapedDuration = 2.02f,
                VictimDeathDuration = 2.03f
            },
            new HunterSpecialAttack
            {
                specialAttackType = HunterSpecialAttack.SpecialAttackType.HuntersInstinct,
                specialAttackImage = AssetDatabase.LoadAssetAtPath<Sprite>(SpecialAttackSpritePaths[(int)HunterSpecialAttack.SpecialAttackType.HuntersInstinct]),
                Cooldown = 60f,
                HuntersInstinctDuration = 12f
            },
            new HunterSpecialAttack
            {
                specialAttackType = HunterSpecialAttack.SpecialAttackType.HuntersVision,
                specialAttackImage = AssetDatabase.LoadAssetAtPath<Sprite>(SpecialAttackSpritePaths[(int)HunterSpecialAttack.SpecialAttackType.HuntersVision]),
                Cooldown = 60f,
                HuntersVisionDuration = 12f,
                HuntersVisionAudioClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Horror Multiplayer Template by Redicion Studio/Audio/Vision/HuntersVisionSound.wav")
            },
            new HunterSpecialAttack
            {
                specialAttackType = HunterSpecialAttack.SpecialAttackType.RapidRush,
                specialAttackImage = AssetDatabase.LoadAssetAtPath<Sprite>(SpecialAttackSpritePaths[(int)HunterSpecialAttack.SpecialAttackType.RapidRush]),
                Cooldown = 60f,
                RapidRushDuration = 5f,
                RapidRushSprintSpeed = 6.1f
            }
        };
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
            GUILayout.Label("Create New Killer", EditorStyles.boldLabel);
            RedicionStudio.Wizard.HelpBoxWithLink.ShowHelpBoxWithLink("You can find a tutorial video on creating killers at this URL:", "https://www.youtube.com/watch?v=1uWE2wBnUHM", MessageType.Info);

            GUI.backgroundColor = Color.white;
            EditorGUILayout.BeginVertical("box");

            // Killer Name
            EditorGUILayout.LabelField("Killer Name");
            EditorGUILayout.HelpBox("The Killer Name must be unique, ensuring that no other killer use the same name. Only letters and numbers with no spaces are allowed.", MessageType.Info);
            killerName = EditorGUILayout.TextField(killerName);
            if (!string.IsNullOrEmpty(killerName) && !Regex.IsMatch(killerName, @"^[a-zA-Z0-9]+$"))
            {
                EditorGUILayout.HelpBox("Killer Name must contain only letters and numbers with no spaces.", MessageType.Error);
            }

            GUILayout.Space(10);

            // Killer ID
            EditorGUILayout.LabelField("Killer ID");
            EditorGUILayout.HelpBox("The killer's ID must be unique, ensuring that no other killer use the same ID.", MessageType.Info);
            killerID = EditorGUILayout.IntField(killerID);

            GUILayout.Space(10);

            // Preview Model Prefab
            EditorGUILayout.LabelField("Preview Model Prefab");
            previewModelPrefab = (GameObject)EditorGUILayout.ObjectField(previewModelPrefab, typeof(GameObject), false);

            GUILayout.Space(10);

            // Killer Sprite
            EditorGUILayout.LabelField("Killer Sprite");
            killerSprite = (Sprite)EditorGUILayout.ObjectField(killerSprite, typeof(Sprite), false);

            GUILayout.Space(10);

            // Tooltip Text
            EditorGUILayout.LabelField("Tooltip Text");
            tooltipText = EditorGUILayout.TextField(tooltipText);

            GUILayout.Space(10);

            // Killer Rarity
            EditorGUILayout.LabelField("Killer Rarity");
            killerRarity = (ItemSO.Rarity)EditorGUILayout.EnumPopup(killerRarity);

            GUILayout.Space(10);

            // Killer Price
            EditorGUILayout.LabelField("Killer Price");
            killerPrice = EditorGUILayout.IntField(killerPrice);

            GUILayout.Space(10);

            // Killer Sell Price
            EditorGUILayout.LabelField("Killer Sell Price");
            killerSellPrice = EditorGUILayout.IntField(killerSellPrice);

            GUILayout.Space(10);

            // Additional Fields
            EditorGUILayout.LabelField("Killer Model Name");
            EditorGUILayout.HelpBox("Please enter the name of the game object that contains the mesh for the new killer to be activated in the Player.prefab when selected.", MessageType.Info);
            killerModelName = EditorGUILayout.TextField(killerModelName);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Killer Animator");
            killerAnimator = (AnimatorOverrideController)EditorGUILayout.ObjectField(killerAnimator, typeof(AnimatorOverrideController), false);
            if (killerAnimator == null)
                killerAnimator = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>("Assets/Horror Multiplayer Template by Redicion Studio/Animation/PlayerAnimator_Hunter01.overrideController");

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Killer Avatar");
            killerAvatar = (Avatar)EditorGUILayout.ObjectField(killerAvatar, typeof(Avatar), false);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Move Speed");
            MoveSpeed = EditorGUILayout.FloatField(MoveSpeed);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Sprint Speed");
            SprintSpeed = EditorGUILayout.FloatField(SprintSpeed);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Movement Multiplier");
            MovementMultiplier = EditorGUILayout.FloatField(MovementMultiplier);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Can Sprint");
            canSprint = EditorGUILayout.Toggle(canSprint);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Killer Chase Music");
            killerChaseMusic = (AudioClip)EditorGUILayout.ObjectField(killerChaseMusic, typeof(AudioClip), false);

            GUILayout.Space(10);

            // Killer Special Attacks Info
            EditorGUILayout.HelpBox("Please select exactly three special attacks for the killer.", MessageType.Info);

            GUI.contentColor = originalContentColor;

            // Killer Special Attacks
            EditorGUILayout.LabelField("Killer Special Attacks");
            for (int i = 0; i < killerSpecialAttacks.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Special Attack " + (i + 1));
                killerSpecialAttacks[i].specialAttackType = (HunterSpecialAttack.SpecialAttackType)EditorGUILayout.EnumPopup("Type", killerSpecialAttacks[i].specialAttackType);
                killerSpecialAttacks[i].specialAttackImage = (Sprite)EditorGUILayout.ObjectField("Image", killerSpecialAttacks[i].specialAttackImage, typeof(Sprite), false);
                killerSpecialAttacks[i].Cooldown = EditorGUILayout.FloatField("Cooldown", killerSpecialAttacks[i].Cooldown);

                // Specific fields for each special attack type
                switch (killerSpecialAttacks[i].specialAttackType)
                {
                    case HunterSpecialAttack.SpecialAttackType.BlackoutStrike:
                        killerSpecialAttacks[i].BlackoutStrikeDuration = EditorGUILayout.FloatField("Duration", killerSpecialAttacks[i].BlackoutStrikeDuration);
                        killerSpecialAttacks[i].BlackoutStrikeAudioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", killerSpecialAttacks[i].BlackoutStrikeAudioClip, typeof(AudioClip), false);
                        break;
                    case HunterSpecialAttack.SpecialAttackType.Finisher:
                        killerSpecialAttacks[i].FinisherDuration = EditorGUILayout.FloatField("Finisher Duration", killerSpecialAttacks[i].FinisherDuration);
                        killerSpecialAttacks[i].FightDuration = EditorGUILayout.FloatField("Fight Duration", killerSpecialAttacks[i].FightDuration);
                        killerSpecialAttacks[i].VictimFightAnimatorTriggerName = EditorGUILayout.TextField("Victim Fight Animator Trigger", killerSpecialAttacks[i].VictimFightAnimatorTriggerName);
                        killerSpecialAttacks[i].HunterFightAnimatorTriggerName = EditorGUILayout.TextField("Hunter Fight Animator Trigger", killerSpecialAttacks[i].HunterFightAnimatorTriggerName);
                        killerSpecialAttacks[i].HunterKillAnimatorTriggerName = EditorGUILayout.TextField("Hunter Kill Animator Trigger", killerSpecialAttacks[i].HunterKillAnimatorTriggerName);
                        killerSpecialAttacks[i].VictimDeathAnimatorTriggerName = EditorGUILayout.TextField("Victim Death Animator Trigger", killerSpecialAttacks[i].VictimDeathAnimatorTriggerName);
                        killerSpecialAttacks[i].HunterDisabledAnimatorTriggerName = EditorGUILayout.TextField("Hunter Disabled Animator Trigger", killerSpecialAttacks[i].HunterDisabledAnimatorTriggerName);
                        killerSpecialAttacks[i].VictimEscapedAnimatorTriggerName = EditorGUILayout.TextField("Victim Escaped Animator Trigger", killerSpecialAttacks[i].VictimEscapedAnimatorTriggerName);
                        killerSpecialAttacks[i].HunterDisabledDuration = EditorGUILayout.FloatField("Hunter Disabled Duration", killerSpecialAttacks[i].HunterDisabledDuration);
                        killerSpecialAttacks[i].VictimEscapedDuration = EditorGUILayout.FloatField("Victim Escaped Duration", killerSpecialAttacks[i].VictimEscapedDuration);
                        killerSpecialAttacks[i].VictimDeathDuration = EditorGUILayout.FloatField("Victim Death Duration", killerSpecialAttacks[i].VictimDeathDuration);
                        break;
                    case HunterSpecialAttack.SpecialAttackType.HuntersInstinct:
                        killerSpecialAttacks[i].HuntersInstinctDuration = EditorGUILayout.FloatField("Duration", killerSpecialAttacks[i].HuntersInstinctDuration);
                        break;
                    case HunterSpecialAttack.SpecialAttackType.HuntersVision:
                        killerSpecialAttacks[i].HuntersVisionDuration = EditorGUILayout.FloatField("Duration", killerSpecialAttacks[i].HuntersVisionDuration);
                        killerSpecialAttacks[i].HuntersVisionAudioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", killerSpecialAttacks[i].HuntersVisionAudioClip, typeof(AudioClip), false);
                        break;
                    case HunterSpecialAttack.SpecialAttackType.RapidRush:
                        killerSpecialAttacks[i].RapidRushDuration = EditorGUILayout.FloatField("Duration", killerSpecialAttacks[i].RapidRushDuration);
                        killerSpecialAttacks[i].RapidRushSprintSpeed = EditorGUILayout.FloatField("Sprint Speed", killerSpecialAttacks[i].RapidRushSprintSpeed);
                        break;
                }

                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    killerSpecialAttacks.RemoveAt(i);
                    i--; // Adjust index after removal
                }
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Special Attack", GUILayout.Height(20)))
            {
                if (killerSpecialAttacks.Count < 3)
                {
                    killerSpecialAttacks.Add(new HunterSpecialAttack());
                }
            }

            GUILayout.Space(10);

            if (EditorGUIUtility.isProSkin)
                GUI.contentColor = Color.white;   // Dark Mode
            else
                GUI.contentColor = Color.black;   // Light Mode

            EditorGUILayout.LabelField("Killer Weapon Item");
            killerWeaponItem = (GameObject)EditorGUILayout.ObjectField(killerWeaponItem, typeof(GameObject), false);

            GUILayout.Space(10);

            EditorGUILayout.HelpBox("Please enter the name of the GameObject to which the item prefabs should be assigned. By default, this is the ItemParent GameObject.", MessageType.Info);
            EditorGUILayout.LabelField("Item Parent Name");
            itemParentName = EditorGUILayout.TextField(itemParentName);

            GUILayout.Space(10);

            // Is Default Killer
            EditorGUILayout.LabelField("Is Default Killer");
            isDefaultKiller = EditorGUILayout.Toggle(isDefaultKiller);

            GUILayout.Space(10);

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            if (IsValidKiller() && killerSpecialAttacks.Count == 3)
            {
                if (GUILayout.Button("Create Killer", GUILayout.Height(40)))
                {
                    CreateKiller();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please fill in all fields and ensure exactly three special attacks are selected before creating the killer.", MessageType.Warning);
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

        private bool IsValidKiller()
        {
            if (string.IsNullOrEmpty(killerName) || !Regex.IsMatch(killerName, @"^[a-zA-Z0-9]+$"))
            {
                return false;
            }

            if (previewModelPrefab == null || killerSprite == null || string.IsNullOrEmpty(tooltipText) || killerRarity == ItemSO.Rarity.None ||
                killerPrice <= 0 || killerSellPrice < 0 || string.IsNullOrEmpty(killerModelName) || killerAvatar == null || killerAnimator == null ||
                killerChaseMusic == null || killerWeaponItem == null || string.IsNullOrEmpty(itemParentName))
            {
                return false;
            }

            return true;
        }

        private void CreateKiller()
        {
            // Create Killer ScriptableObject
            KillerSO killer = ScriptableObject.CreateInstance<KillerSO>();
            killer.uniqueName = killerName;
            killer.killerID = killerID;
            killer.previewModelPrefab = previewModelPrefab;
            killer.sprite = killerSprite;
            killer.tooltipText = tooltipText;
            killer.rarity = killerRarity;
            killer.price = killerPrice;
            killer.sellPrice = killerSellPrice;

            // Save Killer ScriptableObject
            string assetPath = "Assets/Horror Multiplayer Template by Redicion Studio/InventorySystem/Resources/ItemSOs/Killers/" + killerName + ".asset";
            AssetDatabase.CreateAsset(killer, assetPath);
            AssetDatabase.SaveAssets();

            // Create and save model prefab
            GameObject model = new GameObject(killerName + "_Model");
            string prefabPath = "Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/" + killerName + "_Model.prefab";
            GameObject modelPrefab = PrefabUtility.SaveAsPrefabAsset(model, prefabPath);
            killer.modelPrefab = modelPrefab;
            DestroyImmediate(model);

            // Update Killer ScriptableObject with model prefab
            EditorUtility.SetDirty(killer);
            AssetDatabase.SaveAssets();

            // Add killer to KillerSelectorManager
            AddKillerToKillerSelectorManager(killer);

            // Add hunter to HunterAbilities
            AddHunterToHunterAbilities(killer);

            Debug.Log("Killer created successfully at " + assetPath + " with model prefab at " + prefabPath);
        }

        private void AddKillerToKillerSelectorManager(KillerSO killer)
        {
            // Load the Player prefab
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/Player.prefab");
            if (playerPrefab != null)
            {
                KillerSelectorManager killerSelectorManager = playerPrefab.GetComponent<KillerSelectorManager>();
                if (killerSelectorManager != null)
                {
                    List<ItemSO> killerItemList = new List<ItemSO>(killerSelectorManager.killerItemSOs);
                    killerItemList.Add(killer);
                    killerSelectorManager.killerItemSOs = killerItemList.ToArray();

                    if (isDefaultKiller)
                    {
                        killerSelectorManager.defaultKillerSO = killer;
                        killerSelectorManager.defaultKillerId = killer.killerID;
                    }

                    // Mark the prefab as dirty to save changes
                    EditorUtility.SetDirty(killerSelectorManager);
                    AssetDatabase.SaveAssets();

                    Debug.Log("Killer added to KillerSelectorManager successfully.");
                }
                else
                {
                    Debug.LogError("KillerSelectorManager component not found on the Player prefab.");
                }
            }
            else
            {
                Debug.LogError("Player prefab not found at the specified path.");
            }
        }

        private void AddHunterToHunterAbilities(KillerSO killer)
        {
            // Load the Player prefab
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/Player.prefab");
            if (playerPrefab != null)
            {
                HunterAbilities hunterAbilities = playerPrefab.GetComponent<HunterAbilities>();
                if (hunterAbilities != null)
                {
                    List<HunterAbilities.Hunter> hunterList = new List<HunterAbilities.Hunter>(hunterAbilities.hunters);

                    HunterAbilities.Hunter newHunter = new HunterAbilities.Hunter
                    {
                        name = killerName,
                        HunterID = killerID,
                        AnimatorController = killerAnimator,
                        HunterAvatar = killerAvatar,
                        MoveSpeed = MoveSpeed,
                        SprintSpeed = SprintSpeed,
                        MovementMultiplier = MovementMultiplier,
                        canSprint = canSprint,
                        ChaseMusic = killerChaseMusic,
                        _specialAttacks = killerSpecialAttacks.ToArray(),
                        hunterWeaponItem = killerWeaponItem
                    };

                    // Find the item parent by name
                    Transform itemParent = FindChildByName(playerPrefab.transform, itemParentName);
                    if (itemParent != null)
                    {
                        newHunter.itemParent = itemParent;
                    }
                    else
                    {
                        Debug.LogError("Item parent with the specified name not found in the Player prefab.");
                    }

                    // Find all child GameObjects with SkinnedMeshRenderer components
                    GameObject modelParent = FindChildByName(playerPrefab.transform, killerModelName)?.gameObject;
                    if (modelParent != null)
                    {
                        List<GameObject> hunterMeshes = new List<GameObject>();
                        SkinnedMeshRenderer[] skinnedMeshRenderers = modelParent.GetComponentsInChildren<SkinnedMeshRenderer>();
                        foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
                        {
                            hunterMeshes.Add(renderer.gameObject);
                        }
                        newHunter.HunterMesh = hunterMeshes.ToArray();
                    }
                    else
                    {
                        Debug.LogError("Model parent with the specified name not found in the Player prefab.");
                    }

                    hunterList.Add(newHunter);
                    hunterAbilities.hunters = hunterList.ToArray();

                    // Mark the prefab as dirty to save changes
                    EditorUtility.SetDirty(hunterAbilities);
                    AssetDatabase.SaveAssets();

                    Debug.Log("Hunter added to HunterAbilities successfully.");
                }
                else
                {
                    Debug.LogError("HunterAbilities component not found on the Player prefab.");
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