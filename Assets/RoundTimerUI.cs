using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundTimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;


    void Start()
    {

        GameManager.Instance.roundTimer.OnValueChanged += (float previousValue, float newValue) =>
        {
            UpdateRoundDownTimerUI(newValue);
        };

    }

    
    void Update()
    {
        
    }


    private void UpdateRoundDownTimerUI(float newValue)
    {
        int min = (int)(newValue/60);
        int sec = (int)(newValue%60);

        string minute = min.ToString();
        string second = sec.ToString();

        if (min < 10)
        {
            minute = "0" + min;
        }
        if(sec < 10)
        {
            second = "0" + sec;
        }

        messageText.text = minute + ":" + second;

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
