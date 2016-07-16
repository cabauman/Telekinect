using UnityEngine;
using System.Collections;
using OpenNI;

public class InitScript : MonoBehaviour 
{

	public string	remoteIP = "127.0.0.1";
	public int		remotePort = 25000;
	public int		listenPort = 25000;
	public string	remoteGUID = "";
	public bool		useNat = false;
	public string	connectionInfo = "";

	GameObject		localPlayer;

	// Use this for initialization
	void Start () 
    {
        Debug.Log("Startup");

        OpenNIUserTracker userTracker;
        userTracker = OpenNIUserTracker.Instance();

        if (userTracker == null)
        {
            //kinect not present / available
            //create keyboard / mouse controller

            Debug.LogWarning("Kinect not found");

			//go = GameObject.Find("KeyboardPlayerCharacter");

			//if (go == null)
			//{
			//    go = Instantiate(Resources.Load("KeyboardPLayerCharacter")) as GameObject;
			//    go.name = "KeyboardPlayerCharacter";
			//    DontDestroyOnLoad(go);
			//}
        }
        else
        {
           //Kinect found - create kinect based controller
            Debug.Log("Kinect Controller Started");

            localPlayer = GameObject.Find("KinectPlayerCharacter");

            if (localPlayer == null)
            {
                localPlayer = Instantiate(Resources.Load("KinectPlayerCharacter")) as GameObject;
                localPlayer.name = "KinectPlayerCharacter";
            }

			localPlayer.transform.position = new Vector3(0.0f, 1.0f, 0.0f);
			localPlayer.transform.rotation = Quaternion.identity;
        }


	}

	void Update()
	{
		if(localPlayer != null && localPlayer.GetComponent<KinectCharacterController>().userID != 0 && Network.peerType == NetworkPeerType.Disconnected)
		{
			Debug.Log("Single Player level loading");
			//single calibrated player, load up the level
			localPlayer.GetComponent<KinectCharacterController>().canMove = true;
			Application.LoadLevel(2);
		}
		else if(Network.connections.Length > 0)
		{
			Debug.Log("Multiplayer Level Loading");

			if(localPlayer != null)
				localPlayer.GetComponent<KinectCharacterController>().canMove = true;

			Application.LoadLevel(2);
		}
	}

	void OnGUI()
	{
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		if(Network.peerType == NetworkPeerType.Disconnected)
		{
			useNat = GUILayout.Toggle(useNat, "Use NAT punchthrough");

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);

			GUILayout.BeginVertical();
			if(GUILayout.Button("Connect"))
			{
				if(useNat)
				{
					if(remoteGUID == "")
						Debug.LogWarning("Invalid GUID given, must be a valid one as reported by Network.player.guid or returned in a HostData struture from the master server");
					else
						Network.Connect(remoteGUID);
				}
				else
				{
					Network.Connect(remoteIP, remotePort);
				}
			}

			if(GUILayout.Button("Start Server"))
			{
				Network.InitializeServer(5, listenPort, useNat);
			}

			GUILayout.EndVertical();

			if(useNat)
			{
				remoteGUID = GUILayout.TextField(remoteGUID, GUILayout.MinWidth(145));
			}
			else
			{
				remoteIP = GUILayout.TextField(remoteIP, GUILayout.MinWidth(100));
				remotePort = int.Parse(GUILayout.TextField(remotePort.ToString()));
			}
		}
		else
		{
			if (useNat)
				GUILayout.Label("GUID: " + Network.player.guid + " - ");

			GUILayout.Label("Local IP/port: " + Network.player.ipAddress + "/" + Network.player.port);
			GUILayout.Label(" - External IP/port: " + Network.player.externalIP + "/" + Network.player.externalPort);

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			if (GUILayout.Button ("Disconnect"))
				Network.Disconnect(200);
		}

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	void OnServerInitialized()
	{
		Debug.Log("Server started");

		if(OpenNIUserTracker.Instance() == null)
		{
			Debug.LogWarning("Kinect not detected!");
			//return;
		}

        localPlayer = GameObject.Find("KinectPlayerCharacter");
		Destroy(localPlayer);

		if( OpenNIUserTracker.Instance() != null )
		{
			localPlayer = Network.Instantiate(Resources.Load("KinectPlayerCharacter"), new Vector3(0.0f, 3.0f, 0.0f), Quaternion.identity, 0 ) as GameObject;
			localPlayer.name = "KinectPlayerCharacter";
			Camera.main.GetComponent<CameraSmoothFollow>().target = localPlayer.GetComponent<KinectCharacterController>().camMount;
		}

	}

	//called on the client
	void OnConnectedToServer()
	{
		Debug.Log("Connected to Server");

		if(OpenNIUserTracker.Instance() == null)
		{
			Debug.LogWarning("Kinect not detected!");
			//return;
		}

        localPlayer = GameObject.Find("KinectPlayerCharacter");
		Destroy(localPlayer);

		if( OpenNIUserTracker.Instance() != null )
		{
			localPlayer = Network.Instantiate(Resources.Load("KinectPlayerCharacter"), new Vector3(0.0f, 3.0f, 0.0f), Quaternion.identity, 0 ) as GameObject;
			localPlayer.name = "KinectPlayerCharacter";
			Camera.main.GetComponent<CameraSmoothFollow>().target = localPlayer.GetComponent<KinectCharacterController>().camMount;
		}

	}

	//called on the server
	void OnPlayerConnected( NetworkPlayer player )
	{
		Debug.Log("Player Connected");
	}

}
