using UnityEngine;
using System.Collections;

public class ExitToTitle : MenuButton
{
    public override void Activate()
    {
        myMenu.Deactivate();
		
		HandFSM.playerController.ExitToTitle();
    }
}
