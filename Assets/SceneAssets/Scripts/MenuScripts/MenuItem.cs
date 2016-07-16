using UnityEngine;
using System.Collections;

public abstract class MenuItem : MonoBehaviour 
{

    public enum ItemState {animatingUp, idle, activated, deactivated, animatingDown};
   
    public ItemState    itemState;
    public Vector3      itemStartOffset;         //the offset from the player to the button's starting location
    public Vector3      itemEndOffset;           //the offset from the player to the button's ending location
    public float        animationTime = 1.0f;    //how long it takes for the button to go from its start offset, to its end offset
    public float        currentTime;

    public Menu         myMenu;                 //reference back to the menu this menu item belongs toscenese
	
	public Material	menuMaterial ;
        
	// Use this for initialization
	void Start () 
    {
	    itemState = ItemState.animatingUp;
        currentTime = 0.0f;

        Physics.IgnoreCollision(myMenu.terrain, this.GetComponent<Collider>(), true);
	}
	
	// Update is called once per frame
	void Update () 
    {
        ItemUpdate();
	}

    public virtual void ItemUpdate()
    {
        switch (itemState)
        {
            case ItemState.animatingUp:

                currentTime += Time.deltaTime / Time.timeScale;
                transform.localPosition = Vector3.Lerp( transform.localPosition, itemEndOffset, currentTime / animationTime);

                menuMaterial.SetFloat("_Cutoff", Mathf.Max(0.01f, 1 - (currentTime / animationTime)));

                if(currentTime >= animationTime)
                {
                    itemState = ItemState.idle;

					int children = this.transform.GetChildCount();
					for(int child = 0; child < children; child++)
					{
						this.transform.GetChild(child).gameObject.active = true;
					}

                    GetComponent<Rigidbody>().isKinematic = false;
                    //collider.isTrigger = false;
					GetComponent<Rigidbody>().velocity = Vector3.zero;
					GetComponent<Rigidbody>().isKinematic = true;
                }

                break;
            case ItemState.idle:

                //restrict movement to the local z axis of the object
				GetComponent<Rigidbody>().isKinematic = false;
			
				GetComponent<Rigidbody>().velocity = new Vector3(0.0f, 0.0f, Vector3.Dot(this.transform.forward, GetComponent<Rigidbody>().velocity));
//                Vector3 localVelocity = rigidbody.velocity;                             //Get object velocity in world space
//                localVelocity = transform.InverseTransformDirection(localVelocity);     //transform to local space
//                localVelocity.x = localVelocity.y = 0.0f;                               //set X and Y components to zero
//                rigidbody.velocity = transform.TransformDirection(localVelocity);       //transform back to world space
				this.gameObject.transform.Translate(GetComponent<Rigidbody>().velocity * (Time.deltaTime / Time.timeScale));
			
				GetComponent<Rigidbody>().isKinematic = true;

                break;
            case ItemState.deactivated:
	
				GetComponent<Rigidbody>().isKinematic = false;
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                GetComponent<Rigidbody>().isKinematic = true;
			
                //collider.isTrigger = true;
                itemState = ItemState.animatingDown;
                currentTime = 0.0f;


                break;
            case ItemState.animatingDown:

                currentTime += Time.deltaTime / Time.timeScale;
                transform.localPosition = Vector3.Lerp( transform.localPosition, itemStartOffset, currentTime / animationTime);

                menuMaterial.SetFloat("_Cutoff",  (currentTime / animationTime));

                if(currentTime >= animationTime)
                {
                    itemState = ItemState.animatingUp;
					
					int children = this.transform.GetChildCount();
					for(int child = 0; child < children; child++)
					{
						this.transform.GetChild(child).gameObject.active = false;
					}

                    currentTime = 0.0f;
                    myMenu.activeItemCount--;
                    this.gameObject.active = false;
                }


                break;
        }	
    }

    void OnGUI()
    {
            //GUILayout.BeginArea (new Rect (0, 900, 300, 300));

            //    GUILayout.Box(string.Format("Button state: {0}", itemState.ToString()));

            //GUILayout.EndArea();
    }

}
