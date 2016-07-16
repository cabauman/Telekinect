using UnityEngine;
using System.Collections;


[RequireComponent(typeof(NetworkView))]
public class NewInitScript : MonoBehaviour
{
    // Networking
    public string remoteIP = "127.0.0.1";
    public int remotePort = 25000;
    public int listenPort = 25000;
    public string remoteGUID = "";
    public bool useNat = false;
    public bool serverReady = false;
    public bool clientReady = false;
    public string connectionInfo = "";
    public int numPlayers = 1;
    public int rdyPlayers = 0;
    

    // GUI Button Variables
    public float centerX = Screen.width / 2;
    public float centerY = Screen.height / 2;
    public float adjustX = 0;
    public float adjustY = 0;
    public float boxWidth = 250;
    public float boxWidth2 = 655;
    public float boxHeight = 150;
    public float spacing = 25;
	
	//is the kinect working?
	bool kinectIsAvailable = false;
    bool isMultiplayer = false;
	
	public bool kinectOverride = false ;

    GameObject localPlayer;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Startup");
		
		if(kinectOverride == false)
		{
	        OpenNIUserTracker userTracker;
	        userTracker = OpenNIUserTracker.Instance();
	
	        if (userTracker == null)
	        {
	            //kinect not present / available
	            Debug.LogWarning("Kinect not found");
				
				kinectIsAvailable = false;
	        }
	        else
	        {
	            //Kinect found - create kinect based controller
	            Debug.Log("Kinect Controller Started");
	
	            kinectIsAvailable = true;
	        }
		}
		else
			kinectIsAvailable = true ;
    }

    void Update()
    {

    }
    
    void OnGUI()
    {	
        GUIStyle BigFont = new GUIStyle();
		BigFont.fontSize = 23;
		BigFont.fontStyle = FontStyle.Bold;
		BigFont.normal.textColor = Color.white;
        
        GUILayout.Space(spacing);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
            
		if ( ! kinectIsAvailable)
		{
			GUILayout.BeginArea (new Rect (Screen.width/2 - 150, Screen.height/2 - 150, 300, 300));
			//GUI.color = Color.white;
			GUILayout.Label("Kinect Not Found!", BigFont);
			GUILayout.EndArea();
		}

        else if ( ! isMultiplayer && (kinectIsAvailable || kinectOverride) )
        {
            if (GUI.Button(new Rect(Screen.width - boxWidth, 0, boxWidth, boxHeight), "SINGLE PLAYER"))
            {
                StartGame();
            }


            if (GUI.Button(new Rect(0, 0, boxWidth, boxHeight), "MULTIPLAYER"))
            {
                isMultiplayer = true;
            }
			
			if (GUI.Button	(new Rect(0,Screen.height - (boxHeight) ,boxWidth	,boxHeight), "CREDITS"))
			{
				Application.LoadLevel(3);	
			}
        }
        else
        {
            if (Network.isServer)
            {

                GUI.Box(new Rect((Screen.width - boxWidth2) / 2, 0, boxWidth2, boxHeight), "MULTIPLAYER");

                GUI.Label(new Rect((Screen.width - 200) / 2, 20, 500, 20), "Local IP/port: " + Network.player.ipAddress + "/" + Network.player.port);
                GUI.Label(new Rect((Screen.width - 400) / 2, 40, 800, 20), " - External IP/port: " + Network.player.externalIP + "/" + Network.player.externalPort);


                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
				
				GUI.Label(new Rect((Screen.width - 150) / 2, 60, 250, 20), "Number of Players");
                numPlayers = int.Parse(GUI.TextField(new Rect((Screen.width + 80) / 2, 60, 20, 20), numPlayers.ToString()));
                //Server Toggle Ready Button
                if (rdyPlayers == numPlayers)
                {
                    if (GUI.Button(new Rect((Screen.width - boxWidth) / 2, (Screen.height - boxHeight) / 2, boxWidth, boxHeight), "Start Multiplayer Game"))
                    {
                        this.GetComponent<NetworkView>().RPC("StartGame", RPCMode.All);
                    }
                }
                //GUILayout.EndHorizontal();
               // GUILayout.BeginVertical();
                GUILayout.Space(200);

                if (GUI.Button(new Rect((Screen.width - 150) / 2, 80, 150, 20), "Disconnect")) 
                {
                    Network.Disconnect(200);
                }
            }
            else if (Network.isClient)
            {
				GUI.Button(new Rect((Screen.width - boxWidth) / 2, (Screen.height - boxHeight) / 2, boxWidth, boxHeight), "Connected. Waiting For Server...");
            }
            else
            {
                if (GUI.Button(new Rect(Screen.width - boxWidth, 0, boxWidth, boxHeight), "BACK TO MENU"))
                {
                    isMultiplayer = false;
                }

                GUI.Box(new Rect(0, 0, boxWidth, boxHeight), "MULTIPLAYER");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);

                GUILayout.BeginVertical();
                if (GUILayout.Button("Connect"))
                {
                    Network.Connect(remoteIP, remotePort);
                }

                if (GUILayout.Button("Start Server"))
                {
                    Network.InitializeServer(32, listenPort, useNat);
                    Network.maxConnections = 16;
                }

                GUILayout.EndVertical();


                remoteIP = GUILayout.TextField(remoteIP, GUILayout.MinWidth(100));
                remotePort = int.Parse(GUILayout.TextField(remotePort.ToString()));

            }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
    
    void OnServerInitialized()
    {

    }

    //called on the client
    
    void OnConnectedToServer()
    {

    }

    
    //called on the server
    
    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player Connected");        
        rdyPlayers++;
    }
	
	void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		if(Network.isServer)
		{
			rdyPlayers = 0;
		}
		else if(Network.isClient)
		{
				
		}
		
	}

    void OnPlayerDisconnect(NetworkPlayer Player)
    {
        Debug.Log("Player Disconnected");
        rdyPlayers--;
    }

    [RPC]
    void StartGame()
    {
        Debug.Log("Game Started");
		
		if(Network.peerType != NetworkPeerType.Disconnected)
			localPlayer = Network.Instantiate(Resources.Load("KinectPlayerCharacter"), new Vector3(0.0f, 3.0f, 0.0f), Quaternion.identity, 0 ) as GameObject;
		else
        	localPlayer = Instantiate(Resources.Load("KinectPlayerCharacter")) as GameObject;
        localPlayer.name = "KinectPlayerCharacter";

		localPlayer.transform.position = new Vector3(0.0f, 1.0f, 0.0f);
		localPlayer.transform.rotation = Quaternion.identity;
		
		Application.LoadLevel(2);
		
    }
}
