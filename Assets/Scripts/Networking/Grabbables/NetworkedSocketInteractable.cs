using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

[OrderAfter(typeof(NetworkedGrabbable))]
[RequireComponent(typeof(NetworkedGrabbable))]
[RequireComponent(typeof(Collider))]
public class NetworkedSocketInteractable : NetworkBehaviour
{
    [SerializeField] public SocketType socketType;
    [SerializeField] public Vector3 attachOffset;

    private Transform followTransform;
    private NetworkedSocketReceiver receiver;

    private NetworkedGrabbable grabbable;
    private NetworkTransform networkTransform;
    private NetworkRigidbody networkRigidbody;

    public override void Spawned()
    {
        base.Spawned();

        grabbable = GetComponent<NetworkedGrabbable>();
        networkTransform = GetComponent<NetworkTransform>();
        networkRigidbody = GetComponent<NetworkRigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!grabbable.IsGrabbed) return;

        receiver = other.gameObject.GetComponent<NetworkedSocketReceiver>();
        if (receiver == null || !receiver.acceptedSocketTypes.Contains(socketType) || receiver.Occupied) return;
    }

    private void OnTriggerStay(Collider other)
    {
        if (receiver == null || other.gameObject != receiver.gameObject) return;

        if (!grabbable.IsGrabbed && followTransform == null)
        {
            receiver.AttachObject();
            followTransform = receiver.attachTransform; // Switch to flag to avoid storing transform?
            networkRigidbody.Rigidbody.isKinematic = true;          
        }
        else if (grabbable.IsGrabbed && followTransform != null) // Grabbed again
        {
            receiver.DetachObject();
            followTransform = null;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (followTransform == null) return;

        if (networkRigidbody.Rigidbody.isKinematic == false)
            networkRigidbody.Rigidbody.isKinematic = true;

        Follow(transform, followTransform);
    }

    public override void Render()
    {
        if (followTransform == null) return;

        Follow(networkTransform.InterpolationTarget.transform, followTransform);
    }

    void Follow(Transform followingtransform, Transform followedTransform)
    {
        followingtransform.position = followedTransform.position;
        followingtransform.rotation = followedTransform.rotation; // may need to make networked
    }
}

public enum SocketType
{
    Needle
}
