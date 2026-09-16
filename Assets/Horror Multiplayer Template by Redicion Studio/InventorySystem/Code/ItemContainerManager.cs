// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class ItemContainerManager : NetworkBehaviour
    {
        public ItemContainerManagerItem[] items;
        public int maximumNumberOfItemsToBeSpawned = 3;
        public Collider triggerCollider;
        public Animator animator;
        public string openAnimatorTriggerName;
        public GameObject interactionIndicator;

        public Transform[] spawnPositions;

        [SyncVar] public bool isUsable = true;

        [HideInInspector] public List<Transform> availableSpawnPositions;
        private List<ItemContainerManagerItem> availableItems;

        [HideInInspector] public List<GameObject> instantiatedItems;

        void Start()
        {
            if (triggerCollider == null)
            {
                Debug.LogError("Trigger Collider not assigned to ItemContainerManager. Please assign it in the inspector.");
            }

            availableSpawnPositions = new List<Transform>(spawnPositions);
            availableItems = new List<ItemContainerManagerItem>(items);
        }

        public void SpawnItems()
        {
            isUsable = false;
            animator.SetTrigger(openAnimatorTriggerName);
            RpcSetUsedState();

            int itemsToSpawn = Mathf.Min(maximumNumberOfItemsToBeSpawned, Mathf.Min(availableSpawnPositions.Count, availableItems.Count));

            for (int i = 0; i < itemsToSpawn; i++)
            {
                int randomPositionIndex = Random.Range(0, availableSpawnPositions.Count);
                int randomItemIndex = Random.Range(0, availableItems.Count);

                Transform spawnPosition = availableSpawnPositions[randomPositionIndex];
                ItemContainerManagerItem selectedItem = availableItems[randomItemIndex];

                GameObject spawnedItem = Instantiate(selectedItem.itemPrefab, spawnPosition.position, Quaternion.identity);
                NetworkServer.Spawn(spawnedItem);

                instantiatedItems.Add(spawnedItem);

                spawnedItem.GetComponent<GameplayItem>().followItemContainerPosition = true;
                spawnedItem.GetComponent<GameplayItem>().itemContainer = GetComponent<NetworkIdentity>();
                spawnedItem.GetComponent<GameplayItem>().itemContainerPositionIndex = randomPositionIndex;

                availableSpawnPositions.RemoveAt(randomPositionIndex);
                availableItems.RemoveAt(randomItemIndex);
            }
        }

        [ClientRpc]
        void RpcSetUsedState()
        {
            isUsable = false;
            animator.SetTrigger(openAnimatorTriggerName);
        }

        [ClientRpc]
        void RpcSetPlayerCurrentItemContainer(NetworkIdentity player, bool remove)
        {
            if (!remove)
                player.GetComponent<PlayerInteractionModule>().currentItemContainer = GetComponent<NetworkIdentity>();
            else
                player.GetComponent<PlayerInteractionModule>().currentItemContainer = null;
        }

        void OnTriggerEnter(Collider other)
        {
            if (isServer && isUsable && other.GetComponent<PlayerInteractionModule>() != null)
            {
                if (other.GetComponent<PlayerInteractionModule>().currentItemContainer == null)
                {
                    other.GetComponent<PlayerInteractionModule>().currentItemContainer = GetComponent<NetworkIdentity>();
                    RpcSetPlayerCurrentItemContainer(other.GetComponent<NetworkIdentity>(), false);
                }
                else if (other.GetComponent<PlayerInteractionModule>().currentItemContainer.netId != GetComponent<NetworkIdentity>().netId)
                {
                    other.GetComponent<PlayerInteractionModule>().currentItemContainer = GetComponent<NetworkIdentity>();
                    RpcSetPlayerCurrentItemContainer(other.GetComponent<NetworkIdentity>(), false);
                }
            }
            if (!isServer)
            {
                if (isUsable)
                {
                    if (other.GetComponent<HunterAbilities>() != null && other.GetComponent<HunterAbilities>()._isHunter)
                        interactionIndicator.SetActive(false);
                    else
                        interactionIndicator.SetActive(true);
                }
                else
                    interactionIndicator.SetActive(false);
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (isServer && other.GetComponent<PlayerInteractionModule>() != null)
            {
                if (other.GetComponent<PlayerInteractionModule>().currentItemContainer != null)
                {
                    other.GetComponent<PlayerInteractionModule>().currentItemContainer = null;
                    RpcSetPlayerCurrentItemContainer(other.GetComponent<NetworkIdentity>(), true);
                }
            }
            if (!isServer)
            {
                interactionIndicator.SetActive(false);
            }
        }
    }

    [System.Serializable]
    public class ItemContainerManagerItem
    {
        public GameObject itemPrefab;
    }
}