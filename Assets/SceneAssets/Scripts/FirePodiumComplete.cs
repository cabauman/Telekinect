using UnityEngine;
using System.Collections;

public class FirePodiumComplete : MonoBehaviour
{
	public Factory_Water_Elemental 	drippyFactory;
	public GameObject 		gate;
	public bool 			isTriggered = false;
	public GameObject		elementSpawn ;
	
	void Triggered()
	{
		if(isTriggered)
			return;
		
		isTriggered = true;
		
		elementSpawn.GetComponent<Element_Pedestal_Script>().Spawn( ) ;
		
		drippyFactory.gameObject.SetActiveRecursively(true);
		drippyFactory.isActive = true;
		
		gate.SendMessage("Triggered", SendMessageOptions.DontRequireReceiver) ;
		

	}
}

