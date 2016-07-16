using UnityEngine;
using System.Collections;

public class Element_Pedestal_Script : MonoBehaviour
{

	public Transform SpawnPoint ;
	public GameObject Element ;
	
	public bool m_bSpawnonStart ;
	
	void Start()
	{
		if(m_bSpawnonStart)
			Spawn();		
	}
	
	public void Spawn()
	{
		Element.GetComponent<Drop_Puzzle_Element>().spawnPoint = SpawnPoint ;
		
		if(Network.isServer)
			Element = Network.Instantiate(Element, SpawnPoint.position, Quaternion.identity, 0) as GameObject;
		else if ( ! Network.isClient)
			Element = Object.Instantiate(Element, SpawnPoint.position, Quaternion.identity) as GameObject ;
	}
}
