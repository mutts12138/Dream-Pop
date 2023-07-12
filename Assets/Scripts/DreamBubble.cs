using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DreamBubble : BasePoppable
{
    [SerializeField] private float popTimer = 3f;
    [SerializeField] private float popPowerDistance = 10f;
    [SerializeField] private float popExplosionLifeSpan = 0.5f;
    private float gravityScale = -20f;

    private bool inflated = false;

    [SerializeField] private GameObject dreamBubbleVisual;
    //to get renderer
    [SerializeField] private GameObject dB_popExplosionVisual;
    //to get transform
    [SerializeField] private GameObject dB_popExplosionUpVisual;
    [SerializeField] private GameObject dB_popExplosionDownVisual;
    [SerializeField] private GameObject dB_popExplosionLeftVisual;
    [SerializeField] private GameObject dB_popExplosionRightVisual;



    private void Awake()
    {
        canPop = true;
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
        gameObject.layer = LayerMask.NameToLayer("dreamBubbleDeflate");

    }

    private void Update()
    {
        HandlePopTimer();
        HandleGravity();
        if (!inflated)
        {
            if (!CheckPlayerOnBubble())
            {
                InflateBubble();
            }
        }
        
    }
    
    private void HandlePopTimer()
    {
        if (canPop == true)
        {
            popTimer -= Time.deltaTime;
            if (popTimer < 0)
            {
                Pop();
            }
        }
        else
        {
            popExplosionLifeSpan -= Time.deltaTime;
            if (popExplosionLifeSpan < 0)
            {
                Destroy(gameObject);
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

    private bool CheckPlayerOnBubble()
    {
        int layerNumber = 8;
        int layerMask;
        layerMask = 1 << layerNumber;

        if(!(Physics.OverlapBox(new Vector3(transform.position.x, transform.position.y +1, transform.position.z), new Vector3(1, 1, 1), transform.rotation, layerMask).Length == 0))
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
        dreamBubbleVisual.GetComponent<Transform>().localScale = new Vector3(2, 2, 2);

        //set dreambubble's layer to inflated
        gameObject.layer = LayerMask.NameToLayer("dreamBubble");


        //bring up all player, item up above the dream bubble.
        int layerNumber;
        int layerMask;
        layerNumber = 8;
        layerMask = 1 << layerNumber;
        Collider[] everythingWithinDB = Physics.OverlapBox(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), new Vector3(1, 1, 1), transform.rotation, layerMask);
        foreach (Collider col in everythingWithinDB)
        {
            col.gameObject.transform.position += new Vector3(0, 2, 0);
        }
    }


    public override void Pop()
    {
        //BUG: sometimes not destroying the block
        canPop = false;
        gravityScale = 0;
        Vector3 offsetTransformPosition = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
        Ray rayUp = new Ray(offsetTransformPosition, new Vector3(0, 0, 1));
        Ray rayDown = new Ray(offsetTransformPosition, new Vector3(0, 0, -1));
        Ray rayLeft = new Ray(offsetTransformPosition, new Vector3(-1, 0, 0));
        Ray rayRight = new Ray(offsetTransformPosition, new Vector3(1, 0, 0));
        Ray rayBelow = new Ray(offsetTransformPosition, new Vector3(0,-1,0));


        Ray[] ray4Directions = { rayUp, rayDown, rayLeft, rayRight, rayBelow };

        int layerNumber = 6;
        int layerMask;
        layerMask = 1 << layerNumber;

        float[] explosionRange = new float[5];

        //raycast in 5 directions, store obj hit distance in explosionRange[]
        int counter = 0;
        foreach (Ray ray in ray4Directions)
        {
            if (Physics.Raycast(ray, out RaycastHit raycastHit, popPowerDistance, layerMask))
            {
                if(raycastHit.transform.TryGetComponent(out BasePoppable poppable))
                {
                    Debug.Log("got component");
                    if (poppable.GetCanPop() == true)
                    {
                        Debug.Log("called pop");
                        poppable.Pop();
                    }
                    
                }
                explosionRange[counter] = raycastHit.distance;
            }
            else
            {
                explosionRange[counter] = popPowerDistance;
            }
            counter++;
        }


        //explosionRange[]: 0:up, 1:down, 2:left, 3:right 4:below
        dB_popExplosionUpVisual.transform.localScale += new Vector3(0.75f, 0.75f, explosionRange[0] - 1f);
        dB_popExplosionDownVisual.transform.localScale += new Vector3(0.75f, 0.75f, explosionRange[1] - 1f);
        dB_popExplosionLeftVisual.transform.localScale += new Vector3(explosionRange[2] - 1f, 0.75f, 0.75f);
        dB_popExplosionRightVisual.transform.localScale += new Vector3(explosionRange[3] - 1f, 0.75f, 0.75f);


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


    
}
