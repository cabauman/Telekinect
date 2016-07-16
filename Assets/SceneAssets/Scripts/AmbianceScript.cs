using UnityEngine;
using System.Collections;

public class AmbianceScript : MonoBehaviour 
{
	
	public AudioSource[] MusicSources;

	float trackPlayTime = 0.0f; //how long the current track has been playing (if over some random (ish) threashold, change tracks.
	int currentTrack = 0;
	int nextTrack = -1;
	bool switchingTracks = false ;

	// Use this for initialization
	void Start () 
	{
		transform.parent = Camera.main.gameObject.transform;
		transform.localPosition = Vector3.zero ;
		
		float TrackNumber = RNG.Instance().fUni(0, MusicSources.Length - 1);  //find a number from 0 to number of sources available
		nextTrack = (int)(TrackNumber + 0.5f );
		
		currentTrack = nextTrack ;

		MusicSources[currentTrack].Play();
		MusicSources[currentTrack].volume = 1.0f ;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
		if( Switch_Tracks() )
		{
			while(currentTrack == nextTrack)
			{
				float TrackNumber = RNG.Instance().fUni(0, MusicSources.Length - 1);  //find a number from 0 to number of sources available
				nextTrack = (int)(TrackNumber + 0.5f );
			}
			
			MusicSources[nextTrack].Play() ;
			trackPlayTime = 0;
		}
		
		if( switchingTracks)
		{
			MusicSources[currentTrack].volume -= 0.1f; 
			MusicSources[nextTrack].volume += 0.1f;
			
			if (MusicSources[currentTrack].volume <= 0 && MusicSources[nextTrack].volume >= 1.0f)
			{
				MusicSources[currentTrack].Stop() ;
				
				currentTrack = nextTrack;
				nextTrack = -1;
				switchingTracks = false;
			}
		}
	}	

	bool Switch_Tracks()
	{
		if (trackPlayTime >= MusicSources[currentTrack].clip.length - 3 )
		{
			float TrackNumber = RNG.Instance().fUni(0, MusicSources.Length - 1);  //find a number from 0 to number of sources available
			nextTrack = (int)(TrackNumber + 0.5f );
			
			switchingTracks = true;
			return true;
		}
		else
		{
			trackPlayTime += Time.deltaTime;
			return false;
		}
	}
	
//	void OnGUI()
//	{
//		GUILayout.Box(string.Format("Track Play Time: {0}", trackPlayTime.ToString()));
//		GUILayout.Box(string.Format("Track Title: {0}", MusicSources[currentTrack].clip.name) );
//		GUILayout.Box(string.Format("Track Length: {0}", MusicSources[currentTrack].clip.length.ToString() ) );
//		GUILayout.Box(string.Format("Track Volume: {0}", MusicSources[currentTrack].volume.ToString() ) ) ;
//	}
}
