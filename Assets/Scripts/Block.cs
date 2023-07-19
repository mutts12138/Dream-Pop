using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Block : BasePoppable
{
    
    private float gravityScale = -10f;

    private void Awake()
    {
        SetCanPop(true);
    }

    private void Update()
    {
        HandleGravity();
    }

    private void HandleGravity()
    {
        float gravity = gravityScale * Time.deltaTime;
        if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y +0.1f, transform.position.z), Vector3.down,out RaycastHit raycastHit, 1f))
        {
            transform.position -= Vector3.down * gravity;
        }
        else
        {

            transform.position = new Vector3(transform.position.x, Mathf.Floor(transform.position.y), transform.position.z);
            
        }
    }
    public override void Pop()
    {
        SetCanPop(false);
        Destroy(gameObject);
    }


}
