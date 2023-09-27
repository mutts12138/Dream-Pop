using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using QFSW.QC;

public class SceneLoader : MonoBehaviour
{

    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance != null)
        {

            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public enum Scene
    {
        MainMenu,
        Lobby,
        WaitingRoom,
        Loading,
        TestMap
    }

    private static Scene targetScene;


    [Command]
    public static void testLoad(int number)
    {
        if (number == 0)
        {
            SceneManager.LoadScene("MainMenu");
        }
        if (number == 1)
        {
            SceneManager.LoadScene("Lobby");
        }
    }
    
    public static void Load(Scene targetScene)
    {
        SceneLoader.targetScene = targetScene;
        SceneManager.LoadScene(targetScene.ToString());
        Debug.Log("load scene " + targetScene.ToString());
    }

    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
        
    }

    public static void SceneLoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
