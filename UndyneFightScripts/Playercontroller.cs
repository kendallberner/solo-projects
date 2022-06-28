using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Playercontroller : MonoBehaviour
{
    private float targetRotation, displayRotation;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            Rotate(90);
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            Rotate(0);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            Rotate(270);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            Rotate(180);

        displayRotation = Mathf.Lerp(displayRotation, targetRotation, Time.deltaTime * 25f);
        transform.eulerAngles = new Vector3(0, 0, displayRotation);
    }

    private void Rotate(int end)
    {
        displayRotation -= Mathf.Floor(displayRotation / 360) * 360;

        if (end - displayRotation > 180)
            targetRotation = end - 360;
        else if (displayRotation - end > 180)
            targetRotation = end + 360;
        else
            targetRotation = end;
    }
}
