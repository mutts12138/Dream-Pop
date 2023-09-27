using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockObject : MonoBehaviour
{
    [SerializeField] private BlockObjectSO blockObjectSO;

    public BlockObjectSO GetBlockObjectSO()
    {
        return blockObjectSO;
    }
}
