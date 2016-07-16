using UnityEngine;
using System.Collections;
using System.Collections.Generic ;

public class Projectile_Crystal_Generator : MonoBehaviour 
{
	public float projectileRadius ;
	public float crystalGenerationSpeed ;
	public float firingRate ;
	public float firingDelay ;
	public float projectileVelocity ;
	public int numberOfCrystals = 6 ;
	int currentCrystal = 0;
	float step ;
	float theta ;
	float initialTimer ;
	bool  firing = false ;
	
	public ParticleSystem particles ;
	
	List<GameObject> crystals = new List<GameObject>()  ;
	
    public GameObject generatedProjectile;

	// Use this for initialization
	void Start () 
    {
    	if(Network.isClient)
			this.enabled = false;
		
		step = (Mathf.PI * 2) / numberOfCrystals ;
		initialTimer = crystalGenerationSpeed ;
		theta = step ;
	}
	
	// Update is called once per frame
	void Update () 
    {
		if( !firing )
		{
			crystalGenerationSpeed -= Time.deltaTime ;
			
			if(crystalGenerationSpeed <= 0.0f && currentCrystal < numberOfCrystals)
			{
				crystals.Add(GenerateCrystal());
				crystalGenerationSpeed = initialTimer ;
				currentCrystal++ ;
	        }
			
			if( crystals.Count == numberOfCrystals)
			{
				particles.enableEmission = false ;
				FireCrystals() ;
			}
			
		
		}
		else
		{	
			firingRate -= Time.deltaTime;
			firingDelay -=Time.deltaTime;
			
			if( firingDelay <= 0.0f && firingRate <= 0.0f && currentCrystal < numberOfCrystals)
			{
				firingRate = initialTimer ;
				crystals[currentCrystal].transform.parent = null ;
				crystals[currentCrystal].GetComponent<Collider>().isTrigger = false;
				crystals[currentCrystal].GetComponent<Rigidbody>().velocity = crystals[currentCrystal].transform.forward * projectileVelocity;	
				currentCrystal++ ;
	        }
			
		}
	}
	
	GameObject GenerateCrystal()
	{
		
		Vector3 position = new Vector3( projectileRadius * Mathf.Cos(theta), projectileRadius * Mathf.Sin(theta) , 0.0f) ;
		position = this.gameObject.transform.TransformPoint(position) ;
		GameObject go;
		
		if(Network.isServer)
			go = (GameObject)Network.Instantiate(generatedProjectile, position, this.gameObject.transform.rotation, 0);
		else
			go = (GameObject)Instantiate(generatedProjectile, position, this.gameObject.transform.rotation);
		
		theta += step ;
	
        go.GetComponent<Renderer>().material.color = new Color(RNG.Instance().fGen(), RNG.Instance().fGen(), RNG.Instance().fGen());
		go.transform.parent = this.gameObject.transform ;
		go.GetComponent<Collider>().isTrigger = true ;
		go.AddComponent<Element_Crystal>() ;
		
		return go ;

	}
	
	void FireCrystals()
	{
		initialTimer = firingRate;
		currentCrystal = 0 ;
		firing = true ;
		
	}
	
}
