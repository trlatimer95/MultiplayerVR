using Fusion;
using UnityEngine;

public class NetworkRig : NetworkBehaviour
{
    public bool IsLocalNetworkRig => Object.HasStateAuthority;

    [SerializeField] private GameObject _headVisuals;
    [SerializeField] private GameObject _bodyVisuals;

    [Header("Rig Components")]
    [SerializeField] private NetworkTransform _characterTransform;
    [SerializeField] private NetworkTransform _headTransform;
    [SerializeField] private NetworkTransform _bodyTransform;
    [SerializeField] private NetworkTransform _leftHandTransform;
    [SerializeField] private NetworkTransform _rightHandTransform;

    [Header("Animators")]
    [SerializeField] private Animator _leftHandAnimator;
    [SerializeField] private Animator _rightHandAnimator;

    private float _leftHandGripTarget;
    private float _leftHandGripCurrent;
    private float _leftHandTriggerTarget;
    private float _leftHandTriggerCurrent;
    public bool _leftHandGrabbing;
    private float _rightHandGripTarget;
    private float _rightHandGripCurrent;
    private float _rightHandTriggerTarget;
    private float _rightHandTriggerCurrent;
    public bool _rightHandGrabbing;
    
    private string animatorGripParam = "Grip";
    private string animatorTriggerParam = "Trigger";

    HardwareRig _hardwareRig;

    public float CurrentLeftHandTriggerValue
    {
        get { return _leftHandTriggerCurrent; }
    }

    public float CurrentRightHandTriggerValue
    {
        get { return _rightHandTriggerCurrent; }
    }

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
            _bodyVisuals.SetActive(false);
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

            _leftHandGripTarget = inputData.LeftHandGripValue;
            _leftHandTriggerTarget = inputData.LeftHandTriggerValue;
            _rightHandGripTarget = inputData.RightHandGripValue;
            _rightHandTriggerTarget = inputData.RightHandTriggerValue;

            _leftHandGrabbing = inputData.LeftHandGrabbing;
            _rightHandGrabbing = inputData.RightHandGrabbing;
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

            if (_rightHandGripCurrent != _rightHandGripTarget)
            {
                _rightHandGripCurrent = Mathf.MoveTowards(_rightHandGripCurrent, _rightHandGripTarget, Time.deltaTime * 10);
                _rightHandAnimator.SetFloat(animatorGripParam, _rightHandGripCurrent);
            }
            if (_rightHandTriggerCurrent != _rightHandTriggerTarget)
            {
                _rightHandTriggerCurrent = Mathf.MoveTowards(_rightHandTriggerCurrent, _rightHandTriggerTarget, Time.deltaTime * 10);
                _rightHandAnimator.SetFloat(animatorTriggerParam, _rightHandTriggerCurrent);
            }

            if (_leftHandGripCurrent != _leftHandGripTarget)
            {
                _leftHandGripCurrent = Mathf.MoveTowards(_leftHandGripCurrent, _leftHandGripTarget, Time.deltaTime * 10);
                _leftHandAnimator.SetFloat(animatorGripParam, _leftHandGripCurrent);
            }
            if (_leftHandTriggerCurrent != _leftHandTriggerTarget)
            {
                _leftHandTriggerCurrent = Mathf.MoveTowards(_leftHandTriggerCurrent, _leftHandTriggerTarget, Time.deltaTime * 10);
                _leftHandAnimator.SetFloat(animatorTriggerParam, _leftHandTriggerCurrent);
            }
        }
    }
}
