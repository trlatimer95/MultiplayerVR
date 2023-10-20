using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[OrderAfter(typeof(NetworkedGrabbable))]
public class NetworkedSocketReceiver : NetworkBehaviour
{
    [SerializeField] public List<SocketType> acceptedSocketTypes;
    [SerializeField] public Transform attachTransform;
 
    [Networked] public bool Occupied { get; private set; }

    public void AttachObject() // May expect to include attached object
    {
        Occupied = true;
    }

    public void DetachObject()
    {
        Occupied = false;
    }
}
