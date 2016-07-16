using UnityEngine;
using System.Collections;


public class VineDestructionScript : MonoBehaviour 
{
    //References to the outside mesh particle emitters
    public GameObject InnerFire_Ref;
    public GameObject OuterFire_Ref;
    public GameObject Smoke_Ref;
    Element_Base ball_element;
    //Particle Animators to capture that data from the emitters
    ParticleAnimator InnerFireanimator;
    ParticleAnimator OuterFireanimator;
    ParticleAnimator Smokeanimator;

    //vector to change the scale of the mesh emitters
    Vector3 Scale_Factor = new Vector3( 0, 1, 0 );
    Vector3 Smoke_Move_Factor = new Vector3(0, 0, .01f);

    //booleans for activation of elements of the code
    bool OnFire;
    bool AvailableForWater;

    //Global Sound variables
    GameObject fireSound;

	// Use this for initialization
    void Start()
    {
        //turn off the emitters of the elements if they are not so already
        InnerFire_Ref.GetComponent<ParticleEmitter>().emit = false;
        OuterFire_Ref.GetComponent<ParticleEmitter>().emit = false;
        Smoke_Ref.GetComponent<ParticleEmitter>().emit = false;
        //then apply it to the particle meshes
        InnerFire_Ref.transform.localScale = Scale_Factor ;
        OuterFire_Ref.transform.localScale = Scale_Factor ;
        Smoke_Ref.transform.localScale = Scale_Factor ;
        //make the scale vectors x & z the desired added amounts for scale per frame
        Scale_Factor.x = 0.035f ;
        Scale_Factor.z = 0.035f ;

        //booleans for allowing the ball to hit only once and to activate the scaling of the mesh emitters.
        //also allows the player to throw water onto the fire
        OnFire = false;
        AvailableForWater = false;

        //get the particle animators from each of the References to change the amount of growth per second for the particles
        InnerFireanimator = InnerFire_Ref.transform.GetComponent<ParticleAnimator>();
        OuterFireanimator = OuterFire_Ref.transform.GetComponent<ParticleAnimator>();
        Smokeanimator = Smoke_Ref.transform.GetComponent<ParticleAnimator>();
        //set all the default sizeGrow to 0
        InnerFireanimator.sizeGrow = 0;
        OuterFireanimator.sizeGrow = 0;
        Smokeanimator.sizeGrow = 0;
    }

	
	// Update is called once per frame
	void Update ()
    {
        //if the ball collision has occurred and triggered to be on fire
        if (OnFire && !AvailableForWater)
        {
            //begin emitting particles
            InnerFire_Ref.GetComponent<ParticleEmitter>().emit = true;
            OuterFire_Ref.GetComponent<ParticleEmitter>().emit = true;

            if (Smoke_Ref.transform.localScale.x < 10)
                Smoke_Ref.transform.localPosition += Smoke_Move_Factor;
               
            Smoke_Ref.GetComponent<ParticleEmitter>().emit = true;
            
            //add the scale factor vector to the current local scale of each mesh emitter
            InnerFire_Ref.transform.localScale += Scale_Factor ;
            OuterFire_Ref.transform.localScale += Scale_Factor ;
            Smoke_Ref.transform.localScale += Scale_Factor ;

            //grow the particles slowly so they do not overtake the area they currently are being emitted from
            if (InnerFireanimator.sizeGrow < 1.2f )
            {
                InnerFireanimator.sizeGrow += .0035f;
                OuterFireanimator.sizeGrow += .0035f;                
            }
            if (Smokeanimator.sizeGrow < .5f )
            {
                Smokeanimator.sizeGrow += .001f;
            }

            //Once the scale has hit this threshold, make ballcollision false again so it stops growing
            //and sets the variable for allowing the player to throw water on it.
            if (Smoke_Ref.transform.localScale.x >= 20 )
            {
                AvailableForWater = true;
            }
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        //checks to see if the collision has a BallBehavior element, only the balls will have this.
        ball_element = collision.gameObject.GetComponent<Element_Base>();

        if (ball_element.ID == -1 && !OnFire) //checks to see if it has been set on fire yet
        {
            //sets a ball collision to true
            OnFire = true;
            //ball_element = Element.NEUTRAL;

            //creates instance of fire sound and places it within the Vine hierarchy
            fireSound = Instantiate(Resources.Load("fire loud"), gameObject.transform.position, Quaternion.identity) as GameObject;
            fireSound.transform.parent = this.gameObject.transform;
        }

        if (ball_element.ID == 1 && OnFire && AvailableForWater)
        {
            InnerFire_Ref.GetComponent<ParticleEmitter>().emit = false;
            OuterFire_Ref.GetComponent<ParticleEmitter>().emit = false;
            Smoke_Ref.GetComponent<ParticleEmitter>().emit = false;

            //ball_element = Element.NEUTRAL;
            this.gameObject.active = false;

            //after fire put out, fireSound destroyed
            Destroy(fireSound);
        }

    }
}
