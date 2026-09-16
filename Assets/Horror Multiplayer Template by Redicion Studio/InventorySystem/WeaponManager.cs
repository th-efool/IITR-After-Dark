// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;

namespace RedicionStudio.InventorySystem
{
    public class WeaponManager : MonoBehaviour
    {
        public Transform Player;

        [Space]
        public bool isAiming = false;
        public float normalSensitivity;
        public float aimSensitivity;
        public Transform CurrentWeaponBulletSpawnPoint;
        public Transform CartridgeEjectEffectSpawnPoint;
        public Vector3 LeftHandPosition;
        public Quaternion LeftHandRotation;
        public GameObject WeaponBulletPrefab;
        public float BulletSpeed;
        public Transform MuzzleFlashEffectPosition;

        [Space]
        [Header("Triggers")]
        public string WeaponIdleTriggerName;
        public string WeaponAimTriggerName;
        public string WeaponShootAnimationName;
        public float WeaponShootAnimationLength = 0.20f;
        public GameObject Crosshair;

        public LayerMask aimColliderLayerMask = new LayerMask();
        public Transform debugTransform;
    }
}
