using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Video;
using Unity.VisualScripting;

public class Camera_followPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Player player;
    [SerializeField] Vector3 camIniPos;

    private CinemachineVirtualCamera vCam;

    // Update is called once per frame
    private void Start()
    {
        vCam = GetComponent<CinemachineVirtualCamera>();
        //hardcoded position
        transform.position = camIniPos;
    }

    private void Update()
    {
        FollowPositionX();
        FOVPositionZ();
        
    }

    private float GetDistanceToPlayer()
    {
        float XZdistanceToPlayer = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(player.transform.position.x, 0, player.transform.position.z));
        //Debug.Log(XZdistanceToPlayer);
        return XZdistanceToPlayer;
    }

    private void FollowPositionX()
    {
        float playerPositionX = player.transform.position.x;
        transform.position = new Vector3(playerPositionX, transform.position.y, transform.position.z);
    }

    private void FOVPositionZ()
    {
        //distance 0-70
        //lerp 0-1
        
        float lerpValue = 1- ((GetDistanceToPlayer()-5) / 70);

        float FOVValue = Mathf.Lerp(15, 25, lerpValue);
        vCam.m_Lens.FieldOfView = FOVValue;
    }
    

        
            
    
}
