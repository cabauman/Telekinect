using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour
{

    public enum MenuState {activating, idle, deactivating};

    public Transform 		MenuLight;
	public Transform 		MenuLogo ;
    public MenuItem[]  		myItems;
    public Menu[]       	subMenus;
    public Menu         	parentMenu;
   
    MenuState           	currentState;
    public int          	activeItemCount = 0;
    public bool         	scaleTime;

    public Collider     	terrain;
   

	// Use this for initialization
	void Start () 
    {
        //myItems = GetComponentsInChildren<MenuItem>();
		
//		Rigidbody[] rigids = GetComponentsInChildren<Rigidbody>();
//		foreach(Rigidbody rigid in rigids)
//		{
//			rigid.detectCollisions = false;	
//		}
		
		MenuItem[] items = GetComponentsInChildren<MenuItem>();
		foreach(MenuItem item in items)
		{
			item.gameObject.transform.parent = this.gameObject.transform;	
		}
	}
	
	// Update is called once per frame
	void Update () 
    {
	    switch(currentState)
        {
            case MenuState.idle:                    //do nothing in idle
                
			if ( MenuLight.GetComponent<Light>().intensity < 2)
			{
				MenuLight.GetComponent<Light>().intensity += 0.1f ;
			}
            
			break;
			
            case MenuState.activating:              //Just switch to idle (menu items will animate to their proper positions)

                currentState = MenuState.idle;
                break;
			
            case MenuState.deactivating:            //Wait for all the menu items to animate to their deactivated positions, then deactivate the game object

                if(activeItemCount == 0)
                {
                    currentState = MenuState.idle;
                    if(scaleTime)
                    {
						if(Network.peerType != NetworkPeerType.Disconnected)
							HandFSM.playerController.GetComponent<NetworkView>().RPC("ResumeTime", RPCMode.All);
						else
							HandFSM.playerController.ResumeTime();
                    }
                    this.gameObject.active = false;
                }
			
				if ( MenuLight.GetComponent<Light>().intensity > 0)
				{
					MenuLight.GetComponent<Light>().intensity -= 0.1f ;
				}
				else
				{
					MenuLight.gameObject.active = false ;
					MenuLogo.gameObject.active = false ;
				}
						
                break;
        }
	}

    public void Activate()
    {

        foreach(MenuItem item in myItems)
        {
            item.gameObject.active = true;

			int children = item.transform.childCount;

			for (int childnumber = 0; childnumber < children; childnumber++)
			{
				Transform child = item.gameObject.transform.GetChild( childnumber );
				//child.gameObject.animation["menu_item_anim"].speed = 10 ;
				child.gameObject.active = true;
			}
			
            item.gameObject.transform.localPosition = item.itemStartOffset;
            item.itemState = MenuItem.ItemState.animatingUp;
            item.currentTime = 0.0f;
            activeItemCount++;
        }
		
		MenuLight.gameObject.active = true ;
		MenuLogo.gameObject.active = true ;

        if(parentMenu != null)
        {
            parentMenu.activeItemCount++;
            parentMenu.Deactivate();
        }

        if(scaleTime)
        {
			if(Network.peerType != NetworkPeerType.Disconnected)
				HandFSM.playerController.GetComponent<NetworkView>().RPC("SlowTime", RPCMode.All);
			else
				HandFSM.playerController.SlowTime();
        }

        currentState = MenuState.activating;
    }

    public void Deactivate()
    {
        foreach(MenuItem item in myItems)
        {
            item.itemState = MenuItem.ItemState.deactivated;
        }

        if(parentMenu != null)
        {
            parentMenu.activeItemCount--;
            parentMenu.Activate();
        }

        currentState = MenuState.deactivating;
    }

}
