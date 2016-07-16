using UnityEngine;
using System.Collections;

public class StartGame : MenuButton
{
    public override void Activate()
    {
        myMenu.Deactivate();

		Destroy(myMenu.gameObject);

        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

		Application.LoadLevel("Volcano");
    }
}
