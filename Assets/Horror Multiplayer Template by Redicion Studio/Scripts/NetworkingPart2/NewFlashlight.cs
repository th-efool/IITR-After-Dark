// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class NewFlashlight : GameplayItem
    {
        [Header("FlashLight")]
        public Light lightComponent;
        public float updateInterval = 0.1f;
        public AudioClip turnOnSound;
        public AudioClip turnOffSound;
        public MeshRenderer flashLightGlass;
        public Color flashLightGlassEmissionColor = Color.white;
        public float flashLightGlassEmissionStrength = 1.0f;

        public override void Use()
        {
            base.Use();

            Material material = flashLightGlass.material;
            Renderer renderer = flashLightGlass.GetComponent<Renderer>();

            if (!lightComponent.enabled)
            {
                AudioSource.PlayClipAtPoint(turnOnSound, transform.position);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", flashLightGlassEmissionColor * flashLightGlassEmissionStrength);
                DynamicGI.SetEmissive(renderer, flashLightGlassEmissionColor * flashLightGlassEmissionStrength);
            }
            else
            {
                AudioSource.PlayClipAtPoint(turnOffSound, transform.position);
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
                DynamicGI.SetEmissive(renderer, Color.black);
            }

            lightComponent.enabled = !lightComponent.enabled;
        }

        public override void Putdown()
        {
            base.Putdown();
            isAiming = false;
            lightComponent.enabled = false;
        }

        private void Start()
        {
            StartCoroutine(c_Update());
        }

        private IEnumerator c_Update()
        {
            while (true)
            {
                M_Update();

                yield return new WaitForSeconds(updateInterval);
            }
        }

        private void M_Update()
        {
            if (hasAuthority && IsOwned && _myOwner.GetComponent<NetworkIdentity>().netId == NetworkClient.localPlayer.gameObject.GetComponent<NetworkIdentity>().netId && !_myOwner.GetComponent<HunterAbilities>()._inFight && _myOwner.gameObject.GetComponent<HunterAbilities>()._canUseItems && !_myOwner.gameObject.GetComponent<HunterAbilities>().isBlocked && !_myOwner.gameObject.GetComponent<CharacterManager>().isHealing && !_myOwner.gameObject.GetComponent<PlayerInteractionModule>().isClimbing && !_myOwner.gameObject.GetComponent<PlayerInteractionModule>().isUsingRadio)
            {
                if (_input.aim)
                {
                    if (!isAiming)
                    {
                        isAiming = true;
                        CmdSetAimStatus(true);
                    }
                }
                else
                {
                    if (isAiming)
                    {
                        isAiming = false;
                        CmdSetAimStatus(false);
                    }
                }
            }
        }

        [Command]
        private void CmdSetAimStatus(bool value)
        {
            if (IsOwned && _myOwner != null && _myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                isAiming = value;
                if (value)
                {
                    if (!_myOwner.GetComponent<ManageTPController>().isFirstPerson)
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.aimAnimatorTriggerName);
                    else
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonAimAnimatorTriggerName);
                }
                else
                {
                    if (!_myOwner.GetComponent<ManageTPController>().isFirstPerson)
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                    else
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
                }
                RpcUpdateAimStatus(value);
            }
        }

        [ClientRpc]
        private void RpcUpdateAimStatus(bool value)
        {
            if (IsOwned && _myOwner != null && _myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                if (value)
                {
                    if (!_myOwner.GetComponent<ManageTPController>().isFirstPerson)
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.aimAnimatorTriggerName);
                    else
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonAimAnimatorTriggerName);
                }
                else
                {
                    if (!_myOwner.GetComponent<ManageTPController>().isFirstPerson)
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                    else
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
                }
                isAiming = value;
            }
        }
    }
}