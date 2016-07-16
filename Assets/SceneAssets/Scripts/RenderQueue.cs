using UnityEngine;
using System.Collections;

public class RenderQueue : MonoBehaviour
{
    public int valueForRender;

	// Use this for initialization
	void Start () 
    {
        GetComponent<Renderer>().material.renderQueue = valueForRender;
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
