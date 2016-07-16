using UnityEngine;
using System.Collections;

public class Rusty_Button_Controller : MonoBehaviour
{
	public Trigger myTrigger;
	public GameObject vines;
	public Factory_Fire_Elemental fireFactory;
	public Spike_Challenge_Controller_Script[] spikes;
	public float timer = 20.0f;
	
	void Triggered()
	{
		if(vines != null)
			vines.GetComponent<Animation>().Play("RootRise");
		GetComponent<Animation>().Play("Press_Rusty_Button");
		foreach (Spike_Challenge_Controller_Script spike in spikes)
			spike.Disable();
		
		if(fireFactory.gameObject.active == false)
		{
			fireFactory.gameObject.SetActiveRecursively(true);
			fireFactory.isActive = true;
		}
	}
	
	void Update()
	{
		if(Network.isClient)
			return;
		
		if(myTrigger.isActive)
		{
			timer -= Time.deltaTime;
			if(timer <= 0.0f)
			{
				if(Network.isServer)
					this.GetComponent<NetworkView>().RPC("DepressRustyButton", RPCMode.All);
				else if(! Network.isClient)
					DepressRustyButton();
			}
		}
	}
	
	[RPC]
	public void DepressRustyButton()
	{
		myTrigger.isActive = false;
		myTrigger.triggerCount = 1;
		myTrigger.triggerAsleep = false;
		myTrigger.ActivatedAsset = this.gameObject;
		GetComponent<Animation>().Play("Depress_Rusty_Button");
		foreach (Spike_Challenge_Controller_Script spike in spikes)
			spike.Enable();
		
		
		timer = 20.0f;		
	}
}

