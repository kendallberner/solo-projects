using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    private float yPos;

    private void Start()
    {
        yPos = transform.localPosition.y;
    }

    private void Update()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, yPos, Time.deltaTime * 10f));
    }

    public void SetYPos(float newYpos)
    {
        yPos = newYpos;
    }
}
