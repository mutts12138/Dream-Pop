using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpObject : MonoBehaviour
{
    [SerializeField] private PickUpObjectSO pickUpObjectSO;
    public int speedUp {get; private set;}
    public int bubbleUp { get; private set;}
    public int powerUp { get; private set;}

    private void Start()
    {
        speedUp = pickUpObjectSO.speedUp;
        bubbleUp = pickUpObjectSO.bubbleUp;
        powerUp = pickUpObjectSO.powerUp;
    }

    public PickUpObjectSO GetPickUpObjectSO()
    {
        return pickUpObjectSO;
    }
}
