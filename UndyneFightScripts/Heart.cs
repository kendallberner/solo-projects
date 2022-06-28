using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Heart : MonoBehaviour
{
    public Transform healthbar;
    public Image healthbarGreen, healthbarWhite, healthbarRed;
    public float healthSpeed = 5f;
    public float shakeSpeed = 250f;
    public int shakeCount = 5;

    private float maxHealth = 100;
    private float health;

    private bool shakeLock;

    private float healthbarOriginalPosX, healthbarOriginalPosY;

    private void Start()
    {
        health = maxHealth;

        healthbarOriginalPosX = healthbar.position.x;
        healthbarOriginalPosY = healthbar.position.y;

        Gamemanager.OnGameStateChanged += OnGameStateChanged;
    }

    private void Update()
    {
        healthbarWhite.fillAmount = Mathf.Lerp(healthbarWhite.fillAmount, health / maxHealth, Time.deltaTime * healthSpeed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        health -= 20;
        healthbarGreen.fillAmount = health / maxHealth;

        ShakeHealthbar();

        if(health <= 0)
            Gamemanager.instance.UpdateGameState(GAME_STATE.GAME_END);
    }

    private async void ShakeHealthbar()
    {
        if (shakeLock)
            return;
        shakeLock = true;

        for(int i = 0; i < shakeCount; i++)
        {
            await Shake(healthbarOriginalPosX + Random.Range(-50,50), healthbarOriginalPosY + Random.Range(-50, 50));
        }
        await Shake(healthbarOriginalPosX, healthbarOriginalPosY);

        shakeLock = false;
    }

    private async Task Shake(float targetX, float targetY)
    {
        float endTime = Time.time + 1/(5f * shakeSpeed);
        healthbar.position = new Vector3(targetX, targetY);
        while(Time.time < endTime)
        {
            await Task.Yield();
        }
    }
    private void OnGameStateChanged(GAME_STATE newState)
    {
        switch (newState)
        {
            case GAME_STATE.PLAYING:
                health = maxHealth;
                healthbarGreen.fillAmount = health / maxHealth;
                break;
            case GAME_STATE.GAME_END:
                break;
        }
    }
}
