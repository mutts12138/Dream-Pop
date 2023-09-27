using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerEnsureSingleton : MonoBehaviour
{
    public static NetworkManagerEnsureSingleton Instance;
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance != null)
        {

            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
