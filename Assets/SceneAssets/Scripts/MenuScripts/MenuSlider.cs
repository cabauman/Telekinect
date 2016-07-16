using UnityEngine;
using System.Collections;

public abstract class MenuSlider : MenuItem 
{

    public float   minValue;
    public float   maxValue;
    public float   currentValue;
    public float   slideEnd = 2.0f;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	public override void ItemUpdate ()
    {
	    base.ItemUpdate();

        if(itemState == ItemState.idle)
        {
            float currentDist = (this.itemEndOffset - this.gameObject.transform.localPosition).magnitude;

            if(currentDist > slideEnd)
            {
                this.transform.localPosition = this.itemEndOffset + (this.transform.forward * slideEnd);
                this.GetComponent<Rigidbody>().velocity = Vector3.zero;
                currentDist = slideEnd;
            }

            if((itemEndOffset.z - transform.localPosition.z) < 0.0f)   //only allow slider to move on positive z
            {
                transform.localPosition = itemEndOffset;
            } 
 
            currentValue = ((currentDist / slideEnd ) * (maxValue - minValue)) + minValue;      //percentage of the amount we have moved clamped to the min and max values

            UpdateSliderValue();
        }
	}

    public abstract void UpdateSliderValue();
}