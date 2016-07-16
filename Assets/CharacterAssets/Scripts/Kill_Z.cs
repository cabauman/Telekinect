//DLoment-10/05/11
//When called upon it will instantly kill the player by setting the health of the player to 0.0f.
using UnityEngine;
using System.Collections;

public class Kill_Z : MonoBehaviour
{
     void  OnTriggerEnter( Collider collision )
    {
		if(Network.isClient)
			return;
		
	    KinectCharacterController tempPlayer = collision.transform.root.GetComponent<KinectCharacterController>() ;
	    Agent_FSM agent = collision.transform.root.GetComponent<Agent_FSM>() ;
		Drop_Puzzle_Element puzzleElement = collision.transform.root.GetComponent<Drop_Puzzle_Element>();
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
    }
}
