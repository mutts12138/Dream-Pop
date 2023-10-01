using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class BlockSO : ScriptableObject
{
    public string blockName;
    public BlockObject blockObjectPrefab;
    //sprite?


    //it is only initialized on the server
    public abstract Block InitializeBlock(BlockObject blockObject);

    
}
