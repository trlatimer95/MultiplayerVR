using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[OrderAfter(typeof(NetworkedGrabbable))]
public class NetworkedSocketReceiver : NetworkBehaviour
{
    [SerializeField] public List<SocketType> acceptedSocketTypes;
    [SerializeField] public Transform attachTransform;

    // Add event when attach changed, subscribe method in NetworkedSyringe

    public NetworkedSocketInteractable currentAttachedInteractable;
 
    [Networked] public bool Occupied { get; private set; }

    public void AttachObject(NetworkedSocketInteractable attachedInteractable) // May expect to include attached object
    {
        Occupied = true;
        currentAttachedInteractable = attachedInteractable;
    }

    public void DetachObject()
    {
        Occupied = false;
        currentAttachedInteractable = null;
    }
}
