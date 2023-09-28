using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Block : NetworkBehaviour, Ipoppable
{
    [SerializeField] private bool canBeDestroyed;
    private bool isPopped = false;
    private float gravityScale = -10f;
    private void Awake()
    {
        if (!IsServer) return;
        //gameObject.GetComponent<NetworkObject>().Spawn(true);
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        //gameObject.GetComponent<NetworkObject>().Spawn(true);
    }

    private void Update()
    {
        HandleGravity();
        if (!IsServer) return;
        
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
    public  void Pop()
    {
        SetIsPopped(true);

        if(!canBeDestroyed) return;
        
        Destroy(gameObject);
        
    }

    public bool GetIsPopped()
    {
        return isPopped;
    }

    public void SetIsPopped(bool newIsPopped)
    {
        isPopped = newIsPopped;
    }

    public override void OnDestroy()
    {
        if(IsServer)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
        base.OnDestroy();
    }
}
