using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Block
{
    public BlockSO blockSO { get; }
    protected readonly BlockObject blockObject;
    //there will be blocks that hold item, not destructable, activate triggers etc.

    
    public Block (BlockSO blockSO, BlockObject blockObject)
    {
        this.blockSO = blockSO;
        this.blockObject = blockObject;
    }

    public abstract void PopEffect();
}
