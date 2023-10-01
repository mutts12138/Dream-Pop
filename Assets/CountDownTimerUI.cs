using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static GameManager;

public class CountDownTimerUI : MonoBehaviour
{

    [SerializeField] private TMP_Text messageText;

    private void Awake()
    {
        
    }
    void Start()
    {
        GameManager.Instance.countDownTimer.OnValueChanged += (float previousValue, float newValue) =>
        {
            UpdateCountDownTimerUI(newValue);
        };

        GameManager.Instance.OnCountDownToStart += GameManager_OnCountDownToStart;
        GameManager.Instance.OnGamePlaying += GameManager_OnGamePlaying;


        

    }

    private void OnDisable()
    {
        GameManager.Instance.countDownTimer.OnValueChanged -= (float previousValue, float newValue) =>
        {
            UpdateCountDownTimerUI(newValue);
        };

        GameManager.Instance.OnCountDownToStart -= GameManager_OnCountDownToStart;
        GameManager.Instance.OnGamePlaying -= GameManager_OnGamePlaying;
    }

    private void GameManager_OnCountDownToStart(object sender, System.EventArgs e)
    {
        Show();
    }

    private void GameManager_OnGamePlaying(object sender, System.EventArgs e)
    {
        StartCoroutine(timeBeforeHide(2f));
        
    }

    IEnumerator timeBeforeHide(float delayTimeSeconds)
    {
        WaitForSeconds delayTime = new WaitForSeconds(delayTimeSeconds);
        
        yield return delayTime;

        Hide();
    }

    private void UpdateCountDownTimerUI(float newValue)
    {
        int displayValue = (int)newValue;

        if(newValue > 0)
        {
            messageText.text = (displayValue + 1).ToString();
        }
        else
        {
            messageText.text = "GO!!!!!!!!!";
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
