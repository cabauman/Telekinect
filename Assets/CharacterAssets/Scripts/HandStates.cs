using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenNI;

/****************************************
 * Template for HandStates

public class $STATENAME$ : HandState
{

    static private $STATENAME$ instance;

    static public HandState Instance()
    {
        if(instance == null)
            instance = new $STATENAME$();

        return instance;
    }

    public override void OnEnter(HandFSM hand)
    {

    }

    public override void OnUpdate(HandFSM hand)
    {

    }

    public override void OnExit(HandFSM hand)
    {

    }
}
*****************************************/

public class HandIdleState : HandState
{

    static private HandIdleState instance;

    static public HandState Instance()
    {
        if(instance == null)
            instance = new HandIdleState();

        return instance;
    }

    public override void OnEnter(HandFSM hand)
    {
        hand.isPointLooking = false;
        hand.pointed = null;
        hand.selected = null;
        //hide the reticle
        hand.drawReticle = false;
		int jointIdx = 5 + (6 * hand.handID); //0 is left, 1 is right
		HandFSM.playerController.skeleton.shouldRotateJoint[jointIdx] = false;			 //shoulder joint
		HandFSM.playerController.skeleton.shouldRotateJoint[jointIdx + 1] = false;		 //elbow joint
        
		//if the other hand is not idle then the other hand becomes point looking
        if( hand.otherHand.currentState != HandIdleState.Instance() )
                hand.otherHand.isPointLooking = true;
	
		if ( hand.shield != null && hand.otherHand.currentState == HandPointingState.Instance() )
		{
			hand.shield.isTransferring = true ;
			
			if(Network.peerType != NetworkPeerType.Disconnected)
				hand.GetComponent<NetworkView>().RPC( "ShieldTransfer", RPCMode.Others );

			hand.shield.gameObject.transform.parent = hand.otherHand.transform ;
			hand.otherHand.shield = hand.shield ;
			hand.shield = null;
		}
		else
		{
			if ( hand.shield != null )
				hand.shield.DestroyShield() ;
		}
    }
    public override void OnUpdate(HandFSM hand)
    {
		if(hand.isSwiping)
		{
			if(	HandFSM.playerController.TKenergy >= 85.0f && hand.otherHand.currentState == HandSwipeState.Instance() && 
			  ( Vector3.Dot( hand.handVelocityKS.normalized, hand.otherHand.handVelocityKS.normalized ) >= 0.86 )	&& 
			  ( hand.handVelocityKS.y < 0) && (hand.otherHand.handVelocityKS.y < 0) )
			{
				hand.ChangeState(HandTKHammerState.Instance() ) ;
			}
			else
			{
				hand.ChangeState(HandSwipeState.Instance());
				return;
			}
		}
		//Check if we are above the pointing floor (hand above about waist level)
		if(hand.currentPosKS.y > HandFSM.playerController.headPosKS.y + HandFSM.hotValues.pointingFloorOffset)
		{
            //Check if we are pointing (hand is outside pointing sphere volume in kinect space
            if( hand.currentPosKS.z > HandFSM.playerController.headPosKS.z &&
			        (hand.currentPosKS - (HandFSM.playerController.headPosKS + hand.shoulderOffset)).sqrMagnitude >= (HandFSM.hotValues.pointingRadius * HandFSM.hotValues.pointingRadius))
            {
                hand.ChangeState(HandPointingState.Instance());
			}
        }
    }
	
    public override void OnExit(HandFSM hand)
    {
		int jointIdx = 5 + (6 * hand.handID); //0 is left, 1 is right
		HandFSM.playerController.skeleton.shouldRotateJoint[jointIdx] = true;			 //shoulder joint
		HandFSM.playerController.skeleton.shouldRotateJoint[jointIdx + 1] = true;		 //elbow join
    }
}

public class HandPointingState : HandState
{

    static private HandPointingState instance;

    static public HandState Instance()
    {
        if(instance == null)
            instance = new HandPointingState();

        return instance;
    }

    public override void OnEnter(HandFSM hand)
    {
        //if the other hand is not pointing, then we've become pointlooking
        if(!hand.otherHand.isPointLooking)
            hand.isPointLooking = true;

        //show the reticle
		hand.drawReticle = true;
        hand.reticle.SetFloat("_Cutoff", 0.99f);
        hand.reticle.color =  Color.blue;
    }

    public override void OnUpdate(HandFSM hand)
    {		
		//check if we are swiping
		if(hand.isSwiping)
		{
			if(	HandFSM.playerController.TKenergy >= 85.0f && hand.otherHand.currentState == HandSwipeState.Instance() && 
			  ( Vector3.Dot( hand.handVelocityKS.normalized, hand.otherHand.handVelocityKS.normalized ) >= 0.86 )	&& 
			  ( hand.handVelocityKS.y < 0) && (hand.otherHand.handVelocityKS.y < 0) )
			{
				hand.ChangeState(HandTKHammerState.Instance() ) ;
			}
			else
			{
				hand.ChangeState(HandSwipeState.Instance());
				hand.selected = null;
				return;
			}
		}
        //Only update the pointing if this hand doesn't have something selected
        if(hand.selected == null)
        {
			//look for both hands above head for pause menu
            if(	!HandFSM.hotValues.menu.gameObject.active &&
			   	HandFSM.playerController.isGrounded &&
			   	hand.otherHand.currentPosKS.y >=  HandFSM.playerController.headPosKS.y + 200.0f &&
               	hand.currentPosKS.y >= HandFSM.playerController.headPosKS.y + 200.0f)
            {
                HandFSM.hotValues.menu.gameObject.active = true;
                HandFSM.hotValues.menu.Activate();
            }

            
			if(HandFSM.hotValues.menu.gameObject.active)
                hand.MenuPoint();
            else
                hand.Point();
            
            //If we are pointing at something, and not doing TK Hammer, run the timer
            if(hand.pointed != null && hand.otherHand.currentState != HandTKHammerState.Instance())
            {
                //make sure we get the pointing time incremented in real time - no matter what the current time scale is
                hand.pointTime += (Time.deltaTime / Time.timeScale);
                hand.reticle.SetFloat("_Cutoff", Mathf.Max(0.01f, 1 - (hand.pointTime / HandFSM.hotValues.pointTimeOut)));

                if(hand.pointTime >= HandFSM.hotValues.pointTimeOut) //we just selected something
                {
                    hand.selected = hand.pointed;
                    hand.reticle.SetFloat("_Cutoff", 0.99f);

                    //if the other hand doesn't have anything selected, but it was point looking, this hand becomes point looking
                    //(the hand with the selection becomes priority)
                    if(hand.otherHand.selected == null && hand.otherHand.isPointLooking)
                    {
                        hand.otherHand.isPointLooking = false;
                        hand.isPointLooking = true;
                    }

                    //check if something has become selected and we can do TK on it (not selectable layer)
                    if( hand.selected.gameObject.layer != 10 && hand.shield == null )
                    {
						hand.ChangeState(HandTKMoveState.Instance());
                        return;
                    }
                }
            }
			else
				hand.reticle.SetFloat("_Cutoff", 0.99f);
        }
        else //we have something selected - stick the reticle on it
        {
            Vector3 screenPoint = 
                Camera.main.WorldToScreenPoint(hand.selected.gameObject.transform.position);

			hand.reticleRect = new Rect(screenPoint.x - hand.reticleRect.width / 2,
										Screen.height - ( screenPoint.y + (hand.reticleRect.height / 2) ),
										hand.reticleRect.width,
										hand.reticleRect.height );
                    
        }
        
        //ensure we are still pointing
        //Check if we are below the pointing floor (hand below about waist level) or inside the pointing barrier (sphere)
        if( (hand.currentPosKS.y <= HandFSM.playerController.headPosKS.y + HandFSM.hotValues.pointingFloorOffset) ||
            (hand.currentPosKS - (HandFSM.playerController.headPosKS + hand.shoulderOffset)).sqrMagnitude < (HandFSM.hotValues.pointingRadius * HandFSM.hotValues.pointingRadius))
        {
            hand.isPointLooking = false;

            //set reticle to blue
            hand.reticle.color =  Color.blue;
            hand.ChangeState(HandIdleState.Instance());
        }
		
		
		//begin shield method
		if( (	hand.currentPosKS - hand.otherHand.currentPosKS).magnitude <= HandFSM.hotValues.shieldConjureThreshold && 
		   		hand.shield == null && hand.otherHand.shield == null && hand.otherHand.currentState == HandPointingState.Instance() &&
		   		hand.isPointLooking == true )
		{
			//create the sheild and instantiate the shield
			GameObject GO = null;
			if( Network.peerType != NetworkPeerType.Disconnected )
			{
				GO = (GameObject)Network.Instantiate( Resources.Load("Shield"), hand.transform.position, hand.transform.rotation, 0);
				hand.GetComponent<NetworkView>().RPC("StartSheild", RPCMode.Others, GO.GetComponent<NetworkView>().viewID);
			}
			else 
				GO = Object.Instantiate(Resources.Load("Shield") ) as GameObject ;

			hand.shield = GO.GetComponent<Shield>() ;
			
			hand.shield.gameObject.transform.parent = hand.gameObject.transform ;
			hand.shield.gameObject.transform.localScale = Vector3.one ;
			hand.shield.gameObject.transform.localEulerAngles = new Vector3 ( 90.0f, 0.0f, 0.0f ) ;
			hand.shield.gameObject.transform.localPosition = Vector3.zero ;
			
		}
		else 
		{
			if(hand.shield != null) // if the shield isn't null
			{
				if( hand.shield.isActive == false && hand.shield.dying == false) //if the shield is NOT active and is NOT dying
				{
					//grow the shield based on hand distance
					Vector3 Distance = ( hand.currentPosKS - hand.otherHand.currentPosKS) ; //get the distance vector from hand to hand
					float DistanceMagnitude = Distance.magnitude ; //store the actual magnitude of this vector for use later
					float scale = DistanceMagnitude / 1000 ; //divide this magnitude by 1000 to get from milimeters to meters
					hand.shield.gameObject.transform.localScale = new Vector3 ( scale, scale, scale ) ; //scale the shield according to the new scale factor
							
				}
				else // sheild isActive
				{
					//if the shield isn't dying (just so this isn't called when it alredy is) and the hand is in another state, destroy shield
					if( hand.shield.dying == false &&  hand.currentState != HandPointingState.Instance() ) 
					{
						hand.shield.DestroyShield() ; 
					}
				}
			}
		}
		//end shield method


    }

    public override void OnExit(HandFSM hand)
    {
		hand.reticle.SetFloat("_Cutoff", 0.99f);
    }
}

public class HandSwipeState : HandState
{

    static private HandSwipeState instance;
	
    static public HandState Instance()
    {
        if(instance == null)
            instance = new HandSwipeState();

        return instance;
    }

    public override void OnEnter(HandFSM hand)
    {
		//hide the reticle
		hand.drawReticle = false;
		
		//turn on trail effect
		((GameObject)GameObject.Instantiate(hand.effectHandSwipe, hand.gameObject.transform.position, Quaternion.identity)).transform.parent = hand.gameObject.transform ;
		
		Vector3 handVelocity =  HandFSM.playerController.transform.TransformDirection(hand.handVelocityKS) * 0.01f;
		
		if(Network.isClient)
		{
			hand.GetComponent<NetworkView>().RPC("ClientSwipe", RPCMode.Server, handVelocity);
		}
		else
		{
			//collide against projectiles, moveables and selectables
			int layerMask = (1 << 17) | (1 << 13) | (1 << 10);
			Vector3 swipePos = HandFSM.playerController.gameObject.transform.position + HandFSM.playerController.gameObject.transform.forward * HotValues.Instance().swipeCtr;
			Collider[] swiped = Physics.OverlapSphere(swipePos,
								  					  HotValues.Instance().swipeRadius,
													  layerMask);
			
			foreach(Collider collider in swiped)
			{
				collider.GetComponent<Rigidbody>().velocity = handVelocity / (collider.GetComponent<Rigidbody>().mass * 2.0f);
			}
		}
		
		hand.pointTime = 0.0f ;
    }


    public override void OnUpdate(HandFSM hand)
    {
		if ( (hand.pointTime / Time.timeScale) >= 0.5f )
		{
			hand.ChangeState( HandIdleState.Instance() );
		}
//		float temp = Vector3.Dot( hand.handVelocityKS.normalized, hand.otherHand.handVelocityKS.normalized ) ;
//		
//		if(	HandFSM.playerController.TKenergy >= 85.0f && hand.otherHand.currentState == HandSwipeState.Instance() && 
//		  ( Vector3.Dot( hand.handVelocityKS.normalized, hand.otherHand.handVelocityKS.normalized ) >= 0.86 )	&& 
//		  ( hand.handVelocityKS.y < 0) && (hand.otherHand.handVelocityKS.y < 0) )
//		{
//			hand.ChangeState(HandTKHammerState.Instance() ) ;
//		}
		
		hand.pointTime += Time.deltaTime ;
    }

    public override void OnExit(HandFSM hand)
    {

    }
}

public class HandTKMoveState : HandState
{

    static private HandTKMoveState instance;
	public int	   TKenergyDrainOn;

    static public HandState Instance()
    {
        if(instance == null)
            instance = new HandTKMoveState();

        return instance;
    }

    public override void OnEnter(HandFSM hand)
    {
        hand.prevPosKS = hand.currentPosKS; //use previous position as 'centroid' for TK
        hand.centroidOffset = hand.currentPosKS - HandFSM.playerController.headPosKS; //for calculating the velocity we are moving the hand - vector offset from the head location
        
		if(! hand.selected.GetComponent<Rigidbody>().isKinematic && hand.selected.gameObject.tag != "RotateOnly")
			hand.selected.GetComponent<Rigidbody>().velocity = Vector3.zero;  //make sure the object is not moving

		MenuItem menuItem = hand.selected.gameObject.GetComponent<MenuItem>();
		if(menuItem != null)
			TKenergyDrainOn = 0;
		else
			TKenergyDrainOn = 1;

		HandFSM.playerController.TKRegenOn = false;
    }

    public override void OnUpdate(HandFSM hand)
    {
        //Special case for buttons - when they deactivate we need to clear the selection
        if(hand.selected == null || ( hand.selected.GetComponent<Rigidbody>().isKinematic && ! HandFSM.hotValues.menu.gameObject.active ) )
        {
            hand.selected = null;
            hand.ChangeState(HandIdleState.Instance());
            return;
        }
		
		//take some TK energy
		HandFSM.playerController.TKenergy -= HandFSM.hotValues.TKenergyDrain * TKenergyDrainOn * Time.deltaTime ;
        //'stick' the reticle to the object
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(hand.selected.gameObject.transform.position);

		hand.reticleRect = new Rect(screenPoint.x - hand.reticleRect.width / 2,
									Screen.height - ( screenPoint.y + (hand.reticleRect.height / 2) ),
									hand.reticleRect.width, hand.reticleRect.height );
			        //calculate centroid location
	        Vector3 centroid = HandFSM.playerController.headPosKS + hand.centroidOffset;
	        //centroid += HandFSM.playerController.headDeltaKS;  //this keeps hand centroid relative to head position
	        //calculate hand movement delta
	        hand.TKmove = hand.currentPosKS - centroid;
	        Vector3 handVelocity = hand.handVelocityKS * 0.01f ;
	        //store previous position
	        hand.prevPosKS = hand.currentPosKS;
	
	        //Caclulate a 2D (X and Z axis) vector from the player to the object
	        Vector2 player2DPos = new Vector2(HandFSM.playerController.transform.position.x, HandFSM.playerController.transform.position.z);
	        Vector2 obj2DPos = new Vector2(hand.selected.transform.position.x, hand.selected.transform.position.z);
	        float toObject = (obj2DPos - player2DPos).sqrMagnitude;
	        
	        //limit how far we can do TK (stop movement on Z axis if too close or too far - 9.0 is 3.0 meters ^2)
	        if( ((toObject > HandFSM.hotValues.maxSearchDistance * HandFSM.hotValues.maxSearchDistance) && (hand.TKmove.z > 0.0f)) ||
	            ((toObject < 9.0f) && (hand.TKmove.z < 0.0f)) )
	        {
	            hand.TKmove.z = 0.0f;
	        }
	
	
	        //Transform the movement vector from the player's local coordinate system and divide by 100
	        hand.TKmove = (HandFSM.playerController.transform.TransformDirection(hand.TKmove) * 0.01f) + HandFSM.playerController.GetComponent<Rigidbody>().velocity;
	
	
	        //update the object's momentum (velocity / mass)
			if(Network.isClient && hand.selected.GetComponent<NetworkView>() != null)
				hand.GetComponent<NetworkView>().RPC("ClientTKMove", RPCMode.Server, hand.selected.GetComponent<NetworkView>().viewID, hand.TKmove);
			
			if( hand.selected.gameObject.GetComponent<Rigidbody>().isKinematic )
			{
				hand.selected.gameObject.GetComponent<Rigidbody>().isKinematic = false;
				hand.selected.gameObject.GetComponent<Rigidbody>().velocity = hand.TKmove / hand.selected.gameObject.GetComponent<Rigidbody>().mass;
				hand.selected.gameObject.GetComponent<Rigidbody>().isKinematic = true;
			}
			else
				hand.selected.gameObject.GetComponent<Rigidbody>().velocity = hand.TKmove / hand.selected.gameObject.GetComponent<Rigidbody>().mass;

	        //If the hand goes faster than the limit velocity, we throw the object
	        if(hand.isSwiping)
	        {   
	            //rotate the velocity from the player's local space
	            handVelocity =  HandFSM.playerController.transform.TransformDirection(handVelocity);
	
	            //check if the other hand has something selected
	            if(hand.otherHand.selected != null)
	            {
	                //check the direction we are throwing against the direction to the other hand's selection, 
	                //if we are within 60 degrees, throw the object at the other hand's selection
	                Vector3 toTarget = (hand.otherHand.selected.transform.position - hand.selected.transform.position).normalized;
	                float   cosAngle = Vector3.Dot(handVelocity.normalized, toTarget);
	                if(cosAngle >= 0.5f) //cosine of 60 degrees is 0.5, smaller angles have larger cosines
	                    handVelocity = toTarget * handVelocity.magnitude; //vector to the target * the speed we threw the object
	            }
				else //try to find a target
				{
					int selectableLayer = 1 << 10;
					float speed = handVelocity.magnitude;
					handVelocity /= speed;
					Vector3 spherePos = hand.selected.gameObject.transform.position + (handVelocity * HandFSM.hotValues.TKThrowSearchRadius);
					Collider[] targets = Physics.OverlapSphere(spherePos, HandFSM.hotValues.TKThrowSearchRadius, selectableLayer);
					float currentScore = Mathf.Infinity; //score = cosAngle * distance * distance - this biases the score to closer objects over those more along center -- lower scores are better
					Vector3 toTarget = new Vector3();
					foreach(Collider target in targets)
					{
						toTarget = (target.gameObject.transform.position - hand.selected.gameObject.transform.position);
						float distance = toTarget.magnitude;
						toTarget /= distance;
						float cosAngle = Vector3.Dot (handVelocity, toTarget);
						float score = distance * distance * cosAngle;
						if(cosAngle >= 0.5f)
						{
							if(score < currentScore)
								currentScore = score;
						}

					}
					if(currentScore < Mathf.Infinity)
					{
						handVelocity = toTarget * speed;
					}
					else
						handVelocity *= speed;
				}
	
				if(Network.isClient && hand.selected.GetComponent<NetworkView>() != null)
				{
					hand.GetComponent<NetworkView>().RPC("ClientTKMove", RPCMode.Server, hand.selected.GetComponent<NetworkView>().viewID, handVelocity);
					hand.GetComponent<NetworkView>().RPC("ClientStopTK", RPCMode.Server);
				}
				
				if( hand.selected.gameObject.GetComponent<Rigidbody>().isKinematic )
				{
					hand.selected.gameObject.GetComponent<Rigidbody>().isKinematic = false;
					hand.selected.gameObject.GetComponent<Rigidbody>().velocity = handVelocity  / hand.selected.gameObject.GetComponent<Rigidbody>().mass;
					hand.selected.gameObject.GetComponent<Rigidbody>().isKinematic = true;
				}
				else
					hand.selected.gameObject.GetComponent<Rigidbody>().velocity = handVelocity  / hand.selected.gameObject.GetComponent<Rigidbody>().mass;
				
				HandFSM.playerController.TKenergy -= HandFSM.hotValues.TKenergyThrow * TKenergyDrainOn;
				HandFSM.playerController.TKenergy = HandFSM.playerController.TKenergy < 0.0f ? 0.0f : HandFSM.playerController.TKenergy;
	
	
	            hand.ChangeState(HandIdleState.Instance());
	        }
        //we are going out of TK (dropped the object / went into throwing mode / ran out of energy)
        if( (hand.currentPosKS.y <= HandFSM.playerController.headPosKS.y + HandFSM.hotValues.pointingFloorOffset) ||
            (hand.currentPosKS - (HandFSM.playerController.headPosKS + hand.shoulderOffset)).sqrMagnitude < (HandFSM.hotValues.pointingRadius * HandFSM.hotValues.pointingRadius * 0.25f) ||
			(HandFSM.playerController.TKenergy <= 0.0f) )
        {
            //when we drop an object, set it's velocity to zero, then let gravity take over
			if(Network.isClient && hand.selected.GetComponent<NetworkView>() != null)
			{
				hand.GetComponent<NetworkView>().RPC("ClientTKMove", RPCMode.Server, hand.selected.GetComponent<NetworkView>().viewID, Vector3.zero);
				hand.GetComponent<NetworkView>().RPC("ClientStopTK", RPCMode.Server);
			}

			if( hand.selected.gameObject.GetComponent<Rigidbody>().isKinematic )
			{
				hand.selected.gameObject.GetComponent<Rigidbody>().isKinematic = false;
				hand.selected.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
				hand.selected.gameObject.GetComponent<Rigidbody>().isKinematic = true;
			}
			else
				hand.selected.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

            hand.ChangeState(HandIdleState.Instance());
        }
    }

    public override void OnExit(HandFSM hand)
    {
        if(hand.otherHand.currentState != HandTKMoveState.Instance() && hand.otherHand.shield == null)
			HandFSM.playerController.TKRegenOn = true;
    }
}

//Only one hand at a time should be in the HandTKHammerState - if both hands get in HandTKHammerState it is an error
public class HandTKHammerState : HandState
{

    static private HandTKHammerState instance;

    static public HandState Instance()
    {
        if(instance == null)
            instance = new HandTKHammerState();

        return instance;
    }

    public override void OnEnter(HandFSM hand)
    {
		if( hand.otherHand.shield != null ) //if the other hand has a shield when we go into hammer mode, destroy it.
		{
			hand.otherHand.shield.DestroyShield();
		}
		
		//take away remaining player's TK energy, use it to calculate power of impact
		float hitPower = HandFSM.playerController.TKenergy / 85.0f;
		HandFSM.playerController.TKenergy = 0.0f;
		HandFSM.playerController.GetComponent<Rigidbody>().velocity = Vector3.zero;
	
		//drop a TK Hammer 'projectile'
		GameObject go = null;
		Vector3 spawnLocation = HandFSM.playerController.gameObject.transform.position - Vector3.up ;				
		//spawn the projectile effect for the Hammah
		if( Network.isClient )
		{
			hand.GetComponent<NetworkView>().RPC("HammerHit", RPCMode.Server, hitPower);
			//go to Idle state
			hand.ChangeState(HandIdleState.Instance());
			return ;
		}
		else if( Network.isServer )
			go = (GameObject)Network.Instantiate(Resources.Load("Projectile_TKHammer"), spawnLocation, HandFSM.playerController.transform.localRotation, 0);
		else
			go = (GameObject)Object.Instantiate(Resources.Load("Projectile_TKHammer"), spawnLocation, HandFSM.playerController.transform.localRotation);
	
		Projectile_Script hammerProjectile = go.GetComponent<Projectile_Script>();
		
		hammerProjectile.creator = HandFSM.playerController.gameObject ;
		hammerProjectile.projectileDamage = hitPower * 110.0f;
		hammerProjectile.explosionRadius = hitPower * 10.0f;
		
		//go to Idle state
		hand.ChangeState(HandSwipeState.Instance());
	}

    public override void OnUpdate(HandFSM hand)
	{
	
    }

    public override void OnExit(HandFSM hand)
    {

    }
}