using UnityEngine;
using System.Collections;

public class NetworkSkeleton : MonoBehaviour 
{
	public KinectCharacterController 	character = null;
	public OpenNISkeleton				skeleton;
	public Quaternion[]					networkTransforms = new Quaternion[16] ;
	public float						prevTime;
	public float 						frameTime = 1.0f;
	public float						currentLerpTime;
	public int							currentAnim;

	//public Transform					head;
	//public Transform					torso;
	//public Transform					leftShoulder;
	//public Transform					leftElbow;
	//public Transform					leftHand;
	//public Transform					rightShoulder;
	//public Transform					rightElbow;
	//public Transform					rightHand;


	// Use this for initialization
	void Awake ()  //was start
	{
		character = this.gameObject.GetComponent<KinectCharacterController>();
		networkTransforms = new Quaternion[16] ;
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
	
		if(  Network.peerType != NetworkPeerType.Disconnected &&! this.GetComponent<NetworkView>().isMine)
		{
			currentLerpTime += Time.deltaTime / Time.timeScale;
			float lerpAmount = currentLerpTime / frameTime;
			skeleton.Head.rotation = Quaternion.Slerp( networkTransforms[0], networkTransforms[8], lerpAmount );
			skeleton.Torso.rotation = Quaternion.Slerp( networkTransforms[1], networkTransforms[9], lerpAmount );
			skeleton.LeftShoulder.rotation = Quaternion.Slerp( networkTransforms[2], networkTransforms[10], lerpAmount );
			skeleton.LeftElbow.rotation = Quaternion.Slerp( networkTransforms[3], networkTransforms[11], lerpAmount );
			skeleton.LeftHand.rotation = Quaternion.Slerp( networkTransforms[4], networkTransforms[12], lerpAmount );
			skeleton.RightShoulder.rotation = Quaternion.Slerp( networkTransforms[5], networkTransforms[13], lerpAmount );
			skeleton.RightElbow.rotation = Quaternion.Slerp( networkTransforms[6], networkTransforms[14], lerpAmount );
			skeleton.RightHand.rotation = Quaternion.Slerp( networkTransforms[7], networkTransforms[15], lerpAmount );
		}
	}

	//interpolate the player character's transform over the network, as well as transmit / recieve skeleton information
	//for animating via Kinect
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{	
		Vector3 pos = Vector3.zero;
		Vector3 velocity = Vector3.zero;
		Quaternion rot = Quaternion.identity;

		Quaternion h = Quaternion.identity;
		Quaternion t = Quaternion.identity;
		Quaternion ls = Quaternion.identity;
		Quaternion le = Quaternion.identity;
		Quaternion lh = Quaternion.identity;
		Quaternion rs = Quaternion.identity;
		Quaternion re = Quaternion.identity;
		Quaternion rh = Quaternion.identity;
		
		float animSpeed = 1.0f;

		//we are sending info across the network
		if(stream.isWriting)
		{
			pos = this.transform.position;
			velocity = this.GetComponent<Rigidbody>().velocity;
			rot = this.transform.rotation;

			h = skeleton.Head.rotation;
			t = skeleton.Torso.rotation;
			ls = skeleton.LeftShoulder.rotation;
			le = skeleton.LeftElbow.rotation;
			lh = skeleton.LeftHand.rotation;
			rs = skeleton.RightShoulder.rotation;
			re = skeleton.RightElbow.rotation;
			rh = skeleton.RightHand.rotation;
			
			animSpeed = character.animSpeed;

			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
			stream.Serialize(ref velocity);

			stream.Serialize(ref h);
			stream.Serialize(ref t);
			stream.Serialize(ref ls);
			stream.Serialize(ref le);
			stream.Serialize(ref lh);
			stream.Serialize(ref rs);
			stream.Serialize(ref re);
			stream.Serialize(ref rh);
			
			stream.Serialize(ref character.currentAnim);
			stream.Serialize(ref animSpeed);
		}
		//we are receiving info from the network
		else
		{
			frameTime = (float)info.timestamp - prevTime;
			prevTime = (float)info.timestamp;
			currentLerpTime = 0.0f;
			
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
			stream.Serialize(ref velocity);

			stream.Serialize(ref h);
			stream.Serialize(ref t);
			stream.Serialize(ref ls);
			stream.Serialize(ref le);
			stream.Serialize(ref lh);
			stream.Serialize(ref rs);
			stream.Serialize(ref re);
			stream.Serialize(ref rh);
			
			stream.Serialize(ref currentAnim);
			stream.Serialize(ref animSpeed);

			this.transform.position = pos;
			this.transform.rotation = rot;
			this.GetComponent<Rigidbody>().velocity = velocity;
			
			for(int idx = 0; idx < 8 ; idx++)
				networkTransforms[idx] = networkTransforms[idx+8];
			
			networkTransforms[8] = h;
			networkTransforms[9] = t;
			networkTransforms[10] = ls;
			networkTransforms[11] = le;
			networkTransforms[12] = lh;
			networkTransforms[13] = rs;
			networkTransforms[14] = re;
			networkTransforms[15] = rh;
			
			if(character.currentAnim != currentAnim)
			{
				character.animatedModel.GetComponent<Animation>().CrossFade(character.anims[currentAnim]);
				character.currentAnim = currentAnim;
			}
			character.animatedModel.GetComponent<Animation>()[character.anims[currentAnim]].speed = animSpeed;
		}
	}
}
