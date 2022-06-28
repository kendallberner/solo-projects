using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Thing : MonoBehaviour
{
    public new string name;

    public bool isRule;

    private float targetX, targetY;

    private void Awake()
    {
        targetX = transform.position.x;
        targetY = transform.position.y;
    }

    private void Update()
    {
        float newX = Mathf.Abs(Mathf.Abs(transform.position.x) - Mathf.Abs(targetX)) < .01 ? targetX : Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * 25f);
        float newY = Mathf.Abs(Mathf.Abs(transform.position.y) - Mathf.Abs(targetY)) < .01 ? targetY : Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * 25f);
        transform.position = new Vector3(newX, newY);
    }

    public int X()
    {
        return Mathf.RoundToInt(targetX);
    }

    public int Y()
    {
        return Mathf.RoundToInt(targetY);
    }

    public void SetTargetCoords(int x, int y)
    {
        targetX = x;
        targetY = y;
        //transform.position = new Vector3(x, y);
    }

    public abstract bool IsPush();
    public abstract bool IsStop();
}
