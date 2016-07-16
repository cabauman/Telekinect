using UnityEngine;
using System.Collections;

public class MP_AREA_Battle_Controller : MonoBehaviour
{
	
	public GameObject[] MP_Battle_Factories;
	// Use this for initialization
	void Start ()
	{
		
	}

	// Update is called once per frame
	void Update ()
	{
		
	}
	
	public void Triggered()
	{
		foreach( GameObject factory in MP_Battle_Factories )
		{
			factory.GetComponent<Agent_Factory>().isActive = true;
		}
	}
}

