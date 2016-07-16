using UnityEngine;
using System.Collections;

public class FootStep_Script : MonoBehaviour 
{
	public AudioSource[] MusicSources;

	int currentTrack = 0;
	int nextTrack = -1;
	float walkingTrackInterval, runningTrackInterval = 0 ;
	
	float SpeedTimer = 0.0f ;
			
	// Use this for initialization
	void Start () 
	{
		gameObject.transform.parent = HandFSM.playerController.transform;
		gameObject.transform.localPosition = Vector3.zero ;
		gameObject.transform.Translate(0, -1.0f, 0) ;
		
		walkingTrackInterval = 0.5f ;
		runningTrackInterval = 0.25f ;


		float TrackNumber = RNG.Instance().fUni(0, MusicSources.Length - 1);  //find a number from 0 to number of sources available
		nextTrack = (int)(TrackNumber + 0.5f );
		
		currentTrack = nextTrack ;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if( HandFSM.playerController.isGrounded)
		{
			float playerVelocity = HandFSM.playerController.GetComponent<Rigidbody>().velocity.magnitude;
			
			if ( playerVelocity  > 0.5f && playerVelocity < 3.5f )
			{
				//walking
				if(SpeedTimer >= walkingTrackInterval)
				{
					MusicSources[currentTrack].Play() ;
					Switch_Tracks() ;
					SpeedTimer = 0.0f;
				}
				
			}
			else if(playerVelocity >= 3.5f)
			{
				//running
				if(SpeedTimer >= runningTrackInterval)
				{
					MusicSources[currentTrack].Play() ;
					Switch_Tracks() ;
					SpeedTimer = 0.0f;
				}
			}
			
			SpeedTimer += Time.deltaTime ;
		}

	}
	
	void Switch_Tracks()
	{
			float TrackNumber = RNG.Instance().fUni(0, MusicSources.Length - 1);  //find a number from 0 to number of sources available
			currentTrack = (int)(TrackNumber + 0.5f );
	}
	
//	void OnGUI()
//	{
//		GUILayout.Box(string.Format("Velocity: {0}", HandFSM.playerController.rigidbody.velocity.magnitude.ToString()));
//		GUILayout.Box(string.Format("Interval Timer: {0}", SpeedTimer.ToString() ) ) ;		
//	}
	
	
}//end class