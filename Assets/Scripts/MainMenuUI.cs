using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        playButton.onClick.AddListener(() => 
        {
            SceneLoader.Load(SceneLoader.Scene.Lobby);
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
