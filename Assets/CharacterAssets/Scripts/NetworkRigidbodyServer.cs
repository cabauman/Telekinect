using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))]
public class NetworkRigidbodyServer : MonoBehaviour {
	
	//public double m_InterpolationBackTime = 0.1;
	//public double m_ExtrapolationLimit = 0.5;

	public bool TKactive = false;
	public bool TKrotateActive = false;
	public Vector3 TKvelocity;
	public Vector3 TKangularVelocity;
	
	//internal struct  State
	//{
	//    internal double timestamp;
	//    internal Vector3 pos;
	//    internal Vector3 velocity;
	//    internal Quaternion rot;
	//    internal Vector3 angularVelocity;
	//}
	
	//// We store twenty states with "playback" information
	//State[] m_BufferedState = new State[20];
	//// Keep track of what slots are used
	//int m_TimestampCount;

	void Start()
	{
		if(Network.isServer && ! GetComponent<NetworkView>().isMine)
		{
			//Make sure Server owns this object's NetworkViewID
			NetworkViewID newID = Network.AllocateViewID();
			GetComponent<NetworkView>().RPC("SetID", RPCMode.OthersBuffered, newID);
			GetComponent<NetworkView>().viewID = newID;
		}
	}

	void Update()
	{
		//if a remote client is controlling this rigidbody with telekinesis
		//make sure the server rigidbody is using those input velocities every frame
		if(Network.isServer)
		{
			if(TKactive)
				this.GetComponent<Rigidbody>().velocity = TKvelocity;
			if(TKrotateActive)
				this.GetComponent<Rigidbody>().angularVelocity = TKangularVelocity;
		}
	}

	[RPC]
	void SetID(NetworkViewID newID)
	{
		GetComponent<NetworkView>().viewID = newID;
	}

	[RPC]
	void StopTK(NetworkMessageInfo info)
	{
		TKactive = false;
	}
	
	[RPC]
	void StopTKRotate(NetworkMessageInfo info)
	{
		TKrotateActive = false;
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 pos = Vector3.zero;
		Vector3 velocity = Vector3.zero;
		Quaternion rot = Quaternion.identity;
		Vector3 angularVelocity = Vector3.zero;

		// Send data - only if server
		if (Network.isServer && stream.isWriting)
		{
			//Debug.Log("Writing to Stream");

			pos = GetComponent<Rigidbody>().position;
			rot = GetComponent<Rigidbody>().rotation;
			velocity = GetComponent<Rigidbody>().velocity;
			angularVelocity = GetComponent<Rigidbody>().angularVelocity;

			stream.Serialize(ref pos);
			stream.Serialize(ref velocity);
			stream.Serialize(ref rot);
			stream.Serialize(ref angularVelocity);
		}
		// Read data from server
		else if(Network.isClient)
		{
			stream.Serialize(ref pos);
			stream.Serialize(ref velocity);
			stream.Serialize(ref rot);
			stream.Serialize(ref angularVelocity);

			GetComponent<Rigidbody>().position = pos;
			GetComponent<Rigidbody>().rotation = rot;
			GetComponent<Rigidbody>().velocity = velocity;
			GetComponent<Rigidbody>().angularVelocity = angularVelocity;

		}
	}
}
