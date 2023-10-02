using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(PickUpHolder))]
public class Block_PickUpBasic : Block
{
    BlockSO_PickUpBasic blockSO_PickUpBasic;

    public Block_PickUpBasic(BlockSO blockSO, BlockObject blockObject) : base(blockSO, blockObject)
    {
        blockSO_PickUpBasic = (BlockSO_PickUpBasic)blockSO;
    }
    

    public override void PopEffect()
    {
        //tell the game manager to instantiate pickup object, and then destroy self

        Debug.Log("block poped, pickup spawned");
        if(blockObject.TryGetComponent<PickUpHolder>(out PickUpHolder pickUpHolder))
        {
            foreach(PickUp pickUp in pickUpHolder.GetPickUpsHeld().Values)
            {
                int count = pickUp.GetPickUpStacks();
                for (int i = count; i > 0; i--)
                {
                    GameMultiplayer.Instance.SpawnPickUpObject(blockObject.transform.position, pickUp.pickUpSO);
                }   
            }
        }
        else
        {
            Debug.Log("pickUpholder doesnt exist on this block");
        }

        blockObject.DestroySelf();
        
    }

    //call by gamemanager to add pickup based on pickup pool of the map
    
}
