using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroInformationPanel : MonoBehaviour
{
    public static HeroInformationPanel instance;

    public List<GameObject> rangePreviewGOs;

    public static GameObject stats;
    public static Text atk;
    public static Text def;
    public static Text res;
    public static Text block;
    public static Text hitpoints;

    public static Hero hero;
    public static GameObject useAbilityButton;
    public static Text abilityButtonText;
    public static Image spFillGauge;

    private void Awake()
    {
        instance = this;

        for (int i = 0; i < 5; i++)
        {
            rangePreviewGOs[i].SetActive(false);
        }

        stats = GameObject.Find("Stats");
        atk = GameObject.Find("ATK").GetComponent<Text>();
        def = GameObject.Find("DEF").GetComponent<Text>();
        res = GameObject.Find("RES").GetComponent<Text>();
        block = GameObject.Find("Block").GetComponent<Text>();
        hitpoints = GameObject.Find("Hitpoints").GetComponent<Text>();

        useAbilityButton = GameObject.Find("UseAbilityButton");
        abilityButtonText = GameObject.Find("AbilityButtonText").GetComponent<Text>();
        spFillGauge = GameObject.Find("SPFillGauge").GetComponent<Image>();

        gameObject.SetActive(false);
    }

    public void NewHeroSelectedUpdate()
    {
        Time.timeScale = Constants.SLOW_TIME_SCALE;
        useAbilityButton.GetComponent<Image>().sprite = hero.GetSkillSprite();
        UpdateRangePreview();
    }

    public void UpdateRangePreview()
    {
        Range range = hero.range;
        List<Vector3> centers = range.GetPositionCenters();
        List<Vector3> scales = range.GetScales();
        for (int i = 0; i < centers.Count; i++)
        {
            GameObject rangePreviewGO = rangePreviewGOs[i];
            rangePreviewGO.SetActive(true);
            rangePreviewGO.transform.position = centers[i];
            rangePreviewGO.transform.localScale = scales[i];
        }
        for (int i = centers.Count; i < rangePreviewGOs.Count; i++)
        {
            rangePreviewGOs[i].SetActive(false);
        }
    }

    private void Update()
    {
        if(hero == null || !hero.gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            Time.timeScale = Constants.NORMAL_TIME_SCALE;
        }
            
        SetATK(hero.GetAdjustedAttack());
        SetDEF(hero.GetAdjustedDefense());
        SetRES(hero.GetAdjustedResistance());
        SetBlock(hero.GetAdjustedBlock());
        SetHitpoints(hero.hitpoints);
        SetSP(hero.sp, hero.skillSPCost);
    }

    public void RetreatHero()
    {
        hero.Retreat();
    }

    public void SetHero(Hero _hero)
    {
        hero = _hero;
    }

    public void SetATK(float atkFloat)
    {
        float difference = atkFloat - hero.baseAttack;
        atk.text = "ATK: " + Convert.ToInt32(atkFloat) + GetDifferenceText(difference);
    }

    public void SetDEF(float defFloat)
    {
        float difference = defFloat - hero.defense;
        def.text = "DEF: " + Convert.ToInt32(defFloat) + GetDifferenceText(difference);
    }

    public void SetRES(float resFloat)
    {
        float difference = resFloat - hero.resistance;
        res.text = "RES: " + Convert.ToInt32(resFloat) + GetDifferenceText(difference);
    }

    public void SetBlock(float blockFloat)
    {
        float difference = blockFloat - hero.blockCount;
        block.text = "Block: " + Convert.ToInt32(blockFloat) + GetDifferenceText(difference);
    }

    public void SetHitpoints(float hitpointsFloat)
    {
        hitpoints.text = "Hitpoints: " + Convert.ToInt32(hitpointsFloat);
    }

    private string GetDifferenceText(float difference)
    {
        if (difference == 0)
            return "";
        else if (difference > 0)
            return " (+" + Convert.ToInt32(difference) + ")";
        else
            return " (" + Convert.ToInt32(difference) + ")";
    }

    public void SetSP(float sp, float skillSPCost)
    {
        abilityButtonText.text = Math.Floor(sp) + "/" + Math.Floor(skillSPCost);
        spFillGauge.fillAmount = sp / skillSPCost;
    }
}
