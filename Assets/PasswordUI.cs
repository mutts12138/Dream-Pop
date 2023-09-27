using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordUI : MonoBehaviour
{
    public static PasswordUI Instance;

    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] Button submitBTN;
    [SerializeField] Button cancelBTN;
    [SerializeField] LobbyUI lobbyUI;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        
    }

    private void Start()
    {
        passwordInputField.onEndEdit.AddListener((string newPassword) =>
        {
            LobbyManager.Instance.password = newPassword;
        });


        submitBTN.onClick.AddListener(() =>
        {
            LobbyManager.Instance.JoinLobbyByCode(LobbyManager.Instance.lobbyCode);
            Hide();
        });

        cancelBTN.onClick.AddListener(() =>
        {
            LobbyManager.Instance.password = null;
            lobbyUI.Show();
            Hide();
        });

        Hide();
    }

    public void Show()
    {
        passwordInputField.text = null;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
