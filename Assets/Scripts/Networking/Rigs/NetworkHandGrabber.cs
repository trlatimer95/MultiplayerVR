using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[OrderAfter(typeof(NetworkRig))]
public class NetworkHandGrabber : NetworkBehaviour
{
    [Networked] NetworkedGrabbable GrabbedObject { get; set; }

    public HandEnum HandSide;

    [HideInInspector] public NetworkRig networkRig;
    [HideInInspector] public NetworkTransform networkTransform;

    private Collider lastCheckedCollider;
    private NetworkedGrabbable lastCheckGrabbable;

    private void Awake()
    {
        networkRig = GetComponentInParent<NetworkRig>();
        networkTransform = GetComponent<NetworkTransform>();
    }

    private void Update()
    {
        if (!networkRig.IsLocalNetworkRig) return;
        if (GrabbedObject == null) return;

        if ((HandSide == HandEnum.LeftHand && !networkRig._leftHandGrabbing) || (HandSide == HandEnum.RightHand && !networkRig._rightHandGrabbing)) 
            Ungrab(GrabbedObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!networkRig.IsLocalNetworkRig) return;
        if (GrabbedObject != null) return;

        NetworkedGrabbable grabbable;

        // Obtain current grabbable
        if (lastCheckedCollider == other)
            grabbable = lastCheckGrabbable;
        else
            grabbable = other.GetComponentInParent<NetworkedGrabbable>();

        lastCheckedCollider = other;
        lastCheckGrabbable = grabbable;

        if (grabbable != null && ((HandSide == HandEnum.LeftHand && networkRig._leftHandGrabbing) || (HandSide == HandEnum.RightHand && networkRig._rightHandGrabbing)))
            Grab(grabbable);             
    }

    public void Grab(NetworkedGrabbable grabbable)
    {
        grabbable.Grab(this);
        GrabbedObject = grabbable;
    }

    public void Ungrab(NetworkedGrabbable grabbable)
    {
        grabbable.Ungrab();
        GrabbedObject = null;
    }
}

public enum HandEnum
{
    LeftHand,
    RightHand,
}
