using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Block_PickUpBasic : Block
{
    BlockSO_PickUpBasic blockSO_PickUpBasic;

    public List<PickUpSO> pickUpsHeld;


    public Block_PickUpBasic(BlockSO blockSO, BlockObject blockObject) : base(blockSO, blockObject)
    {
        blockSO_PickUpBasic = (BlockSO_PickUpBasic)blockSO;
        pickUpsHeld = new List<PickUpSO>();
    }
    public void AddPickUp(PickUpSO pickUpSO)
    {
        pickUpsHeld.Add(pickUpSO);
    }

    public override void PopEffect()
    {
        //tell the game manager to instantiate pickup object, and then destroy self

        Debug.Log("block poped, pickup spawned");
        foreach(PickUpSO pickUpSO in pickUpsHeld)
        {
            GameMultiplayer.Instance.SpawnPickUpObject(blockObject.transform.position, pickUpSO);
        }

        blockObject.DestroySelf();
        
    }

    //call by gamemanager to add pickup based on pickup pool of the map
    
}
