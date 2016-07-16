using UnityEngine;
using System.Collections;

public class KillBoxScript : MonoBehaviour 
{

	void OnTriggerExit(Collider colliderObject)
	{
		if(Network.isClient)
			return;
		
	    KinectCharacterController tempPlayer = colliderObject.transform.root.GetComponent<KinectCharacterController>() ;
	    Agent_FSM agent = colliderObject.transform.root.GetComponent<Agent_FSM>() ;
		Drop_Puzzle_Element puzzleElement = colliderObject.transform.root.GetComponent<Drop_Puzzle_Element>();
	    //Debug.Break() ;
	
	    if (tempPlayer != null)
	    {
			if(Network.isServer && tempPlayer.GetComponent<NetworkView>().isMine || Network.peerType == NetworkPeerType.Disconnected)
	        	tempPlayer.health = 0.0f ;
			else
				tempPlayer.GetComponent<NetworkView>().RPC("DamageClientPlayer", tempPlayer.gameObject.GetComponent<NetworkView>().owner, 110.0f);
	    }
	    else if(agent != null)
	    {
	        agent.health = 0.0f;
	    }
		else if(puzzleElement != null)
		{
			puzzleElement.Respawn();

		}
		else
		{
			Destroy(colliderObject.gameObject) ;
		}
	}
}
