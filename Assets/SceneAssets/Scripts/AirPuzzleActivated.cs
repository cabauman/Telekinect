using UnityEngine;
using System.Collections;

public class AirPuzzleActivated : MonoBehaviour
{
	public Factory_Crystal_Elemental crystalFactory;
	public bool isTriggered = false ;
	public GameObject elementSpawn ;
	
	void Triggered()
	{
		if( isTriggered)
			return ;
		
		isTriggered = true ;
		crystalFactory.GetComponent<Animation>().Play("RaiseCrystalFactory");
		crystalFactory.isActive = true;
		elementSpawn.GetComponent<Element_Pedestal_Script>().Spawn( ) ;
	}
}

