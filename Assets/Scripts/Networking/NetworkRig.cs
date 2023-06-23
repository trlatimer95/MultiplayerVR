using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRig : NetworkBehaviour
{
    public bool IsLocalNetworkRig => Object.HasStateAuthority;

    [SerializeField] private GameObject _headVisuals;

    [Header("Rig Components")]
    [SerializeField] private NetworkTransform _characterTransform;
    [SerializeField] private NetworkTransform _headTransform;
    [SerializeField] private NetworkTransform _bodyTransform;
    [SerializeField] private NetworkTransform _leftHandTransform;
    [SerializeField] private NetworkTransform _rightHandTransform;

    HardwareRig _hardwareRig;

    public override void Spawned()
    {
        base.Spawned();

        if (IsLocalNetworkRig)
        {
            _hardwareRig = FindObjectOfType<HardwareRig>();
            if (_hardwareRig == null)
            {
                Debug.LogError("Missing Hardware Rig in the scene");
            }

            _headVisuals.SetActive(false);
        }
        else
        {
            Debug.Log("This is a client object");
        }
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (GetInput<XRRigInputData>(out var inputData))
        {
            _characterTransform.transform.SetPositionAndRotation(inputData.CharacterPosition, inputData.CharacterRotation);
            _headTransform.transform.SetPositionAndRotation(inputData.HeadsetPosition, inputData.HeadsetRotation);
            _bodyTransform.transform.SetPositionAndRotation(inputData.BodyPosition, inputData.BodyRotation);
            _leftHandTransform.transform.SetPositionAndRotation(inputData.LeftHandPosition, inputData.LeftHandRotation);
            _rightHandTransform.transform.SetPositionAndRotation(inputData.RightHandPosition, inputData.RightHandRotation);
        }
    }

    public override void Render()
    {
        base.Render();

        if (IsLocalNetworkRig)
        {
            _headTransform.InterpolationTarget.SetPositionAndRotation(_hardwareRig._headTransform.position, _hardwareRig._headTransform.rotation);
            _characterTransform.InterpolationTarget.SetPositionAndRotation(_hardwareRig._characterTransform.position, _hardwareRig._characterTransform.rotation);
            _bodyTransform.InterpolationTarget.SetPositionAndRotation(_hardwareRig._bodyTransform.position, _hardwareRig._bodyTransform.rotation);
            _leftHandTransform.InterpolationTarget.SetPositionAndRotation(_hardwareRig._leftHandTransform.position, _hardwareRig._leftHandTransform.rotation);
            _rightHandTransform.InterpolationTarget.SetPositionAndRotation(_hardwareRig._rightHandTransform.position, _hardwareRig._rightHandTransform.rotation);
        }
    }
}
