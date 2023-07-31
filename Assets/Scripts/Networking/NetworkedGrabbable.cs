using Fusion;
using Fusion.XR.Shared;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.UI;

[OrderAfter(typeof(NetworkHandGrabber))]
public class NetworkedGrabbable : NetworkBehaviour
{
    [HideInInspector]
    public NetworkTransform networkTransform;
    public NetworkRigidbody networkRigidbody;

    [Networked(OnChanged = nameof(OnGrabberChanged))]
    public NetworkHandGrabber CurrentGrabber { get; set; }

    [Networked] private Vector3 LocalPositionOffset { get; set; }
    [Networked] private Quaternion LocalRotationOffset { get; set; }

    public bool IsGrabbed => CurrentGrabber != null;
    public bool expectedIsKinematic = true;

    [Header("Advanced Options")]
    public bool extrapolateWhileTakingAuthority = true;
    public bool isTakingAuthority = false;

    private Vector3 localPositionOffsetWhileTakingAuthority;
    private Quaternion localRotationOffsetWhileTakingAuthority;
    private NetworkHandGrabber grabberWhileTakingAuthority;

    private void Awake()
    {
        networkTransform = GetComponent<NetworkTransform>();
        networkRigidbody = GetComponent<NetworkRigidbody>();
    }

    public override void Spawned()
    {
        base.Spawned();

        if (networkRigidbody)
            expectedIsKinematic = (networkRigidbody.ReadNetworkRigidbodyFlags() & NetworkRigidbodyBase.NetworkRigidbodyFlags.IsKinematic) != 0;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (!IsGrabbed) return;

        Follow(followingtransform: transform, followedTransform: CurrentGrabber.transform, LocalPositionOffset, LocalRotationOffset);
    }

    public override void Render()
    {
        if (isTakingAuthority && extrapolateWhileTakingAuthority)
        {
            ExtrapolateWhileTakingAuthority();
            return;
        }

        if (!IsGrabbed) return;

        // Extrapolation: Make visual representation follow grabber, adding position/rotation offsets
        // We extrapolate for all users: we know that the grabbed object should follow accuratly the grabber, even if the network position might be a bit out of sync
        Follow(followingtransform: networkTransform.InterpolationTarget.transform, followedTransform: CurrentGrabber.networkTransform.InterpolationTarget.transform, LocalPositionOffset, LocalRotationOffset);
    }

    public static void OnGrabberChanged(Changed<NetworkedGrabbable> changed)
    {
        // Load previous state to find previous grabber
        changed.LoadOld();
        NetworkHandGrabber previousGrabber = null;
        if (changed.Behaviour.CurrentGrabber != null)
            previousGrabber = changed.Behaviour.CurrentGrabber;

        // Reload current state to see current grabber
        changed.LoadNew();

        if (previousGrabber)
            changed.Behaviour.DidUngrab();
        if (changed.Behaviour.CurrentGrabber)
            changed.Behaviour.DidGrab();
    }

    public void Ungrab()
    {
        CurrentGrabber = null;
    }

    public async void Grab(NetworkHandGrabber newGrabber)
    {
        // Find grabbable position/rotation in grabber referential
        localPositionOffsetWhileTakingAuthority = newGrabber.transform.InverseTransformPoint(transform.position);
        localRotationOffsetWhileTakingAuthority = Quaternion.Inverse(newGrabber.transform.rotation) * transform.rotation;
        grabberWhileTakingAuthority = newGrabber;

        // Ask and wait to receive StateAuthority to move object
        isTakingAuthority = true;
        await Object.WaitForStateAuthority();      
        isTakingAuthority = false;

        // Obtained authority, set network variables
        LocalPositionOffset = localPositionOffsetWhileTakingAuthority;
        LocalRotationOffset = localRotationOffsetWhileTakingAuthority;

        // Start following position in FixedUpdateNetwork
        CurrentGrabber = grabberWhileTakingAuthority;
    }

    void DidGrab()
    {
        if (networkRigidbody) networkRigidbody.Rigidbody.isKinematic = true;
    }

    void DidUngrab()
    {
        if (networkRigidbody) networkRigidbody.Rigidbody.isKinematic = expectedIsKinematic; // TODO May need to add velocity?
    }

    void ExtrapolateWhileTakingAuthority()
    {
        if (grabberWhileTakingAuthority == null) return;

        // Extrapolation: Make visual representation follow grabber, adding position/rotation offsets
        // We use grabberWhileTakingAuthority instead of CurrentGrabber as we are currently waiting for the authority transfer: the network vars are not already set, so we use the temporary versions
        Follow(followingtransform: networkTransform.InterpolationTarget.transform, followedTransform: grabberWhileTakingAuthority.networkTransform.InterpolationTarget.transform, localPositionOffsetWhileTakingAuthority, localRotationOffsetWhileTakingAuthority);
    }

    void Follow(Transform followingtransform, Transform followedTransform, Vector3 localPositionOffsetToFollowed, Quaternion localRotationOffsetTofollowed)
    {
        followingtransform.position = followedTransform.TransformPoint(localPositionOffsetToFollowed);
        followingtransform.rotation = followedTransform.rotation * localRotationOffsetTofollowed;
    }
}
