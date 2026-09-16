// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using System.Linq;
using System.IO;
using Mirror;

namespace RedicionStudio.Wizard
{
    public class CreateItemWindow : EditorWindow
    {
        public enum ItemType { Sword, Throwable, Flashlight, DefaultItem, Consumable, Firearm }

        public ItemType itemType;
        public string itemName;
        public Sprite icon;
        public int damage;
        public float useCooldown;
        public bool canUsedByHunter;
        public bool canAim;
        public string aimAnimatorTriggerName;
        public string idleAnimatorTriggerName;
        public string reloadAnimatorTriggerName;
        public AnimationClip reloadAnimation;
        public AnimationClip aimAnimation;
        public AnimationClip firstPersonAimAnimation;
        public string firstPersonAimAnimatorTriggerName;
        public AnimationClip firstPersonIdleAnimation;
        public string firstPersonIdleAnimatorTriggerName;
        public GameObject bulletPrefab;
        public float bulletSpeed = 300f;
        public GameObject cartridgeEjectPrefab;
        public float reloadDuration = 1f;
        public AudioClip[] swingSounds;
        public GameObject itemModel;
        public string attackAnimationTriggerName;
        public float attackAnimationLength;
        public float blockMovementCooldown;
        public float cancelAnimationCooldown;
        public float requiredStamina;
        public string cancelAttackAnimatorTriggerName = "CancelSwordAttack";
        public AnimationClip attackAnimation;
        public AnimationClip idleAnimation;
        public bool shouldBeSpawnedFromContainers;
        public float throwForce = 8f;
        public GameObject throwableObjectPrefab;
        public int amountOfHealthToRegenerate = 50;
        public AnimationClip consumingLoopAnimation;
        public string stopConsumingAnimatorTrigger;
        public string useConsumableAnimatorTrigger;

        private bool isValid;
        private bool showAimAnimatorTriggerName;
        private Texture2D redicionstudioIcon;
        private Vector2 scrollPos;

        public int mapId;

        //[MenuItem("Horror Multiplayer Template by Redicion Studio/Create Item Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<CreateItemWindow>("Create Item");
            window.minSize = new Vector2(400, 600);
        }

        private void OnEnable()
        {
            bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/Bullet.prefab");
            cartridgeEjectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Horror Multiplayer Template by Redicion Studio/Effects/WeaponEffects/Prefabs/CartridgeEjectEffect.prefab");
            throwableObjectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/Firecracker.prefab");
            redicionstudioIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/RedicionStudioLogo/RedicionStudio_Profil_Picture.png");
        }

        private void OnGUI()
        {
            Color originalColor = GUI.backgroundColor;
            Color contentColor = GUI.contentColor;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Display the title image
            Texture2D titleImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/ItemCreation/WizardItemCreation.png");
            if (titleImage != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(titleImage);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            //GUI.backgroundColor = new Color(0.8f, 0.85f, 0.9f); // Light pastel blue color
            if (EditorGUIUtility.isProSkin)
                GUI.contentColor = Color.white;   // Dark Mode
            else
                GUI.contentColor = Color.black;   // Light Mode

            // Rest of the UI
            GUILayout.Label("Create New Item", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(0.8f, 0.85f, 0.9f); // Light pastel blue color
            HelpBoxWithLink.ShowHelpBoxWithLink("You can find a tutorial video on creating items at this URL:", "https://www.youtube.com/watch?v=w99_k7BebV0", MessageType.Info);
            GUI.backgroundColor = originalColor;
            itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", itemType);
            itemName = EditorGUILayout.TextField("Item Name", itemName);
            mapId = EditorGUILayout.IntField("Map Id", mapId);
            // Reset the GUI content color to the original color
            GUI.contentColor = contentColor;
            icon = (Sprite)EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);
            if (EditorGUIUtility.isProSkin)
                GUI.contentColor = Color.white;   // Dark Mode
            else
                GUI.contentColor = Color.black;   // Light Mode
            damage = EditorGUILayout.IntField("Damage", damage);
            useCooldown = EditorGUILayout.FloatField("Use Cooldown", useCooldown);
            canUsedByHunter = EditorGUILayout.Toggle("Can Be Used By Hunter", canUsedByHunter);

            showAimAnimatorTriggerName = itemType == ItemType.Throwable || itemType == ItemType.Flashlight || itemType == ItemType.Firearm;

            if (itemType == ItemType.Firearm || itemType == ItemType.Flashlight)
            {
                canAim = true; // Automatically set canAim to true for Firearm and Flashlight
                GUI.enabled = false; // Make canAim non-editable
            }
            else if (itemType == ItemType.Sword || itemType == ItemType.Consumable)
            {
                canAim = false; // Automatically set canAim to false for Sword, Bandage, and Consumable
                GUI.enabled = false; // Make canAim non-editable
            }
            canAim = EditorGUILayout.Toggle("Can Aim", canAim);
            GUI.enabled = true; // Reset GUI.enabled

            if (showAimAnimatorTriggerName)
            {
                aimAnimatorTriggerName = EditorGUILayout.TextField("Aim Animator Trigger Name", aimAnimatorTriggerName);
            }

            idleAnimatorTriggerName = EditorGUILayout.TextField("Idle Animator Trigger Name", idleAnimatorTriggerName);
            firstPersonIdleAnimatorTriggerName = EditorGUILayout.TextField("First Person Idle Animator Trigger Name", firstPersonIdleAnimatorTriggerName);
            itemModel = (GameObject)EditorGUILayout.ObjectField("Item Model", itemModel, typeof(GameObject), false);

            if (itemType == ItemType.Firearm)
            {
                reloadAnimatorTriggerName = EditorGUILayout.TextField("Reload Animator Trigger Name", reloadAnimatorTriggerName);
                reloadAnimation = (AnimationClip)EditorGUILayout.ObjectField("Reload Animation", reloadAnimation, typeof(AnimationClip), false);
                aimAnimation = (AnimationClip)EditorGUILayout.ObjectField("Aim Animation", aimAnimation, typeof(AnimationClip), false);
                firstPersonAimAnimation = (AnimationClip)EditorGUILayout.ObjectField("First Person Aim Animation", firstPersonAimAnimation, typeof(AnimationClip), false);
                firstPersonAimAnimatorTriggerName = EditorGUILayout.TextField("First Person Aim Trigger Name", firstPersonAimAnimatorTriggerName);
                bulletPrefab = (GameObject)EditorGUILayout.ObjectField("Bullet Prefab", bulletPrefab, typeof(GameObject), false);
                bulletSpeed = EditorGUILayout.FloatField("Bullet Speed", bulletSpeed);
                cartridgeEjectPrefab = (GameObject)EditorGUILayout.ObjectField("Cartridge Eject Prefab", cartridgeEjectPrefab, typeof(GameObject), false);
                reloadDuration = EditorGUILayout.FloatField("Reload Duration", reloadDuration);
            }

            if (itemType == ItemType.Flashlight)
            {
                aimAnimation = (AnimationClip)EditorGUILayout.ObjectField("Aim Animation", aimAnimation, typeof(AnimationClip), false);
                firstPersonAimAnimation = (AnimationClip)EditorGUILayout.ObjectField("First Person Aim Animation", firstPersonAimAnimation, typeof(AnimationClip), false);
                firstPersonAimAnimatorTriggerName = EditorGUILayout.TextField("First Person Aim Trigger Name", firstPersonAimAnimatorTriggerName);
            }

            if (itemType == ItemType.Throwable)
            {
                throwForce = EditorGUILayout.FloatField("Throw Force", throwForce);
                throwableObjectPrefab = (GameObject)EditorGUILayout.ObjectField("Throwable Object Prefab", throwableObjectPrefab, typeof(GameObject), false);
            }

            if (itemType == ItemType.Consumable)
            {
                amountOfHealthToRegenerate = EditorGUILayout.IntField("Amount of Health to Regenerate", amountOfHealthToRegenerate);
                consumingLoopAnimation = (AnimationClip)EditorGUILayout.ObjectField("Consuming Loop Animation", consumingLoopAnimation, typeof(AnimationClip), false);
                stopConsumingAnimatorTrigger = EditorGUILayout.TextField("Stop Consuming Animator Trigger", stopConsumingAnimatorTrigger);
                useConsumableAnimatorTrigger = EditorGUILayout.TextField("Use Consumable Animator Trigger", useConsumableAnimatorTrigger);
            }

            bool isSwordOrChainsaw = itemType == ItemType.Sword;

            GUI.enabled = isSwordOrChainsaw;

            if (isSwordOrChainsaw)
            {
                // Swing Sounds
                if (swingSounds == null)
                {
                    swingSounds = new AudioClip[0];
                }

                int newSize = EditorGUILayout.IntField("Number of Swing Sounds", swingSounds.Length);
                if (newSize != swingSounds.Length)
                {
                    System.Array.Resize(ref swingSounds, newSize);
                }

                for (int i = 0; i < swingSounds.Length; i++)
                {
                    swingSounds[i] = (AudioClip)EditorGUILayout.ObjectField($"Swing Sound {i + 1}", swingSounds[i], typeof(AudioClip), false);
                }

                // Sword/Chainsaw specific fields
                attackAnimationTriggerName = EditorGUILayout.TextField("Attack Animation Trigger Name", attackAnimationTriggerName);
                attackAnimationLength = EditorGUILayout.FloatField("Attack Animation Length", attackAnimationLength);
                blockMovementCooldown = EditorGUILayout.FloatField("Block Movement Cooldown", blockMovementCooldown);
                cancelAnimationCooldown = EditorGUILayout.FloatField("Cancel Animation Cooldown", cancelAnimationCooldown);
                requiredStamina = EditorGUILayout.FloatField("Required Stamina", requiredStamina);
                cancelAttackAnimatorTriggerName = EditorGUILayout.TextField("Cancel Attack Animator Trigger Name", cancelAttackAnimatorTriggerName);
                attackAnimation = (AnimationClip)EditorGUILayout.ObjectField("Attack Animation", attackAnimation, typeof(AnimationClip), false);
                useCooldown = 3.04f;
            }

            GUI.enabled = true;

            // Other fields
            idleAnimation = (AnimationClip)EditorGUILayout.ObjectField("Idle Animation", idleAnimation, typeof(AnimationClip), false);
            firstPersonIdleAnimation = (AnimationClip)EditorGUILayout.ObjectField("First Person Idle Animation", firstPersonIdleAnimation, typeof(AnimationClip), false);
            shouldBeSpawnedFromContainers = EditorGUILayout.Toggle("Should be spawned from item containers", shouldBeSpawnedFromContainers);

            // Reset the GUI background color to the original color
            //GUI.backgroundColor = originalColor;

            GUILayout.Space(10);
            isValid = ValidateFields();
            if (isValid)
            {
                if (GUILayout.Button("Create Item", GUILayout.Height(40)))
                {
                    CreateItem();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please fill in all fields.", MessageType.Warning);
            }

            GUILayout.Space(10);
            GUI.contentColor = contentColor;
            DrawTextWithIcon("Developed by Florian Lauka from Redicion Studio", redicionstudioIcon, CreateItemWindow.ShowWindow);

            Rect labelRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Link);
            if (Event.current.type == EventType.MouseUp && labelRect.Contains(Event.current.mousePosition))
            {
                Application.OpenURL("https://redicionstudio.com/");
            }

            EditorGUILayout.EndScrollView();
        }

        private bool ValidateFields()
        {
            bool valid = !string.IsNullOrEmpty(itemName) && itemModel != null && icon != null;

            if (itemType == ItemType.Sword)
            {
                valid &= swingSounds != null && swingSounds.Length > 0;
                valid &= !string.IsNullOrEmpty(attackAnimationTriggerName);
                valid &= attackAnimation != null;
                valid &= idleAnimation != null;
                valid &= firstPersonIdleAnimation != null;
            }

            if (itemType == ItemType.Firearm)
            {
                valid &= !string.IsNullOrEmpty(reloadAnimatorTriggerName);
                valid &= reloadAnimation != null;
                valid &= aimAnimation != null;
                valid &= firstPersonAimAnimation != null;
                valid &= !string.IsNullOrEmpty(firstPersonAimAnimatorTriggerName);
                valid &= bulletPrefab != null;
                valid &= cartridgeEjectPrefab != null;
                valid &= bulletSpeed > 0;
            }

            if (itemType == ItemType.Flashlight)
            {
                valid &= aimAnimation != null;
                valid &= firstPersonAimAnimation != null;
                valid &= !string.IsNullOrEmpty(firstPersonAimAnimatorTriggerName);
            }

            if (itemType == ItemType.Throwable)
            {
                valid &= throwableObjectPrefab != null;
            }

            if (itemType == ItemType.Consumable)
            {
                valid &= consumingLoopAnimation != null;
                valid &= !string.IsNullOrEmpty(stopConsumingAnimatorTrigger);
                valid &= !string.IsNullOrEmpty(useConsumableAnimatorTrigger);
            }

            return valid;
        }

        private void CreateItem()
        {
            if (itemModel == null)
            {
                Debug.LogError("Item model must be assigned.");
                return;
            }

            string prefabPath = $"Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/Items/{itemName}_Item.prefab";
            GameObject itemPrefab = new GameObject(itemName + "_Item");

            // Add Model to Prefab
            GameObject modelInstance = Instantiate(itemModel, itemPrefab.transform);
            modelInstance.name = "Model";

            GameObject interactionIndicatorPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/InteractionIndicator.prefab", typeof(GameObject));
            GameObject interactionIndicator = Instantiate(interactionIndicatorPrefab, itemPrefab.transform);
            interactionIndicator.name = "InteractionIndicator";

            // Load CooldownUI prefab reference
            GameObject cooldownUIPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/CooldownUI.prefab", typeof(GameObject));

            // Create and set HitParticlePosition if needed
            GameObject hitParticlePosition = null;
            if (itemType == ItemType.Sword)
            {
                hitParticlePosition = new GameObject("HitParticlePosition");
                hitParticlePosition.transform.SetParent(itemPrefab.transform);
            }

            // Add required components
            itemPrefab.AddComponent<NetworkIdentity>();
            Rigidbody rb = itemPrefab.AddComponent<Rigidbody>();
            rb.mass = 1;
            rb.linearDamping = 0;
            rb.angularDamping = 0.05f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            BoxCollider boxCollider = itemPrefab.AddComponent<BoxCollider>();
            boxCollider.isTrigger = false;

            /*SphereCollider sphereCollider = itemPrefab.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.center = Vector3.zero;
            sphereCollider.radius = 1;*/

            AudioSource audioSource = itemPrefab.AddComponent<AudioSource>();

            // Add item type specific component
            Component itemComponent = null;
            switch (itemType)
            {
                case ItemType.Sword:
                    itemComponent = itemPrefab.AddComponent<Sword>();
                    break;
                case ItemType.Throwable:
                    itemComponent = itemPrefab.AddComponent<Throwable>();
                    break;
                case ItemType.Flashlight:
                    itemComponent = itemPrefab.AddComponent<NewFlashlight>();
                    break;
                case ItemType.DefaultItem:
                    itemComponent = itemPrefab.AddComponent<GameplayItem>();
                    break;
                case ItemType.Consumable:
                    itemComponent = itemPrefab.AddComponent<Consumable>();
                    break;
                case ItemType.Firearm:
                    itemComponent = itemPrefab.AddComponent<Firearm>();
                    break;
            }

            // Set component properties
            if (itemComponent != null)
            {
                System.Type componentType = itemComponent.GetType();

                // Check and set each field
                SetFieldIfExists(componentType, itemComponent, "icon", icon);
                SetFieldIfExists(componentType, itemComponent, "interactionIndicator", interactionIndicator);
                SetFieldIfExists(componentType, itemComponent, "ItemName", itemName);
                SetFieldIfExists(componentType, itemComponent, "Damage", damage);
                SetFieldIfExists(componentType, itemComponent, "useCooldown", useCooldown);
                SetFieldIfExists(componentType, itemComponent, "canUsedByHunter", canUsedByHunter);
                SetFieldIfExists(componentType, itemComponent, "canAim", canAim);
                if (showAimAnimatorTriggerName)
                {
                    SetFieldIfExists(componentType, itemComponent, "aimAnimatorTriggerName", aimAnimatorTriggerName);
                }
                SetFieldIfExists(componentType, itemComponent, "idleAnimatorTriggerName", idleAnimatorTriggerName);
                SetFieldIfExists(componentType, itemComponent, "firstPersonIdleAnimatorTriggerName", firstPersonIdleAnimatorTriggerName);

                if (componentType == typeof(Sword) || componentType == typeof(Chainsaw))
                {
                    BoxCollider swordBoxCollider = itemPrefab.AddComponent<BoxCollider>();
                    swordBoxCollider.isTrigger = true;
                    SetFieldIfExists(componentType, itemComponent, "swingSounds", swingSounds);
                    SetFieldIfExists(componentType, itemComponent, "cooldownUI", cooldownUIPrefab);
                    SetFieldIfExists(componentType, itemComponent, "attackAnimationTriggerName", attackAnimationTriggerName);
                    SetFieldIfExists(componentType, itemComponent, "attackAnimationLength", attackAnimationLength);
                    SetFieldIfExists(componentType, itemComponent, "blockMovement", true);
                    SetFieldIfExists(componentType, itemComponent, "blockMovementCooldown", blockMovementCooldown);
                    SetFieldIfExists(componentType, itemComponent, "cancelAnimation", true);
                    SetFieldIfExists(componentType, itemComponent, "cancelAnimationCooldown", cancelAnimationCooldown);
                    SetFieldIfExists(componentType, itemComponent, "requiredStamina", requiredStamina);
                    SetFieldIfExists(componentType, itemComponent, "cancelAttackAnimatorTriggerName", cancelAttackAnimatorTriggerName);

                    if (hitParticlePosition != null)
                    {
                        SetFieldIfExists(componentType, itemComponent, "hitParticlePosition", hitParticlePosition.transform);
                    }

                    // Set hit prefabs
                    SetFieldIfExists(componentType, itemComponent, "bloodHitPrefab", (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Horror Multiplayer Template by Redicion Studio/Effects/WeaponEffects/Prefabs/BulletImpactFleshSmallEffect.prefab", typeof(GameObject)));
                    SetFieldIfExists(componentType, itemComponent, "metalHitPrefab", (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Horror Multiplayer Template by Redicion Studio/Effects/WeaponEffects/Prefabs/CarImpactMetalEffect.prefab", typeof(GameObject)));
                    SetFieldIfExists(componentType, itemComponent, "woodHitPrefab", (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Horror Multiplayer Template by Redicion Studio/Effects/WeaponEffects/Prefabs/BulletImpactWoodEffect.prefab", typeof(GameObject)));
                    SetFieldIfExists(componentType, itemComponent, "stoneHitPrefab", (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Horror Multiplayer Template by Redicion Studio/Effects/WeaponEffects/Prefabs/BulletImpactStoneEffect.prefab", typeof(GameObject)));
                }

                if (componentType == typeof(Firearm))
                {
                    SetFieldIfExists(componentType, itemComponent, "reloadAnimatorTriggerName", reloadAnimatorTriggerName);
                    SetFieldIfExists(componentType, itemComponent, "reloadAnimation", reloadAnimation);
                    SetFieldIfExists(componentType, itemComponent, "aimAnimation", aimAnimation);
                    SetFieldIfExists(componentType, itemComponent, "firstPersonAimAnimatorTriggerName", firstPersonAimAnimatorTriggerName);
                    SetFieldIfExists(componentType, itemComponent, "bulletPrefab", bulletPrefab);
                    SetFieldIfExists(componentType, itemComponent, "bulletSpeed", bulletSpeed);
                    SetFieldIfExists(componentType, itemComponent, "cartridgeEjectPrefab", cartridgeEjectPrefab);
                    SetFieldIfExists(componentType, itemComponent, "reloadDuration", reloadDuration);

                    GameObject bulletSpawnPoint = new GameObject("_bulletSpawnPointPosition");
                    bulletSpawnPoint.transform.SetParent(itemPrefab.transform);
                    SetFieldIfExists(componentType, itemComponent, "_bulletSpawnPointPosition", bulletSpawnPoint.transform);

                    GameObject cartridgeEjectSpawnPoint = new GameObject("_cartridgeEjectSpawnPointPosition");
                    cartridgeEjectSpawnPoint.transform.SetParent(itemPrefab.transform);
                    SetFieldIfExists(componentType, itemComponent, "_cartridgeEjectSpawnPointPosition", cartridgeEjectSpawnPoint.transform);

                    SetFieldIfExists(componentType, itemComponent, "useAnimationLayer4WhenAiming", true);
                }

                if (componentType == typeof(NewFlashlight))
                {
                    SetFieldIfExists(componentType, itemComponent, "aimAnimation", aimAnimation);
                    SetFieldIfExists(componentType, itemComponent, "firstPersonAimAnimatorTriggerName", firstPersonAimAnimatorTriggerName);
                    SetFieldIfExists(componentType, itemComponent, "useAnimationLayer4WhenAiming", true);

                    // Create FlashLight GameObject
                    GameObject flashLightObj = new GameObject("FlashLight");
                    Light light = flashLightObj.AddComponent<Light>();
                    light.type = LightType.Spot;
                    light.range = 20f;
                    light.spotAngle = 30f;
                    light.intensity = 9f;
                    light.bounceIntensity = 1f;
                    light.enabled = false;
                    flashLightObj.transform.SetParent(itemPrefab.transform);

                    SetFieldIfExists(componentType, itemComponent, "lightComponent", light);

                    // Assign Audio Clips
                    AudioClip turnOnSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Horror Multiplayer Template by Redicion Studio/Audio/Flashlight/FlashlightTurnOnSound.wav");
                    AudioClip turnOffSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Horror Multiplayer Template by Redicion Studio/Audio/Flashlight/FlashlightTurnOffSound.wav");
                    SetFieldIfExists(componentType, itemComponent, "turnOnSound", turnOnSound);
                    SetFieldIfExists(componentType, itemComponent, "turnOffSound", turnOffSound);

                    // Assign flashLightGlass
                    MeshRenderer[] meshRenderers = itemModel.GetComponentsInChildren<MeshRenderer>();
                    if (meshRenderers.Length > 0)
                    {
                        SetFieldIfExists(componentType, itemComponent, "flashLightGlass", meshRenderers[0]);
                    }
                }

                if (componentType == typeof(Throwable))
                {
                    SetFieldIfExists(componentType, itemComponent, "throwForce", throwForce);
                    SetFieldIfExists(componentType, itemComponent, "throwableObjectPrefab", throwableObjectPrefab);
                }

                if (componentType == typeof(Consumable))
                {
                    SetFieldIfExists(componentType, itemComponent, "amountOfHealthToRegenerate", amountOfHealthToRegenerate);
                }

                // Set audio source reference
                SetFieldIfExists(componentType, itemComponent, "audioSource", audioSource);

                // Set _itemMesh
                MeshRenderer[] itemMeshRenderers = itemPrefab.GetComponentsInChildren<MeshRenderer>();
                SetFieldIfExists(componentType, itemComponent, "_itemMesh", itemMeshRenderers);
            }

            // Save the prefab
            PrefabUtility.SaveAsPrefabAsset(itemPrefab, prefabPath);
            // Add the new item to the GameplayItemList
            GameplayItem newItem = itemPrefab.GetComponent<GameplayItem>();

            if (newItem != null)
            {
                GameplayItemPreparer.AddItemToList(newItem);
            }
            DestroyImmediate(itemPrefab);

            // Add the item prefab to the Registered Spawnable Prefabs list
            AddPrefabToSpawnableList(prefabPath);

            // Add the item prefab to the RoomManager
            AddToRoomManager(prefabPath);

            // Add the item prefab to the ItemContainerManager if applicable
            if (shouldBeSpawnedFromContainers)
            {
                AddToItemContainerManager(prefabPath);
            }

            // Create Animation States and Transitions
            CreateAnimatorStatesAndTransitions();
        }

        private void SetFieldIfExists(System.Type type, Component component, string fieldName, object value)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(component, value);
            }
        }

        private void CreateAnimatorStatesAndTransitions()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Horror Multiplayer Template by Redicion Studio/Animation/StarterAssetsThirdPerson.controller");

            // Add common parameters
            AddAnimatorParameterIfNotExists(controller, "isFirstPerson", AnimatorControllerParameterType.Bool);

            if (itemType == ItemType.Firearm && reloadAnimation != null && aimAnimation != null && idleAnimation != null)
            {
                AddAnimatorParameterIfNotExists(controller, reloadAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, aimAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, idleAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, firstPersonAimAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, firstPersonIdleAnimatorTriggerName, AnimatorControllerParameterType.Trigger);

                CreateLayerAnimatorStatesAndTransitions(controller.layers[4], controller, true, true);
                CreateLayerAnimatorStatesAndTransitions(controller.layers[5], controller, true, true);
                CreateLayerAnimatorStatesAndTransitions(controller.layers[6], controller, true, true);
            }
            else if (itemType == ItemType.Flashlight && aimAnimation != null && idleAnimation != null)
            {
                AddAnimatorParameterIfNotExists(controller, aimAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, idleAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, firstPersonAimAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, firstPersonIdleAnimatorTriggerName, AnimatorControllerParameterType.Trigger);

                CreateLayerAnimatorStatesAndTransitions(controller.layers[4], controller, true, true);
                CreateLayerAnimatorStatesAndTransitions(controller.layers[6], controller, true, true);
            }
            else if (itemType == ItemType.Throwable && aimAnimation != null && idleAnimation != null)
            {
                AddAnimatorParameterIfNotExists(controller, aimAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, idleAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, firstPersonAimAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, firstPersonIdleAnimatorTriggerName, AnimatorControllerParameterType.Trigger);

                CreateLayerAnimatorStatesAndTransitions(controller.layers[6], controller, true, true);
            }
            else if (itemType == ItemType.Consumable && consumingLoopAnimation != null)
            {
                AddAnimatorParameterIfNotExists(controller, stopConsumingAnimatorTrigger, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, useConsumableAnimatorTrigger, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, firstPersonIdleAnimatorTriggerName, AnimatorControllerParameterType.Trigger);

                CreateConsumableLayerAnimatorStatesAndTransitions(controller.layers[3], controller);
                CreateConsumableLayerAnimatorStatesAndTransitions(controller.layers[5], controller);
                CreateConsumableLayerAnimatorStatesAndTransitions(controller.layers[6], controller);
            }
            if (itemType == ItemType.Sword && attackAnimation != null && idleAnimation != null)
            {
                AddAnimatorParameterIfNotExists(controller, attackAnimationTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, idleAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, cancelAttackAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, firstPersonIdleAnimatorTriggerName, AnimatorControllerParameterType.Trigger);

                CreateLayer0SwordAnimatorStatesAndTransitions(controller.layers[0]);
                CreateLayer5And6SwordAnimatorStatesAndTransitions(controller.layers[5]);
                CreateLayer5And6SwordAnimatorStatesAndTransitions(controller.layers[6]);
            }
            else if (itemType == ItemType.DefaultItem && idleAnimation != null)
            {
                if (aimAnimatorTriggerName != null)
                    AddAnimatorParameterIfNotExists(controller, aimAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, idleAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                if (firstPersonAimAnimatorTriggerName != null)
                    AddAnimatorParameterIfNotExists(controller, firstPersonAimAnimatorTriggerName, AnimatorControllerParameterType.Trigger);
                AddAnimatorParameterIfNotExists(controller, firstPersonIdleAnimatorTriggerName, AnimatorControllerParameterType.Trigger);

                CreateLayerAnimatorStatesAndTransitions(controller.layers[6], controller, canAim, true);
            }
        }

        private void CreateLayerAnimatorStatesAndTransitions(AnimatorControllerLayer layer, AnimatorController controller, bool createAimState = true, bool createIdleState = true)
        {
            AnimatorStateMachine sm = layer.stateMachine;

            if (createIdleState)
            {
                // Create IdleAnimation State
                AnimatorState idleState = sm.AddState(idleAnimation.name);
                idleState.motion = idleAnimation;

                // Create FirstPersonIdleAnimation State
                AnimatorState firstPersonIdleState = sm.AddState(firstPersonIdleAnimation.name);
                firstPersonIdleState.motion = firstPersonIdleAnimation;

                // Create Transition from Any State to IdleAnimation State
                AnimatorStateTransition idleTransition = sm.AddAnyStateTransition(idleState);
                idleTransition.AddCondition(AnimatorConditionMode.If, 0, idleAnimatorTriggerName);
                idleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "isFirstPerson");
                idleTransition.duration = 0.25f; // Set transition duration to 0.25
                idleTransition.hasExitTime = false;

                // Create Transition from Any State to FirstPersonIdleAnimation State
                AnimatorStateTransition firstPersonIdleTransition = sm.AddAnyStateTransition(firstPersonIdleState);
                firstPersonIdleTransition.AddCondition(AnimatorConditionMode.If, 0, firstPersonIdleAnimatorTriggerName);
                firstPersonIdleTransition.AddCondition(AnimatorConditionMode.If, 0, "isFirstPerson");
                firstPersonIdleTransition.duration = 0.25f; // Set transition duration to 0.25
                firstPersonIdleTransition.hasExitTime = false;
            }

            if (createAimState)
            {
                // Create AimAnimation State
                AnimatorState aimState = sm.AddState(aimAnimation.name);
                aimState.motion = aimAnimation;

                // Create FirstPersonAimAnimation State
                AnimatorState firstPersonAimState = sm.AddState(firstPersonAimAnimation.name);
                firstPersonAimState.motion = firstPersonAimAnimation;

                // Create Transition from Any State to AimAnimation State
                AnimatorStateTransition aimTransition = sm.AddAnyStateTransition(aimState);
                aimTransition.AddCondition(AnimatorConditionMode.If, 0, aimAnimatorTriggerName);
                aimTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "isFirstPerson");
                aimTransition.duration = 0.25f; // Set transition duration to 0.25
                aimTransition.hasExitTime = false;

                // Create Transition from Any State to FirstPersonAimAnimation State
                AnimatorStateTransition firstPersonAimTransition = sm.AddAnyStateTransition(firstPersonAimState);
                firstPersonAimTransition.AddCondition(AnimatorConditionMode.If, 0, firstPersonAimAnimatorTriggerName);
                firstPersonAimTransition.AddCondition(AnimatorConditionMode.If, 0, "isFirstPerson");
                firstPersonAimTransition.duration = 0.25f; // Set transition duration to 0.25
                firstPersonAimTransition.hasExitTime = false;
            }

            if (itemType == ItemType.Firearm)
            {
                // Create ReloadAnimation State
                AnimatorState reloadState = sm.AddState(reloadAnimation.name);
                reloadState.motion = reloadAnimation;

                // Create Transition from Any State to ReloadAnimation State
                AnimatorStateTransition reloadTransition = sm.AddAnyStateTransition(reloadState);
                reloadTransition.AddCondition(AnimatorConditionMode.If, 0, reloadAnimatorTriggerName);
                reloadTransition.duration = 0.25f; // Set transition duration to 0.25
                reloadTransition.hasExitTime = false;
            }
        }

        private void CreateLayer0SwordAnimatorStatesAndTransitions(AnimatorControllerLayer layer)
        {
            AnimatorStateMachine sm = layer.stateMachine;

            // Create AttackAnimation State
            AnimatorState attackState = sm.AddState(attackAnimation.name);
            attackState.motion = attackAnimation;

            // Create Transition from Any State to AttackAnimation State
            AnimatorStateTransition attackTransition = sm.AddAnyStateTransition(attackState);
            attackTransition.AddCondition(AnimatorConditionMode.If, 0, attackAnimationTriggerName);
            attackTransition.duration = 0f;
            attackTransition.hasExitTime = false;

            // Find Idle Walk Run Blend Tree and Hunter Idle Walk Run Blend Tree
            AnimatorState idleWalkRunBlendTree = sm.states.FirstOrDefault(s => s.state.name == "Idle Walk Run Blend").state;
            AnimatorState hunterIdleWalkRunBlendTree = sm.states.FirstOrDefault(s => s.state.name == "Hunter Idle Walk Run Blend").state;

            if (idleWalkRunBlendTree != null)
            {
                // Transition from Attack State to Idle Walk Run Blend Tree
                AnimatorStateTransition toIdleTransition1 = attackState.AddTransition(idleWalkRunBlendTree);
                toIdleTransition1.AddCondition(AnimatorConditionMode.IfNot, 0, "isHunter");
                toIdleTransition1.duration = 0.25f;
                toIdleTransition1.hasExitTime = true;

                AnimatorStateTransition toIdleTransition2 = attackState.AddTransition(idleWalkRunBlendTree);
                toIdleTransition2.AddCondition(AnimatorConditionMode.If, 0, cancelAttackAnimatorTriggerName);
                toIdleTransition2.AddCondition(AnimatorConditionMode.IfNot, 0, "isHunter");
                toIdleTransition2.duration = 0.25f;
                toIdleTransition2.hasExitTime = true;
            }

            if (hunterIdleWalkRunBlendTree != null)
            {
                // Transition from Attack State to Hunter Idle Walk Run Blend Tree
                AnimatorStateTransition toHunterIdleTransition1 = attackState.AddTransition(hunterIdleWalkRunBlendTree);
                toHunterIdleTransition1.AddCondition(AnimatorConditionMode.If, 0, "isHunter");
                toHunterIdleTransition1.duration = 0.25f;
                toHunterIdleTransition1.hasExitTime = true;

                AnimatorStateTransition toHunterIdleTransition2 = attackState.AddTransition(hunterIdleWalkRunBlendTree);
                toHunterIdleTransition2.AddCondition(AnimatorConditionMode.If, 0, cancelAttackAnimatorTriggerName);
                toHunterIdleTransition2.AddCondition(AnimatorConditionMode.If, 0, "isHunter");
                toHunterIdleTransition2.duration = 0.25f;
                toHunterIdleTransition2.hasExitTime = true;
            }
        }

        private void CreateLayer5And6SwordAnimatorStatesAndTransitions(AnimatorControllerLayer layer)
        {
            AnimatorStateMachine sm = layer.stateMachine;

            // Create IdleAnimation State
            AnimatorState idleState = sm.AddState(idleAnimation.name);
            idleState.motion = idleAnimation;

            // Create FirstPersonIdleAnimation State
            AnimatorState firstPersonIdleState = sm.AddState(firstPersonIdleAnimation.name);
            firstPersonIdleState.motion = firstPersonIdleAnimation;

            // Create AttackAnimation State
            AnimatorState attackState = sm.AddState(attackAnimation.name);
            attackState.motion = attackAnimation;

            // Transition from Any State to Attack State
            AnimatorStateTransition toAttackTransition = sm.AddAnyStateTransition(attackState);
            toAttackTransition.AddCondition(AnimatorConditionMode.If, 0, attackAnimationTriggerName);
            toAttackTransition.duration = 0f;
            toAttackTransition.hasExitTime = false;

            // Transition from Attack State to Idle State
            AnimatorStateTransition attackToIdleTransition1 = attackState.AddTransition(idleState);
            attackToIdleTransition1.AddCondition(AnimatorConditionMode.IfNot, 0, "isFirstPerson");
            attackToIdleTransition1.duration = 0.25f;
            attackToIdleTransition1.hasExitTime = true;

            AnimatorStateTransition attackToIdleTransition2 = attackState.AddTransition(idleState);
            attackToIdleTransition2.AddCondition(AnimatorConditionMode.If, 0, cancelAttackAnimatorTriggerName);
            attackToIdleTransition2.AddCondition(AnimatorConditionMode.IfNot, 0, "isFirstPerson");
            attackToIdleTransition2.duration = 0.25f;
            attackToIdleTransition2.hasExitTime = true;

            // Transition from Any State to Idle State
            AnimatorStateTransition idleTransition = sm.AddAnyStateTransition(idleState);
            idleTransition.AddCondition(AnimatorConditionMode.If, 0, idleAnimatorTriggerName);
            idleTransition.AddCondition(AnimatorConditionMode.IfNot, 0, "isFirstPerson");
            idleTransition.duration = 0.25f;
            idleTransition.hasExitTime = false;

            // Transition from Any State to FirstPersonIdle State
            AnimatorStateTransition firstPersonIdleTransition = sm.AddAnyStateTransition(firstPersonIdleState);
            firstPersonIdleTransition.AddCondition(AnimatorConditionMode.If, 0, firstPersonIdleAnimatorTriggerName);
            firstPersonIdleTransition.AddCondition(AnimatorConditionMode.If, 0, "isFirstPerson");
            firstPersonIdleTransition.duration = 0.25f;
            firstPersonIdleTransition.hasExitTime = false;

            // Transition from Attack State to FirstPersonIdle State
            AnimatorStateTransition attackToFirstPersonIdleTransition1 = attackState.AddTransition(firstPersonIdleState);
            attackToFirstPersonIdleTransition1.AddCondition(AnimatorConditionMode.If, 0, cancelAttackAnimatorTriggerName);
            attackToFirstPersonIdleTransition1.AddCondition(AnimatorConditionMode.If, 0, "isFirstPerson");
            attackToFirstPersonIdleTransition1.duration = 0.25f;
            attackToFirstPersonIdleTransition1.hasExitTime = true;

            AnimatorStateTransition attackToFirstPersonIdleTransition2 = attackState.AddTransition(firstPersonIdleState);
            attackToFirstPersonIdleTransition2.AddCondition(AnimatorConditionMode.If, 0, "isFirstPerson");
            attackToFirstPersonIdleTransition2.duration = 0.25f;
            attackToFirstPersonIdleTransition2.hasExitTime = true;
        }

        private void CreateConsumableLayerAnimatorStatesAndTransitions(AnimatorControllerLayer layer, AnimatorController controller)
        {
            AnimatorStateMachine sm = layer.stateMachine;

            // Create ConsumingLoopAnimation State
            AnimatorState consumingState = sm.AddState(consumingLoopAnimation.name);
            consumingState.motion = consumingLoopAnimation;

            // Create Transition from Any State to ConsumingLoopAnimation State
            AnimatorStateTransition useTransition = sm.AddAnyStateTransition(consumingState);
            useTransition.AddCondition(AnimatorConditionMode.If, 0, useConsumableAnimatorTrigger);
            useTransition.duration = 0.25f; // Set transition duration to 0.25
            useTransition.hasExitTime = false;

            // Find existing Empty State
            AnimatorState emptyState = sm.states.FirstOrDefault(s => s.state.name == "Empty").state;
            if (emptyState == null)
            {
                Debug.LogError("No existing Empty state found in the animator. Please ensure there is an Empty state available.");
                return;
            }

            // Create Transition from ConsumingLoopAnimation State to Empty State
            AnimatorStateTransition stopTransition = consumingState.AddTransition(emptyState);
            stopTransition.AddCondition(AnimatorConditionMode.If, 0, stopConsumingAnimatorTrigger);
            stopTransition.duration = 0.25f; // Set transition duration to 0.25
            stopTransition.hasExitTime = false;
        }

        private void AddAnimatorParameterIfNotExists(AnimatorController controller, string parameterName, AnimatorControllerParameterType type)
        {
            if (!controller.parameters.Any(p => p.name == parameterName))
            {
                controller.AddParameter(parameterName, type);
            }
        }

        private void AddPrefabToSpawnableList(string prefabPath)
        {
            string scenePath = "Assets/Horror Multiplayer Template by Redicion Studio/Scenes/MainScene.unity";

            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);

            GameObject networkManagerObj = GameObject.Find("NetworkManager");
            if (networkManagerObj != null)
            {
                CustomNetManager customNetManager = networkManagerObj.GetComponent<CustomNetManager>();
                if (customNetManager != null)
                {
                    GameObject itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (itemPrefab != null && !customNetManager.spawnPrefabs.Contains(itemPrefab))
                    {
                        customNetManager.spawnPrefabs.Add(itemPrefab);

                        EditorUtility.SetDirty(customNetManager);

                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(networkManagerObj.scene);
                        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                    }
                    else
                    {
                        Debug.LogError("Prefab konnte nicht geladen werden oder ist bereits in der Liste.");
                    }
                }
                else
                {
                    Debug.LogError("CustomNetManager-Komponente wurde nicht gefunden.");
                }
            }
            else
            {
                Debug.LogError("NetworkManager-Objekt wurde nicht gefunden.");
            }
        }

        private void AddToRoomManager(string prefabPath)
        {
            string scenePath = "Assets/Horror Multiplayer Template by Redicion Studio/Scenes/MainScene.unity";
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);

            GameObject roomManagerObj = GameObject.Find("RoomManager");
            if (roomManagerObj != null)
            {
                RoomManager roomManager = roomManagerObj.GetComponent<RoomManager>();
                if (roomManager != null)
                {
                    GameObject itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    MatchObject newMatchObject = new MatchObject
                    {
                        name = itemName,
                        objectPrefab = itemPrefab,
                        spawnPoint = new GameObject(itemName + "SpawnPoint").transform
                    };

                    GameObject matchObjectSpawnpointsObj = null;

                    foreach (MatchMap matchMap in roomManager.matchMaps)
                    {
                        if (matchMap.mapId == mapId)
                        {
                            GameObject parentObj = GameObject.Find(matchMap.mapGameObject.name);

                            if (parentObj != null)
                            {
                                Transform matchObjectSpawnpointsTransform = parentObj.transform.Find("MatchObjectSpawnpoints");

                                if (matchObjectSpawnpointsTransform != null)
                                {
                                    matchObjectSpawnpointsObj = matchObjectSpawnpointsTransform.gameObject;
                                }
                                else
                                {
                                    matchObjectSpawnpointsObj = GameObject.Find("MatchObjectSpawnpoints");
                                }
                            }
                            else
                            {
                                matchObjectSpawnpointsObj = GameObject.Find("MatchObjectSpawnpoints");
                            }
                        }
                    }

                    if (matchObjectSpawnpointsObj != null)
                    {
                        newMatchObject.spawnPoint.SetParent(matchObjectSpawnpointsObj.transform);
                    }

                    foreach(MatchMap matchMap in roomManager.matchMaps)
                    {
                        if(matchMap.mapId == mapId)
                        {
                            ArrayUtility.Add(ref matchMap.matchObjects, newMatchObject);
                            EditorUtility.SetDirty(roomManager);
                        }
                    }
                }
                else
                {
                    Debug.LogError("RoomManager component not found on RoomManager GameObject.");
                }
            }
            else
            {
                Debug.LogError("RoomManager GameObject not found in the scene.");
            }
        }

        private void AddToItemContainerManager(string prefabPath)
        {
            string[] containerPrefabPaths = new string[]
            {
            "Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/ItemCabinet.prefab",
            "Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/ItemDesk.prefab"
            };

            foreach (string containerPrefabPath in containerPrefabPaths)
            {
                GameObject containerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(containerPrefabPath);
                ItemContainerManager itemContainerManager = containerPrefab.GetComponent<ItemContainerManager>();

                if (itemContainerManager != null)
                {
                    GameObject itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    ItemContainerManagerItem newItem = new ItemContainerManagerItem
                    {
                        itemPrefab = itemPrefab
                    };

                    ArrayUtility.Add(ref itemContainerManager.items, newItem);
                    EditorUtility.SetDirty(itemContainerManager);
                }
                else
                {
                    Debug.LogError("ItemContainerManager component not found on " + containerPrefabPath);
                }
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

    public static class HelpBoxWithLink
    {
        public static void ShowHelpBoxWithLink(string message, string url, MessageType messageType)
        {
            // Erstellen Sie eine vertikale Box, um den Text und den Link anzuzeigen
            EditorGUILayout.BeginVertical("box");

            // Zeigen Sie die Hilfe-Box-Nachricht an
            EditorGUILayout.HelpBox(message, messageType);

            // F�gen Sie einen anklickbaren Link hinzu
            if (GUILayout.Button(url, EditorStyles.linkLabel))
            {
                Application.OpenURL(url);
            }

            EditorGUILayout.EndVertical();
        }
    }
}