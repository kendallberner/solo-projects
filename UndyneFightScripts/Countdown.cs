using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float startFontSize, endFontSize;
    [SerializeField] private int countdownStart;

    private float timeElapsed = 0f;

    private void Start()
    {
        countdownText.text = countdownStart.ToString();
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime * 2f;

        countdownText.fontSize = Mathf.Lerp(startFontSize, endFontSize, timeElapsed);

        if(timeElapsed >= 1f)
        {
            timeElapsed = 0f;
            countdownStart--;

            if(countdownStart <= 0)
                Destroy(this.gameObject);
            else
                countdownText.text = countdownStart.ToString();
        }
    }
}
