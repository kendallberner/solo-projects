using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Melantha : Hero
{
    private void Awake()
    {
        spRecoveryStyle = SP_RECOVERY_STYLE.AUTOMATIC;
        skillSPCost = 5;
        skillDuration = 15f;
        SkillSetup();

        rangeBuildingBlocks = Range.Get1x2BuildingBlocks();
    }

    private new void Start()
    {
        base.Start();
    }

    private new void Update()
    {
        base.Update();
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        ApplyBuff(new StatBuff("ATK Up Talent", STAT.ATTACK, 0f, 0.08f, 9999f));
    }

    public void ATKUp()
    {
        if (sp >= maxSp)
        {
            ApplyBuff(new StatBuff("MelanthaATKUp", STAT.ATTACK, 0f, 0.50f, skillDuration));
            StartCoroutine(EndATKUp());
        }
    }

    public IEnumerator EndATKUp()
    {
        yield return new WaitForSeconds(skillDuration);

        skillActive = false;
    }

    protected override void SetAbilityButton()
    {
        HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(ATKUp);
    }
}
