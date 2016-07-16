using UnityEngine;
using System.Collections;

public class Factory_Crystal_Elemental : Agent_Factory 
{
    static int factory_ID = 2;

    public override void Create_Agent()
    {
		GameObject crystalElemental = null;

		if( Network.isServer )
			crystalElemental = (GameObject)Network.Instantiate(Resources.Load("Enemy_Crystal_Elemental"), spawn_point.position , spawn_point.rotation, 0);
		else
            crystalElemental = (GameObject)Instantiate(Resources.Load("Enemy_Crystal_Elemental"));

        Agent_FSM crystalElemental_FSM = crystalElemental.gameObject.GetComponent<Agent_FSM>();

        crystalElemental_FSM.birth_place = this;
        crystalElemental_FSM.ID = factory_ID;

    }
}