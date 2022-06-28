using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintText : MonoBehaviour
{
    private bool hideHint = false;

    private void Start()
    {
        hideHint = PlayerPrefs.GetInt("highscore", 0) >= 30 ? true : false;
        if (hideHint) Destroy(gameObject);
    }

    private void Update()
    {
        
    }
}
