using UnityEngine;
using System.Collections;

public class Timed_Trigger : Trigger 
{
	public float deactivation_timer = 5;
	float time_since_activation = 0;
	
	
	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Network.isClient)
		{
			return ;
		}
		
		if( time_since_activation >= deactivation_timer )
		{
			Deactivate();
			time_since_activation = 0;
		}
		else
			time_since_activation += Time.deltaTime;
	}
}