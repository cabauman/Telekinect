using UnityEngine;
using System.Collections;

public class AdvanceParticles : MonoBehaviour {
	
	public float advanceTime;
	
	// Use this for initialization
	void Start () 
	{
		ParticleEmitter[] emitters = this.gameObject.GetComponentsInChildren<ParticleEmitter>();
		foreach( ParticleEmitter emitter in emitters )
		{
			for(int i = 0; i < (int)advanceTime; i++)
				emitter.Simulate( 1.0f );	
		}
	}
	
}
