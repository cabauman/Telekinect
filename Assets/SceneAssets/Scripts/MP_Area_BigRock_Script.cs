using UnityEngine;
using System.Collections;

public class MP_Area_BigRock_Script : MonoBehaviour 
{
	public int triggerCount = 0;
	
	public GameObject movingPlate;
	public MP_AREA_SpawnElements elementSpawner;
	
	public void Triggered()
	{
		triggerCount++;
		if(triggerCount == 1)
		{
			this.gameObject.GetComponent<Animation>().Play("MP_AREA_BigRockRise");
			
			//kill the moving plate
			movingPlate.GetComponent<Puzzle_MovingPlate>().enabled = false;
			movingPlate.GetComponent<Rigidbody>().useGravity = true;
			movingPlate.GetComponent<Rigidbody>().isKinematic = false;
			movingPlate.gameObject.layer = 0;
			
			elementSpawner.Spawn(-1);
			elementSpawner.Spawn(1);
			elementSpawner.Spawn(2);
			elementSpawner.Spawn(-2);
		}
		else
			this.gameObject.GetComponent<Animation>().Play("MP_AREA_BigRockRotate&Lay");
	}

}
