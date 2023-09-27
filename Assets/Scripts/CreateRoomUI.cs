using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class CreateRoomUI : MonoBehaviour
{
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] Toggle privateRoomToggle;
    [SerializeField] Button createRoomButton;
    [SerializeField] Button backButton;

    [SerializeField] LobbyUI lobbyUI;
    

    
    private void Awake()
    {
        roomNameInputField.onEndEdit.AddListener((string newRoomName) =>
        {
            LobbyManager.Instance.lobbyName = newRoomName;
        });

        passwordInputField.onEndEdit.AddListener((string newPassword) =>
        {
            LobbyManager.Instance.password =  newPassword;
        });

        privateRoomToggle.onValueChanged.AddListener((bool newIsPrivate) =>
        {
            LobbyManager.Instance.isPrivate = newIsPrivate;
            if(newIsPrivate == true)
            {
                passwordInputField.interactable = true;
            }
            else
            {
                passwordInputField.interactable= false;
            }
            
        });

        createRoomButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.CreateLobby(LobbyManager.Instance.lobbyName, LobbyManager.Instance.isPrivate, LobbyManager.Instance.password);
            Hide();
        });

        backButton.onClick.AddListener(() =>
        {
            Hide();
        });

        
    }

    private void Start()
    {


        passwordInputField.interactable = false;
        Hide();
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
