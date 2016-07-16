using UnityEngine;
using System.Collections;

public class CrystalPodiumComplete : MonoBehaviour
{
	public Factory_Wind_Elemental 	windyFactory;
	public FanBlade_Triggered_Crystalization_Script		fanBlade ;
	
	public bool 			isTriggered = false;
	public GameObject		elementSpawn;
	
	void Triggered()
	{
		if(isTriggered)
			return;
		
		isTriggered = true;
		
		windyFactory.gameObject.SetActiveRecursively(true);
		windyFactory.isActive = true;//sets the factory to active, not Unity Active.
		
		fanBlade.Triggered() ;
		
		elementSpawn.GetComponent<Element_Pedestal_Script>().Spawn(  ) ;
	}
	
}

