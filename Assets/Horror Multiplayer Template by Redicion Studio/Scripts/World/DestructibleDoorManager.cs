using System.Collections;
using System.Collections.Generic;
using RedicionStudio;
using UnityEngine;

public class DestructibleDoorManager : MonoBehaviour
{
    public float explosionForce = 10f;
    public float explosionRadius = 5f;

    private void Start()
    {
        DestroyDoor();
    }

    public void DestroyDoor()
    {
        ApplyExplosionForce(gameObject);
    }

    private void ApplyExplosionForce(GameObject destructibleDoor)
    {
        Rigidbody[] rigidbodies = destructibleDoor.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rigidbodies)
        {
            Vector3 direction = rb.transform.position - transform.position;
            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }
    }

    private void Update()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            return;

        if (RoomManager._instance != null && RoomManager._instance.MatchEnding)
        {
            Destroy(gameObject);
        }
    }
}
