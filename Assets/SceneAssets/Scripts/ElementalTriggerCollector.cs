using UnityEngine;
using System.Collections;

public class ElementalTriggerCollector : MonoBehaviour 
{

	public int currentCount = 0;
	public GameObject masterObject;
	public int triggerCount = 4;
	public void Triggered()
	{
		currentCount++;
		if(currentCount == triggerCount && masterObject != null)
			masterObject.SendMessage("Triggered");
	}
	
	public void UnTriggered()
	{
		currentCount--;	
	}
}
