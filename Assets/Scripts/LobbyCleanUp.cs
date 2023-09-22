using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyCleanUp : MonoBehaviour
{

    private void Awake()
    {
        if (WaitingRoomManager.Instance != null)
        {
            Destroy(WaitingRoomManager.Instance.gameObject);
        }

        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }
    }

}
