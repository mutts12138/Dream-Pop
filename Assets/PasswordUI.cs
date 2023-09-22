using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordUI : MonoBehaviour
{
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] Button submitBTN;
    [SerializeField] Button cancelBTN;
    [SerializeField] LobbyUI lobbyUI;
    private void Awake()
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

        
    }

    private void Start()
    {
        LobbyManager.Instance.OnJoinFailedPassword += (object sender, EventArgs e) => { Show(); };
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
