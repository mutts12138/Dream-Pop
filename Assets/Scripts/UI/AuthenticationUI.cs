using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticationUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Button authenticateBTN;
    [SerializeField] private Button backBTN;
    
    private void Awake()
    {
        

        playerNameInputField.onEndEdit.AddListener((string newPlayerName) =>
        {
            AuthenticationManager.Instance.playerName = newPlayerName;
        });

        authenticateBTN.onClick.AddListener(() =>
        {
            AuthenticationManager.Instance.Authenticate(AuthenticationManager.Instance.playerName);

        });

        backBTN.onClick.AddListener(() =>
        {
            Hide();
        });

        Hide();
    }
    private void Start()
    {
        AuthenticationManager.Instance.OnAuthenticationSuccess += (object sender, EventArgs e) =>
        {
            SceneLoader.Load(SceneLoader.Scene.Lobby);
        };

        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
