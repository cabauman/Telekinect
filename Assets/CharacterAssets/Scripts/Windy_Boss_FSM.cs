using UnityEngine;
using System.Collections;

public class Windy_Boss_FSM : Finite_State_Machine
{
    public enum Attack_State { ANIMATING, 
                               FIRING, 
                               IDLE };

	public Factory_Water_Elemental[] Minion_Factories ;
    
    public Transform    current_target;
    public Attack_State current_attack_state;
   
    public int          max_hits;
    public int          hit_count;

    //Firing Speed
    public float        fire_rate;
    public float        animate_time;
    public float        interval;
    public float        active_radius = 75.0f;

    //Animation Variables
    public Vector3      projectile_start_pos;
    public Vector3      projectile_end_pos;
    public GameObject   projectile_particle_base;

    //Current projectile
    public GameObject   projectile_reference;
    public GameObject   current_projectile;

	// Use this for initialization
	public override void Start () 
    {
        current_state = Boss_Calm.Instance();
        Change_State( Boss_Calm.Instance() );
	}
	
	// Update is called once per frame
	public override void Update () 
    {
        current_state.Update(this);
	}

    void OnCollisionEnter(Collision collision)
    {
        VolcanoLavaProjectile projectile = collision.gameObject.GetComponent<VolcanoLavaProjectile>();
        if (projectile != null)
        {
            Element_Base element = (Element_Base)projectile.gameObject.GetComponent(typeof(Element_Base));
            if (element.ID - 2 == 0) //earth + air = 0
            {
                hit_count++;
				Destroy(projectile.gameObject);
                if (hit_count >= max_hits)
                {
					foreach ( Factory_Water_Elemental Minions in Minion_Factories)
						Destroy(Minions.gameObject);

                    Destroy(this.gameObject);
                }
            }
        }
    }
}