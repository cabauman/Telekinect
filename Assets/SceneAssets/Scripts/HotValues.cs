using UnityEngine;
using System.Collections;

public class HotValues : MonoBehaviour 
{

    private static HotValues instance = null;


    //Character Controller hot values
    public float    moveDeadZoneX       = 120.0f;
    public float    moveDeadZoneY       = 80.0f;
    public float    moveDeadZoneZ       = 120.0f;
    public float    moveSensitivityX    = 2.5f;
    public float    moveAdjustX         = 0.0f;
    public float    moveAdjustZ         = 0.0f;
    public float    moveSensitivityZ    = 2.0f;
    public float    maxMovementSpeed    = 60.0f;
    public float    maxSlope            = Mathf.Cos(Mathf.Deg2Rad * 60.0f); //Cosine of the max slope
    public float    jumpForce           = 15.0f;
    public float    pointDeadZone       = 0.3f;
	public float    pointSensitivity    = 1.0f;
    public float    maxRotationSpeed    = 100.0f;

    //Hand FSM hot values
    public float    pointTimeOut        = 1.0f;
    public float   	swipeThresholdVelocity    = 2500.0f;
	public float	TKenergyDrain		= 0.0f;
	public float	TKenergyThrow		= 20.0f;
	public float 	TKHammerSwipeEpsilon = 1.0f ;

    public float    fieldOfView         = Mathf.Deg2Rad * 7.0f; 
    public float    maxSearchDistance   = 50.0f;

    public float    pointingRadius;	 		//radius of pointing bound (outside of this radius, the user is pointing), currently set in KinectCharacterController to .75 of arm length
    public float    pointingFloorOffset;	//y offset from head to pointing floor (below this plane the user is not pointing), currently set in KinectCharacterController to height of hip

	//Swipe sphere of influence - offset from player character (x, y, z, scale)
	public float	swipeCtr = 0.8f;
	public float	swipeRadius = 2.25f;
	public float	TKThrowSearchRadius = 10.0f;	
	//Shield HotValues
	public float shieldTKConjureCost = 10.0f ;
	public float shieldTKActiveCost = 0.0f ;
	public float shieldConjureThreshold  = 150 ; 
	public float shieldActivateThreshold = 1;

    //Current spawnpoint
    public CheckPoint spawnPoint = null;

    //Current menu
    public Menu     menu;

    //Screen dimension values
    public int      screenHalfWidth;
    public int      screenHalfHeight;

    //Create the HotValues as a singleton game object
    public static HotValues Instance ()
    {
        if( instance == null )
        {
            GameObject go = new GameObject();
            instance = go.AddComponent<HotValues>();
            go.name = "HotValues";
			DontDestroyOnLoad(go);
        }

        return instance;
    }

	// Use this for initialization
	void Start () 
    {

	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
