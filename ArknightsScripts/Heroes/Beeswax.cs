using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Beeswax : Hero
{
    [Header("Unity Setup Fields (Beeswax)")]
    public GameObject muzzleFlareEffect;
    public GameObject damageImpactEffect;
    public GameObject growingSandstormEffect;

    public GameObject guardianObeliskPrefab;
    private GuardianObelisk guardianObelisk;

    private List<Range.RangeBuildingBlock> growingSandstormBuildingBlocks;

    private void Awake()
    {
        spRecoveryStyle = SP_RECOVERY_STYLE.AUTOMATIC;
        if (selectedSkill == 1)
        {
            skillSPCost = 5;
            skillDuration = 10f;
        }
        else
        {
            skillSPCost = 10;
            skillDuration = 10f;

            GameObject go = Instantiate(guardianObeliskPrefab, Vector3.zero, Quaternion.identity);
            guardianObelisk = go.GetComponent<GuardianObelisk>();
            guardianObelisk.beeswax = this;
            go.SetActive(false);
        }
        SkillSetup();

        rangeBuildingBlocks = Range.GetAuraBuildingBlocks();
        growingSandstormBuildingBlocks = Range.GetWideAuraBuildingBlocks();
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        //ApplyBuff(new StatBuff("Channeler Caster DEF", STAT.DEFENSE, 0f, 2.00f, 9999f));
        //ApplyBuff(new StatBuff("Channeler Caster RES", STAT.RESISTANCE, 20f, 0f, 9999f));

        ApplyBuff(Buffs.ChannelerCasterDEFBuff);
        ApplyBuff(Buffs.ChannelerCasterRESBuff);
    }

    private new void Update()
    {
        base.Update();
        if (skillActive)
        {
            
        }
        else
        {
            GainHealth(.025f * GetAdjustedMaxHitpoints() * Time.deltaTime);
        }
    }

    protected override void UpdateTarget()
    {
        targetCharacters = GetCharactersInRange();
    }

    protected override void Attack(List<Character> targets, Character target)
    {
        if (skillActive)
        {
            GameObject muzzleFlareEffectInstance = Instantiate(muzzleFlareEffect, transform.position, Quaternion.identity);
            Destroy(muzzleFlareEffectInstance, 5f);

            foreach (Character enemy in targets)
            {
                GameObject damageImpactEffectInstance = Instantiate(damageImpactEffect, enemy.transform.position, Quaternion.identity);
                Destroy(damageImpactEffectInstance, 3f);
                enemy.TakeDamage(GetAdjustedAttack(), DAMAGE_TYPE.ARTS, Constants.DISPLAY_DAMAGE_NUMBER);
            }
        }
    }

    public void GrowingSandstorm()
    {
        if(sp >= skillSPCost)
        {
            EndBuff("Channeler Caster DEF");
            EndBuff("Channeler Caster RES");

            ResetAttackCooldown();
            range = Range.GetRangeFromBuildingBlocks(growingSandstormBuildingBlocks, transform.position.x, transform.position.z, direction);
            heroPanel.UpdateRangePreview();
            ApplyBuff(new StatBuff("GrowingSandstormAttack", STAT.ATTACK, 0f, 0.40f, skillDuration));

            GameObject growingSandstormEffectInstance = Instantiate(growingSandstormEffect, transform.position, transform.rotation);
            StartCoroutine(EndGrowingSandstorm(growingSandstormEffectInstance));
        }
    }

    private IEnumerator EndGrowingSandstorm(GameObject growingSandstormEffectInstance)
    {
        yield return new WaitForSeconds(skillDuration);

        Destroy(growingSandstormEffectInstance);

        ApplyBuff(Buffs.ChannelerCasterDEFBuff);
        ApplyBuff(Buffs.ChannelerCasterRESBuff);

        range = Range.GetRangeFromBuildingBlocks(rangeBuildingBlocks, transform.position.x, transform.position.z, direction);
        heroPanel.UpdateRangePreview();
        skillActive = false;
    }

    public void PrepareToBuildObelisk()
    {
        if (sp >= skillSPCost)
        {
            BuildManager buildManager = BuildManager.instance;
            buildManager.SelectHeroToPlace(guardianObelisk.gameObject);
            heroPanel.gameObject.SetActive(false);
        }
    }

    public void OnGuardianObeliskSpawn()
    {
        EndBuff("Channeler Caster DEF");
        EndBuff("Channeler Caster RES");

        ResetAttackCooldown();

        StartCoroutine(EndGuardianObelisk());
    }

    private IEnumerator EndGuardianObelisk()
    {
        yield return new WaitForSeconds(skillDuration);

        ApplyBuff(Buffs.ChannelerCasterDEFBuff);
        ApplyBuff(Buffs.ChannelerCasterRESBuff);
        skillActive = false;

        if(guardianObelisk.gameObject.activeSelf)
            guardianObelisk.Die();
    }

    protected override void SetAbilityButton()
    {
        if (selectedSkill == 1)
            HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(GrowingSandstorm);
        else if(selectedSkill == 2)
            HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(PrepareToBuildObelisk);
    }
}
