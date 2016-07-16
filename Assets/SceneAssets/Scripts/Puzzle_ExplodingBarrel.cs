using UnityEngine;
using System.Collections;

public class Puzzle_ExplodingBarrel : MonoBehaviour 
{
    public Trigger Trigger1;
    public Trigger Trigger2;
    public Trigger Trigger3;
    public Trigger Trigger4;
    public Trigger Trigger5;
    public Trigger Trigger6;
    public GameObject UN_ActivatedAsset, ActivatedAsset, explosion;

    public float timer = 0.0f;
    public float maxTimeDelta = 1.0f;
    public bool timerActive = false;
    public bool puzzleActive = true;

    // Use this for initialization
    void Awake()
    {
        Trigger1.isActive = false;
        Trigger2.isActive = false;
        Trigger3.isActive = false;
        Trigger4.isActive = false;
        Trigger5.isActive = false;
        Trigger6.isActive = false;

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

        if (timer == 0.0f && Trigger1.isActive || Trigger2.isActive || Trigger3.isActive || Trigger4.isActive || Trigger5.isActive || Trigger6.isActive)
            timerActive = true;

        if (timer != 0.0f && !Trigger1.isActive && !Trigger2.isActive && !Trigger3.isActive && !Trigger4.isActive && !Trigger5.isActive && !Trigger6.isActive)
            Destroy(this);

        if ((timer < maxTimeDelta) && Trigger1.isActive && Trigger2.isActive && Trigger3.isActive && Trigger4.isActive && Trigger5.isActive && Trigger6.isActive)
        {
            if (puzzleActive)
            {
                if (UN_ActivatedAsset != null)
                    UN_ActivatedAsset.SetActiveRecursively(false);

                if (ActivatedAsset != null)
                    ActivatedAsset.SetActiveRecursively(true);

                puzzleActive = false;

                Destroy(this.gameObject);
                explosion = Instantiate(explosion, this.gameObject.transform.position, Quaternion.identity) as GameObject;
            }
        }
    }
}
