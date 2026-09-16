// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class StreetLightManager : MonoBehaviour
    {
        public Light streetLight;
        public MeshRenderer streetLightMesh;

        void Update()
        {
            if (streetLight.enabled == true)
            {
                if (!streetLightMesh.enabled)
                    streetLightMesh.enabled = true;
            }
            else
            {
                if (streetLightMesh.enabled)
                    streetLightMesh.enabled = false;
            }
        }
    }
}
