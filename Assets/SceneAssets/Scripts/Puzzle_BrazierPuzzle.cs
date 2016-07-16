using UnityEngine;
using System.Collections;

public class Puzzle_BrazierPuzzle : MonoBehaviour 
{
    public Trigger Brazier1;
    public Trigger Brazier2;
    public Trigger Brazier3;
    public Trigger Brazier4;
    public GameObject UN_ActivatedAsset, ActivatedAsset;

    public float timer = 0.0f;
    public float maxTimeDelta = 2.0f;
    public bool timerActive = false;
    public bool puzzleActive = true;

	// Use this for initialization
	void Awake () 
    {
        Brazier1.isActive = false;
        Brazier2.isActive = false;
        Brazier3.isActive = false;
        Brazier4.isActive = false;

        if (UN_ActivatedAsset == null)
        {
            UN_ActivatedAsset.SetActiveRecursively(true);
            Debug.Log("No 'un-active' object. Set object in editor");
        }

        if (ActivatedAsset == null)
        {
            ActivatedAsset.SetActiveRecursively(false);
            Debug.Log("No 'active' object. Set object in editor");
        }
	}

    // Update is called once per frame
    void Update()
    {
        if (timerActive)
            timer += Time.deltaTime;

        if (timer >= maxTimeDelta)
            timerActive = false;

        if (timer == 0.0f && Brazier1.isActive || Brazier2.isActive || Brazier3.isActive || Brazier4.isActive)
            timerActive = true;

        if (timer != 0.0f && !Brazier1.isActive && !Brazier2.isActive && !Brazier3.isActive && !Brazier4.isActive)
            timer = 0.0f;

        if ((timer < maxTimeDelta) && Brazier1.isActive && Brazier2.isActive && Brazier3.isActive && Brazier4.isActive)
        {
            if (puzzleActive)
            {
                if (UN_ActivatedAsset != null)
                    UN_ActivatedAsset.SetActiveRecursively(false);

                if (ActivatedAsset != null)
                    ActivatedAsset.SetActiveRecursively(true);

                puzzleActive = false;
            }
        }
	}
}
