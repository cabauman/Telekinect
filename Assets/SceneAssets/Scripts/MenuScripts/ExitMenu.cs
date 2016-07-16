using UnityEngine;
using System.Collections;

public class ExitMenu : MenuButton
{
    public override void Activate()
    {
        myMenu.Deactivate();
    }

}
