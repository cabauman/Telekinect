using UnityEngine;
using System.Collections;

public class FanBlade_Triggered_Crystalization_Script : MonoBehaviour
{
	public ParticleEmitter fanBladeParticles ;
	public FanActivation fanWindArea ;
	public Factory_Wind_Elemental windyFactory ;
	
	public Trigger myTrigger ;
		
	public void Triggered()
	{
		if( myTrigger.isActive == true )
		{
			Element_Crystal crystal =this.gameObject.AddComponent<Element_Crystal>() ;
			
			crystal.useParticle = crystal.isPool = crystal.canTransfer = crystal.useMesh = true ;
			crystal.destroyObject = false ; 
			crystal.CrystalScale = new Vector3( 3.0f, 1.0f, 3.0f ) ;
			
			this.gameObject.GetComponent<Animation>().Stop() ;
			fanBladeParticles.emit = false ;
			
			windyFactory.isActive = false ;
			
//			ParticleEmitter windParticles = windyFactory.gameObject.GetComponentInChildren<ParticleEmitter>() ;
//			
//			if (windParticles != null	)
//				windParticles.emit = false ;
			
			if (Network.peerType != NetworkPeerType.Disconnected)
			{
				fanWindArea.GetComponent<NetworkView>().RPC("Deactivate", RPCMode.All) ;
			}
			else
			{
				fanWindArea.Deactivate() ;		
			}
		}
		else
		{
			if( this.GetComponent<Animation>().isPlaying == false )
			{
				this.GetComponent<Animation>().Play() ;
				fanBladeParticles.emit = true ;
				if (Network.peerType != NetworkPeerType.Disconnected)
				{
					fanWindArea.GetComponent<NetworkView>().RPC("Activate", RPCMode.All) ;
				}
				else
				{
					fanWindArea.Activate() ;		
				}
			}
		}
	}
	
	public void UnTriggered()
	{
	}
}

