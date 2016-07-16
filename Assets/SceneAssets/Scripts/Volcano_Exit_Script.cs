using UnityEngine;
using System.Collections;

public class Volcano_Exit_Script : MonoBehaviour 
{
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 9)
		{
			Destroy(HotValues.Instance().menu.gameObject);

			Application.LoadLevel("Title") ;
		}
	}

}
