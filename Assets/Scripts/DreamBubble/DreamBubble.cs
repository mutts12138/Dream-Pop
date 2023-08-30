using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class DreamBubble : NetworkBehaviour, Ipoppable
{
    private float popTimer = 3f;
    private float inflateTimer = 1f;

    [SerializeField] private float popPowerDistance = 10f;
    private bool popExplosionEnabled = false;
    private float popExplosionLifeSpan = 0.5f;
    private float gravityScale = -20f;

    private bool canPop = true;
    private bool inflated = false;

    [SerializeField] private GameObject dreamBubbleVisual;
    //to get renderer
    [SerializeField] private GameObject dB_popExplosionVisual;
    //to get transform
    [SerializeField] private GameObject dB_popExplosionUpVisual;
    [SerializeField] private GameObject dB_popExplosionDownVisual;
    [SerializeField] private GameObject dB_popExplosionLeftVisual;
    [SerializeField] private GameObject dB_popExplosionRightVisual;

    float[] explosionRanges;

    private Player player;
    

    public override void OnNetworkSpawn()
    {
        

        SetCanPop(true);
        //hide popExplosionRender
        Renderer[] popExplosionRendererArray = dB_popExplosionVisual.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in popExplosionRendererArray)
        {
            renderer.enabled = false;
        }

        //deflate
        dreamBubbleVisual.GetComponent<Transform>().localPosition = new Vector3(0,0,0);
        dreamBubbleVisual.GetComponent<Transform>().localScale = new Vector3(1.75f, 1, 1.75f);
        //int layerNumber = 7;
        //int layerMask;
        //layerMask = 1 << layerNumber;
        //gameObject.layer = LayerMask.NameToLayer("dreamBubbleDeflate");

    }

    private void Update()
    {
        
        if (!IsServer) return;

        HandleInflateBubble();
        HandlePopTimer();
        HandlePopExplosionApplyHit();


        HandleGravity();
    }
    
    private void HandlePopTimer()
    {
        if (GetCanPop() == true)
        {
            popTimer -= Time.deltaTime;
            if (popTimer < 0)
            {
                Pop();
            }
        }
        
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

    private void HandleInflateBubble()
    {
        if (!inflated)
        {
            if (!CheckPlayerOnBubble() || inflateTimer < 0)
            {
                InflateBubble();
            }

            inflateTimer -= Time.deltaTime;

        }
    }

    private bool CheckPlayerOnBubble()
    {
        int playerLayer = 3;
        int AsleepPlayerLayer = 7;
        
        int layerMask;

        layerMask = 1 << playerLayer;
        layerMask = layerMask | 1 << AsleepPlayerLayer;
        

        if (!(Physics.OverlapBox(new Vector3(transform.position.x, transform.position.y +1, transform.position.z), new Vector3(1, 1, 1), transform.rotation, layerMask).Length == 0))
        {
            return true;
        }
        else
        {
            return false;
        }

        
    }


    public bool GetInflated()
    {
        return inflated;
    }

    public void InflateBubble()
    {
        //place deflated bubble, visual and collider
        //when player not stepping on it anymore, inflate, boxcastall foreach check if play
        //when player place another bubble, inflate
        //when inflated, everything inside boxcastall get position.y set to this.position.y +1
        inflated = true;
        dreamBubbleVisual.GetComponent<Transform>().localPosition = new Vector3(0, 1, 0);
        dreamBubbleVisual.GetComponent<Transform>().localScale = new Vector3(1.5f, 1.5f, 1.5f);

        //set dreambubble's layer to inflated
        //gameObject.layer = LayerMask.NameToLayer("dreamBubble");


        //bring up all player, item up above the dream bubble.
        //player layers: 3, 7, 8
        //item layer: 9


        int playerLayer = 3;
        int AsleepPlayerLayer = 7;
        int pickUpLayer = 9;
        int layerMask;

        layerMask = 1 << playerLayer;
        layerMask = layerMask | 1 << AsleepPlayerLayer;
        layerMask = layerMask | 1 <<  pickUpLayer;
        //Debug.Log(layerMask);

        Collider[] colliders = Physics.OverlapBox(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), new Vector3(1, 1, 1), transform.rotation, layerMask);
        foreach (Collider col in colliders)
        {
            col.gameObject.transform.position += new Vector3(0, 2, 0);
        }
        
    }


    public void Pop()
    {
        if (!IsServer) return;
        explosionRanges = CalculateExplosionRanges();
        popExplosionEnableVisualClientRpc(explosionRanges);

        popExplosionEnabled = true;

    }

    private float[] CalculateExplosionRanges()
    {
        float[] explosionRanges;
        //BUG: sometimes not destroying the block
        SetCanPop(false);
        gravityScale = 0;
        Vector3 offsetTransformPosition = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
        Ray rayUp = new Ray(offsetTransformPosition, new Vector3(0, 0, 1));
        Ray rayDown = new Ray(offsetTransformPosition, new Vector3(0, 0, -1));
        Ray rayLeft = new Ray(offsetTransformPosition, new Vector3(-1, 0, 0));
        Ray rayRight = new Ray(offsetTransformPosition, new Vector3(1, 0, 0));
        Ray rayTop = new Ray(offsetTransformPosition, new Vector3(0, 1, 0));
        Ray rayBelow = new Ray(offsetTransformPosition, new Vector3(0,-1,0));
        

        Ray[] ray4Directions = { rayUp, rayDown, rayLeft, rayRight, rayTop, rayBelow};

        int layerNumber = 6;
        int layerMask;
        layerMask = 1 << layerNumber;

        explosionRanges = new float[6];

        //raycast in 6 directions, get explosionRange[] for visual and actual hitbox, pop dreambubbles hit
        int counter = 0;
        foreach (Ray ray in ray4Directions)
        {
            if (Physics.Raycast(ray, out RaycastHit raycastHit, popPowerDistance, layerMask))
            {
                if (raycastHit.transform.TryGetComponent(out Ipoppable poppable))
                {
                    //Debug.Log("got component");
                    if (poppable.GetCanPop() == true)
                    {
                        //Debug.Log("called pop");
                        poppable.Pop();
                    }

                }
                explosionRanges[counter] = raycastHit.distance;
            }
            else
            {
                explosionRanges[counter] = popPowerDistance;
            }
            counter++;
        }
        //explosionRange[]: 0:up, 1:down, 2:left, 3:right 4:top 5:below
        return explosionRanges;
    }

    //client does the apply visual
    [ClientRpc]
    private void popExplosionEnableVisualClientRpc(float[] explosionRanges)
    {
        const float popExplosionVisualSize = 0.75f;
        //visual
        dB_popExplosionUpVisual.transform.localScale += new Vector3(popExplosionVisualSize, popExplosionVisualSize, explosionRanges[0] - 1f);
        dB_popExplosionDownVisual.transform.localScale += new Vector3(popExplosionVisualSize, popExplosionVisualSize, explosionRanges[1] - 1f);
        dB_popExplosionLeftVisual.transform.localScale += new Vector3(explosionRanges[2] - 1f, popExplosionVisualSize, popExplosionVisualSize);
        dB_popExplosionRightVisual.transform.localScale += new Vector3(explosionRanges[3] - 1f, popExplosionVisualSize, popExplosionVisualSize);
        //dream bubble Renderer.enable = false
        dreamBubbleVisual.GetComponent<Renderer>().enabled = false;
        //dream bubble collider.enable = false
        GetComponent<Collider>().enabled = false;
        //pop explosion Renderer.enable = true
        Renderer[] popExplosionRendererArray = dB_popExplosionVisual.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in popExplosionRendererArray)
        {
            renderer.enabled = true;
        }
    }

    //server does the applyhit

    private void HandlePopExplosionApplyHit()
    {
        if (popExplosionEnabled == false) return;
        if (popExplosionLifeSpan > 0)
        {
            if (popExplosionLifeSpan > 0.15)
            {
                Quaternion newRotation = new Quaternion(1, 0, 0, 0);
                Vector3 newCenter = new Vector3(0, 0, 0);
                Vector3 newHalfExtent = new Vector3(0, 0, 0);

                //player  mask
                int layerNumber = 3;
                int layerMask;
                layerMask = 1 << layerNumber;

                //horizontal and vertical hitbox

                for (int i = 0; i < 2; i++)
                {
                    const float halfExtentXY = 0.75f;

                    //explosionRange[]: 0:up, 1:down, 2:left, 3:right 

                    if (i == 0)
                    {
                        //vertical
                        newRotation = Quaternion.Euler(0, 0, 0);
                        newCenter = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + ((explosionRanges[0] - explosionRanges[1]) / 2));
                        newHalfExtent = new Vector3(halfExtentXY, halfExtentXY, (explosionRanges[0] + explosionRanges[1]) / 2);
                    }
                    else
                    {
                        //horizontal
                        newRotation = Quaternion.Euler(0, 90, 0);
                        newCenter = new Vector3(transform.position.x + ((explosionRanges[3] - explosionRanges[2]) / 2), transform.position.y + 1f, transform.position.z);
                        newHalfExtent = new Vector3(halfExtentXY, halfExtentXY, (explosionRanges[3] + explosionRanges[2]) / 2);

                    }


                    foreach (Collider collider in Physics.OverlapBox(newCenter, newHalfExtent, newRotation, layerMask))
                    {
                        //check if gameobject is player
                        if (collider.TryGetComponent(out Player player))
                        {
                            if (player.GetCurrentPlayerState() == Player.PlayerStates.normal)
                            {
                                player.SetCurrentPlayerStateClientRpc(Player.PlayerStates.asleep);
                                //Debug.Log("playerHit");
                            }
                        }

                    }

                }
            }
            popExplosionLifeSpan -= Time.deltaTime;
        }
        else
        {
            player.RestoreBubbleCountClientRpc();

            gameObject.GetComponent<NetworkObject>().Despawn();
            Destroy(gameObject);
        }
        

        //check item
    }

    public void SetPlayer(Player owningPlayer)
    { 
        player = owningPlayer;
    }

    public bool GetCanPop()
    {
        return canPop;
    }

    public void SetCanPop(bool poppable)
    {
        canPop = poppable;
    }

    public void SetPopPowerDistance(int bubblePowerLevel)
    {
        popPowerDistance = 2 * bubblePowerLevel;
    }
}
