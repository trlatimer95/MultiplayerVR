using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[OrderAfter(typeof(NetworkedGrabbable))]
[RequireComponent(typeof(NetworkedGrabbable))]
public class NetworkedSyringe : NetworkBehaviour
{
    [Header("Plunger Movement")]
    [SerializeField] Transform plungerTransform;
    [SerializeField] float maxPlungerDrawValue;
    [SerializeField] float drawSpeed = 5.0f;

    private NetworkedGrabbable grabbable;
    private NetworkHandGrabber currentGrabber;
    private NetworkRig currentGrabberRig;
    private float triggerInputValue;
    private float previousInputValue = 0f;
    private bool drawPlunger = true;

    private void Awake()
    {
        grabbable = GetComponent<NetworkedGrabbable>();        
    }

    private void OnEnable()
    {
        grabbable.onGrabChanged += GrabbableChanged;
    }

    private void OnDisable()
    {
        grabbable.onGrabChanged -= GrabbableChanged;
    }

    private void GrabbableChanged(bool isGrabbed)
    {
        if (isGrabbed)
        {
            currentGrabberRig = grabbable.CurrentGrabber.GetComponentInParent<NetworkRig>();
            currentGrabber = grabbable.CurrentGrabber;
        }          
        else
        {
            currentGrabberRig = null;
            currentGrabber = null;
        }           
    }

    public override void FixedUpdateNetwork()
    {
        if (currentGrabber == null) return;

        previousInputValue = triggerInputValue;

        // Check for trigger input
        if (currentGrabber.HandSide == HandEnum.LeftHand)
            triggerInputValue = currentGrabberRig.CurrentLeftHandTriggerValue;
        else if (currentGrabber.HandSide == HandEnum.RightHand)
            triggerInputValue = currentGrabberRig.CurrentRightHandTriggerValue;

        // Reverse direction when trigger is released
        if (previousInputValue != 0 && triggerInputValue == 0)
            drawPlunger = !drawPlunger;
    }

    public override void Render()
    {
        if (triggerInputValue == 0) return;

        // Draw Syringe
        if (drawPlunger && plungerTransform.localPosition.y > maxPlungerDrawValue)
        {
            Vector3 targetPosition = new Vector3(plungerTransform.localPosition.x, maxPlungerDrawValue, plungerTransform.localPosition.z);
            Vector3 translation = (targetPosition - plungerTransform.localPosition).normalized * drawSpeed * triggerInputValue * Runner.DeltaTime;

            if (Vector3.Distance(plungerTransform.localPosition, targetPosition) > translation.magnitude)
                plungerTransform.Translate(translation);
            else
                plungerTransform.localPosition = targetPosition;
        }
        // Push Syringe
        else if (!drawPlunger && plungerTransform.localPosition.y < 0)
        {
            Vector3 targetPosition = new Vector3(plungerTransform.localPosition.x, 0f, plungerTransform.localPosition.z);
            Vector3 translation = (targetPosition - plungerTransform.localPosition).normalized * drawSpeed * triggerInputValue * Runner.DeltaTime;

            if (Vector3.Distance(plungerTransform.localPosition, targetPosition) > translation.magnitude)
                plungerTransform.Translate(translation);
            else 
                plungerTransform.localPosition = targetPosition;
        }
    }
}
