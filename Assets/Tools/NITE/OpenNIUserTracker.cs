using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using OpenNI;

public class OpenNIUserTracker : MonoBehaviour 
{
	public static OpenNIContext 	context;
	public SceneAnalyzer			sceneAnalyzer;
	public Matrix4x4				floorTransform = new Matrix4x4();
	public bool						hasFloor = false;
	public int 						MaxCalibratedUsers;
	
	private UserGenerator 			userGenerator;
	private SkeletonCapability 		skeletonCapability;
	private PoseDetectionCapability poseDetectionCapability;
	private string 					calibPose;

	private List<int> 				allUsers;
	private List<int> 				calibratedUsers;
	private List<int> 				calibratingUsers;

    private static OpenNIUserTracker instance;

    public static OpenNIUserTracker Instance()
    {
        if (instance == null)
        {

            //try to get a context
            context = OpenNIContext.Instance();

            if (! OpenNIContext.ValidContext())
            {
                Debug.Log("OpenNI not inited");
                return instance;
            }

            Debug.Log("User Tracker Instantiated");
            GameObject go = new GameObject();
            go.name = "NITE";
            instance = go.AddComponent<OpenNIUserTracker>();
			instance.sceneAnalyzer = new SceneAnalyzer(context.context);

            //Add the depthmap viewer
            go.AddComponent<OpenNIDepthmapViewer>();

        }
        return instance;
    }
	
	public IList<int> AllUsers
	{
		get { return allUsers.AsReadOnly(); }
	}
	public IList<int> CalibratedUsers
	{
		get { return calibratedUsers.AsReadOnly(); }
	}
	public IList<int> CalibratingUsers
	{
		get {return calibratingUsers.AsReadOnly(); }
	}
	
	public bool Mirror
	{
		get { return context.Mirror; }
		set { context.Mirror = value; }
	}
	
	bool AttemptToCalibrate
	{
		get { return calibratedUsers.Count < MaxCalibratedUsers; }
	}

    public void OnApplicationQuit()
    {
        instance = null;
    }

    // Use this for initialization
	void Start()
	{

        if (instance == null)
            Instance();

        calibratedUsers = new List<int>();
        calibratingUsers = new List<int>();
        allUsers = new List<int>();
        this.userGenerator = new UserGenerator(context.context);
        this.skeletonCapability = this.userGenerator.SkeletonCapability;
        this.poseDetectionCapability = this.userGenerator.PoseDetectionCapability;
        this.calibPose = this.skeletonCapability.CalibrationPose;
        this.skeletonCapability.SetSkeletonProfile(SkeletonProfile.All);

        this.userGenerator.NewUser += new EventHandler<NewUserEventArgs>(userGenerator_NewUser);
        this.userGenerator.LostUser += new EventHandler<UserLostEventArgs>(userGenerator_LostUser);
        this.poseDetectionCapability.PoseDetected += new EventHandler<PoseDetectedEventArgs>(poseDetectionCapability_PoseDetected);
        this.skeletonCapability.CalibrationComplete+= new EventHandler<CalibrationProgressEventArgs>(skeletonCapability_CalibrationComplete);

        this.skeletonCapability.SetSmoothing(0.75f);
		this.floorTransform = Matrix4x4.identity;

        DontDestroyOnLoad(this.gameObject);
	}

	void Update () 
	{
		if (OpenNIContext.ValidContext())
		{
			context.Update();
			
			if( ! hasFloor )
			{
				Plane3D floorPlane;
				
			    try
			    {
			        floorPlane = sceneAnalyzer.Floor;
			    }
			    catch (OpenNI.GeneralException /*ex*/)
			    {
			        //Debug.Log("Hasn't found the floor yet... ");
			        //Debug.Log(ex.Message);
			        return;
			    }
				
				Debug.Log("Floor found");
				
//				floorPlane = sceneAnalyzer.Floor;
//				
//				Vector3 floorNormal = new Vector3(floorPlane.Normal.X, floorPlane.Normal.Y, floorPlane.Normal.Z);
//				
//				floorTransform.SetColumn(1, floorNormal); 												//up
//				floorTransform.SetColumn(0, Vector3.Cross(floorNormal, Vector3.forward ));				//right
//				floorTransform.SetColumn(2, Vector3.Cross(floorTransform.GetColumn(0), floorNormal));	//forward
//				floorTransform.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f) );
				
				hasFloor = true;
			}
		}
	}
	
	//restart the kinect
	public void Restart()
	{
		context.Restart();
		context = null;
        //try to get a context
        context = OpenNIContext.Instance();
		this.sceneAnalyzer = new SceneAnalyzer(context.context);
		allUsers.Clear();
 		calibratedUsers.Clear();
		calibratingUsers.Clear();
		
		Destroy(this.gameObject.GetComponent<OpenNIDepthmapViewer>());
		this.gameObject.AddComponent<OpenNIDepthmapViewer>();
		
		
		this.Start();
	}

    void skeletonCapability_CalibrationComplete(object sender, CalibrationProgressEventArgs e)
    {
        if (e.Status == CalibrationStatus.OK)
        {
            if (AttemptToCalibrate)
            {
                print("Starting tracking");
                this.skeletonCapability.StartTracking(e.ID);
                calibratedUsers.Add(e.ID);
            }
        }
        else
        {
            if (AttemptToCalibrate)
            {
                this.poseDetectionCapability.StartPoseDetection(calibPose, e.ID);
            }
        }
		calibratingUsers.Remove(e.ID);
    }

    void poseDetectionCapability_PoseDetected(object sender, PoseDetectedEventArgs e)
    {
        print("Pose detected");
        this.poseDetectionCapability.StopPoseDetection(e.ID);
        if (AttemptToCalibrate)
        {
            print("Starting calibration");
            this.skeletonCapability.RequestCalibration(e.ID, true);
			calibratingUsers.Add(e.ID);
        }
    }

    void userGenerator_LostUser(object sender, UserLostEventArgs e)
    {
        allUsers.Remove(e.ID);
        if (calibratedUsers.Contains(e.ID))
        {
            calibratedUsers.Remove(e.ID);
        }
		if (calibratingUsers.Contains(e.ID))
		{
			calibratingUsers.Remove(e.ID);
		}

        if (AttemptToCalibrate)
        {
            AttemptCalibrationForAllUsers();
        }
    }

    void userGenerator_NewUser(object sender, NewUserEventArgs e)
    {
        allUsers.Add(e.ID);
        if (AttemptToCalibrate)
        {
            print("Starting pose detection");
            this.poseDetectionCapability.StartPoseDetection(this.calibPose, e.ID);
        }
    }
	
	void AttemptCalibrationForAllUsers()
	{
		foreach (int id in userGenerator.GetUsers())
		{
			if (!skeletonCapability.IsCalibrating(id) && !skeletonCapability.IsTracking(id))
			{
				this.poseDetectionCapability.StartPoseDetection(this.calibPose, id);
			}
		}
	}
	
	public void UpdateSkeleton(int userId, OpenNISkeleton skeleton)
	{
		// make sure we have skeleton data for this user
		if (!skeletonCapability.IsTracking(userId))
		{
			return;
		}
		
		// Use torso as root
		SkeletonJointTransformation skelTrans = new SkeletonJointTransformation();
		skelTrans = skeletonCapability.GetSkeletonJoint(userId, SkeletonJoint.Torso);
		skeleton.UpdateRoot(skelTrans.Position.Position);
		
		// update each joint with data from OpenNI
		foreach (SkeletonJoint joint in Enum.GetValues(typeof(SkeletonJoint)))
		{
            if (skeletonCapability.IsJointAvailable(joint))
            {
				skelTrans = skeletonCapability.GetSkeletonJoint(userId, joint);
				skeleton.UpdateJoint(joint, skelTrans);
            }
		}
	}
	
	public Vector3 GetUserCenterOfMass(int userId)
	{
		Point3D com = userGenerator.GetCoM(userId);
		return new Vector3(com.X, com.Y, -com.Z);
	}

    public Vector2 GetPointingDirection( int userId, ref Vector3 ArmPointingVector, int arm )
    {

        //not tracking, or negative 'arm' value
		if (!skeletonCapability.IsTracking(userId) || arm < 0)
		{
			return Vector2.zero;
		}

        //if arm is 0, we get the left elbow (7), if it's 1 we get the right (13)
        SkeletonJointTransformation Elbow = skeletonCapability.GetSkeletonJoint( userId, (SkeletonJoint)(7 + (6 * arm))) ;


        //if arm is a 1, it will stay a one: (1 * 2) - 1
        //if arm is a 0 it will become a -1: (0 * 2) - 1
        arm = (arm << 1) - 1;

        ArmPointingVector.x = Elbow.Orientation.X1 * (arm); //negative for left, positive for right
        ArmPointingVector.y = -Elbow.Orientation.X2 * (arm);
        ArmPointingVector.z = Elbow.Orientation.X3;

        ArmPointingVector.Normalize();

        Vector2 pointingAxes;

        pointingAxes.x = Vector3.Dot(ArmPointingVector, Vector3.right);
        pointingAxes.y = Vector3.Dot(ArmPointingVector, Vector3.up);

        return pointingAxes;
    }

    public float GetArmLength(int userId)
    {
        if (!skeletonCapability.IsTracking(userId))
            return 0.0f;

        SkeletonJointPosition CurrentJoint = skeletonCapability.GetSkeletonJointPosition(userId, SkeletonJoint.LeftShoulder);
        SkeletonJointPosition NextJoint = skeletonCapability.GetSkeletonJointPosition(userId, SkeletonJoint.LeftElbow);

        //shoulder to elbow length
        Vector3 LeftArm = new Vector3(NextJoint.Position.X - CurrentJoint.Position.X,
                                      NextJoint.Position.Y - CurrentJoint.Position.Y,
                                      NextJoint.Position.Z - CurrentJoint.Position.Z );
         
        float LeftArmLength = LeftArm.magnitude;

        //elbow to hand length
        CurrentJoint = NextJoint;
        NextJoint = skeletonCapability.GetSkeletonJointPosition(userId, SkeletonJoint.LeftHand);

        LeftArm.x = NextJoint.Position.X - CurrentJoint.Position.X;
        LeftArm.y = NextJoint.Position.Y - CurrentJoint.Position.Y;
        LeftArm.z = NextJoint.Position.Z - CurrentJoint.Position.Z;

        LeftArmLength += LeftArm.magnitude;

        //now do the right arm
        CurrentJoint = skeletonCapability.GetSkeletonJointPosition(userId, SkeletonJoint.RightShoulder);
        NextJoint = skeletonCapability.GetSkeletonJointPosition(userId, SkeletonJoint.RightElbow);

        Vector3 RightArm = new Vector3(NextJoint.Position.X - CurrentJoint.Position.X,
                                       NextJoint.Position.Y - CurrentJoint.Position.Y,
                                       NextJoint.Position.Z - CurrentJoint.Position.Z );

        float RightArmLength = RightArm.magnitude;

        CurrentJoint = NextJoint;
        NextJoint = skeletonCapability.GetSkeletonJointPosition(userId, SkeletonJoint.RightHand);

        RightArm.x = NextJoint.Position.X - CurrentJoint.Position.X;
        RightArm.y = NextJoint.Position.Y - CurrentJoint.Position.Y;
        RightArm.z = NextJoint.Position.Z - CurrentJoint.Position.Z;

        RightArmLength += RightArm.magnitude;

        //return the average arm length
        return (LeftArmLength + RightArmLength) / 2.0f;
    }

    public Vector3 GetHeadPos(int userId)
    {
        if (!skeletonCapability.IsTracking(userId))
        {
            return Vector3.zero;
        }

		SkeletonJointPosition HeadJoint = skeletonCapability.GetSkeletonJointPosition(userId, SkeletonJoint.Head);
        //SkeletonJointTransformation HeadJoint = skeletonCapability.GetSkeletonJoint(userId, SkeletonJoint.Head);
		
		Vector3 headPos = floorTransform.inverse.MultiplyPoint3x4( new Vector3(HeadJoint.Position.X, HeadJoint.Position.Y, HeadJoint.Position.Z) );
		//Vector3 headPos = new Vector3(HeadJoint.Position.Position.X, HeadJoint.Position.Position.Y, HeadJoint.Position.Position.Z);
		headPos.z = -headPos.z;
		return headPos;

        //return new Vector3(HeadJoint.Position.Position.X, HeadJoint.Position.Position.Y, -HeadJoint.Position.Position.Z);
    }

    public Vector3 GetHipPos(int userId)
    {
        if (!skeletonCapability.IsTracking(userId))
            return Vector3.zero;

		SkeletonJointPosition Hip = skeletonCapability.GetSkeletonJointPosition(userId,  SkeletonJoint.LeftHip);
        //SkeletonJointTransformation Hip = skeletonCapability.GetSkeletonJoint(userId, SkeletonJoint.LeftHip);
        Vector3 hipPos = floorTransform.inverse.MultiplyPoint3x4( new Vector3(Hip.Position.X, Hip.Position.Y, Hip.Position.Z) );
		//Vector3 hipPos = new Vector3(Hip.Position.X, Hip.Position.Y, Hip.Position.Z);
		hipPos.z = -hipPos.z;
        return hipPos;
    }

    public Vector3 GetHandPos(int userId, int hand)
    {
        //Zero if not tracking, or hand value is negative
        if (!skeletonCapability.IsTracking(userId) || hand < 0)
            return Vector2.zero;


        //return left hand (9) if hand is 0, right hand (15) if hand is 1 (see SkeletonJoint enumeration)
		SkeletonJointPosition handJoint = skeletonCapability.GetSkeletonJointPosition(userId, (SkeletonJoint)(9 + (hand * 6)));
        //SkeletonJointTransformation handJoint = skeletonCapability.GetSkeletonJoint(userId, (SkeletonJoint)(9 + (hand * 6)));

		Vector3 handPos = floorTransform.inverse.MultiplyPoint3x4( new Vector3(handJoint.Position.X, handJoint.Position.Y, handJoint.Position.Z) );
		//Vector3 handPos = new Vector3(handJoint.Position.X, handJoint.Position.Y, handJoint.Position.Z);
		handPos.z = -handPos.z;
		return handPos;
		
        //return new Vector3(handJoint.Position.Position.X, handJoint.Position.Position.Y, -handJoint.Position.Position.Z);     
    }

    //0 for left side, 1 for right
    public Vector3 GetShoulderPos(int userId, int side)
    {
        //Zero if not tracking, or hand value is negative
        if (!skeletonCapability.IsTracking(userId) || side < 0)
            return Vector2.zero;


        //return left shoulder (6) if hand is 0, right shoulder (12) if hand is 1 (see SkeletonJoint enumeration)
		SkeletonJointPosition shoulderJoint = skeletonCapability.GetSkeletonJointPosition(userId, (SkeletonJoint)(6 + (side * 6)));
        //SkeletonJointTransformation shoulderJoint = skeletonCapability.GetSkeletonJoint(userId, (SkeletonJoint)(6 + (side * 6)));

		Vector3 shoulderPos = floorTransform.inverse.MultiplyPoint3x4( new Vector3(shoulderJoint.Position.X, shoulderJoint.Position.Y, shoulderJoint.Position.Z) );
		//Vector3 shoulderPos = new Vector3(shoulderJoint.Position.X, shoulderJoint.Position.Y, shoulderJoint.Position.Z) ;
		shoulderPos.z = -shoulderPos.z;
		return shoulderPos;
        //return new Vector3(shoulderJoint.Position.X, shoulderJoint.Position.Y, -shoulderJoint.Position.Z);
    }

    public float GetHandDistance(int userId, int hand)
    {
        if (!skeletonCapability.IsTracking(userId) || hand < 0)
            return 0.0f;

        //left shoulder is 6, right is 12, left hand is 9, right hand is 15
        int shoulder = 6 + (hand * 6);

        SkeletonJointPosition handJoint = skeletonCapability.GetSkeletonJointPosition(userId, (SkeletonJoint)(shoulder + 3));
        SkeletonJointPosition shoulderJoint = skeletonCapability.GetSkeletonJointPosition(userId, (SkeletonJoint)shoulder);

		//no need to rotate through the floor matrix - the rotation would preserve the distance
        return new Vector3(handJoint.Position.X - shoulderJoint.Position.X,
                           handJoint.Position.Y - shoulderJoint.Position.Y,
                           handJoint.Position.Z - shoulderJoint.Position.Z).magnitude;
    }
	
}