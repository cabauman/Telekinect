using UnityEngine;
using System.Collections;

public class VolcanoLavaProjectile : MonoBehaviour 
{
    public Element_Base currentElement;
	public float timeToDie;
	public float timeLeft;
	// Use this for initialization
	void Start () 
    {
	    currentElement = (Element_Base)this.gameObject.AddComponent<Element_Fire>();
        currentElement.isPool = true;
        currentElement.useParticle = true;
        currentElement.canTransfer = true;
		currentElement.destroyObject = false;

		timeLeft = timeToDie;
	}
	
	// Update is called once per frame
	void Update () 
    {
	    timeLeft -= Time.deltaTime;
		this.GetComponent<Renderer>().material.SetFloat("_Cutoff", 1 - (timeLeft / timeToDie));

		if(timeLeft <= 0.0f)
		{
			Destroy(this.gameObject);
		}
	}

    void OnCollisionEnter(Collision collision)
    {
		switch (collision.gameObject.layer)
		{
			case 9:		//We hit the player - do some damage
				if(this.GetComponent<Rigidbody>().velocity.magnitude >= 7.0f)
				{
					KinectCharacterController player = collision.gameObject.GetComponent<KinectCharacterController>();
					player.health -= 20.0f;  //20 damage if we are moving fast
				}
				

			break;

//			case 14:	//We hit an orb (or rather an orb hit us)
//				Element_Base orbElement = (Element_Base)collision.gameObject.GetComponent(typeof(Element_Base));
//				if(orbElement != null)
//				{
//					//if we've been hit with the opposite element and we are on fire (we got hit with water)
//					if((orbElement.ID + currentElement.ID) == 0 &&
//						currentElement.ID == -1)
//					{
//						currentElement.DestroyElement();
//						currentElement = (Element_Base)this.gameObject.AddComponent<Element_Crystal>();
//						currentElement.isPool = true;
//						currentElement.useParticle = true;
//						currentElement.canTransfer = false;
//						currentElement.destroyObject = false;
//					}
//				}
//			break;

		}
    }

	void OnCollisionStay(Collision collision)
	{
		if(collision.gameObject.layer == 9)
		{
			KinectCharacterController player = collision.gameObject.GetComponent<KinectCharacterController>();
			player.health -= 20.0f * Time.deltaTime;  //20 damage per second we touch the player
		}
	}
}
