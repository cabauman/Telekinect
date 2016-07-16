using UnityEngine;
using System.Collections;

public class KinectCharacterController : MonoBehaviour 
{

    public OpenNIUserTracker    userTracker;
	public OpenNISkeleton       skeleton;
    
    public Transform            camMount;

    public int                  userID;

    //Head info
	public Vector3				prevHeadPosKS;
	public float				headVelocity;
    public Vector3              headPosKS;
    public Vector3              headDeltaKS;
    public Vector3              headStartPosKS;
	public Vector3 				moveVector;
    public bool                 tracking = false;
	public bool					isGrounded = true;
	public bool					canMove = true;

    //Hands
    public HandFSM[]            hands = new HandFSM[2];
	
    //Reference to HotValues
    public HotValues            hotValues;

	//Player's health and telekinetic energy
    public float                health = 100.0f;
	public float				TKenergy = 100.0f;

    public float				aspectRatio;
	
	static int					slowTimeCount = 0;
	
	//Animation tracks
	public string[]				anims;
	public GameObject			animatedModel;	
	public int		 			currentAnim;
	public float				animSpeed = 1.0f;

	//HUD textures / material, adjustment and placement info
		
		//Health Indication
		public Material             screenVeins;

		//Telekinetic energy indication
		public Material			TKenergyBar;
		public Material			TKenergyBarChaser;
		public Material			TKenergyBarFrame;
		public Rect				TKenergyBarRect;
		public float			TKenergyChaser = 100.0f;	
		public bool				TKRegenOn = true;

		//head position indication
		public Material			HeadPositionIndicator;
		public Material			HeadPositionIndicatorFrame;
		public Rect				HeadPositionIndicatorRect;
		public Rect	            HeadPositionIndicatorFrameRect;
		public Vector2			HeadPositionIndicatorOffset;

	    public GameObject		HUDCompassObject;

    //Manually set spawn point for quick testing by dragging spawn point prefabs onto this variable
    public CheckPoint			manualSpawnPoint;

	void Start () 
    {
		DontDestroyOnLoad(this.gameObject);
		
		hands[0].otherHand = hands[1];
        hands[0].handID = 0;
        hands[1].otherHand = hands[0];
        hands[1].handID = 1;

		if( Network.peerType != NetworkPeerType.Disconnected && ! GetComponent<NetworkView>().isMine )
		{
			//remote player
			//disable the kinect controller

			hands[0].enabled = false;
			hands[1].enabled = false;
			this.enabled = false;
			
			this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll; 
			
			return;
		}

	    if(userTracker == null) userTracker = OpenNIUserTracker.Instance();
        if(userTracker != null)
		{
			userTracker.enabled = true;
        	userTracker.MaxCalibratedUsers = 1;
		}

        headStartPosKS = headPosKS = prevHeadPosKS = Vector3.zero;
        tracking = false;

        HandFSM.playerController = this;

        if(hotValues == null) hotValues = HotValues.Instance();

        hotValues.screenHalfHeight = Screen.height / 2;
        hotValues.screenHalfWidth = Screen.width / 2;

		TKenergyBarRect = new Rect(0, Screen.height / 3, Screen.width / 40, Screen.height / 3);

		float HPwidth = Screen.width / 10;
		HeadPositionIndicatorFrameRect = new Rect(Screen.width - HPwidth, 0, HPwidth, HPwidth);
		HPwidth = HeadPositionIndicatorFrameRect.width / 5.0f;
		HeadPositionIndicatorRect = new Rect( (HeadPositionIndicatorFrameRect.xMin + (HeadPositionIndicatorFrameRect.width / 2)) - (HPwidth / 2),
											  (HeadPositionIndicatorFrameRect.width / 2) - (HPwidth / 2),
											  HPwidth,
											  HPwidth );

		HeadPositionIndicatorOffset = Vector2.zero;
		
	}
	
	void Update() 
    {

		// look for a new userId if we dont have one
		if (0 == userID)
		{
			// just take the first calibrated user, then start the game
			if (userTracker != null && userTracker.CalibratedUsers.Count > 0)
			{
				userID = userTracker.CalibratedUsers[0];
               // Application.LoadLevel(2);
			}
		}
		
		// update our skeleton based on active user id
		else
		{
            // is the user still valid?
			if (!userTracker.CalibratedUsers.Contains(userID)) //No longer valid
			{
				HandFSM.userID = userID = 0;
                tracking = false;
				skeleton.RotateToCalibrationPose();

                return;
    		}

            //user just started being tracked
            if (tracking == false) 
            {
                headStartPosKS = prevHeadPosKS = userTracker.GetHeadPos(userID);
                tracking = true;
                hotValues.pointingRadius = userTracker.GetArmLength(userID) * 0.75f ;
                hotValues.pointingFloorOffset =  userTracker.GetHipPos(userID).y - headStartPosKS.y;
                hands[0].shoulderOffset = userTracker.GetShoulderPos(userID, 0) - headStartPosKS;
                hands[1].shoulderOffset = userTracker.GetShoulderPos(userID, 1) - headStartPosKS;
                HandFSM.userID = userID;

            }
			else if(canMove)
			{
				//see if the player is on the ground
				bool prevGround = isGrounded;
				isGrounded = Physics.Raycast(gameObject.transform.position, -transform.up, 1.2f);
				if(!isGrounded && prevGround)
				{
					animatedModel.GetComponent<Animation>().CrossFade(anims[2]);
					currentAnim = 2;
				}
				Debug.DrawLine(gameObject.transform.position, gameObject.transform.position - transform.up * 1.1f, Color.green);
				
				//Head-based movement
                headPosKS = userTracker.GetHeadPos(userID);
				headVelocity = headPosKS.y - prevHeadPosKS.y;
				prevHeadPosKS = headPosKS;
                headDeltaKS = headPosKS - headStartPosKS;
				
				//update the head position indicator
				HeadPositionIndicatorOffset.x = (headDeltaKS.x * hotValues.moveSensitivityX) / 20.0f;
				HeadPositionIndicatorOffset.y = (-headDeltaKS.z * hotValues.moveSensitivityZ) / 20.0f; //y is inverted in screen space
				//isGrounded = Physics.Raycast(gameObject.transform.position, -transform.up, 1.2f);		//check if we are close enough to the ground (something is directly below us)

                moveVector = Vector3.zero ;
				
				moveVector.x = Mathf.Max(0.0f, Mathf.Abs(headDeltaKS.x) - hotValues.moveDeadZoneX) * Mathf.Sign(headDeltaKS.x) * (0.1f * hotValues.moveSensitivityX);
				moveVector.z = Mathf.Max(0.0f, Mathf.Abs(headDeltaKS.z) - hotValues.moveDeadZoneZ) * Mathf.Sign(headDeltaKS.z) * (0.1f * hotValues.moveSensitivityZ);
				moveVector = Vector3.ClampMagnitude(moveVector, hotValues.maxMovementSpeed);
				
				if(isGrounded)
				{
					//If we're moving, check for slope
					if (moveVector.x + moveVector.z != 0.0f)
					{
						//Figure out which animation to play
						float moveMag = new Vector2(moveVector.x, moveVector.z).magnitude;
						if(moveMag > 40.0f)
						{
							if(currentAnim != 1)
							{
								animatedModel.GetComponent<Animation>().CrossFade(anims[1], 0.3f);
								currentAnim = 1;
							}
						}
						else if(currentAnim != 0)
						{
							animatedModel.GetComponent<Animation>().CrossFade(anims[0], 0.3f);
							currentAnim = 0;
						}
						
						animSpeed = moveMag / 40.0f;
						animatedModel.GetComponent<Animation>()[anims[currentAnim]].speed = animSpeed;
						
						//Transform move vector to local space
						moveVector.y = 0.0f;
						moveVector = gameObject.transform.TransformDirection(moveVector);
						//look vector is from the center of the character, to where the move vector is pointed along the ground
						Vector3 lookVector = gameObject.transform.position + moveVector.normalized;
						lookVector.y -= 1.0f; //The center of the character is 1.0m above his feet
				
						int everythingButShield = ~(32772);
						RaycastHit hitInfo;
						if (Physics.Raycast(gameObject.transform.position, lookVector - gameObject.transform.position, out hitInfo, 2.0f, everythingButShield))
						{
						    //If we've hit something, check its normal against the world up vector
						    //if the angle is less than MaxSlope, go ahead and move
						    //cosine of an angle less than 90 increases as the angle gets smaller, so we check to make sure the dot product
						    //is greater than the maximum slope : (cos(theta) > cos(MaxSlope))
						   Debug.DrawLine(gameObject.transform.position, lookVector);

						   if (Vector3.Dot(hitInfo.normal, Vector3.up) > hotValues.maxSlope)
						   {
						       //gameObject.rigidbody.velocity = new Vector3(moveVector.x, gameObject.rigidbody.velocity.y, moveVector.z);
								this.gameObject.GetComponent<Rigidbody>().AddForce(moveVector, ForceMode.Acceleration );
						   }
						}
						else //nothing in front of us - we are in the air or standing on a cliff or steep slope, go ahead and move
							this.gameObject.GetComponent<Rigidbody>().AddForce(moveVector, ForceMode.Acceleration );
							//gameObject.rigidbody.velocity = new Vector3(moveVector.x, gameObject.rigidbody.velocity.y, moveVector.z);
						
						Vector3 maxGroundVelocity = Vector3.ClampMagnitude( this.GetComponent<Rigidbody>().velocity, moveVector.magnitude * 0.05f );
						maxGroundVelocity.y = this.gameObject.GetComponent<Rigidbody>().velocity.y;
						this.gameObject.GetComponent<Rigidbody>().velocity = maxGroundVelocity;
					}
					else
						animatedModel.GetComponent<Animation>().Stop();
								
					//jump (if there is something under us to jump off of and the menu isn't active)
					if (headVelocity > 20.0f && ! hotValues.menu.gameObject.active && isGrounded)
					{
						gameObject.GetComponent<Rigidbody>().AddRelativeForce(gameObject.transform.up * hotValues.jumpForce, ForceMode.Impulse);
					}
				}
				else if( currentAnim != 2 )
				{
					currentAnim = 2;
					animatedModel.GetComponent<Animation>().CrossFade(anims[2]);
				}

			} //end if(canMove)
			
			//Update pointing / selection 
			for(int currentHand = 0; currentHand < 2; currentHand++)
				hands[currentHand].HandUpdate();

            //Update camera
            for(int currentHand = 0; currentHand < 2; currentHand++)
            {

				float dT = Time.deltaTime / Time.timeScale;

                if(hands[currentHand].isPointLooking)
                {
					//Don't move the camera if the menu is up
					if(hotValues.menu.gameObject.active)
					{
						//Make sure the CamMount is rotated back to zero
						camMount.transform.localRotation = Quaternion.Slerp(camMount.transform.localRotation, Quaternion.identity, dT);
						Vector3 camMountTargetPos = new Vector3(0.0f, 1.35f, 0.0f);
						camMount.transform.localPosition = Vector3.Lerp(camMount.transform.localPosition, camMountTargetPos, dT);						
					}
					//camera for tracking selected target
                    else if(hands[currentHand].selected != null) //tracking a selected target
                    {
                        //If pointLookingHand is left (0), camMount is on the left (-0.394), if it's right (1) cammount is on the right (+0.394);
                        //Location is relative to parent
                        Vector3 camMountTargetPos = new Vector3(-0.3f + (0.6f * currentHand), 1.35f, 0.0f);
                        camMount.transform.localPosition = Vector3.Lerp(camMount.transform.localPosition, camMountTargetPos, dT);

                        Quaternion targetRotation = Quaternion.LookRotation(hands[currentHand].selected.transform.position - gameObject.transform.position);
                        Quaternion camMountRotation = Quaternion.LookRotation(hands[currentHand].selected.transform.position - camMount.transform.position);

                        camMountRotation.y = 0.0f; //rotate only on X (pitch)
                        camMountRotation.z = 0.0f;

                        targetRotation.z = 0.0f; //rotate only on Y (yaw)
                        targetRotation.x = 0.0f;

                        gameObject.transform.rotation = targetRotation; //Quaternion.Slerp(gameObject.transform.rotation, targetRotation, dT);    //rotate character to face object
                        camMount.transform.localRotation = camMountRotation; //Quaternion.Slerp(camMount.transform.localRotation, camMountRotation, dT);
                    }
					else //tracking where hand is pointed
                    {

                        //If pointLookingHand is left (0), camMount is on the left (-0.394), if it's right (1) camMount is on the right (+0.394);
                        //Location is relative to parent
                        Vector3 camMountTargetPos = new Vector3(-0.3f + (0.6f * currentHand), 1.35f, 0.0f);
                        camMount.transform.localPosition = Vector3.Lerp(camMount.transform.localPosition, camMountTargetPos, dT);

                        //Vector3 pointingRay = new Vector3(0.0f, 0.0f, 0.0f);
                        //Vector2 pointingVec = userTracker.GetPointingDirection(userID, ref pointingRay, currentHand);
                        Vector2 pointingVec = hands[currentHand].reticleScreenPoint;
                        pointingVec.x = (float)(pointingVec.x - hotValues.screenHalfWidth) / (float)hotValues.screenHalfWidth;
                        pointingVec.y = (float)(pointingVec.y - hotValues.screenHalfHeight) / (float)hotValues.screenHalfHeight;

                        pointingVec.x = Mathf.Clamp(pointingVec.x, -1.0f, 1.0f);
                        pointingVec.y = (pointingVec.y < -1.0f || pointingVec.y > 1.0f) ? 0.0f : pointingVec.y;

                        float pointingXmag = Mathf.Abs(pointingVec.x);
                        float deltaPointingX = pointingXmag - hotValues.pointDeadZone;

                        float pointingYmag = Mathf.Abs(pointingVec.y);
                        float deltaPointingY = pointingYmag - hotValues.pointDeadZone;

                        if (deltaPointingX > 0.0f)
                        {
                            //rotate object based on arm position
                            deltaPointingX = (pointingVec.x / pointingXmag) * deltaPointingX;
                            gameObject.transform.Rotate(Vector3.up * (deltaPointingX * hotValues.pointSensitivity) * (hotValues.maxRotationSpeed * dT));
                        }

                        if (deltaPointingY > 0.0f)
                        {
                            //rotate cam mount based on arm position
                            deltaPointingY = (pointingVec.y / pointingYmag) * deltaPointingY;
                            camMount.transform.Rotate(Vector3.left * deltaPointingY * hotValues.pointSensitivity * (hotValues.maxRotationSpeed * dT));
							
							float camVerticalAngle = camMount.transform.rotation.eulerAngles.x;
							if(camVerticalAngle < 320.0f && camVerticalAngle > 20.0f)
							{
								if(Vector3.Dot (camMount.transform.forward, Vector3.up) < 0.0f)
									camVerticalAngle = 20.0f;
								else
									camVerticalAngle = 320.0f;
							}
							Vector3 eulerAngles = new Vector3(camVerticalAngle, camMount.transform.rotation.eulerAngles.y, camMount.transform.rotation.eulerAngles.z);
							camMount.transform.rotation = Quaternion.Euler(eulerAngles);
                        }
                    }

                    break; //break out early - only one hand can be pointlooking at a time
                }
                else if(currentHand == 1 && ! hands[0].isPointLooking)   //neither hand pointing nor tracking a target
                {
                    //Make sure the CamMount is rotated back to zero
                    camMount.transform.localRotation = Quaternion.Slerp(camMount.transform.localRotation, Quaternion.identity, hotValues.maxRotationSpeed * dT);
					Vector3 camMountTargetPos = new Vector3(0.0f, 1.35f, 0.0f);
					camMount.transform.localPosition = Vector3.Lerp(camMount.transform.localPosition, camMountTargetPos, dT);
                }
            }

			//Update the compass

			//HUDCompassObject.transform.eulerAngles = new Vector3(0.0f, 107.9405f, 0.0f);
			//HUDCompassObject.transform.localPosition = new Vector3(0.0f, 0.55f, 1.0f);
			//HUDCompassObject.transform.localEulerAngles = new Vector3(0.0f, 107.9405f, 0.0f);
			//HUDCompassObject.transform.eulerAngles = new Vector3(0.0f, 107.0f, 0.0f);

			//HUDCompassObject.transform.position = Camera.main.transform.position;
			//HUDCompassObject.transform.rotation = Camera.main.transform.rotation;
			//HUDCompassObject.transform.Translate(new Vector3(0.0f, 0.55f, 1.0f));
			//HUDCompassObject.transform.Rotate(Vector3.up, 107.9405f, Space.Self);
        }

		//replenish health over time
		if (health > 100.0f)
			health = 100.0f;
		else if(health < 100.0f)
		{
			//see if we've died, if so respawn
			if (health <= 0.0f)
			{
				//If dead respawn to the current spawn point and reset health
				this.gameObject.transform.position = hotValues.spawnPoint.GenerateSpawnPoint(); 
				this.gameObject.transform.rotation = hotValues.spawnPoint.transform.rotation;
				health = 100.0f;
			}

			health += 5.0f * Time.deltaTime;

		}

		//replenish TK energy over time
		if(TKenergy > 100.0f)
		{
			TKenergy = 100.0f;
			TKenergyChaser = 100.0f;
		}
		else if( TKRegenOn && TKenergy < 100.0f)
		{
			TKenergy = TKenergy < 0.0f ? 0.0f : TKenergy;

			TKenergy += 10.0f * Time.deltaTime;

			TKenergyChaser = (TKenergyChaser > TKenergy) ? TKenergyChaser - (35.0f * Time.deltaTime) : TKenergy;
			TKenergyChaser = TKenergyChaser < 0.0f ? 0.0f : TKenergyChaser;
		}

		//Update HUD overlays with alpha cutoff
		screenVeins.SetFloat("_Cutoff", (health / 100.0f));
		TKenergyBar.SetFloat("_Cutoff", 1.0f - (TKenergy / 100.0f));
		TKenergyBarChaser.SetFloat("_Cutoff", 1.0f - (TKenergyChaser / 100.0f));

	}//Update()

	//do procedural animations (Kinect input) in LateUpdate - after other animations have been applied.
	void LateUpdate()
	{
		if(userID != 0)
		{		
			userTracker.UpdateSkeleton(userID, skeleton);
		}
	}
	
	void OnCollisionEnter(Collision collision)
    {

    }

    void OnLevelWasLoaded( int level )
    {
		if( ! this.enabled )
			return;

		canMove = true;
	
        if (manualSpawnPoint != null)
		{
			hotValues.spawnPoint = manualSpawnPoint;
		}
        else
		{
			hotValues.spawnPoint = GameObject.Find("SpawnPoint").GetComponent<CheckPoint>();	
		}

		this.gameObject.transform.position = hotValues.spawnPoint.GenerateSpawnPoint();
		this.gameObject.transform.rotation = hotValues.spawnPoint.transform.rotation;
		
		//Set the main camera to our camera mount
        Camera.main.GetComponent<CameraSmoothFollow>().target = camMount;

        //Create the pause menu for this level
		hotValues.menu = GameObject.Find("PauseMenu").GetComponent<Menu>();
		hotValues.menu.transform.parent = this.gameObject.transform;
		hotValues.menu.transform.localPosition = Vector3.zero;
		hotValues.menu.transform.localRotation = Quaternion.identity;
		hotValues.menu.gameObject.active = false;
			
    }

	void OnGUI()
	{
		if( ! this.enabled )
			return;

		if (userID == 0 && userTracker != null)
		{
			if (userTracker.CalibratingUsers.Count > 0)
			{
				// Calibrating
				GUILayout.Box(string.Format("Calibrating: {0}", userTracker.CalibratingUsers[0]));
			}
			else
			{
				// Looking
				GUILayout.BeginArea (new Rect (Screen.width/2 - 150, Screen.height/2 - 150, 300, 300));
				GUILayout.Box("Waiting for single player to calibrate");
				GUILayout.EndArea();
			}
		}
		else
		{
			// Calibrated
            //Vector3 dummy = new Vector3();
			GUILayout.Box(string.Format("Head Velocity: {0}", headVelocity.ToString()));
            //GUILayout.Box(string.Format("Pointing Direction: {0}", UserTracker.GetPointingDirection( userId,ref dummy ).ToString() ) );
            //GUILayout.Box(string.Format("Head Position: {0}", HeadPos.ToString() ) );
            //GUILayout.Box(string.Format("Pointing Radius: {0}", hotValues.pointingRadius.ToString()));
            //GUILayout.Box(string.Format("Pointing Floor Offset: {0}", hotValues.pointingFloorOffset.ToString()));
            //GUILayout.Box(string.Format("Throwing Offset: {0}", hotValues.throwingOffset.ToString())); 
            //GUILayout.Box(string.Format("Position: {0}", gameObject.transform.position.ToString() ) );
            //GUILayout.Box(string.Format("Hand Pointing Pos: {0}", leftHandPointingPos.ToString()));
			//GUILayout.Box(string.Format("TKenergy: {0}", TKenergy.ToString()));
			//GUILayout.Box(string.Format("IsGrounded: {0}", isGrounded.ToString()));
			//GUILayout.Box(string.Format("Head Y position: {0}", headPosKS.y.ToString()));
			//GUILayout.Box(string.Format("Velocity: {0}", this.rigidbody.velocity.ToString()));
			//GUILayout.Box(string.Format("Move Vector: {0}", this.moveVector.ToString()));

			//Draw the HUD objects
            if (Event.current.type.Equals(EventType.Repaint))
            {
				//health indicator
                Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), screenVeins.mainTexture, screenVeins);
                
				//telekinetic energy indicator
				Graphics.DrawTexture(TKenergyBarRect, TKenergyBarChaser.mainTexture, TKenergyBarChaser);
				Graphics.DrawTexture(TKenergyBarRect, TKenergyBar.mainTexture, TKenergyBar);
				Graphics.DrawTexture(TKenergyBarRect, TKenergyBarFrame.mainTexture, TKenergyBarFrame);

				//head position indicator
				Graphics.DrawTexture(HeadPositionIndicatorFrameRect, HeadPositionIndicatorFrame.mainTexture, HeadPositionIndicator);
				Graphics.DrawTexture( new Rect(HeadPositionIndicatorRect.xMin + HeadPositionIndicatorOffset.x,
											   HeadPositionIndicatorRect.yMin + HeadPositionIndicatorOffset.y,
											   HeadPositionIndicatorRect.width,
											   HeadPositionIndicatorRect.height), 
									  		   HeadPositionIndicator.mainTexture, HeadPositionIndicator);
            }
		}
	}

	[RPC]
	void DamageClientPlayer( float damage, NetworkMessageInfo info )
	{
		this.health -= damage;
	}

	[RPC]
	void PushClientPlayer( float power, Vector3 location, float radius, NetworkMessageInfo info )
	{
		this.GetComponent<Rigidbody>().AddExplosionForce(power, location, radius, 1.5f, ForceMode.Impulse);
	}
	
	[RPC]
	public void ExitToTitle()
	{
		if(Network.isClient)
		{
			Network.RemoveRPCs( HandFSM.playerController.GetComponent<NetworkView>().viewID.owner );
			
			Network.Destroy( HandFSM.playerController.hands[0].GetComponent<NetworkView>().viewID );
			
			Network.Destroy( HandFSM.playerController.hands[1].GetComponent<NetworkView>().viewID );
			
			Network.Destroy( HandFSM.playerController.GetComponent<NetworkView>().viewID );

		}
		else if(Network.isServer)
		{
			HandFSM.playerController.GetComponent<NetworkView>().RPC("ExitToTitle", RPCMode.Others);
			
			Network.RemoveRPCs( HandFSM.playerController.GetComponent<NetworkView>().viewID.owner );
			Network.Destroy( HandFSM.playerController.GetComponent<NetworkView>().viewID );
		}
		else
			Destroy( HandFSM.playerController.gameObject );

		Application.LoadLevel("Telekinect_Init");
	}
	
	[RPC]
	public void SlowTime()
	{
		slowTimeCount++;
		Time.timeScale = 0.1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; 	
	}
	
	[RPC]
	public void ResumeTime()
	{
		slowTimeCount--;
		if(slowTimeCount == 0)
		{
			Time.timeScale = 1.0f;
	        Time.fixedDeltaTime = 0.02f * Time.timeScale;	
		}
	}


}
