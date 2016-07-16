using UnityEngine;
using System.Collections;

public class Element_Crystal : Element_Base
{
    public Vector3 CrystalScale = new Vector3(0.35f, 0.35f, 0.35f);
	
	void Start()
    {
        this.ID = 2;

         if( useParticle )
         {
			AttachElementParticles() ;
         }
     }
	
	public override void AttachElementParticles ()
	{
					
		if (useMesh == true )
		{
	        this.elementMesh = Instantiate(Resources.Load("Crystal_Sphere")) as GameObject;
	        this.elementMesh.transform.parent = this.transform;
	        this.elementMesh.transform.localPosition = Vector3.zero;
	        this.elementMesh.transform.localScale = Vector3.zero ;
	        this.elementMesh.GetComponent<Renderer>().material.color = new Color(RNG.Instance().fGen(), RNG.Instance().fGen(), RNG.Instance().fGen(), RNG.Instance().fGen()); 
		}
		else
		{
			this.elementMesh = this.gameObject ;
		}
		
	    particleSystemObject = Instantiate(Resources.Load("Element_Crystal_ParticleSystem")) as GameObject;
	    particleSystemObject.transform.parent = this.elementMesh.transform;
	    particleSystemObject.transform.localPosition = Vector3.zero;
	     
	    particleSystemObject.GetComponent<MeshFilter>().mesh = this.elementMesh.GetComponent<MeshFilter>().mesh;
	       
	    Vector3 ParticleScale = Vector3.one;
	    particleSystemObject.transform.localScale = ParticleScale;
	    
	    particleSystemObject.GetComponent<ParticleEmitter>().maxSize = this.gameObject.transform.localScale.x * 0.5f; 
	    particleSystemObject.GetComponent<ParticleEmitter>().minSize = this.gameObject.transform.localScale.y * 0.5f ; 
	
	    if (particleSystemObject.GetComponent<ParticleEmitter>().minSize > particleSystemObject.GetComponent<ParticleEmitter>().maxSize)
	    {
	        float temp = particleSystemObject.GetComponent<ParticleEmitter>().minSize;
	        particleSystemObject.GetComponent<ParticleEmitter>().minSize = particleSystemObject.GetComponent<ParticleEmitter>().maxSize;
	        particleSystemObject.GetComponent<ParticleEmitter>().maxSize = temp;
	    }

	}

     public override void Update()  
     {
		base.Update();
		
         if ( useMesh && this.elementMesh.transform.localScale.x < CrystalScale.x)
         {
             this.elementMesh.transform.localScale += CrystalScale * Time.deltaTime * 0.5f; //take two seconds to grow to full size
         }

     }
};