using UnityEngine;
using System.Collections;

public class AirPuzzleComplete : MonoBehaviour
{
	public Factory_Crystal_Elemental	 crystalFactory;
	public void Triggered()
	{
		crystalFactory.gameObject.GetComponent<Animation>()["RaiseCrystalFactory"].time = 1.0f;
		crystalFactory.gameObject.GetComponent<Animation>()["RaiseCrystalFactory"].speed = -1.0f;
		crystalFactory.gameObject.GetComponent<Animation>().Play("RaiseCrystalFactory");
		
		crystalFactory.isActive = false;
	}
}

