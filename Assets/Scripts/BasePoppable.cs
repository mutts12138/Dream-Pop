using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePoppable : MonoBehaviour
{
    //virtual vs abstract?

    public bool canPop;
    public virtual void Pop() { }

    public virtual bool GetCanPop() 
    {
        return canPop;
    }

    public virtual void SetCanPop(bool isPoppable)
    {
        canPop = isPoppable;
    }
}
