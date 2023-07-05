using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardBlock : Block
{
    private void Awake()
    {
        canPop = false;
    }
}
