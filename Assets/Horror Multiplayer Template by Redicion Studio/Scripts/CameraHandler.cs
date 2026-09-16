// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;

namespace RedicionStudio
{
    public class CameraHandler : MonoBehaviour
    {
        public Transform camTrans;
        public Transform pivot;
        public Transform Character;
        public Transform mainTransform;
        public Transform targetLook;

        public bool leftPivot;
        public float delta;

        public float mouseX;
        public float mouseY;
        public float smoothX;
        public float smoothY;
        public float smoothXVelocity;
        public float smoothYVelocity;
        public float lookAngle;
        public float titlAngle;

        // Distance, height and rotationSpeed settings
        public float cameraDistance = 1.65f;
        public float cameraHeight = 1.31f;
        public float rotationSpeed = 0.4f;

        void Start()
        {
            // Networking
            GameManager.GameEvent_SpectatePlayer += StickCameraToPlayer;

            transform.position = camTrans.position;
            transform.forward = targetLook.forward;
        }

        private void StickCameraToPlayer(GameObject _player)
        {
            Character = _player.transform;
        }

        void Update()
        {
            if (Character && GetComponent<Spectator>().spectateMode)
            {
                Tick();
            }
        }

        void LateUpdate()
        {
            if (Character && GetComponent<Spectator>().spectateMode)
            {
                Tick();
            }
        }

        void FixedUpdate()
        {
            if (Character && GetComponent<Spectator>().spectateMode)
            {
                Tick();
            }
        }

        void Tick()
        {
            delta = Time.deltaTime;

            HandlePosition();
            HandleRotation();

            Vector3 targetPosition = Vector3.Lerp(mainTransform.position, Character.position, 1);
            mainTransform.position = targetPosition;
        }

        void TargetLook()
        {
            Ray ray = new Ray(camTrans.position, camTrans.forward * 2000);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                targetLook.position = Vector3.Lerp(targetLook.position, hit.point, Time.deltaTime * 40);
            }
            else
            {
                targetLook.position = Vector3.Lerp(targetLook.position, targetLook.forward * 200, Time.deltaTime * 5);
            }
        }

        void HandlePosition()
        {
            float targetX = 0;
            float targetY = cameraHeight;
            float targetZ = -cameraDistance;

            if (leftPivot)
            {
                targetX = -targetX;
            }

            Vector3 newPivotPosition = pivot.localPosition;
            newPivotPosition.x = targetX;
            newPivotPosition.y = targetY;

            Vector3 newCameraPosition = camTrans.localPosition;
            newCameraPosition.z = targetZ;

            float t = delta * 5;
            pivot.localPosition = Vector3.Lerp(pivot.localPosition, newPivotPosition, t);
            camTrans.localPosition = Vector3.Lerp(camTrans.localPosition, newCameraPosition, t);
        }

        void HandleRotation()
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");

            smoothX = mouseX;
            smoothY = mouseY;

            lookAngle += smoothX * rotationSpeed;
            Quaternion targetRot = Quaternion.Euler(0, lookAngle, 0);
            mainTransform.rotation = targetRot;

            titlAngle -= smoothY * rotationSpeed;
            titlAngle = Mathf.Clamp(titlAngle, -80, 80);
            pivot.localRotation = Quaternion.Euler(titlAngle, 0, 0);
        }
    }
}