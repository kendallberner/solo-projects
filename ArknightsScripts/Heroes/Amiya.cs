using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class Amiya : Hero
{
    [Header("Unity Setup Fields (Amiya)")]
    public GameObject muzzleFlareEffect;
    public GameObject damageImpactEffect;
    public GameObject tacticalChantEffect;
    public GameObject spiritBurstEffect;
    public GameObject chimeraEffect;

    private List<Range.RangeBuildingBlock> chimeraBuildingBlocks;

    private void Awake()
    {
        spRecoveryStyle = SP_RECOVERY_STYLE.AUTOMATIC;
        if (selectedSkill == 1)
        {
            skillSPCost = 10;
            skillDuration = 10f;
        }
        else if (selectedSkill == 2)
        {
            skillSPCost = 10;
            skillDuration = 10f;
        }
        else
        {
            skillSPCost = 10;
            skillDuration = 10f;
        }
        SkillSetup();

        rangeBuildingBlocks = Range.Get3x3And1BuildingBlocks();
        chimeraBuildingBlocks = Range.GetWideRangeMedicBuildingBlocks();
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
        model.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }

    protected override void UpdateTarget()
    {
        if (skillActive && selectedSkill == 2)
        {
            targetCharacters = GetCharactersInRange();
            return;
        }
        else
        {
            base.UpdateTarget();
        }
    }

    protected override IEnumerator ActualAttack(List<Character> targets, Character target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (target.isDead)
        {
            ResetAttackCooldown();
            animator.CrossFade("Idle", .1f);
            //animator.Play("Mage Girl Idle Animation");
            yield break;
        }

        GainSP(2f);

        if (skillActive && selectedSkill == 2)
        {
            StartCoroutine(SpiritBurstAttack(targets));
        }
        else if (skillActive && selectedSkill == 3)
        {
            target.TakeDamage(GetAdjustedAttack(), DAMAGE_TYPE.PURE, Constants.DISPLAY_DAMAGE_NUMBER);
        }
        else
        {
            Vector3 firepointToEnemy = target.transform.position - firePoint.position;

            GameObject muzzleFlareEffectInstance = Instantiate(muzzleFlareEffect, firePoint.position, Quaternion.Euler(firepointToEnemy.x, firepointToEnemy.y, firepointToEnemy.z));
            Destroy(muzzleFlareEffectInstance, 2f);

            target.TakeDamage(GetAdjustedAttack(), DAMAGE_TYPE.ARTS, Constants.DONT_DISPLAY_DAMAGE_NUMBER);
            if (target.GetComponent<Enemy>().isDead)
                GainSP(8f);
        }
    }

    private IEnumerator SpiritBurstAttack(List<Character> targets)
    {
        for (int i = 0; i < 8; i++)
        {
            yield return new WaitForSeconds(.1f);
            Character target = targets[rand.Next(0, targets.Count)];
            target.TakeDamage(GetAdjustedAttack() * .5f, DAMAGE_TYPE.ARTS, Constants.DISPLAY_DAMAGE_NUMBER);
        }
    }

    public void TacticalChant()
    {
        if(sp >= skillSPCost)
        {
            ApplyBuff(new StatBuff("Amiya Tactical Chant", STAT.ATTACK_SPEED, 0f, 0.70f, skillDuration));

            GameObject tacticalChantEffectInstance = Instantiate(tacticalChantEffect, transform.position, transform.rotation);
            StartCoroutine(EndTacticalChant(tacticalChantEffectInstance));
        }
    }

    private IEnumerator EndTacticalChant(GameObject tacticalChantEffectInstance)
    {
        yield return new WaitForSeconds(skillDuration);

        Destroy(tacticalChantEffectInstance);
        skillActive = false;
    }

    public void SpiritBurst()
    {
        if (sp >= skillSPCost)
        {
            GameObject spiritBurstEffectInstance = Instantiate(spiritBurstEffect, transform.position, transform.rotation);
            StartCoroutine(EndSpiritBurst(spiritBurstEffectInstance));
        }
    }

    private IEnumerator EndSpiritBurst(GameObject spiritBurstEffectInstance)
    {
        yield return new WaitForSeconds(skillDuration);

        Destroy(spiritBurstEffectInstance);
        skillActive = false;
        StartCoroutine(StunForSeconds(10f));
    }

    public void Chimera()
    {
        if (sp >= skillSPCost)
        {
            range = Range.GetRangeFromBuildingBlocks(chimeraBuildingBlocks, transform.position.x, transform.position.z, direction);
            heroPanel.UpdateRangePreview();
            ApplyBuff(new StatBuff("Amiya Chimera ATK", STAT.ATTACK, 0f, 1.60f, skillDuration));
            ApplyBuff(new StatBuff("Amiya Chimera HP", STAT.HITPOINTS, 0f, 0.75f, skillDuration));

            GameObject chimeraEffectInstance = Instantiate(chimeraEffect, transform.position, transform.rotation);
            StartCoroutine(EndChimera(chimeraEffectInstance));
        }
    }

    private IEnumerator EndChimera(GameObject chimeraEffectInstance)
    {
        yield return new WaitForSeconds(skillDuration);

        Destroy(chimeraEffectInstance);
        skillActive = false;
        Retreat();
    }

    protected override void SetAbilityButton()
    {
        if(selectedSkill == 1)
            HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(TacticalChant);
        else if(selectedSkill == 2)
            HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(SpiritBurst);
        else
            HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(Chimera);
    }
}
