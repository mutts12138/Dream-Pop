using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageUI : MonoBehaviour
{
    public static MessageUI Instance;

    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button closeBTN;

    public delegate void CloseFunction();
    public CloseFunction closeFunction;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        closeBTN.onClick.AddListener(() =>
        {
            if (closeFunction != null) 
            {
                closeFunction();
            } 
            
            closeFunction = null;
            Hide();
        });
        
    }


    // Start is called before the first frame update
    void Start()
    {
        Hide();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMessage(string text, bool enableCloseBTN)
    {
        Show();
        
        messageText.text = text;
        if (enableCloseBTN)
        {
            ShowCloseBTN();
        }
        else
        {
            HideCloseBTN();
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

    public void ShowCloseBTN()
    {
        closeBTN.gameObject.SetActive(true);
    }
    public void HideCloseBTN()
    {
        closeBTN.gameObject.SetActive(false);
    }
}
