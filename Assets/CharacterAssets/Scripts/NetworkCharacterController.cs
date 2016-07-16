using UnityEngine;
using System.Collections;

public class NetworkCharacterController : MonoBehaviour 
{

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnNetworkInstantiate( NetworkMessageInfo info )
	{
		Debug.Log("OnNetworkInstantiate called");

		if(GetComponent<NetworkView>().isMine)
		{
			//local player
			GetComponent<KinectCharacterController>().enabled = true;
		}
		else
		{
			//remote player
			//disable the kinect controller
			GetComponent<KinectCharacterController>().hands[0].enabled = false;
			GetComponent<KinectCharacterController>().hands[1].enabled = false;
			GetComponent<KinectCharacterController>().enabled = false;
			DontDestroyOnLoad(this);
		}
	}
}
