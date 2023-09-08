using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Buff
{
    public BuffSO buff { get; }

    protected float duration;
    protected int effectStacks;
    protected readonly BuffHolder buffHolder;
    public bool isFinished;

    public Buff(BuffSO buff, BuffHolder buffHolder)
    {
        this.buff = buff;
        this.buffHolder = buffHolder;
    }


    //buff duration tick down
    public void Tick(float delta)
    {
        duration -= delta;
        if(duration <= 0)
        {
            BuffEnded();
            isFinished = true;
        }
    }

    public void OnAddBuff()
    {
        if(buff.isEffectStacked || duration <= 0)
        {
            ApplyBuffEffect();
            effectStacks++;
        }

        if(buff.isDurationStacked || duration <= 0)
        {
            duration += buff.duration;
        }
    }

    protected abstract void ApplyBuffEffect();
    public abstract void BuffEnded();

}
