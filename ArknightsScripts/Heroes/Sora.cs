using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sora : Hero
{
    [Header("Unity Setup Fields (Sora)")]
    public GameObject healingAuraEffect;
    public GameObject damageBoostAuraEffect;

    private List<Range.RangeBuildingBlock> hymnOfRespiteBuildingBlocks;

    private void Awake()
    {
        spRecoveryStyle = SP_RECOVERY_STYLE.AUTOMATIC;
        if (selectedSkill == 1)
        {
            skillSPCost = 5;
            skillDuration = 15f;
        }
        else
        {
            skillSPCost = 10;
            skillDuration = 10f;
        }
        SkillSetup();

        rangeBuildingBlocks = Range.GetAuraBuildingBlocks();
        hymnOfRespiteBuildingBlocks = Range.GetWideAuraBuildingBlocks();
    }

    private new void Start()
    {
        base.Start();
    }

    private new void Update()
    {
        base.Update();
        if(skillActive && selectedSkill == 1)
        {
            foreach (Character character in targetCharacters)
            {
                if (character.CompareTag(playerTag))
                    character.GainHealth(GetAdjustedAttack() * .70f * Time.deltaTime);
                else
                    StartCoroutine(character.SleepForSeconds(.10f));
            }
        }
        else if(skillActive && selectedSkill == 2)
        {
            foreach (Character character in targetCharacters)
            {
                character.GainHealth(GetAdjustedAttack() * .10f * Time.deltaTime);
                if(character != this)
                    character.ApplyBuff(new StatBuff("Hymn of Battle", STAT.ATTACK, GetAdjustedAttack() * .90f, 0.00f, .10f));
            }
        }
        else
        {
            foreach (Character character in targetCharacters)
            {
                character.GainHealth(GetAdjustedAttack() * .10f * Time.deltaTime);
            }
        }
    }

    protected override List<Character> GetCharactersInRange()
    {
        List<Character> viableTargets = new List<Character>();

        if(skillActive && selectedSkill == 1)
        {
            Character[] characters = GameObject.FindObjectsOfType<Character>();
            foreach (Character character in characters)
            {
                Vector3 characterPosition = character.transform.position;
                if (!character.isDead && range.AreCoordsWithinRange(characterPosition.x, characterPosition.z))
                    viableTargets.Add(character);
            }
        }
        else
        {
            Hero[] heroes = GameObject.FindObjectsOfType<Hero>();
            foreach (Hero hero in heroes)
            {
                Vector3 heroPosition = hero.transform.position;
                if (range.AreCoordsWithinRange(heroPosition.x, heroPosition.z))
                    viableTargets.Add(hero);
            }
        }

        return viableTargets;
    }

    protected override void UpdateTarget()
    {
        targetCharacters = GetCharactersInRange();
    }

    protected override void Attack(List<Character> targets, Character target)
    {

    }

    private void Encore()
    {
        if (rand.NextDouble() < .5)
            GainSP(maxSp * .25f);
    }

    public void HymnOfRespite()
    {
        if(sp >= skillSPCost)
        {
            range = Range.GetRangeFromBuildingBlocks(hymnOfRespiteBuildingBlocks, transform.position.x, transform.position.z, direction);
            heroPanel.UpdateRangePreview();
            UpdateTarget();

            StartCoroutine(EndHymnOfRespite());
        }
    }

    private IEnumerator EndHymnOfRespite()
    {
        yield return new WaitForSeconds(skillDuration);

        skillActive = false;
        range = Range.GetRangeFromBuildingBlocks(rangeBuildingBlocks, transform.position.x, transform.position.z, direction);
        heroPanel.UpdateRangePreview();
        UpdateTarget();
        Encore();
    }

    public void HymnOfBattle()
    {
        if (sp >= skillSPCost)
        {
            StartCoroutine(EndHymnOfBattle());
        }
    }

    private IEnumerator EndHymnOfBattle()
    {
        yield return new WaitForSeconds(skillDuration);

        skillActive = false;
        Encore();
    }

    protected override void SetAbilityButton()
    {
        if(selectedSkill == 1)
            HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(HymnOfRespite);
        else
            HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(HymnOfBattle);
    }
}
