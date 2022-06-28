using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Letter : MonoBehaviour
{
    [SerializeField] private Image image;

    private Color color = Color.white;
    private Vector3 targetScale = new Vector3(1f,1f);
    private Vector3 backgroundTargetScale = new Vector3(1f, 1f);

    private void Update()
    {
        image.color = Color.Lerp(image.color, color, Time.deltaTime * 4f);

        Vector3 newScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 36f);
        transform.localScale = new Vector3(newScale.x, newScale.y, newScale.z);
        if (transform.localScale.x > 1.09f) SetScale(new Vector3(1f, 1f, 0f));

        //Vector3 newBackgroundScale = Vector3.Lerp(image.transform.localScale, backgroundTargetScale, Time.deltaTime * 72f);
        //image.transform.localScale = newBackgroundScale;
    }

    public void SetColor(Color _color)
    {
        color = _color;
    }

    public void SetScale(Vector3 scale)
    {
        targetScale = scale;
    }

    public void SetBackgroundScale(Vector3 scale)
    {
        backgroundTargetScale = scale;
    }
}
