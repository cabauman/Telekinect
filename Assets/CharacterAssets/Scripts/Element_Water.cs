using UnityEngine;
using System.Collections;

public class Element_Water : Element_Base
{
     void Start()
    {
        this.ID = 1;

		if (useParticle)
		{
			AttachElementParticles() ;
		}
		
     }
	
	public override void AttachElementParticles()
	{
            particleSystemObject = Instantiate(Resources.Load("Element_Water_ParticleSystem")) as GameObject;
            particleSystemObject.transform.parent = this.transform;
            particleSystemObject.transform.localPosition = Vector3.zero;

            Vector3 scale = Vector3.one;
            particleSystemObject.transform.localScale = scale;

            ParticleEmitter[] emitters;
            emitters = GetComponentsInChildren<ParticleEmitter>();
            foreach (ParticleEmitter emitter in emitters)
            {
			
				emitter.GetComponent<MeshFilter>().mesh = this.gameObject.GetComponent<MeshFilter>().mesh ;
                emitter.maxSize = this.gameObject.transform.localScale.x * (emitter.maxSize * 2f) ;
                emitter.minSize = this.gameObject.transform.localScale.y * (emitter.minSize * 2f) ;

                if (emitter.minSize > emitter.maxSize)
                {
                    float temp = emitter.minSize;
                    emitter.minSize = emitter.maxSize;
                    emitter.maxSize = temp;
                }             
            }
            particleSystemObject.GetComponent<ParticleAnimator>().force = new Vector3(0.0f, -9.8f, 0.0f);
         
	}
};