using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(PickUpObject))]
public class PickUp : NetworkBehaviour
{
    private PickUpObject pickUpObject;

    private float gravityScale = -20f;
    

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        pickUpObject = gameObject.GetComponent<PickUpObject>();

    }

    // Update is called once per frame
    void Update()
    {
        HandleGravity();
    }

    public void PlayerPickedUp(Player player)
    {
        player.ChangeCharacterBaseStatLevels(pickUpObject.speedUp, pickUpObject.bubbleUp, pickUpObject.powerUp);
        Destroy(gameObject);
    }

    private void HandleGravity()
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
