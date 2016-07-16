using UnityEngine;
using System.Collections;

public class Elemental_Podium_Challenge_Controller_Script : MonoBehaviour
{
		
	public GameObject Object_ToBe_Triggered ; //Jenga
	
	public int Podiums_Activated ;
	public int numberOfPodiums ;
	
	
	public void TriggerObject()
	{
		if(Network.isClient)
			return;
		
		if( Podiums_Activated >= numberOfPodiums)
		{
			//do whatever we want to do with the object (may need to be overloaded for each instance of the Podium Challenge)
			if(Network.isServer)
				this.gameObject.GetComponent<NetworkView>().RPC("PodiumControllerRemoteTrigger", RPCMode.Others);
			
			Object_ToBe_Triggered.gameObject.SetActiveRecursively(true) ;
		
		}
//		else //Podium was completed, then moved to an uncompleted state
//		{
//			//reverse the above action, or do something different for the challenge.
//			Object_ToBe_Triggered.gameObject.active = true ;
//		}
		
	}
	
	[RPC]
	public void PodiumControllerRemoteTrigger()
	{
		Object_ToBe_Triggered.gameObject.SetActiveRecursively(true) ;
	}
}
