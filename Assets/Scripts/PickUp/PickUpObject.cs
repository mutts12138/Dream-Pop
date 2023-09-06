using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class PickUpObject : NetworkBehaviour
{

    [SerializeField] private PickUpSO pickUpSO;

    bool isPickedUp = false;

    

    private void Awake()
    {
        if (!IsServer) return;
        //gameObject.GetComponent<NetworkObject>().Spawn(true);
    }

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        StartCoroutine(collisionDetectionAsync(0.1f));
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        HandleGravity(-20f);
    }


    //let server run this update
    /*private void HandlePickUpCollision()
    {
        //player capsule
        Vector3 capsuleBotPoint = new Vector3(transform.position.x, transform.position.y + (playerRadius) + 0.01f, transform.position.z);
        Vector3 capsuleTopPoint = transform.position + (Vector3.up * (playerHeight - (playerRadius) - 0.01f));
        float capsuleRadius = playerRadius + 0.5f;

        

        //check collision
        

        

    }*/

    IEnumerator collisionDetectionAsync(float newTickRate)
    {
        WaitForSeconds tickRate = new WaitForSeconds(newTickRate);

        float overlapSphereRadius = 1f;

        int layerNumber = 3;
        int layerMask;
        layerMask = 1 << layerNumber;

        while (enabled && IsServer && isPickedUp == false)
        {
            Collider[] pickUpColliders = Physics.OverlapSphere(transform.position, overlapSphereRadius, layerMask);

            foreach (Collider pickUpCollider in pickUpColliders)
            {
                if(pickUpCollider.gameObject.TryGetComponent<PickUpHolder>(out PickUpHolder pickUpHolder)){

                    PickUpHolderPickedUp(pickUpHolder);
                    isPickedUp = true;
                }


                
            }  
            yield return tickRate;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        
    }


    public void PickUpHolderPickedUp(PickUpHolder pickUpHolder)
    {
        //NetworkManager.SpawnManager.GetPlayerNetworkObject(clientID).GetComponent<Player>().ChangeCharacterBaseStatLevelsClientRpc(pickUpObject.speedUp, pickUpObject.bubbleUp, pickUpObject.powerUp);
        //player.ChangeCharacterBaseStatLevelsClientRpc(pickUpObject.speedUp, pickUpObject.bubbleUp, pickUpObject.powerUp);
        

        pickUpHolder.AddPickUpToInventory(pickUpSO.InitializePickUpEffect(pickUpHolder));


        //despawn after picked up
        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }









    //make a separate script for gravity
    private void HandleGravity(float gravityScale)
    {
        float gravity = gravityScale * Time.deltaTime;
        if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z), Vector3.down, out RaycastHit raycastHit, 1f))
        {

            transform.position -= Vector3.down * gravity;
        }
        else
        {
            transform.position = new Vector3(transform.position.x, Mathf.Floor(transform.position.y), transform.position.z);
        }

    }
}
