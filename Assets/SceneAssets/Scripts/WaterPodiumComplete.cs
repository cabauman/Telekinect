using UnityEngine;
using System.Collections;

public class WaterPodiumComplete : MonoBehaviour 
{
	bool isTriggered = false ;
	public GameObject elementSpawn ;
	
	void Triggered()
	{
		if(isTriggered)
			return ;
		
		isTriggered = true ;
		elementSpawn.GetComponent<Element_Pedestal_Script>().Spawn( ) ;
	}

}
