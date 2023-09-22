using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private AuthenticationUI authenticationUI;
    private void Awake()
    {
        

        playButton.onClick.AddListener(() => 
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                authenticationUI.Show();
                //if not authenticated yet, open authentication window

            }
            else
            {
                //if already authenticated then just load scene
                SceneLoader.Load(SceneLoader.Scene.Lobby);
            }
            
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    
}
