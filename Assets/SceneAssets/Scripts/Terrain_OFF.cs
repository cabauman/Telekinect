using UnityEngine;
using System.Collections;

public class Terrain_OFF : MonoBehaviour
{

    
    //ArrayList colliders = new ArrayList() ;
    public Collider Terrain;
    public GameObject TerrainONTrigger;

    //KeyboardCharacterController controller;
    KinectCharacterController Kinect_Controller;
    
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnTriggerEnter(Collider collider)
    {
        //controller = collider.GetComponent<KeyboardCharacterController>();
        Kinect_Controller = collider.GetComponent<KinectCharacterController>();
        HotValues.Instance().maxSlope = 0;
       // controller.MaxSlope = 0;

        if (/*controller != null||*/  Kinect_Controller != null)
        {
            TerrainONTrigger.active = true;

            if (collider.gameObject.layer == 9 || collider.gameObject.layer == 14)
            {
                Physics.IgnoreCollision(collider, Terrain, true);
                Debug.Log("Terrain Collision off");
                this.gameObject.active = false;
            }
            else
            {
                Debug.Log("Not The Player Entering");
            }
        }
       
    }

    void OnTriggerStay(Collider collider)
    {
    }

}