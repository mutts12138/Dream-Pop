using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuffSO : ScriptableObject
{
    public string buffName;
    public bool isDebuff;
    //buff icon
    public float duration;
    public bool isDurationStacked;
    public bool isEffectStacked;

    public abstract Buff InitializeBuff(BuffHolder buffHolder);

}
