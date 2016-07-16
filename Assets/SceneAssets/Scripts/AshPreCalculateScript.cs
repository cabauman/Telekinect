using UnityEngine;
using System.Collections;

public class AshPreCalculateScript : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        for(int i = 0; i < 15; i++)
            gameObject.GetComponent<ParticleEmitter>().Simulate(1.0f);
    }
	
	// Update is called once per frame
	void Update (){}
}
