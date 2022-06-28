using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Durnar : Hero
{
    [Header("Unity Setup Fields (Durnar)")]
    public GameObject muzzleFlareEffect;

    private void Awake()
    {
        if (selectedSkill == 1)
        {
            spRecoveryStyle = SP_RECOVERY_STYLE.AUTOMATIC;
            skillSPCost = 5;
        }
        else
        {
            spRecoveryStyle = SP_RECOVERY_STYLE.DEFENSIVE;
            skillSPCost = 10;
            skillDuration = 5f;
        }
        SkillSetup();

        rangeBuildingBlocks = Range.Get1x1BuildingBlocks();
    }

    private new void Start()
    {
        base.Start();
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        ApplyBuff(new StatBuff("Durnar Talent ATK", STAT.ATTACK, 0f, 0.04f, 9999f));
        ApplyBuff(new StatBuff("Durnar Talent DEF", STAT.DEFENSE, 0f, 0.04f, 9999f));
    }

    private new void Update()
    {
        base.Update();
    }

    protected override void UpdateTarget()
    {
        targetCharacters.Clear();

        foreach (Enemy enemy in enemiesBlocked.Values)
        {
            targetCharacters.Add(enemy);
        }
    }

    protected override void Attack(List<Character> targets, Character target)
    {
        if(skillActive)
        {
            foreach (Enemy enemy in targets)
            {
                enemy.TakeDamage(GetAdjustedAttack(), DAMAGE_TYPE.ARTS, Constants.DISPLAY_DAMAGE_NUMBER);
            }
        }
        else
        {
            target.TakeDamage(GetAdjustedAttack());
        }
    }

    public void ATKUp()
    {
        if(sp >= skillSPCost)
        {
            ApplyBuff(new StatBuff("Durnar ATKUp Attack", STAT.ATTACK, 0f, 0.50f, skillDuration));

            GameObject effectInstance = Instantiate(muzzleFlareEffect, transform.position, transform.rotation);
            StartCoroutine(EndATKUp(effectInstance));
        }
    }

    private IEnumerator EndATKUp(GameObject effectInstance)
    {
        yield return new WaitForSeconds(skillDuration);

        Destroy(effectInstance);
        skillActive = false;
    }

    public void ShieldedCounterattack()
    {
        if (sp >= skillSPCost)
        {
            ApplyBuff(new StatBuff("Shielded Counterattack ATK", STAT.ATTACK, 0f, 0.50f, skillDuration));

            GameObject effectInstance = Instantiate(muzzleFlareEffect, transform.position, transform.rotation);
            StartCoroutine(EndShieldedCounterattack(effectInstance));
        }
    }

    private IEnumerator EndShieldedCounterattack(GameObject effectInstance)
    {
        yield return new WaitForSeconds(skillDuration);

        Destroy(effectInstance);
        skillActive = false;
    }

    protected override void SetAbilityButton()
    {
        if(selectedSkill == 1)
            HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(ATKUp);
        else
            HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(ShieldedCounterattack);
    }
}
