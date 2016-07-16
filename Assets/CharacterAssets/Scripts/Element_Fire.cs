using UnityEngine;
using System.Collections;

public class Element_Fire : Element_Base
{
	void Start()
	{
		this.ID = -1;

		if (useParticle)
		{
			AttachElementParticles() ;
		}

	}
	
	public override void AttachElementParticles()
	{
		particleSystemObject = Instantiate(Resources.Load("Element_Fire_ParticleSystem")) as GameObject;
		particleSystemObject.transform.parent = this.transform;
		particleSystemObject.transform.localPosition = Vector3.zero;
		
		this.particleSystemObject.transform.localScale = Vector3.one;

		ParticleEmitter[] emitters;
		emitters = GetComponentsInChildren<ParticleEmitter>();
		foreach (ParticleEmitter emitter in emitters)
		{
			emitter.GetComponent<MeshFilter>().mesh = this.gameObject.GetComponent<MeshFilter>().mesh ;
			emitter.maxSize = this.gameObject.transform.localScale.x;
			emitter.minSize = this.gameObject.transform.localScale.y;
		}

		Vector3 particleVelocity = new Vector3(0.0f, this.gameObject.transform.localScale.y, 0.0f);
		particleSystemObject.GetComponent<ParticleEmitter>().worldVelocity = particleVelocity;
	}
};