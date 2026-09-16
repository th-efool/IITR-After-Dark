// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class Image_Rotate_To_camera : MonoBehaviour
    {
        public Transform Cam;

        public void Start()
        {
            if (Cam == null)
                Cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }

        void Update()
        {
            if (Cam == null)
            {
                transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(-Cam.GetComponent<Camera>().transform.forward, Cam.GetComponent<Camera>().transform.up);
            }
        }
    }
}
