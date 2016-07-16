using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))]
public class Trigger : MonoBehaviour
{
    public bool isToggle = true;
    public int triggerCount = -1;  //triggers until trigger turns inactive {-1 = infinite, 0 = inactive, 1 -> +inf = # of triggers}
    public GameObject UN_ActivatedAsset, ActivatedAsset;
    public LayerMask collidableAsset = (1 << 9);
    public bool triggerAsleep = false;
    public bool isActive = false;
	
	public int elementID = 0 ;
	
    void Start()
    {
        if (Network.isServer && !this.GetComponent<NetworkView>().viewID.isMine)
        {
            NetworkViewID newID = Network.AllocateViewID();
            this.GetComponent<NetworkView>().RPC("ChangeID", RPCMode.OthersBuffered, newID);
            this.GetComponent<NetworkView>().viewID = newID;
        }
        else if (Network.isClient)
            this.enabled = false;

        if (triggerCount <= 0)
            triggerCount = -1;

        if (UN_ActivatedAsset == null)
            Debug.Log("No 'un-active' object on " + this.gameObject.name + " , assign in the editor");
		else
			UN_ActivatedAsset.SendMessage("Triggered", SendMessageOptions.DontRequireReceiver);

        if (ActivatedAsset == null)
            Debug.Log("No 'active' object on " + this.gameObject.name + " , assign in the editor");
		//else
			//ActivatedAsset.active = false;
			//ActivatedAsset.SendMessage("UnTriggered");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!this.enabled)
            return;

        if (!triggerAsleep)
        {
            if (((1 << collision.collider.gameObject.layer) & collidableAsset.value) != 0)
            {
				if(elementID != 0)
				{
					Element_Base element = collision.collider.gameObject.GetComponent<Element_Base>();
					if(element == null || element.ID != elementID)
						return;
				}
                if (isToggle)
                {
                    switch (triggerCount)
                    {
                        case -1:
                            if (!isActive)
                                Activate();
                            else
                                Deactivate();
                            break;

                        case 0:
                            Activate();
                            triggerAsleep = true;
                            break;

                        default:
                            if (!isActive)
                                Activate();
                            else
                                Deactivate();
                            break;
                    
                    }
                }
                else
                    Activate();

                    if (triggerCount == 0)
                        triggerAsleep = true;

			}
		}
    }

    void OnCollisionExit(Collision collision)
    {

        if (!this.enabled)
            return;

        if (!triggerAsleep)
        {
            if (((1 << collision.collider.gameObject.layer) & collidableAsset.value) != 0)
            {
				if(elementID != 0)
				{
					Element_Base element = collision.collider.gameObject.GetComponent<Element_Base>();
					if(element == null || element.ID != elementID)
						return;
				}
                if (!isToggle)
                    Deactivate();
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {

        if (!this.enabled)
            return;

        if (!triggerAsleep)
        {
            if (((1 << collider.GetComponent<Collider>().gameObject.layer) & collidableAsset.value) != 0)
            {
                if (isToggle)
                {
                    switch (triggerCount)
                    {
                        case -1:
                            if (!isActive)
                                Activate();
                            else
                                Deactivate();
                        break;

                        case 0:
                            Activate();
                            triggerAsleep = true;
                        break;

                        default:
                            if (!isActive)
                                Activate();
                            else
                                Deactivate();
                        break;
                    }
                }
                else
                    Activate();

                if (triggerCount == 0)
                    triggerAsleep = true;
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {

        if (!this.enabled)
            return;

        if (!triggerAsleep)
        {
            if (((1 << collider.GetComponent<Collider>().gameObject.layer) & collidableAsset.value) != 0)
            {
                if (!isToggle)
                    Deactivate();
            }
        }
    }

    public void Activate()
    {
		isActive = true;
        if (UN_ActivatedAsset != null)
			UN_ActivatedAsset.SendMessage("UnTriggered", SendMessageOptions.DontRequireReceiver);
            //UN_ActivatedAsset.SetActiveRecursively(false);

        if (ActivatedAsset != null)
		{
            //ActivatedAsset.SetActiveRecursively(true);
			ActivatedAsset.SendMessage("Triggered", SendMessageOptions.DontRequireReceiver);
		}
        else 
            Debug.Log("no active asset assigned on " + this.gameObject.name + " , assign in the editor");

        if(!triggerAsleep)
            if(triggerCount > 0)
                triggerCount--;


        if (Network.isServer)
            GetComponent<NetworkView>().RPC("ActivateRemoteTrigger", RPCMode.OthersBuffered);
    }

    public void Deactivate()
    {
		isActive = false;
        if (ActivatedAsset != null)
			ActivatedAsset.SendMessage("UnTriggered", SendMessageOptions.DontRequireReceiver);
            //ActivatedAsset.SetActiveRecursively(false);
        else
            Debug.Log("no active asset assigned " + this.gameObject.name + " , assign in the editor");

        if (UN_ActivatedAsset != null)
		{
            //UN_ActivatedAsset.SetActiveRecursively(true);
			ActivatedAsset.SendMessage("Triggered", SendMessageOptions.DontRequireReceiver);	
		}

        if (isToggle)
            if (triggerCount > 0)
                triggerCount--;

        if (Network.isServer)
            GetComponent<NetworkView>().RPC("DeactivateRemoteTrigger", RPCMode.OthersBuffered);
    }

    [RPC]
    public void ActivateRemoteTrigger(NetworkMessageInfo info)
    {
        this.Activate();
    }

    [RPC]
    public void DeactivateRemoteTrigger(NetworkMessageInfo info)
    {
        this.Deactivate();
    }

    [RPC]
    public void ChangeID(NetworkViewID newID, NetworkMessageInfo info)
    {
        this.GetComponent<NetworkView>().viewID = newID;
    }
}