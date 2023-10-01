using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Blocks/BlockSO_PickUpBasic")]
public class BlockSO_PickUpBasic : BlockSO
{
    
    
    public override Block InitializeBlock(BlockObject blockObject)
    {
        Block_PickUpBasic newBlock = new Block_PickUpBasic(this, blockObject);

        return newBlock;
    }
}
