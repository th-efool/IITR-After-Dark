// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class UIElementFaceCamera : MonoBehaviour
    {
        public Transform uiElement;
        private static Transform _camera;

        void Update()
        {
            if (_camera == null)
                _camera = GameObject.Find("MainCamera").transform;
            else
            {
                //Face camera
                uiElement.LookAt(uiElement.position + _camera.rotation * Vector3.forward,
            _camera.rotation * Vector3.up);
            }
        }
    }
}
