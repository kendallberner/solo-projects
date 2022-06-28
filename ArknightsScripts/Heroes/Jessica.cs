using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Jessica : Hero
{
    [Header("Unity Setup Fields (Jessica)")]
    public GameObject muzzleFlareEffect;
    public GameObject skill1MuzzleFlareEffect;
    public GameObject damageImpactEffect;
    public GameObject smokescreenEffect;

    private void Awake()
    {
        if (selectedSkill == 1)
        {
            spRecoveryStyle = SP_RECOVERY_STYLE.OFFENSIVE;
            skillSPCost = 4;
        }
        else
        {
            spRecoveryStyle = SP_RECOVERY_STYLE.AUTOMATIC;
            skillSPCost = 10;
            skillDuration = 5f;
        }
        SkillSetup();

        rangeBuildingBlocks = Range.Get3x4BuildingBlocks();
    }

    private new void Start()
    {
        base.Start();
    }

    private new void Update()
    {
        base.Update();
    }

    protected override void Attack(List<Character> targets, Character target)
    {
        GameObject impactEffectInstance = Instantiate(damageImpactEffect, target.transform.position, target.transform.rotation);
        Destroy(impactEffectInstance, 5f);

        if (selectedSkill == 1 && sp >= skillSPCost)
        {
            sp -= skillSPCost;
            spDraining = true;

            GameObject muzzleFlareEffectInstance = Instantiate(skill1MuzzleFlareEffect, firePoint.position, firePoint.rotation);
            Destroy(muzzleFlareEffectInstance, 5f);

            target.TakeDamage(GetAdjustedAttack() * 2.00f, DAMAGE_TYPE.PHYSICAL, Constants.DISPLAY_DAMAGE_NUMBER);
        }
        else
        {
            GameObject muzzleFlareEffectInstance = Instantiate(muzzleFlareEffect, firePoint.position, firePoint.rotation);
            Destroy(muzzleFlareEffectInstance, 5f);

            target.TakeDamage(GetAdjustedAttack(), DAMAGE_TYPE.PHYSICAL, Constants.DONT_DISPLAY_DAMAGE_NUMBER);
        }
    }

    public void Smokescreen()
    {
        if(sp >= skillSPCost)
        {
            ApplyBuff(new StatBuff("SmokescreenAttack", STAT.ATTACK, 0f, 0.50f, skillDuration));
            ApplyBuff(new StatBuff("SmokescreenDodge", STAT.DODGE, 0.75f, 0.00f, skillDuration));
            ApplyBuff(new StatBuff("SmokescreenASPD", STAT.ATTACK_SPEED, 0.00f, .50f, skillDuration));

            GameObject smokescreenEffectInstance = Instantiate(smokescreenEffect, transform.position, transform.rotation);

            StartCoroutine(EndSmokescreen(smokescreenEffectInstance));
        }
    }

    private IEnumerator EndSmokescreen(GameObject smokescreenEffectInstance)
    {
        yield return new WaitForSeconds(skillDuration);

        Destroy(smokescreenEffectInstance);

        skillActive = false;
    }

    protected override void SetAbilityButton()
    {
        if(selectedSkill == 2)
            HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(Smokescreen);
    }
}
