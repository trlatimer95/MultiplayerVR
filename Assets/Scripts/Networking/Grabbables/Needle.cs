using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle : NetworkBehaviour
{
    public bool VialAttached { get; private set; } = false;

    private NetworkedSocketInteractable socket;
    private NetworkedSocketReceiver reciever;

    private NetworkedSyringe currentAttachedSyringe;
    private Vial currentAttachedVial;

    private void Start()
    {
        socket = GetComponent<NetworkedSocketInteractable>();
        if (socket == null)
            Debug.Log("Unable to find socket component for Needle");

        reciever = GetComponent<NetworkedSocketReceiver>();
        if (reciever == null)
            Debug.Log("Unable to find reciever component for Needle");

        //socket.OnSocketedChanged.AddListener(SocketChanged); // May still need to subscribe to both socket and receiver to handle when need is seperated.
    }

    public override void FixedUpdateNetwork()
    {
        if (currentAttachedSyringe == null && socket.followTransform != null)
            currentAttachedSyringe = socket.followTransform.GetComponentInParent<NetworkedSyringe>();         
        else if (currentAttachedSyringe != null && socket.followTransform == null)
            currentAttachedSyringe = null;
            

        if (currentAttachedVial == null && reciever.Occupied && reciever.currentAttachedInteractable != null)
        {
            currentAttachedVial = reciever.currentAttachedInteractable.GetComponent<Vial>();
            if (currentAttachedSyringe != null)
            {
                currentAttachedSyringe.VialAttached = true;
                currentAttachedSyringe.SetFluidColor(currentAttachedVial.fluidMaterial);
            }
        }             
        else if (currentAttachedVial != null && !reciever.Occupied)
        {
            currentAttachedVial = null;
            if (currentAttachedSyringe != null)
                currentAttachedSyringe.VialAttached = false;
        }                   
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.gameObject.tag == "Vial" && currentAttachedSyringe != null)
    //        currentAttachedSyringe.DrawFluid = true;
    //}

    //private void SocketChanged()
    //{
    //    if (socket.followTransform != null)
    //    {
    //        NetworkedSyringe syringe = socket.followTransform.GetComponent<NetworkedSyringe>();
    //        if (syringe != null)
    //        {
    //            currentAttachedSyringe = syringe;
    //        }
    //    }
    //    else
    //        currentAttachedSyringe = null;
    //}
}
