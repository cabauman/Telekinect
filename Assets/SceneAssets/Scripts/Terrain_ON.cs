using UnityEngine;
using System.Collections;

public class Terrain_ON : MonoBehaviour
{

    public GameObject TerrainOFFTrigger;
    //ArrayList colliders = new ArrayList() ;
    public Collider Terrain;
    //KeyboardCharacterController controller;
    KinectCharacterController Kinect_Controller;
    
	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
	}

    void OnTriggerEnter(Collider collider)
    {
        //controller = collider.GetComponent<KeyboardCharacterController>();
        Kinect_Controller = collider.GetComponent<KinectCharacterController>();
        HotValues.Instance().maxSlope = Mathf.Cos(Mathf.Deg2Rad * 60.0f); //Cosine of the max slope
        //controller.MaxSlope = 0.5f;

        if (/*controller != null ||*/ Kinect_Controller != null)
        {
            TerrainOFFTrigger.active = true;

            if (collider.gameObject.layer == 9 || collider.gameObject.layer == 14)
            {
                Physics.IgnoreCollision(collider, Terrain, false);
                Debug.Log("Terrain Collision On");
                this.gameObject.active = false;
            }
            else
            {
                Debug.Log("Not The Player Leaving");
            }
        }
        
    }

    void OnTriggerStay(Collider collider)
    {
    }

}
