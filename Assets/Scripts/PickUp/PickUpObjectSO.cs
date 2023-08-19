using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PickUpObjectSO : ScriptableObject
{
    public Transform prefab;
    public string objectName;

    public int bubbleUp;
    public int powerUp;
    public int speedUp;

    public int dreamFragCount;
}
