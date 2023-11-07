using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;

[OrderAfter(typeof(NetworkedGrabbable))]
[RequireComponent(typeof(NetworkedGrabbable))]
[RequireComponent(typeof(Collider))]
public class NetworkedSocketInteractable : NetworkBehaviour
{
    [SerializeField] public SocketType socketType;

    public UnityEvent OnSocketedChanged; 

    public Transform followTransform;

    private NetworkedSocketReceiver receiver;

    private NetworkedGrabbable grabbable;
    private NetworkTransform networkTransform;
    private NetworkRigidbody networkRigidbody;

    public override void Spawned()
    {
        base.Spawned();

        if (OnSocketedChanged == null)
            OnSocketedChanged = new UnityEvent();

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
        if (receiver == null || other.gameObject != receiver.gameObject || !receiver.acceptedSocketTypes.Contains(socketType)) return;

        if (!grabbable.IsGrabbed && followTransform == null) // Released within trigger
        {
            receiver.AttachObject(this);
            followTransform = receiver.attachTransform;
            networkRigidbody.Rigidbody.isKinematic = true;
            OnSocketedChanged.Invoke();
        }
        else if (grabbable.IsGrabbed && followTransform != null) // Grabbed again
        {
            receiver.DetachObject();
            followTransform = null;
            OnSocketedChanged.Invoke();
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
        followingtransform.rotation = followedTransform.rotation;
    }
}

public enum SocketType
{
    NeedleSmall,
    NeedleLarge,
    Vial
}
