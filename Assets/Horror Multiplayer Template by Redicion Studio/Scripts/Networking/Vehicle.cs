// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class Vehicle : NetworkBehaviour
    {
        public bool canDrive = false;
        [SyncVar] public bool isControlledByCarAi = false;
    }
}
