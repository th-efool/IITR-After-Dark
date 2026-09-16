// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class PreviewModelManager : MonoBehaviour
    {
        RoomManager roomManager;

        void Start()
        {
            roomManager = null;
            GameObject roomManagerObject = GameObject.FindGameObjectWithTag("RoomManager");

            if (roomManagerObject != null)
            {
                roomManager = roomManagerObject.GetComponent<RoomManager>();
            }
        }

        void Update()
        {
            if (roomManager != null && roomManager.GetComponent<RoomManager>().MatchRunning)
            {
                Destroy(gameObject);
            }
            else if (roomManager == null)
            {
                GameObject roomManagerObject = GameObject.FindGameObjectWithTag("RoomManager");

                if (roomManagerObject != null)
                {
                    roomManager = roomManagerObject.GetComponent<RoomManager>();
                }
            }
        }
    }
}