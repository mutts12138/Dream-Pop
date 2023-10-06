using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class BlockObject : NetworkBehaviour, ActivateByDreamPop
{

    [SerializeField] private BlockSO blockSO;

    private Block block;

    private bool isActivated = false;


    private float gravityScale = -10f;
    private void Awake()
    {
        if (!IsServer) return;
        //gameObject.GetComponent<NetworkObject>().Spawn(true);
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        block = blockSO.InitializeBlock(this);

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

        int pickUpLayerMask = 7;
        int layerMask = 1 << pickUpLayerMask;

        if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y +0.1f, transform.position.z), Vector3.down,out RaycastHit raycastHit, 1f, ~layerMask))
        {
            transform.position -= Vector3.down * gravity;
        }
        else
        {

            transform.position = new Vector3(transform.position.x, Mathf.Floor(transform.position.y), transform.position.z);
            
        }
    }




    public  void Activate()
    {
        if (isActivated) return;
        SetIsActivated(true);
        //destroy self will be included in popeffect if applies.
        block.PopEffect();
    }



    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public bool GetIsActivated()
    {
        return isActivated;
    }

    public void SetIsActivated(bool newIsActivated)
    {
        isActivated = newIsActivated;
    }

    public override void OnDestroy()
    {
        if (IsServer)
        {
            if (gameObject.GetComponent<NetworkObject>().IsSpawned == true)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
            }

        }

        base.OnDestroy();
    }
}
