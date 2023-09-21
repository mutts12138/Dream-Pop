using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticationUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Button authenticateBTN;
    [SerializeField] private Button backBTN;
    [SerializeField] private LobbyUI lobbyUI;
    private void Awake()
    {
        

        playerNameInputField.onEndEdit.AddListener((string newPlayerName) =>
        {
            LobbyManager.Instance.playerName = newPlayerName;
        });

        authenticateBTN.onClick.AddListener(() =>
        {
            if (LobbyManager.Instance.playerName != null)
            {
                LobbyManager.Instance.Authenticate(LobbyManager.Instance.playerName);
                lobbyUI.Show();
                Hide();
            }
            else
            {
                Debug.Log("playername is null");
            }
        });

        backBTN.onClick.AddListener(() =>
        {
            SceneLoader.Load(SceneLoader.Scene.MainMenu);
        });

        if (LobbyManager.Instance.playerName != null)
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
