using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreColliders : MonoBehaviour
{
    public List<Collider> parentColliders;
    public List<GameObject> gameObjectsToIgnore;

    private void Start()
    {
        foreach (Collider collider in parentColliders)
        {
            foreach (GameObject obj in gameObjectsToIgnore)
            {
                Collider[] colliders = obj.GetComponents<Collider>();
                foreach (Collider c in colliders)
                {
                    Physics.IgnoreCollision(collider, c);
                }
            }
        }
    }
}
