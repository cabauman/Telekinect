using UnityEngine;
using System.Collections;


public class Element_Air : Element_Base
{
     void Start()
    {
        this.ID = -2;

        if( useParticle )
		{
			AttachElementParticles() ;
		}
     }
	
	public override void AttachElementParticles()
	{
         particleSystemObject = Instantiate( Resources.Load( "Element_Air_ParticleSystem" ) ) as GameObject;
         particleSystemObject.transform.parent = this.transform;
         particleSystemObject.transform.localPosition = Vector3.zero;
          						
         Vector3 scale = new Vector3( 1.2f, 1.2f, 1.2f );
         particleSystemObject.transform.localScale = scale;
		
         ParticleEmitter[] emitters;
         emitters = GetComponentsInChildren<ParticleEmitter>();
		
         foreach (ParticleEmitter emitter in emitters)
         {
			
			emitter.GetComponent<MeshFilter>().mesh = this.gameObject.GetComponent<MeshFilter>().mesh ;
            emitter.maxSize = this.gameObject.transform.localScale.x * emitter.maxSize *2;
            emitter.minSize = this.gameObject.transform.localScale.y * emitter.minSize *2;

            if (emitter.minSize > emitter.maxSize)
            {
                float temp = emitter.minSize;
                emitter.minSize = emitter.maxSize;
                emitter.maxSize = temp;
            }

         }
	}
};