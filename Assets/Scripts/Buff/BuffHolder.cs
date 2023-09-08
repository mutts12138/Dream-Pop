using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffHolder : MonoBehaviour
{
    private readonly Dictionary<BuffSO, Buff> _buffs = new Dictionary<BuffSO, Buff>();
    

    void Update()
    {
        foreach (var buff in _buffs.Values.ToList())
        {
            buff.Tick(Time.deltaTime);
            if (buff.isFinished)
            {
                _buffs.Remove(buff.buff);

            }

        }

    }


    public void AddBuff(Buff buff)
    {
        if (_buffs.ContainsKey(buff.buff))
        {
            _buffs[buff.buff].OnAddBuff();
        }
        else
        {
            _buffs.Add(buff.buff, buff);
            buff.OnAddBuff();
        }
    }

}
