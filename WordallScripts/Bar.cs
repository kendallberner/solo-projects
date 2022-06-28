using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private Image barImage;

    private float barFillAmount;

    private void Update()
    {
        barImage.fillAmount = Mathf.Lerp(barImage.fillAmount, barFillAmount, Time.deltaTime * 4f);
    }

    public void SetBarFillAmount(float fillAmount)
    {
        barFillAmount = fillAmount;
    }

    public void SetBarText(string text)
    {
        textComponent.text = text;
    }
}
