using UnityEngine;
using System.Collections;

public class Spike_Challenge_Controller_Script : MonoBehaviour {
	
	public Transform[] Spear_Objects ;
	public float timeInBetweenFiring;
	public float pauseTimer = 1.0f ;
	int spearIndex;
	float currentTime;
	bool disabled = false;
	bool noTimer = false ;
	
	// Use this for initialization
	void Start () 
	{
		spearIndex = 0 ;
		currentTime = timeInBetweenFiring ;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(disabled || noTimer)
			return;
		
		currentTime -= Time.deltaTime ;
		
		if( currentTime < 0 ) 
		{
			Spear_Objects[spearIndex++].GetComponent<Animation>().Play() ;
			
			currentTime = timeInBetweenFiring ;

			if (spearIndex >= Spear_Objects.Length)
			{
				spearIndex = 0 ;
				currentTime = pauseTimer ;
			}
		}

	}
	
	public void Disable()
	{
		disabled = true;
		foreach(Transform spike in Spear_Objects)
		{
			spike.gameObject.GetComponent<Animation>().wrapMode = WrapMode.Once;
			spike.gameObject.GetComponent<Animation>().Rewind();
		}
	}
	
	public void Enable()
	{
		disabled = false;
		Start();
	}
}
