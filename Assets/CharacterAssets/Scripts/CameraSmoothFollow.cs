using UnityEngine;
using System.Collections;

public class CameraSmoothFollow : MonoBehaviour 
{
    public Transform target;
    public float distance = 10.0f;
    public float height = 5.0f;
    public float movementDamping = 3.0f;
	public float rotationDamping = 3.0f;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
//	void Update () 
//    {
//			//***REMOVE THIS CODE - FOR TESTING NETWORKING ONLY***//
//		if( target == null )
//		{
//			target = GameObject.FindGameObjectWithTag("Player").transform.FindChild("CameraMount");
//		}
//	}

    void LateUpdate()
    {
        if (target == null)
            return;

		//Debug.DrawLine(target.position, target.position + target.forward);

		Vector3 wantedPosition = (target.position + (target.forward * -distance));
		wantedPosition.y += height;

		Quaternion wantedRotation = Quaternion.LookRotation(target.position - this.transform.position);

		float dT = Time.deltaTime / Time.timeScale;
		this.transform.position = Vector3.Lerp(this.transform.position, wantedPosition, movementDamping * dT);
		this.transform.rotation = Quaternion.Slerp(this.transform.rotation, wantedRotation, rotationDamping * dT);

    }
}
