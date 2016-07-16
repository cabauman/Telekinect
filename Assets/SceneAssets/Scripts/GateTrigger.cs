using UnityEngine;
using System.Collections;

public class GateTrigger : MonoBehaviour
{
	public bool isOpen = true;
	public Element_Pedestal_Script Pedestal ;


	void Triggered()
	{
		if(isOpen)
		{
			this.gameObject.GetComponent<Animation>()["GateAnimation"].speed = 1.0f;
			this.gameObject.GetComponent<Animation>().Play("GateAnimation");
			Pedestal.Spawn() ;
			
		}
		else
		{
			//this.gameObject.animation["GateAnimation"].time = 0.3333333f;
			this.gameObject.GetComponent<Animation>()["GateAnimation"].speed = -1.0f;
			this.gameObject.GetComponent<Animation>().Play("GateAnimation");
		}
		
		isOpen = !isOpen;
	}
}

