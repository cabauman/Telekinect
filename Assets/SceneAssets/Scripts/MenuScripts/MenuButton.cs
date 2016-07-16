using UnityEngine;
using System.Collections;

public abstract class MenuButton : MenuItem
{

    public float        activationDistance = 0.3f;       //how far the button must move towards the player to be considered activated

	// Update is called once per frame
	public override void ItemUpdate () 
    {
	    base.ItemUpdate();

        if(itemState == ItemState.idle)
        {
            if((itemEndOffset.z - transform.localPosition.z) < 0.0f)   
            {
                transform.localPosition = itemEndOffset;
            }       
            else if((transform.localPosition - itemEndOffset).sqrMagnitude > (activationDistance * activationDistance))
            {
                Activate();
            }
        }
	}

    public abstract void Activate();

}
