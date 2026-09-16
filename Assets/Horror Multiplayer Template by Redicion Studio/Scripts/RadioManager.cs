using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class RadioManager : NetworkBehaviour
    {
        [Header("Audio Settings")]
        public List<AudioClip> radioStations;  // List of audio clips for radio stations
        public AudioClip radioNoiseClip;       // Audio clip for radio noise
        public AudioSource audioSource;

        private int currentStationIndex = 0;   // Current station index
        private bool isSwitchingStations = false; // Indicates if switching stations is in progress
        [HideInInspector] public bool isUsed = false;

        [SyncVar(hook = nameof(OnStationChanged))]
        private int syncedStationIndex;

        [SyncVar(hook = nameof(OnRadioStateChanged))]
        private bool isRadioOn = false;

        public GameObject radioCamera;

        [HideInInspector] public bool isRadioDisabled = false;

        void Start()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (isRadioOn && !RoomManager._instance.MatchRunning)
            {
                isRadioOn = false;
            }
        }

        // Change station
        public void ChangeStation(int change)
        {
            if (isRadioOn && !isSwitchingStations)
            {
                // Stop current station sound
                audioSource.Stop();

                // Play the radio noise first
                audioSource.clip = radioNoiseClip;
                audioSource.Play();
                RpcPlayChangeStationSound();

                // Set switching flag to true
                isSwitchingStations = true;

                // Wait until the noise finishes to switch the station
                //Invoke(nameof(CompleteStationSwitch), radioNoiseClip.length);
                StartCoroutine(ChangeStationCoroutine(change, radioNoiseClip.length));
            }
        }

        [ClientRpc]
        void RpcPlayChangeStationSound()
        {
            // Stop current station sound
            audioSource.Stop();

            // Play the radio noise first
            audioSource.clip = radioNoiseClip;
            audioSource.Play();
        }

        IEnumerator ChangeStationCoroutine(int change, float audioClipLength)
        {
            yield return new WaitForSeconds(audioClipLength);

            // Calculate new station index
            currentStationIndex += change;

            if (currentStationIndex >= radioStations.Count)
                currentStationIndex = 0;
            else if (currentStationIndex < 0)
                currentStationIndex = radioStations.Count - 1;

            syncedStationIndex = currentStationIndex;

            CompleteStationSwitch();
        }

        // Method called when the radio noise finishes
        void CompleteStationSwitch()
        {
            // Reset switching flag
            isSwitchingStations = false;

            // Play the new station audio
            UpdateRadioStation();
        }

        // Turn the radio on/off
        public void ToggleRadio()
        {
            if (!isRadioOn)
            {
                isSwitchingStations = true;
                RpcPlayChangeStationSound();
                StartCoroutine(RadioTurnOnCoroutine(radioNoiseClip.length));
            }
            else
            {
                isRadioOn = false;
            }
        }

        IEnumerator RadioTurnOnCoroutine(float audioClipLength)
        {
            yield return new WaitForSeconds(audioClipLength);

            isRadioOn = true;
            RpcTurnOnRadio();
            isSwitchingStations = false;
        }

        [ClientRpc]
        void RpcTurnOnRadio()
        {
            if (radioStations.Count > 0 && isRadioOn)
            {
                audioSource.clip = radioStations[syncedStationIndex];
                audioSource.Play();
            }
        }

        // Hook method called when syncedStationIndex changes
        void OnStationChanged(int oldIndex, int newIndex)
        {
            // Only update the station if we're not in the middle of switching
            if (!isSwitchingStations)
            {
                UpdateRadioStation();
            }
        }

        // Hook method called when radio on/off state changes
        void OnRadioStateChanged(bool oldState, bool newState)
        {
            if (!newState)
            {
                audioSource.Stop();
            }
            else
            {
                UpdateRadioStation();
            }
        }

        void UpdateRadioStation()
        {
            if (radioStations.Count > 0 && isRadioOn)
            {
                audioSource.clip = radioStations[syncedStationIndex];
                audioSource.Play();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<PlayerInteractionModule>().currentRadio = GetComponent<NetworkIdentity>();

                RpcSetRadio(other.GetComponent<NetworkIdentity>());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<PlayerInteractionModule>().currentRadio = null;

                RpcRemoveRadio(other.GetComponent<NetworkIdentity>());
            }
        }

        [ClientRpc]
        void RpcSetRadio(NetworkIdentity playerNetId)
        {
            playerNetId.GetComponent<PlayerInteractionModule>().currentRadio = GetComponent<NetworkIdentity>();
        }

        [ClientRpc]
        void RpcRemoveRadio(NetworkIdentity playerNetId)
        {
            playerNetId.GetComponent<PlayerInteractionModule>().currentRadio = null;
        }

        public void DisableRadio(bool disable)
        {
            if (isRadioOn)
            {
                if (disable)
                {
                    isRadioDisabled = true;
                    AudioClip previousClip = audioSource.clip;

                    // Stop current station sound
                    audioSource.Stop();

                    // Play the radio noise
                    audioSource.clip = radioNoiseClip;
                    audioSource.Play();
                }
                else
                {
                    isRadioDisabled = false;
                    AudioClip previousClip = audioSource.clip;

                    // Stop noise sound
                    audioSource.Stop();

                    // Play the radio station sound
                    audioSource.clip = previousClip;
                    audioSource.Play();
                }
            }
        }
    }
}