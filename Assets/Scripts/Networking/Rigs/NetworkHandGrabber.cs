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

        if (GrabbedObject != null)
        {
            if (HandSide == HandEnum.LeftHand && !networkRig._leftHandGrabbing) 
            {
                Debug.Log("Starting Ungrab with LeftHand");
                Ungrab(GrabbedObject);
            }  
            else if (HandSide == HandEnum.RightHand && !networkRig._rightHandGrabbing) Ungrab(GrabbedObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("TriggerStay entered");

        if (!networkRig.IsLocalNetworkRig) return;
        if (GrabbedObject != null) return;

        Debug.Log("TriggerStay continued");

        NetworkedGrabbable grabbable;

        if (lastCheckedCollider == other)
            grabbable = lastCheckGrabbable;
        else
            grabbable = other.GetComponentInParent<NetworkedGrabbable>();

        lastCheckedCollider = other;
        lastCheckGrabbable = grabbable;

        Debug.Log(other.name);

        if (grabbable != null)
        {
            Debug.Log("Grabbable found");
            if (HandSide == HandEnum.LeftHand && networkRig._leftHandGrabbing)
            {
                Debug.Log("Starting Grab with LeftHand");
                Grab(grabbable);
            }               
            else if (HandSide == HandEnum.RightHand && networkRig._rightHandGrabbing)
            {
                Debug.Log("Starting Grab with RightHand");
                Grab(grabbable);
            }                
        }            
    }

    public void Grab(NetworkedGrabbable grabbable)
    {
        Debug.Log($"Try to grab object {grabbable.gameObject.name} with {gameObject.name}");
        grabbable.Grab(this);
        GrabbedObject = grabbable;
    }

    public void Ungrab(NetworkedGrabbable grabbable)
    {
        Debug.Log($"Try to ungrab object {grabbable.gameObject.name} with {gameObject.name}");
        GrabbedObject.Ungrab();
        GrabbedObject = null;
    }
}

public enum HandEnum
{
    LeftHand,
    RightHand,
}
