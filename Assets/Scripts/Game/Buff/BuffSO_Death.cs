using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Buffs/Debuffs/Death")]
public class BuffSO_Death : BuffSO
{
    public override Buff InitializeBuff(BuffHolder buffHolder)
    {
        return new Buff_Death(this, buffHolder);
    }
}
