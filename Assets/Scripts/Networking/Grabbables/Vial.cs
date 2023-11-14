using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vial : NetworkBehaviour
{
    [SerializeField] public Material fluidMaterial;

    private NetworkedSocketInteractable socket;
    private Transform currentOverrideTransform;

    public override void Spawned()
    {
        socket = GetComponent<NetworkedSocketInteractable>();
        if (socket == null)
            Debug.Log("Unable to find socket interactable on Vial");
    }

    public override void FixedUpdateNetwork()
    {
        if (socket == null || socket.followTransform == null) return;

        if (currentOverrideTransform != null && socket.followTransform == null)
            currentOverrideTransform = null;

        if (currentOverrideTransform == null || (currentOverrideTransform != null && currentOverrideTransform != socket.followTransform))
        {
            currentOverrideTransform = socket.followTransform.GetComponent<NetworkedSocketInteractable>()?.followTransform;
            socket.followTransform = currentOverrideTransform;
        }           
    }
}
