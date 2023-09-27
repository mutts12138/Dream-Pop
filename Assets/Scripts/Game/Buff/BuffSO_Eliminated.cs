using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Buffs/Debuffs/Eliminated")]
public class BuffSO_Eliminated : BuffSO
{
    public override Buff InitializeBuff(BuffHolder buffHolder)
    {
        return new Buff_Eliminated(this, buffHolder);
    }
}
