using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class Gavial : Hero
{
    [Header("Unity Setup Fields (Gavial)")]
    public GameObject skill1SuperHealEffect;
    public GameObject healImpactEffect;

    private float skill1BuffDuration;
    private float skill2BuffDuration;

    private void Awake()
    {
        spRecoveryStyle = SP_RECOVERY_STYLE.AUTOMATIC;
        if (selectedSkill == 1)
        {
            skillSPCost = 5;
            skillMaxCharges = 2;
            skill1BuffDuration = 4f;
        }
        else
        {
            skillSPCost = 10;
            skill2BuffDuration = 7f;
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

    public override void OnSpawn()
    {
        base.OnSpawn();

        GameObject[] allies = GameObject.FindGameObjectsWithTag(playerTag);
        foreach (GameObject ally in allies)
        {
            Hero hero = ally.GetComponent<Hero>();
            if (hero.clazz == CLASS.MEDIC)
            {
                hero.ApplyBuff(new StatBuff("Battlefield Medic ATK", STAT.ATTACK, 0f, .05f, 15f));
                hero.ApplyBuff(new StatBuff("Battlefield Medic DEF", STAT.DEFENSE, 50f, 0f, 15f));
            }
        }
    }

    protected override List<Character> GetCharactersInRange()
    {
        List<Character> viableTargets = new List<Character>();

        GameObject[] allies = GameObject.FindGameObjectsWithTag(playerTag);
        foreach (GameObject ally in allies)
        {
            Vector3 allyPosition = ally.transform.position;
            if (range.AreCoordsWithinRange(allyPosition.x, allyPosition.z))
            {
                if(ally.GetComponent<Character>().enabled)
                    viableTargets.Add(ally.GetComponent<Character>());
            }
        }

        return viableTargets;
    }

    protected override void UpdateTarget()
    {
        List<Character> possibleTargets = GetCharactersInRange();

        Character allyWithLowestPercentHealth = null;
        float lowestPercentHealth = 1f;
        foreach (Character possibleTarget in possibleTargets)
        {
            float percentHealth = possibleTarget.hitpoints / possibleTarget.startHitpoints;
            if (percentHealth < lowestPercentHealth)
            {
                lowestPercentHealth = percentHealth;
                allyWithLowestPercentHealth = possibleTarget;
            }
        }

        if (allyWithLowestPercentHealth != null)
        {
            target = allyWithLowestPercentHealth.transform;
            if (targetCharacters.Count == 0)
                targetCharacters.Add(target.GetComponent<Hero>());
            else
                targetCharacters[0] = target.GetComponent<Hero>();
        }
        else
        {
            target = null;
            targetCharacters.Clear();
        }
    }

    protected override IEnumerator ActualAttack(List<Character> targets, Character target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (selectedSkill == 1 && sp >= skillSPCost)
        {
            GameObject impactEffectInstance = Instantiate(healImpactEffect, target.transform.position, target.transform.rotation);
            StartCoroutine(StopVisualEffectAfterDelay(impactEffectInstance.GetComponent<VisualEffect>(), skill1BuffDuration));

            sp -= skillSPCost;
            spDraining = true;
            target.GainHealth(GetAdjustedAttack());
            target.ApplyBuff(new FunctionBuff("Vitality Restoration", VitalityRestoration, skill1BuffDuration));
        }
        else
        {
            GameObject impactEffectInstance = Instantiate(healImpactEffect, target.transform.position, target.transform.rotation);
            StartCoroutine(StopVisualEffectAfterDelay(impactEffectInstance.GetComponent<VisualEffect>(), .5f));

            target.GainHealth(GetAdjustedAttack());
        }
    }

    public void VitalityRestorationWideRange()
    {
        List<Character> possibleTargets = GetCharactersInRange();
        foreach (Character character in possibleTargets)
        {
            character.ApplyBuff(new FunctionBuff("Vitality Restoration - Wide Range", VitalityRestoration, skill2BuffDuration));
        }
    }

    public void VitalityRestoration(Character buffee)
    {
        float healMultiplier = .26f;
        if (buffee.hitpoints / buffee.GetAdjustedMaxHitpoints() < .50f)
            healMultiplier *= 2;
        buffee.GainHealth(GetAdjustedAttack() * healMultiplier * Time.deltaTime);
    }

    protected override void SetAbilityButton()
    {
        HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(VitalityRestorationWideRange);
    }
}
