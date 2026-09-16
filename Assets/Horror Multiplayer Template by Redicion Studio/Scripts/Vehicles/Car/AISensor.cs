// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class AISensor : MonoBehaviour
    {
        public CarAI carAI;
        bool canHorn = true;

        private void OnTriggerStay(Collider other)
        {
            foreach (string tag in carAI.ObstaclesTags)
            {
                if (other.tag == tag & !carAI.ignoreObstacles)
                {
                    carAI.Brake(true);
                    if (canHorn == true & transform.parent.GetComponent<CarController>().isControlledByCarAi == true)
                    {
                        canHorn = false;
                        StartCoroutine(ReactivateHorning());
                        Horn(transform.parent.GetComponent<CarController>().CarHornID, transform.position, transform.rotation, gameObject.name);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            foreach (string tag in carAI.ObstaclesTags)
            {
                if (other.tag == tag & !carAI.ignoreObstacles)
                {
                    carAI.Brake(false);
                }
            }
        }

        void Horn(int _HornID, Vector3 _position, Quaternion _rotation, string _carServerObjectName)
        {
            GameObject _CarHornSoundPrefab = Instantiate(transform.parent.GetComponent<CarController>().CarHornSoundPrefab, _position, _rotation) as GameObject;

            CarHorn _CarHornSoundPrefabScript = _CarHornSoundPrefab.GetComponent<CarHorn>();

            _CarHornSoundPrefabScript.PlayCarHorn(_HornID, _position, _carServerObjectName);
        }

        IEnumerator ReactivateHorning()
        {
            yield return new WaitForSeconds(10);

            canHorn = true;
        }
    }
}