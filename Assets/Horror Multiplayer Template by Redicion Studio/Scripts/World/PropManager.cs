// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class PropManager : MonoBehaviour
    {
        [Header("Dependencies")]
        public RoomManager roomManager;

        [Header("Police Car Prop")]
        public GameObject policeCarProp;

        void Update()
        {
            if (roomManager.activatePoliceCar)
            {
                if(!policeCarProp.activeInHierarchy)
                {
                    foreach (MatchMap matchMap in roomManager.matchMaps)
                    {
                        if (roomManager.currentSelectedMatchMapId == matchMap.mapId)
                        {
                            policeCarProp.transform.position = matchMap.policeCarPosition;
                            policeCarProp.transform.rotation = matchMap.policeCarRotation;
                        }
                    }
                    policeCarProp.SetActive(true);
                }
            }
            else
            {
                if (policeCarProp.activeInHierarchy)
                    policeCarProp.SetActive(false);
            }
        }
    }
}
