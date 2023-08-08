using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;
using Mono.CSharp;

public class ServerBehaviour : MonoBehaviour
{

    public NetworkDriver m_Driver;
    //m_Connections creates a NativeList to hold all the connections.
    private NativeList<NetworkConnection> m_Connections;

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;

        //The first line of code, m_Driver = NetworkDriver.Create(), creates a NetworkDriver instance without any parameters.
        //Next, m_Driver.Bind binds the NetworkDriver instance to a specific network address and port, and if that doesn't fail, it calls the Listen method.
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port 9000");
        else
            //The call to the Listen method sets the NetworkDriver to the Listen state, which means the NetworkDriver actively listens for incoming connections.
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestory()
    {
        //You must dispose of both NetworkDriver and NativeList because they allocate unmanaged memory. To ensure proper disposal, call the Dispose method when you no longer need them.
        if (m_Driver.IsCreated)
        {
            m_Driver.Dispose();
            m_Connections.Dispose();
        }
    }


}
