using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HardwareRig : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Rig Components")]
    public Transform _characterTransform;
    public Transform _headTransform;
    public Transform _bodyTransform;
    public Transform _leftHandTransform;
    public Transform _rightHandTransform;

    [Header("Controllers")]
    public ActionBasedController _leftHandController;
    public ActionBasedController _rightHandController;

    public float grabThreshold = 0.1f;


    private void Start()
    {
        NetworkManager.Instance.SessionRunner.AddCallbacks(this);
    }

    private void Update()
    {
        _bodyTransform.rotation = Quaternion.Lerp(_bodyTransform.rotation, Quaternion.Euler(new Vector3(0, _headTransform.rotation.eulerAngles.y, 0)), 0.05f);
        _bodyTransform.position = new Vector3(_headTransform.position.x, _bodyTransform.position.y, _headTransform.position.z);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        XRRigInputData inputData = new XRRigInputData();

        inputData.HeadsetPosition = _headTransform.position;
        inputData.HeadsetRotation = _headTransform.rotation;

        inputData.BodyPosition = _bodyTransform.position;
        inputData.BodyRotation = _bodyTransform.rotation;

        inputData.CharacterPosition = _characterTransform.position;
        inputData.CharacterRotation = _characterTransform.rotation;

        inputData.LeftHandPosition = _leftHandTransform.position;
        inputData.LeftHandRotation = _leftHandTransform.rotation;

        inputData.RightHandPosition = _rightHandTransform.position;
        inputData.RightHandRotation = _rightHandTransform.rotation;

        inputData.LeftHandGripValue = _leftHandController.selectAction.action.ReadValue<float>();
        inputData.LeftHandTriggerValue = _leftHandController.activateAction.action.ReadValue<float>();

        inputData.RightHandGripValue = _rightHandController.selectAction.action.ReadValue<float>();
        inputData.RightHandTriggerValue = _rightHandController.activateAction.action.ReadValue<float>();

        inputData.LeftHandGrabbing = inputData.LeftHandGripValue > grabThreshold && inputData.LeftHandTriggerValue > grabThreshold;
        inputData.RightHandGrabbing = inputData.RightHandGripValue > grabThreshold && inputData.RightHandTriggerValue > grabThreshold;

        input.Set(inputData);
    }

    #region Unused Network Callbacks
    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }
    #endregion
}

public struct XRRigInputData : INetworkInput
{
    public Vector3 MainPlayerPosition;
    public Quaternion MainPlayerRotation;

    public Vector3 HeadsetPosition;
    public Quaternion HeadsetRotation;

    public Vector3 BodyPosition;
    public Quaternion BodyRotation;

    public Vector3 CharacterPosition;
    public Quaternion CharacterRotation;

    public Vector3 LeftHandPosition;
    public Quaternion LeftHandRotation;

    public Vector3 RightHandPosition;
    public Quaternion RightHandRotation;

    public float LeftHandGripValue;
    public float LeftHandTriggerValue;

    public float RightHandGripValue;
    public float RightHandTriggerValue;
   
    public bool LeftHandGrabbing;
    public bool RightHandGrabbing;
}
