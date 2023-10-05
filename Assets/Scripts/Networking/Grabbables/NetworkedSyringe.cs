using Fusion;
using UnityEngine;

[OrderAfter(typeof(NetworkedGrabbable))]
[RequireComponent(typeof(NetworkedGrabbable))]
public class NetworkedSyringe : NetworkBehaviour
{
    [Header("Plunger Movement")]
    [SerializeField] Transform plungerTransform;
    [SerializeField] float maxPlungerDrawValue;
    [SerializeField] float drawSpeed = 5.0f;

    [Header("Fluid Settings")]
    [SerializeField] Transform fluidTransform;
    [SerializeField] Transform plungerTipTransform;
    [SerializeField] Transform syringeEndTransform;

    [Networked] private Vector3 localPlungerPosition { get; set; } = Vector3.zero;
    [Networked] private Vector3 localFluidPosition { get; set; } = Vector3.zero;
    [Networked] private Vector3 localFluidScale { get; set; } = Vector3.zero; 

    private NetworkedGrabbable grabbable;
    private NetworkHandGrabber currentGrabber;
    private NetworkRig currentGrabberRig;
    private float triggerInputValue;
    private float previousInputValue = 0f;
    private bool drawPlunger = true;
    private Vector3 initialScale;

    public override void Spawned()
    {
        base.Spawned();

        grabbable = GetComponent<NetworkedGrabbable>();
        grabbable.onGrabChanged += GrabbableChanged;

        initialScale = transform.localScale;

        if (plungerTransform.localPosition.y >= 0)
            fluidTransform.gameObject.SetActive(false);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

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
        if (currentGrabber == null || !grabbable.IsGrabbed) return;

        previousInputValue = triggerInputValue;

        // Check for trigger input
        if (currentGrabber.HandSide == HandEnum.LeftHand)
            triggerInputValue = currentGrabberRig.CurrentLeftHandTriggerValue;
        else if (currentGrabber.HandSide == HandEnum.RightHand)
            triggerInputValue = currentGrabberRig.CurrentRightHandTriggerValue;

        // Reverse direction when trigger is released
        if (previousInputValue != 0 && triggerInputValue == 0)
            drawPlunger = !drawPlunger;

        CalculatePlungerLocation();

        ScaleFluid();
    }

    public override void Render()
    {
        if (!grabbable.IsGrabbed) return;

        CalculatePlungerLocation();
        MovePlunger();

        ScaleFluid();
    }

    private void ScaleFluid()
    {
        if (plungerTransform.localPosition.y >= 0)
        {
            if (fluidTransform.gameObject.activeInHierarchy)
                fluidTransform.gameObject.SetActive(false);
        }
        else
        {
            if (!fluidTransform.gameObject.activeInHierarchy)
                fluidTransform.gameObject.SetActive(true);

            float distance = Vector3.Distance(plungerTipTransform.position, syringeEndTransform.position);
            fluidTransform.localScale = new Vector3(fluidTransform.localScale.x, distance / 2f, fluidTransform.localScale.z);

            Vector3 center = (plungerTipTransform.position + syringeEndTransform.position) / 2f;
            fluidTransform.position = center;
        }       
    }

    private void MovePlunger()
    {
        plungerTransform.localPosition = localPlungerPosition;
    }

    private void CalculatePlungerLocation()
    {
        if (triggerInputValue == 0) return;

        Vector3 targetPosition = Vector3.zero;

        // Draw Syringe
        if (drawPlunger && plungerTransform.localPosition.y > maxPlungerDrawValue)
            targetPosition = new Vector3(plungerTransform.localPosition.x, maxPlungerDrawValue, plungerTransform.localPosition.z);         
        // Push Syringe
        else if (!drawPlunger && plungerTransform.localPosition.y < 0)
            targetPosition = new Vector3(plungerTransform.localPosition.x, 0f, plungerTransform.localPosition.z);

        // Move Plunger locally
        Vector3 translation = (targetPosition - plungerTransform.localPosition).normalized * drawSpeed * triggerInputValue * Runner.DeltaTime;
        plungerTransform.Translate(translation);

        // Save new position for network replication
        localPlungerPosition = plungerTransform.localPosition;
    }
}
