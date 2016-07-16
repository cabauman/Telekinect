using UnityEngine;
using System.Collections;

[RequireComponent (typeof(NetworkView))]
public class HandFSM : MonoBehaviour 
{
    //Reference to the user tracker
    public OpenNIUserTracker       userTracker;

    //State variables
    public HandState		currentState;
    public int					handID; //which hand this is (0 for left, 1 for right) - might be able to get rid of this;
    public static				KinectCharacterController playerController; //reference to the player controller (set by the player controller)
    public bool					isPointLooking = false;
    public HandFSM			otherHand;  //the right will know what the left is up to....
	public Shield				shield;	 //reference to the shield if this hand has one
	public GameObject		effectHandSwipe; //reference to the hand swipe effect.

    //Kinect space joint positions
    public static int		userID;  //The OpenNI ID of the user
    public Vector3		currentPosKS;  //current hand position
    public Vector3		prevPosKS;    //hand position in the previous frame
    public Vector3		centroidOffset;  //'centroid' for the hand (used in TK)
	public Vector3		handVelocityKS ; //kinect space velocity of the hand
	public bool				isSwiping;
	
    //Pointing search data (for ray / sphere casting)
    private static float    cosFOV;
    private static float    searchRadius;        

    //Geometric data for pointing / seleting / TK / throwing
    public Vector3			shoulderOffset;	 //vector offset from head to shoulder, one for each hand

    public float					pointTime;	 //timer for selecting

    //objects we are manipulating
    public GameObject			pointed;
    public GameObject			selected;
	public Vector3				TKmove;

    //GUI / HUD elements
    public Material			reticle;	 //reference to the reticle to use for this hand
    public Vector3			reticleScreenPoint;
	public Rect					reticleRect;
	public bool					drawReticle;

    public static HotValues hotValues;	//used to hold a reference to the HotValues object for the game


	// Use this for initialization
	void Start () 
    {
        if(userTracker == null) userTracker = OpenNIUserTracker.Instance();

        currentState = HandIdleState.Instance();

        if(hotValues == null) hotValues = HotValues.Instance();

		reticleRect = new Rect(0, 0, Screen.width / 10.0f, Screen.width / 10.0f);

		drawReticle = false;

        cosFOV = Mathf.Cos(hotValues.fieldOfView);        //precalc cosine of vield of view
        searchRadius = hotValues.maxSearchDistance * Mathf.Tan(hotValues.fieldOfView);  //defines the radius of the sphere to cast when searching

		GetComponent<NetworkView>().observed = this;

	}

    //Don't use MonoBehavior::Update() - instead, allow the playerController to control the update for the hands
	public void HandUpdate () 
    {
		prevPosKS = currentPosKS ;
        currentPosKS = userTracker.GetHandPos(userID, handID);
		handVelocityKS = ( currentPosKS - prevPosKS ) / (Time.deltaTime / Time.timeScale) ;
		isSwiping = handVelocityKS.sqrMagnitude >= hotValues.swipeThresholdVelocity * hotValues.swipeThresholdVelocity;
		currentState.OnUpdate(this);
	}

    //Search for valid objects (objects we can select and or do TK on)
    public void Point()
    {		
        int moveableLayer   = 0x2000;
        int selectableLayer = 0x0400;
		int projectileLayer = 0x20000;

		Vector3 pointingDirection = Vector3.zero;
		Vector2 pointingAxes;
		pointingAxes = userTracker.GetPointingDirection(userID, ref pointingDirection, handID);

		reticleScreenPoint.x = hotValues.screenHalfWidth + pointingAxes.x * hotValues.screenHalfWidth;
		reticleScreenPoint.y = Screen.height - (hotValues.screenHalfHeight + pointingAxes.y * hotValues.screenHalfHeight);

		Ray pointingRay = Camera.main.ScreenPointToRay( reticleScreenPoint );
		RaycastHit limit;
		float distance;
		if ( Physics.Raycast( pointingRay, out limit, hotValues.maxSearchDistance ) )//, moveableLayer | selectableLayer ) )
		{
		    distance = limit.distance;
		}
		else
		    distance = hotValues.maxSearchDistance;

		reticleRect = new Rect(reticleScreenPoint.x - (reticleRect.width / 2),
							   Screen.height - (reticleScreenPoint.y + ( reticleRect.width / 2 )),
							   reticleRect.width,
							   reticleRect.height );

		Debug.DrawRay(pointingRay.origin, pointingRay.direction, Color.green);
		
		if(shield != null)
			return;

		//Do the sphere cast
		RaycastHit[] potentials;
		potentials = Physics.SphereCastAll(pointingRay,
		                      searchRadius,
		                      distance,
		                      moveableLayer | selectableLayer | projectileLayer );


		//Didn't find any valid objects in the sphere cast - quit (early out)
		if(potentials.Length == 0)
		{
		    if (pointed != null) //reset any objects we were pointing at before
		    {
		        pointed = null;
		        //set reticle to blue
		        reticle.color = Color.blue;
		        //reset selection GUITexture alpha cutoff
		     }

		    return;
		}

		float closestSoFar = Mathf.Infinity;
		GameObject target = null;

		foreach (RaycastHit potential in potentials)
		{
		    Vector3 toTarget = potential.transform.position - Camera.main.transform.position;
            
		    //Only objects that are within our field of view are valid
		    if (cosFOV > Vector3.Dot(pointingRay.direction, toTarget.normalized))
		        continue;

		    else
		    {
		        //See if it's the closest
		        if (toTarget.sqrMagnitude < closestSoFar)
		        {
		            closestSoFar = toTarget.sqrMagnitude;
		            target = potential.collider.gameObject;
		        }
		    }
		}
        
        if(target != null) //found something
        {
            if(pointed == null) //new target - no prior target
            {
                pointed = target;
                pointTime = 0.0f;   //reset the timer

                if(((1 << pointed.gameObject.layer) & moveableLayer) != 0 || 
                   ((1 << pointed.gameObject.layer) & projectileLayer) != 0)
                {
                    //Change reticle color to blue, and shader on object
                    reticle.color =  Color.blue;
                }
                else
                {
                    //change reticle color to red
                    reticle.color =  Color.red;
                }
            }
            else if( target != pointed ) //Pointing at something new
            {
                pointTime = 0.0f;
                pointed = target;

                if(((1 << pointed.gameObject.layer) & moveableLayer) != 0) //New object is moveable
                {
                    //Change object's shader, and change reticle to blue
                    reticle.color =  Color.blue;
                }
                else  //New object is not moveable
                {
                    //change reticle color to red
                    reticle.color =  Color.red;
                }
            }
        }
        //didn't find anything - reset any objects we were pointing at before
        else if (pointed != null) //&& target == null
        {
            pointed = null;
            //set reticle to blue
            reticle.color =  Color.blue;
            //reset selection GUITexture alpha cutoff
            reticle.SetFloat("_Cutoff", 0.99f);
        }
    }

	//Pointing method for when the menu is active
    public void MenuPoint()
    {
        int menuLayer = 1 << 16;

        RaycastHit target;

		Vector3 pointingDirection = Vector3.zero;
		Vector2 pointingAxes;
		pointingAxes = userTracker.GetPointingDirection(userID, ref pointingDirection, handID);

		reticleScreenPoint.x = hotValues.screenHalfWidth + pointingAxes.x * hotValues.screenHalfWidth;
		reticleScreenPoint.y = Screen.height - (hotValues.screenHalfHeight + pointingAxes.y * hotValues.screenHalfHeight);

		Ray pointingRay = Camera.main.ScreenPointToRay( reticleScreenPoint );

		if ( Physics.Raycast( pointingRay, out target, hotValues.maxSearchDistance, menuLayer ) )
		{
            if((pointed == null) || (pointed != target.collider.gameObject))
            {
                pointed = target.collider.gameObject;
                pointTime = 0.0f;
            }
		}
		else
			pointed = null;

		reticleRect = new Rect(reticleScreenPoint.x - reticleRect.width / 2,
							   Screen.height - ( reticleScreenPoint.y + (reticleRect.width / 2) ),
							   reticleRect.width,
							   reticleRect.height );


        reticle.SetFloat("_Cutoff", 0.99f);
    }


    public void ChangeState(HandState newState)
    {
        currentState.OnExit(this);
        currentState = newState;
        currentState.OnEnter(this);
    }

    public void OnGUI()
    {
		if( ! this.enabled )
			return;

        if(userID != 0)
        {
            //Vector3 headPos = HandFSM.playerController.headStartPosKS + HandFSM.playerController.headPosKS;
			GUILayout.BeginArea (new Rect (0, 100 + (100 * handID), 300, 300));
			//
			GUILayout.Box(string.Format("Hand {0} Velocity: {1}", handID.ToString(),
								handVelocityKS.ToString()));


            GUILayout.Box(string.Format("Hand {0} CurrentState: {1}", handID.ToString(), 
            							 currentState.ToString()));

            GUILayout.Box(string.Format("Hand {0} Swiping {1}", handID.ToString(),
            	isSwiping.ToString()));

            //    if(pointed != null)
            //        GUILayout.Box(string.Format("Hand {0} Pointed {1}", handID.ToString(),
            //            pointed.ToString()));

            //    if(selected != null)
            //        GUILayout.Box(string.Format("Hand {0} Selected {1}", handID.ToString(),
            //            selected.ToString()));

            GUILayout.EndArea();

			if ( drawReticle && Event.current.type.Equals(EventType.Repaint))
			{
				Graphics.DrawTexture( reticleRect, reticle.mainTexture, reticle );
			}
        }
    }

	[RPC]
	public void ClientTKMove( NetworkViewID TKmoveViewID, Vector3 remoteTKmove, NetworkMessageInfo info )
	{
		//this doesn't do anything on a client - should only be called on the server

		if(Network.isServer)
		{

			//if( TKmoveViewID )
			//{
			//    this.selected = null;
			//    return;
			//}

	        if( this.selected == null || this.selected.GetComponent<NetworkView>().viewID != TKmoveViewID)
	        {
	            NetworkView selectedView = NetworkView.Find(TKmoveViewID);
				if(selectedView == null)
				{
					this.selected = null;
					return;
				}

	            this.selected = selectedView.gameObject;
				this.selected.GetComponent<NetworkRigidbodyServer>().TKactive = true;
	        }

			Vector3 newVelocity = remoteTKmove / selected.GetComponent<Rigidbody>().mass;
	        this.selected.GetComponent<NetworkRigidbodyServer>().TKvelocity = newVelocity;
			this.selected.GetComponent<Rigidbody>().velocity = newVelocity;
		}
	}

	[RPC]
	public void ClientStopTK( NetworkMessageInfo info )
	{
		if(Network.isServer)
		{
			this.selected.GetComponent<NetworkRigidbodyServer>().TKactive = false;
			this.selected = null;	
		}
	}

	[RPC]
	public void ClientSwipe( Vector3 remoteHandVelocity, NetworkMessageInfo info )
	{
		if(Network.isServer)
		{
			//collide against projectiles, moveables and selectables
			int layerMask = (1 << 17) | (1 << 13) | (1 << 10);
			Vector3 swipePos = HandFSM.playerController.gameObject.transform.position + HandFSM.playerController.gameObject.transform.forward * HotValues.Instance().swipeCtr;
			Collider[] swiped = Physics.OverlapSphere(swipePos,
								  					  HotValues.Instance().swipeRadius,
													  layerMask);
			
			foreach(Collider collider in swiped)
			{
				collider.GetComponent<Rigidbody>().velocity = remoteHandVelocity / (collider.GetComponent<Rigidbody>().mass * 2.0f);
			}				
		}
	}
	[RPC]
	public void StartSheild( NetworkViewID shieldID, NetworkMessageInfo info )
	{
		GameObject GO = NetworkView.Find(shieldID).gameObject;
		this.shield = GO.GetComponent<Shield>() ;
			
		this.shield.gameObject.transform.parent = this.gameObject.transform ;
		this.shield.gameObject.transform.localScale = Vector3.one ;
		this.shield.gameObject.transform.localEulerAngles = new Vector3 ( 90.0f, 0.0f, 0.0f ) ;
		this.shield.gameObject.transform.localPosition = Vector3.zero ;
	}

	[RPC]
	public void ShieldTransfer( NetworkMessageInfo info )
	{
		if(this.shield == null)
			return;

		this.shield.isTransferring = true ;
		this.shield.gameObject.transform.parent = this.otherHand.transform ;
		this.otherHand.shield = this.shield ;
		this.shield.gameObject.transform.localEulerAngles = new Vector3(  90.0f, 0.0f, 0.0f ) ;

		this.shield = null;
	}

	[RPC]
	public void HammerHit(  float hitPower, NetworkMessageInfo info )
	{
		GameObject go = (GameObject)Network.Instantiate(Resources.Load("Projectile_TKHammer"), this.transform.root.position - Vector3.up, this.transform.root.localRotation, 0);

		Projectile_Script hammerProjectile = go.GetComponent<Projectile_Script>();
		hammerProjectile.projectileDamage = hitPower * 110.0f;
		hammerProjectile.explosionRadius = hitPower * 10.0f;
		hammerProjectile.creator = this.transform.root.gameObject;
	}

}//End of HandFSM Class

//HandState interface
abstract public class HandState
{
    public abstract void OnEnter(HandFSM hand);
    public abstract void OnUpdate(HandFSM hand);
    public abstract void OnExit(HandFSM hand);
}
