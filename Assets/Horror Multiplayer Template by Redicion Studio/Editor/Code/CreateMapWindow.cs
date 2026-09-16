// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace RedicionStudio.Wizard
{
    public class CreateMapWindow : EditorWindow
    {
        public string mapName;
        public string mapDescription;
        public Sprite mapImage;
        public int mapId;

        private Texture2D titleImage;
        private Texture2D redicionstudioIcon;
        private Vector2 scrollPos;

        //[MenuItem("Horror Multiplayer Template by Redicion Studio/Create Map Window")]
        public static void ShowWindow()
        {
            CreateMapWindow window = GetWindow<CreateMapWindow>("Create Map");
            window.minSize = new Vector2(400, 600);
        }

        private void OnEnable()
        {
            titleImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Horror Multiplayer Template by Redicion Studio/Textures/Wizard/MapCreation/MapCreation.png");
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
            GUILayout.Label("Create New Map", EditorStyles.boldLabel);
            RedicionStudio.Wizard.HelpBoxWithLink.ShowHelpBoxWithLink(
                "You can find a tutorial video on creating maps at this URL:",
                "https://www.youtube.com/watch?v=W1Wo6NWNF4I",
                MessageType.Info
            );

            GUILayout.Space(10);

            GUI.backgroundColor = Color.white;
            EditorGUILayout.BeginVertical("box");

            // Map Name
            EditorGUILayout.LabelField("Map Name");
            mapName = EditorGUILayout.TextField(mapName);

            GUILayout.Space(10);

            // Map Description
            EditorGUILayout.LabelField("Map Description");
            mapDescription = EditorGUILayout.TextField(mapDescription);

            GUILayout.Space(10);

            // Map Image
            EditorGUILayout.LabelField("Map Image");
            mapImage = (Sprite)EditorGUILayout.ObjectField(mapImage, typeof(Sprite), false);

            GUILayout.Space(10);

            // Map ID
            EditorGUILayout.LabelField("Map ID");

            var roomManager = GameObject.FindObjectOfType<RedicionStudio.RoomManager>();
            if (roomManager != null && roomManager.matchMaps != null)
            {
                mapId = roomManager.matchMaps.Length + 1;
            }

            GUI.enabled = false;
            EditorGUILayout.IntField(mapId);
            GUI.enabled = true;

            GUILayout.Space(10);

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            if (IsValidMap())
            {
                if (GUILayout.Button("Create Map", GUILayout.Height(40)))
                {
                    CreateMap();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please fill in all fields to create the map.", MessageType.Warning);
            }

            GUILayout.Space(10);
            GUI.backgroundColor = originalBackgroundColor;
            GUI.contentColor = originalContentColor;

            DrawTextWithIcon("Developed by Florian Lauka from Redicion Studio", redicionstudioIcon, () =>
            {
                Application.OpenURL("https://redicionstudio.com/");
            });

            EditorGUILayout.EndScrollView();
        }

        private bool IsValidMap()
        {
            return !string.IsNullOrEmpty(mapName) &&
                   !string.IsNullOrEmpty(mapDescription) &&
                   mapImage != null &&
                   mapId > 0;
        }

        private void CreateMap()
        {
            var offlineManager = GameObject.FindObjectOfType<RedicionStudio.OfflineMainMenuManager>();
            if (offlineManager != null)
            {
                var matchMaps = new List<RedicionStudio.ServerCreationMatchMap>(offlineManager.matchMaps);
                matchMaps.Add(new RedicionStudio.ServerCreationMatchMap
                {
                    name = mapName,
                    mapId = mapId,
                    mapImage = mapImage
                });
                offlineManager.matchMaps = matchMaps.ToArray();
                EditorUtility.SetDirty(offlineManager);
            }
            else
            {
                Debug.LogError("OfflineMainMenuManager not found in the scene.");
            }

            var roomManager = GameObject.FindObjectOfType<RedicionStudio.RoomManager>();
            if (roomManager != null)
            {
                var matchMaps = new List<RedicionStudio.MatchMap>(roomManager.matchMaps);

                GameObject mapObject = new GameObject(mapName + " - Map");
                GameObject spawnPoints = new GameObject("Spawnpoints");
                GameObject matchObjectSpawnPoints = new GameObject("MatchObjectSpawnpoints");

                spawnPoints.transform.parent = mapObject.transform;
                matchObjectSpawnPoints.transform.parent = mapObject.transform;

                GameObject[] spawnPointObjects = new GameObject[11];
                string[] spawnPointNames = {
                    "sp1 (Lobby)", "sp2 (Lobby)", "sp3 (Lobby)", "sp4 (Lobby)",
                    "sp_hunter (Match)", "sp_hunter (Match Ended)", "sp1 (Match)",
                    "sp2 (Match)", "sp3 (Match)", "sp4 (Match)", "revivedSurvivorSpawnpoint"
                };

                Transform[] survivorSpawnpoints = new Transform[4];
                Transform[] survivorMatchSpawnpoints = new Transform[4];
                Transform hunterSpawnpoint = null;
                Transform hunterMatchEndedSpawnpoint = null;
                Transform revivedPlayerSpawnpoint = null;

                for (int i = 0; i < spawnPointObjects.Length; i++)
                {
                    spawnPointObjects[i] = new GameObject(spawnPointNames[i]);
                    spawnPointObjects[i].transform.parent = spawnPoints.transform;

                    if (i < 4) survivorSpawnpoints[i] = spawnPointObjects[i].transform;
                    else if (i >= 6 && i <= 9) survivorMatchSpawnpoints[i - 6] = spawnPointObjects[i].transform;
                    else if (i == 4) hunterSpawnpoint = spawnPointObjects[i].transform;
                    else if (i == 5) hunterMatchEndedSpawnpoint = spawnPointObjects[i].transform;
                    else if (i == 10) revivedPlayerSpawnpoint = spawnPointObjects[i].transform;
                }

                matchMaps.Add(new RedicionStudio.MatchMap
                {
                    name = mapName,
                    description = mapDescription,
                    mapId = mapId,
                    mapGameObject = mapObject,
                    survivorSpawnpoints = survivorSpawnpoints,
                    survivorMatchSpawnpoints = survivorMatchSpawnpoints,
                    hunterSpawnpoint = hunterSpawnpoint,
                    hunterMatchEndedSpawnpoint = hunterMatchEndedSpawnpoint,
                    revivedPlayerSpawnpoint = revivedPlayerSpawnpoint,
                    showMapNamePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/ShowMapName.prefab")
                });
                roomManager.matchMaps = matchMaps.ToArray();
                EditorUtility.SetDirty(roomManager);

                Debug.Log($"Map '{mapName}' created successfully with spawn points and MatchObjectSpawnpoints.");
            }
            else
            {
                Debug.LogError("RoomManager not found in the scene.");
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

            Rect labelRect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Link);
            if (Event.current.type == EventType.MouseUp && labelRect.Contains(Event.current.mousePosition))
            {
                onClick?.Invoke();
            }
        }
    }
}
