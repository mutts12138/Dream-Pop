using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class DreamBubble : NetworkBehaviour, Ipoppable
{
    private float timerCountDown = 3.5f;
    private float popTime = 0.5f;
    private float inflateTime = 2.5f;
    private readonly float tickRate = 0.1f;


    [SerializeField] private float popPowerDistance = 10f;
    private float popExplosionLifeSpan = 0.5f;
    private float gravityScale = -20f;

    private bool isPopped = false;
    private bool isInflated = false;

    [SerializeField] private GameObject dreamBubbleVisual;
    //to get renderer
    [SerializeField] private GameObject dB_popExplosionVisual;
    //to get transform
    [SerializeField] private GameObject dB_popExplosionUpVisual;
    [SerializeField] private GameObject dB_popExplosionDownVisual;
    [SerializeField] private GameObject dB_popExplosionLeftVisual;
    [SerializeField] private GameObject dB_popExplosionRightVisual;

    float[] explosionRanges;

    private PlayerCharacter player;

    

    [SerializeField] private BuffSO buffSO;

    public override void OnNetworkSpawn()
    {

        //hide popExplosionRender
        Renderer[] popExplosionRendererArray = dB_popExplosionVisual.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in popExplosionRendererArray)
        {
            renderer.enabled = false;
        }
        //deflate
        dreamBubbleVisual.GetComponent<Transform>().localPosition = new Vector3(0, 0, 0);
        dreamBubbleVisual.GetComponent<Transform>().localScale = new Vector3(1.75f, 1, 1.75f);



        if (!IsServer) return;
        StartCoroutine(InflateTimerAsync(tickRate));
        StartCoroutine(PopTimerAsync(tickRate));
        
    }

    private void Update()
    {
        
        if (!IsServer) return;
        
        if (timerCountDown > 0)
        {
            timerCountDown -= Time.deltaTime;
        }

        HandleGravity();
    }
    IEnumerator InflateTimerAsync(float tickRate)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(tickRate);
        

        while (timerCountDown > inflateTime && isInflated == false)
        {
            
            Debug.Log("timer " + timerCountDown.ToString());
            if (!CheckPlayerOnBubble())
            {
                InflateBubble();
            };
            yield return waitForSeconds;
        }

        InflateBubble();
    }

    IEnumerator PopTimerAsync(float tickRate)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(tickRate);

        while (timerCountDown > popTime && isPopped == false)
        {
            yield return waitForSeconds;
        }
  
        Pop();  
    }


    //coroutine, lower tick rate
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

    private void InflateBubble()
    {
        if (isInflated) return;
        //place deflated bubble, visual and collider
        //when player not stepping on it anymore, inflate, boxcastall foreach check if play
        //when player place another bubble, inflate
        //when inflated, everything inside boxcastall get position.y set to this.position.y +1
        isInflated = true;

        //visual
        InflateBubbleVisualClientRpc();

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
            if (col.gameObject.TryGetComponent<PlayerCharacter> (out PlayerCharacter playersOnTop)){
                playersOnTop.InflateBubblePushUpClientRpc();
            }
            else
            {
                col.gameObject.transform.position += new Vector3(0, 2, 0);
            }
            
        }
        
    }

    [ClientRpc]
    private void InflateBubbleVisualClientRpc()
    {
        dreamBubbleVisual.GetComponent<Transform>().localPosition = new Vector3(0, 1, 0);
        dreamBubbleVisual.GetComponent<Transform>().localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }

    public void Pop()
    {
        if (!IsServer) return;
        if (isPopped == true) return;
        SetIsPopped(true);
        timerCountDown = popTime;

        explosionRanges = CalculateExplosionRanges();
        //visual
        popExplosionEnableVisualClientRpc(explosionRanges);

        StartCoroutine(PopExplosionApplyHitAsync(tickRate));

    }

    private float[] CalculateExplosionRanges()
    {
        float[] explosionRanges;
        //BUG: sometimes not destroying the block
        
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
                    if (poppable.GetIsPopped() == false)
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
    //make coroutine, lower tick rate
    IEnumerator PopExplosionApplyHitAsync(float tickRate)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(tickRate);

        Quaternion newRotation = new Quaternion(1, 0, 0, 0);
        Vector3 newCenter = new Vector3(0, 0, 0);
        Vector3 newHalfExtent = new Vector3(0, 0, 0);

        //player  mask
        int layerNumber = 3;
        int layerMask;
        layerMask = 1 << layerNumber;

        while (timerCountDown > 0)
        {
            if (timerCountDown > 0.15f)
            {
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
                        //get components
                        PlayerCharacter playerHitted = collider.GetComponent<PlayerCharacter>();
                        BuffHolder playerHittedBuffHolder = collider.GetComponent<BuffHolder>();

                        //check if the player is invincible
                        if (!(playerHitted.GetInvincibleStack() > 0)) {
                            //Apply effect
                            //asleep put as a debuff
                            AddBuffToPlayerHitClientRpc(playerHitted.ownerClientID.Value);
                            Debug.Log("player hit, apply asleep debuff");
                        }
                        


                        //change player layer on server to prevent multiple procs
                        //"playerAsleep" = 7
                        //ChangeLayer(7);
                        //player.SetCurrentPlayerStateClientRpc(Player.PlayerStates.asleep);
                        //Debug.Log("playerHit");
                    }  
                }  
            }
            yield return waitForSeconds;
        }

        Debug.Log("Restore bubblecount to: " + player.ownerClientID.Value);
        player.ChangeBubbleCountClientRpc(-1);

        

        //disable instead of destroy
        Destroy(gameObject);
    }

    [ClientRpc]
    public void AddBuffToPlayerHitClientRpc(ulong clientID)
    {
        if (NetworkManager.Singleton.LocalClientId != clientID) return;


        BuffHolder buffHolder = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<BuffHolder>();
        Debug.Log("buffholder == " + buffHolder);
        buffHolder.AddBuff(buffSO.InitializeBuff(buffHolder));
    }

    public void SetPlayer(PlayerCharacter owningPlayer)
    { 
        player = owningPlayer;
    }

    public bool GetInflated()
    {
        return isInflated;
    }

    public bool GetIsPopped()
    {
        return isPopped;
    }

    public void SetIsPopped(bool poppable)
    {
        isPopped = poppable;
    }

    public void SetPopPowerDistance(int bubblePowerLevel)
    {
        popPowerDistance = 2 * bubblePowerLevel;
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
