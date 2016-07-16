using UnityEngine;
using System.Collections;

public class Boss_Calm : State
{
    //static int player_layer = 1 << 9;

    static private Boss_Calm instance;

    private Boss_Calm() { }

    public static Boss_Calm Instance()
    {
        if (instance == null)
            instance = new Boss_Calm();
        return instance;
    }

    public override void OnEnter(Finite_State_Machine FSM)
    {
        Windy_Boss_FSM windyBoss = (Windy_Boss_FSM)FSM;

        ParticleEmitter[] emitters = windyBoss.gameObject.GetComponentsInChildren<ParticleEmitter>();
        foreach(ParticleEmitter emitter in emitters)
            emitter.emit = false;

        Renderer[] renderers = windyBoss.gameObject.GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
    }

    public override void Update(Finite_State_Machine FSM)
    {
        Windy_Boss_FSM boss = (Windy_Boss_FSM)FSM;

        Collider[] player = Physics.OverlapSphere(boss.gameObject.transform.position, boss.active_radius, 512);

        if (player.Length > 0)
        {
            boss.current_target = player[0].transform;
            boss.Change_State(Boss_Attack.Instance());
        }
    }

    public override void OnExit(Finite_State_Machine FSM)
    {
        Windy_Boss_FSM windyBoss = (Windy_Boss_FSM)FSM;

        ParticleEmitter[] emitters = windyBoss.gameObject.GetComponentsInChildren<ParticleEmitter>();
        foreach(ParticleEmitter emitter in emitters)
            emitter.emit = true;

        Renderer[] renderers = windyBoss.gameObject.GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }
    }
}

public class Boss_Attack : State
{
    static private Boss_Attack instance;

    private Boss_Attack() { }

    public static Boss_Attack Instance()
    {
        if (instance == null)
            instance = new Boss_Attack();
        return instance;
    }

    public override void OnEnter(Finite_State_Machine FSM)
    {
        Windy_Boss_FSM boss = (Windy_Boss_FSM)FSM;

        boss.current_attack_state = Windy_Boss_FSM.Attack_State.IDLE;
        boss.interval             = 0.0f;
        boss.hit_count            = 0;
    }

    public override void Update(Finite_State_Machine FSM)
    {
        Windy_Boss_FSM boss = (Windy_Boss_FSM)FSM;

        switch (boss.current_attack_state)
        {
            case Windy_Boss_FSM.Attack_State.IDLE:
                boss.interval += Time.deltaTime;
                if (boss.interval >= boss.fire_rate)
                {
                    //spawn a projectile
                    boss.current_projectile = (GameObject)Object.Instantiate(boss.projectile_reference);
                    boss.current_projectile.transform.position = boss.projectile_start_pos;
                    boss.interval = 0.0f;

                    //create the particle system for 'lifting' the projectile
                    GameObject go = (GameObject)Object.Instantiate(boss.projectile_particle_base);
                    go.transform.parent = boss.current_projectile.transform;
                    go.transform.localPosition = Vector3.zero;
                    
                    //adjust the offset of the particle system if needed

                    //kill the particle system when we go to fire it
                    Object.Destroy(go, boss.animate_time);

                    //animate it
                    boss.current_attack_state = Windy_Boss_FSM.Attack_State.ANIMATING;
                }
                break;

            case Windy_Boss_FSM.Attack_State.ANIMATING:
                boss.interval += Time.deltaTime;
                boss.current_projectile.transform.position = Vector3.Lerp(boss.projectile_start_pos,
                                                                          boss.projectile_end_pos,
                                                                          boss.interval / boss.animate_time);

                if (boss.interval >= boss.animate_time)
                {
                    boss.interval = 0.0f;
                    boss.current_attack_state = Windy_Boss_FSM.Attack_State.FIRING;
                }

                break;  
            
            case Windy_Boss_FSM.Attack_State.FIRING:

                Vector3 toPlayer = (boss.current_target.position - boss.current_projectile.transform.position).normalized;

                boss.current_projectile.GetComponent<Rigidbody>().velocity = toPlayer * 12.5f;

                boss.current_projectile = null;

                boss.interval = 0.0f;
                boss.current_attack_state = Windy_Boss_FSM.Attack_State.IDLE;
                
                break;
        }
    }

    public override void OnExit(Finite_State_Machine FSM)
    {

    }
}