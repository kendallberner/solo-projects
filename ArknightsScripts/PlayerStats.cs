using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public static int DP;
    public int startDP = 60;
    public static Text DPText;
    public Text startDPText;

    public static int Lives;
    public int startLives = 20;
    public static Text LivesText;
    public Text startLivesText;

    private float dpGenerationRate = 1f;
    private float dpGenerationCountdown = 1f;

    private void Start()
    {
        DP = startDP;
        DPText = startDPText;
        Lives = startLives;
        LivesText = startLivesText;
        LivesText.text = Lives.ToString();

        dpGenerationCountdown = 1f / dpGenerationRate;
    }

    private void Update()
    {
        dpGenerationCountdown -= Time.deltaTime;
        if(dpGenerationCountdown <= 0f)
        {
            DP++;
            DPText.text = DP.ToString();
            dpGenerationCountdown = 1f / dpGenerationRate;
        }
    }

    public static void LoseLife(int livesLost)
    {
        Lives -= livesLost;
        LivesText.text = Lives.ToString();
    }

    public static void GainDP(int dpGained)
    {
        DP += dpGained;
        DPText.text = DP.ToString();
    }

    public static void LoseDP(int dpLost)
    {
        DP -= dpLost;
        DPText.text = DP.ToString();
    }
}
