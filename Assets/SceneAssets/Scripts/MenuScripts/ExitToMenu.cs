using UnityEngine;
using System.Collections;

public class GotoSubMenu : MenuButton
{
    public override void Activate()
    {
        myMenu.subMenus[0].gameObject.active = true;
        myMenu.subMenus[0].Activate();
    }
}
