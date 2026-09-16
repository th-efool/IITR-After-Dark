// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class EscapeManager : NetworkBehaviour
    {
        public RoomManager roomManager;

        public GameObject escapeCamera;
        public GameObject spectatorCameraTarget;
        public Animator escapeCameraAnimator;
        public float escapeCameraAnimationLength = 10f;

        public Transform escapeDestination;

        public Spectator spectator;

        GameObject localPlayer;

        Coroutine c_spectatePlayer;

        Coroutine c_BlockPlayerMovement;

        Coroutine c_EndMatch;

        Coroutine c_ShowMatchStatistics;

        public GameObject debugMesh; //only for debug

        bool atLeastOnePlayerisAlive;

        private void Start()
        {
            if (!isServer)
                localPlayer = NetworkClient.localPlayer.gameObject;
        }

        private void Update()
        {
            if (!isServer)
            {
                if (localPlayer == null)
                    localPlayer = NetworkClient.localPlayer.gameObject;

                if (localPlayer.GetComponent<CharacterManager>().escaped && roomManager != null && roomManager.MatchRunning)
                {
                    if (!escapeCamera.activeInHierarchy)
                    {
                        debugMesh.SetActive(false); //only for debug
                        escapeCamera.SetActive(true);
                        escapeCameraAnimator.Rebind();
                        c_spectatePlayer = StartCoroutine(SpectatePlayer());
                        localPlayer.GetComponent<CharacterManager>().LerpCharacterPosition(escapeDestination.position, 0.3f);
                    }
                }
                else
                {
                    escapeCamera.SetActive(false);
                    debugMesh.SetActive(true); //only for debug
                }
            }
        }

        IEnumerator SpectatePlayer()
        {
            yield return new WaitForSeconds(escapeCameraAnimationLength);

            spectatorCameraTarget.transform.position = escapeCamera.transform.position;
            spectator.spectateMode = true;
            //spectate our player controller
            GameManager.GameEvent_SpectatePlayer(spectatorCameraTarget);
        }

        private void OnTriggerStay(Collider obj)
        {
            if (!isServer)
                return;

            if (obj.GetComponent<CharacterManager>() != null)
            {
                if (!obj.GetComponent<CharacterManager>().escaped && !obj.GetComponent<HunterAbilities>()._isHunter)
                {
                    obj.GetComponent<CharacterManager>().Rpc_AllowOnlyPlayerMovementInput(false);
                    obj.GetComponent<CharacterManager>().escaped = true;
                    obj.GetComponent<Player>().timeSurvived += roomManager.playTime;
                    obj.GetComponent<CharacterManager>().TempTimeSurvived += roomManager.playTime;
                    obj.GetComponent<Player>().escaped += 1;
                    obj.GetComponent<CharacterManager>().TempEscaped += 1;
                    RpcEscape(obj.GetComponent<NetworkIdentity>());
                    c_BlockPlayerMovement = StartCoroutine(BlockPlayerMovement(obj.GetComponent<NetworkIdentity>()));
                    if (!obj.GetComponent<CharacterManager>().isSheriff)
                    {
                        foreach (CharacterManager player in roomManager.players)
                        {
                            if (!player.GetComponent<HunterAbilities>()._isHunter && player.health > 0 && !player.escaped)
                            {
                                //a survivor is alive and not escaped
                                atLeastOnePlayerisAlive = true;
                            }
                            else
                            {
                                if (!atLeastOnePlayerisAlive)
                                    c_EndMatch = StartCoroutine(EndMatch());
                            }
                        }
                        if (atLeastOnePlayerisAlive)
                            atLeastOnePlayerisAlive = false;
                    }
                    RpcUpdatePlayerListUI(obj.GetComponent<NetworkIdentity>());
                }
            }
        }

        IEnumerator EndMatch()
        {
            yield return new WaitForSeconds(escapeCameraAnimationLength);

            if (!roomManager.MatchEnding)
                roomManager.EndMatch(0);
        }

        [ClientRpc]
        void RpcEscape(NetworkIdentity networkID)
        {
            networkID.GetComponent<CharacterManager>().escaped = true;
            networkID.GetComponent<Player>().SetTimeSurvivedValue(roomManager.playTime);
            networkID.GetComponent<Player>().SetEscapedValue(1);
            //c_ShowMatchStatistics = StartCoroutine(ShowMatchStatistics(networkID));
        }

        IEnumerator ShowMatchStatistics(NetworkIdentity networkID)
        {
            yield return new WaitForSeconds(3);

            networkID.GetComponent<CharacterManager>().ShowMatchStatistics(networkID.GetComponent<CharacterManager>().TempTimeSurvived, networkID.GetComponent<CharacterManager>().TempDamageDealt, networkID.GetComponent<CharacterManager>().TempCompletedTasks, networkID.GetComponent<CharacterManager>().TempHelpedPlayers, networkID.GetComponent<CharacterManager>().TempInstrumentsUsed, networkID.GetComponent<CharacterManager>().TempKilledPlayers, false, true);
        }

        IEnumerator BlockPlayerMovement(NetworkIdentity networkID)
        {
            yield return new WaitForSeconds(escapeCameraAnimationLength);

            networkID.GetComponent<CharacterManager>().Rpc_SetMovementPermission(false);
        }

        [ClientRpc]
        void RpcUpdatePlayerListUI(NetworkIdentity playerNetId)
        {
            UpdatePlayerListUI(playerNetId);
        }

        void UpdatePlayerListUI(NetworkIdentity playerNetId)
        {
            UIPlayerListItem uiPlayerListItem = FindUIPlayerListItemForPlayer(playerNetId.GetComponent<Player>().username);
            if (uiPlayerListItem != null)
            {
                uiPlayerListItem.escapedUiElement.SetActive(true);
            }
        }

        UIPlayerListItem FindUIPlayerListItemForPlayer(string username)
        {
            foreach (GameObject entry in RoomManager._instance.playerListItems)
            {
                UIPlayerListItem uiPlayerListItem = entry.GetComponent<UIPlayerListItem>();
                if (uiPlayerListItem.playerNameText.text == username)
                {
                    return uiPlayerListItem;
                }
            }
            return null;
        }
    }
}