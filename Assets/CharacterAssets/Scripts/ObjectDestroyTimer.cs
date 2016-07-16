using UnityEngine;
using System.Collections;

public class ObjectDestroyTimer : MonoBehaviour 
{

	public float TimeToLive = 0.0f;
	
	// countdown
	void Update () 
	{
		TimeToLive -= Time.deltaTime;
		if(TimeToLive <= 0.0f)
		{
			Destroy(this.gameObject);
		}
	}
}
