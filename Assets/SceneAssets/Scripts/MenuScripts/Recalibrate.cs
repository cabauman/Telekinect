using UnityEngine;
using System.Collections;

public class Recalibrate : MenuButton
{
    public override void Activate()
    {
		OpenNIUserTracker.Instance().Restart();
		//Destroy(OpenNIUserTracker.Instance().gameObject);
		//Destroy(myMenu.transform.parent.gameObject);
		
		//HandFSM.playerController.userID = 0;
		HandFSM.playerController.userTracker = OpenNIUserTracker.Instance();
		
		HandFSM.playerController.gameObject.transform.position = HotValues.Instance().spawnPoint.GenerateSpawnPoint();
		HandFSM.playerController.gameObject.transform.rotation = HotValues.Instance().spawnPoint.transform.rotation;
		
        myMenu.Deactivate();

//		Destroy(myMenu.gameObject);
//
//        Time.timeScale = 1.0f;
//        Time.fixedDeltaTime = 0.02f * Time.timeScale;
//
//		Application.LoadLevel("Telekinect_Init");
    }
}

