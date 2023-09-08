using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Buffs/Debuffs/Asleep")]
public class BuffSO_Asleep : BuffSO
{
    public override Buff InitializeBuff(BuffHolder buffHolder)
    {
        return new Buff_Asleep(this, buffHolder);
    }
}
