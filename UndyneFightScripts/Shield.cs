using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public TextMeshProUGUI scoreboard;
    public GameObject blockEffect;
    public Transform emissionPoint;

    private int score = 0;

    private void Start()
    {
        Gamemanager.OnGameStateChanged += OnGameStateChanged;
    }

    private void Update()
    {
        float x = Mathf.Lerp(transform.localScale.x, 3, Time.deltaTime * 10);
        float y = Mathf.Lerp(transform.localScale.y, 3, Time.deltaTime * 10);
        transform.localScale = new Vector2(x,y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        score++;
        scoreboard.text = score.ToString();

        GameObject blockEffectInstance = Instantiate(blockEffect, emissionPoint.position, transform.rotation);
        Destroy(blockEffectInstance, 5f);

        transform.localScale = new Vector2(4, 4);
    }

    private void OnGameStateChanged(GAME_STATE newState)
    {
        switch (newState)
        {
            case GAME_STATE.PLAYING:
                score = 0;
                scoreboard.text = score.ToString();
                break;
            case GAME_STATE.GAME_END:
                break;
        }
    }
}
