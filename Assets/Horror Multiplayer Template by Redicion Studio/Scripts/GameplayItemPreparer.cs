// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace RedicionStudio
{
#if UNITY_EDITOR
    public class GameplayItemPreparer : MonoBehaviour
    {
        public static GameplayItemList GetOrCreateItemList()
        {
            GameplayItemList itemList = AssetDatabase.LoadAssetAtPath<GameplayItemList>("Assets/Horror Multiplayer Template by Redicion Studio/InventorySystem/Resources/GameplayItemList.asset");
            if (itemList == null)
            {
                itemList = ScriptableObject.CreateInstance<GameplayItemList>();
                AssetDatabase.CreateAsset(itemList, "Assets/Horror Multiplayer Template by Redicion Studio/InventorySystem/Resources/GameplayItemList.asset");
                AssetDatabase.SaveAssets();
            }
            return itemList;
        }

        public static void AddItemToList(GameplayItem newItem)
        {
            GameplayItemList itemList = GetOrCreateItemList();
            List<GameplayItem> items = new List<GameplayItem>(itemList.items);
            if (!items.Contains(newItem))
            {
                items.Add(newItem);
                itemList.items = items.ToArray();
                EditorUtility.SetDirty(itemList);
                AssetDatabase.SaveAssets();
            }
        }

        [MenuItem("Tools/Horror Multiplayer Template by Redicion Studio/Tools/Prepare Gameplay Items")]
        public static void PrepareGameplayItems()
        {
            string path = "Assets/Horror Multiplayer Template by Redicion Studio/Prefabs/Items/";
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { path });

            List<GameplayItem> gameplayItems = new List<GameplayItem>();

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (prefab != null)
                {
                    GameplayItem item = prefab.GetComponent<GameplayItem>();
                    if (item != null)
                    {
                        gameplayItems.Add(item);
                    }
                }
            }

            GameplayItemList itemList = GetOrCreateItemList();
            itemList.items = gameplayItems.ToArray();
            EditorUtility.SetDirty(itemList);
            AssetDatabase.SaveAssets();
        }
    }
#endif
}