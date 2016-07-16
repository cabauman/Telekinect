using UnityEngine;
using System.Collections;

public class MP_AREA_SpawnElements : MonoBehaviour 
{	
	public Transform fireTransform;
	public Transform airTransform;
	public Transform waterTransform;
	public Transform crystalTransform;
	
	public void Spawn(int elementID)
	{
		if(Network.isClient)
			return;
		
		GameObject newElement = null;
		switch(elementID)
		{
		case -1:
			if(Network.isServer)
				newElement = (GameObject)Network.Instantiate(Resources.Load("Drop_Puzzle_Fire"), fireTransform.position , fireTransform.rotation, 0);
			else
				newElement = (GameObject)Instantiate(Resources.Load("Drop_Puzzle_Fire"), fireTransform.position , fireTransform.rotation);
			
			
			newElement.GetComponent<Drop_Puzzle_Element>().spawnPoint = fireTransform  ;
			
			break;
		case 1:
			if(Network.isServer)
				newElement = (GameObject)Network.Instantiate(Resources.Load("Drop_Puzzle_Water"), waterTransform.position , waterTransform.rotation, 0);
			else
				newElement = (GameObject)Instantiate(Resources.Load("Drop_Puzzle_Water"), waterTransform.position , waterTransform.rotation);
			
			newElement.GetComponent<Drop_Puzzle_Element>().spawnPoint = waterTransform  ;
			
			break;
		case 2:
			if(Network.isServer)
				newElement = (GameObject)Network.Instantiate(Resources.Load("Drop_Puzzle_Crystal"), crystalTransform.position , crystalTransform.rotation, 0);
			else
				newElement = (GameObject)Instantiate(Resources.Load("Drop_Puzzle_Crystal"), crystalTransform.position , crystalTransform.rotation);
			
			newElement.GetComponent<Drop_Puzzle_Element>().spawnPoint = crystalTransform  ;
			
			break;
		case -2:
			if(Network.isServer)
				newElement = (GameObject)Network.Instantiate(Resources.Load("Drop_Puzzle_Air"), airTransform.position , airTransform.rotation, 0);
			else
				newElement = (GameObject)Instantiate(Resources.Load("Drop_Puzzle_Air"), airTransform.position , airTransform.rotation);
			
			newElement.GetComponent<Drop_Puzzle_Element>().spawnPoint = airTransform  ;
			
			break;
		}
	}
}
