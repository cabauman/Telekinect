using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour
{
    public bool isActive;
	public bool dying;
	public bool isTransferring ;
    public GameObject MeshWithCollision;
	float TransferTime ;
    float visibility;

	public AudioSource Source1;
	public AudioSource Source2;
	public AudioSource Source3;

    Renderer[] renderers;

    // Use this for initialization
    void Start()
    {

		//get all the renderers in the shield (children)
        renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material.shader = Shader.Find("Shield"); //get the shader of the renderers
            visibility = 0.0f; 
            renderer.material.SetFloat("_Strength", visibility); //the visibility variable changes the "strength" of the shield shader (the opacity)
		}
		
        isTransferring = isActive = dying = false; //set all booleans to false
     	gameObject.transform.localScale = new Vector3( 0.0f, 0.0f, 0.0f ) ;		//default the scale to 0 so it is invisible

		Source1.GetComponent<AudioSource>().Play();

    }

    // Update is called once per frame
    void Update()
    {		
		if( Network.peerType != NetworkPeerType.Disconnected && ! GetComponent<NetworkView>().isMine )
		    return;


		if(isActive == false)
		{
			if(dying == true) // if the shield should be dying - linearly interpolate the scale to 0
			{
				gameObject.transform.localScale = Vector3.Lerp( gameObject.transform.localScale, Vector3.zero, Time.deltaTime *	2.0f );

				Source2.volume -= 0.1f;

				if (Source2.volume <= 0.5f && Source3.isPlaying == false)
				{
					Source3.Play();
				}
				else
				{
					Source3.volume -= 0.025f;
				}

			}
			else //the shield is created, attached, but still inactive
			{
				if( gameObject.transform.localScale.x > HotValues.Instance().shieldActivateThreshold ) //if the scale of the shield has reached what we want, activate it
				{
					float newScale = HotValues.Instance().shieldActivateThreshold;
					//set the shield to the threshold just to eliminate any strangeness that may have occured from the vector in Kinect Space
					gameObject.transform.localScale = new  Vector3(newScale, newScale, newScale);
					isActive = true; //set the shield to be active
					HandFSM.playerController.TKRegenOn = false;
					HandFSM.playerController.TKenergy -= HotValues.Instance().shieldTKConjureCost;
				}
			}
		
			foreach (Renderer renderer in renderers)
			{
		        visibility = 0.1f + ( 4.0f * (gameObject.transform.localScale.x / HotValues.Instance().shieldActivateThreshold) ); //update the visibility of the shield based off the scale of the shield
		        renderer.material.SetFloat("_Strength", visibility);
		    }	
			
		}
		else
		{			
			if ( isTransferring ) //if the shield should be tranferring from one hand to the other
			{
				TransferTime += Time.deltaTime ;
				gameObject.transform.localPosition = Vector3.Lerp( gameObject.transform.localPosition, Vector3.zero , TransferTime ) ; // LERP the position from one hand to the other
				//transform.localEulerAngles = Vector3.Lerp( transform.localEulerAngles, new Vector3( 90.0f, 0.0f, 0.0f ) , TransferTime ) ; // LERP the angle for the orientation at the hand from one hand to the other
				transform.localEulerAngles = new Vector3(  90.0f, 0.0f, 0.0f ) ; // set the angle for the shield when going to the other hand
				if ( TransferTime >= 1.0f ) //once the transfering time has reached a second, deactivate the transferring
				{
					isTransferring = false ;
					TransferTime = 0.0f ;
				}
			}
			
			//adjust TK level
			if (HandFSM.playerController.TKenergy > 0.0f )
			{
				HandFSM.playerController.TKenergy -= HandFSM.hotValues.shieldTKActiveCost * Time.deltaTime ;
			}
			else
			{
				DestroyShield() ; //run out of energy, destroy the shield
			}
		}

		if (Source1.isPlaying && Source1.volume > 0.5f)
		{
			Source1.volume -= 0.05f;
		}
		else
		{
			Source1.volume -= 0.05f;
			Source2.volume += 0.05f;
		}
    }
	
	public void DestroyShield()
	{
		isActive = false;
		dying = true;

		if(HandFSM.playerController.hands[0].currentState != HandTKMoveState.Instance() &&
		   HandFSM.playerController.hands[1].currentState != HandTKMoveState.Instance() )
				HandFSM.playerController.TKRegenOn = true;

		if(Network.peerType != NetworkPeerType.Disconnected)
			Network.Destroy(this.GetComponent<NetworkView>().viewID);
		else
			Destroy(this.gameObject, 3.0f);

	}
	
	void OnCollisionEnter(Collision collision)
	{
		//Collisions with the shield are only handled on the Server or if in a single-player game
		if( Network.isClient )
			return;

		Projectile_Script projectile = collision.gameObject.GetComponent<Projectile_Script>() ;
		
		if ( projectile != null )
		{
			Debug.Log("projectile blocked");

			if( GetComponent<NetworkView>().isMine )
				HandFSM.playerController.TKenergy -= ( projectile.projectileDamage / 2 );
			//ContactPoint contact = collision.contacts[0] ;
			
			//Vector3 noise = new Vector3( RNG.Instance().fGen() , RNG.Instance().fGen() , RNG.Instance().fGen() ) ;
			Vector3 noise = new Vector3( RNG.Instance().fUni(-5,5) , RNG.Instance().fUni(0, 5) , RNG.Instance().fUni(-5,5) ) ;
			//Vector3 newVelocity = -(contact.normal) + noise ;
			projectile.gameObject.GetComponent<Rigidbody>().velocity = noise * 2;
			//play shieldDamage noise (once we have one...) TODO
		}
	}

	void  OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 localPos = Vector3.zero;
		//Vector3 localAngles = Vector3.zero;
		Vector3 scale = Vector3.zero;
		float vis = 0.0f;
		
		bool[] isPlaying = new bool[3];
		float[] volume = new float[3];

		if(stream.isWriting)
		{
			localPos = this.gameObject.transform.localPosition;
			//localAngles = this.gameObject.transform.localEulerAngles;
			scale = this.gameObject.transform.localScale;
			vis = visibility;
			isPlaying[0] = Source1.isPlaying;
			isPlaying[1] = Source2.isPlaying;
			isPlaying[2] = Source3.isPlaying;
			volume[0] = Source1.volume;
			volume[1] = Source2.volume;
			volume[2] = Source3.volume;
			
			stream.Serialize(ref localPos);
			stream.Serialize(ref scale);
			stream.Serialize(ref vis);
			stream.Serialize(ref isPlaying[0]);
			stream.Serialize(ref isPlaying[1]);
			stream.Serialize(ref isPlaying[2]);
			stream.Serialize(ref volume[0]);
			stream.Serialize(ref volume[1]);
			stream.Serialize(ref volume[2]);
		}
		else
		{
			stream.Serialize(ref localPos);
			stream.Serialize(ref scale);
			stream.Serialize(ref vis);
			stream.Serialize(ref isPlaying[0]);
			stream.Serialize(ref isPlaying[1]);
			stream.Serialize(ref isPlaying[2]);
			stream.Serialize(ref volume[0]);
			stream.Serialize(ref volume[1]);
			stream.Serialize(ref volume[2]);			
			
			if(isPlaying[0] && ! Source1.isPlaying)
				Source1.Play();
			if(isPlaying[1] && ! Source2.isPlaying)
				Source2.Play();
			if(isPlaying[2] && ! Source3.isPlaying)
				Source3.Play();
			
			Source1.volume = volume[0];
			Source2.volume = volume[1];
			Source3.volume = volume[2];

			this.gameObject.transform.localPosition = localPos;
			//this.gameObject.transform.localEulerAngles = localAngles;
			this.gameObject.transform.localScale = scale;

			foreach (Renderer renderer in renderers)
				renderer.material.SetFloat("_Strength", vis);
		}
	}
	
}