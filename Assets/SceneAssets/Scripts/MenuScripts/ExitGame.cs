using UnityEngine;
using System.Collections;

public class ExitGame : MenuButton
{
    public override void Activate()
    {
        myMenu.Deactivate();
		
		HandFSM.playerController.ExitToTitle();

		Application.Quit();
    }
}
