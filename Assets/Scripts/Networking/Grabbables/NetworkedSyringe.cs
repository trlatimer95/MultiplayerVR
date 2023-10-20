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
    [SerializeField] GameObject fluidGameObject;
    [SerializeField] Transform plungerTipTransform;
    [SerializeField] Transform syringeEndTransform;
    [SerializeField] Transform plungerMaxTransform;
    [SerializeField] Animator animController;
    [SerializeField] AnimationClip fluidAnimation;

    [Networked] public Vector3 localPlungerPosition { get; set; } = Vector3.zero;

    private NetworkedGrabbable grabbable;
    private NetworkHandGrabber currentGrabber;
    private NetworkRig currentGrabberRig;
    private float triggerInputValue;
    private float previousInputValue = 0f;
    private bool drawPlunger = true;
    private float maxDistance;

    public override void Spawned()
    {
        base.Spawned();

        if (animController == null)
            animController = GetComponentInChildren<Animator>();

        animController.speed = 0;

        grabbable = GetComponent<NetworkedGrabbable>();
        grabbable.onGrabChanged += GrabbableChanged;

        maxDistance = Vector3.Distance(syringeEndTransform.position, plungerMaxTransform.position);

        if (plungerTransform.localPosition.y >= 0)
            fluidGameObject.SetActive(false);
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
    }

    public override void Render()
    {
        if (!grabbable.IsGrabbed) return;

        CalculatePlungerLocation();
        MovePlunger();

        DrawFluid();
    }

    private void DrawFluid()
    {
        if (plungerTransform.localPosition.y >= 0)
        {
            if (fluidGameObject.activeInHierarchy)
                fluidGameObject.SetActive(false);
        }
        else
        {
            if (!fluidGameObject.activeInHierarchy)
                fluidGameObject.SetActive(true);

            // Find current distance and get percentage of max distance
            float distance = Vector3.Distance(syringeEndTransform.position, plungerTipTransform.position);
            float normalizedDistance = distance / maxDistance;

            // Play animation at normalized distance to get scale and position to match
            animController.Play(fluidAnimation.name, 0, normalizedDistance);
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
