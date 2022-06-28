using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class Shirayuki : Hero
{
    [Header("Unity Setup Fields (Shirayuki)")]
    public GameObject damageImpactEffect;
    public GameObject skillActiveSelfBuffEffect;

    private float aoeRadius = 5f;

    private List<Range.RangeBuildingBlock> longRangeBuildingBlocks;

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
            skillDuration = 5f;
        }
        SkillSetup();

        rangeBuildingBlocks = Range.Get3x5BuildingBlocks();
        longRangeBuildingBlocks = Range.Get3x7BuildingBlocks();
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

        if (eliteLevel == 2)
        {
            baseAttack *= 1.2f;
            attackInterval += .2f;
        }
    }

    protected override void UpdateTarget()
    {
        List<Character> possibleTargets = GetCharactersInRange();

        Character packLeader = null;
        int mostTargetsHit = 0;
        foreach (Character possibleTarget in possibleTargets)
        {
            int targetsHit = 1;
            Collider[] colliders = Physics.OverlapSphere(possibleTarget.transform.position, aoeRadius);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    targetsHit++;
                }
            }
            
            if (targetsHit > mostTargetsHit)
            {
                mostTargetsHit = targetsHit;
                packLeader = possibleTarget;
            }
        }

        if (packLeader != null)
        {
            target = packLeader.transform;
            if (targetCharacters.Count == 0)
                targetCharacters.Add(target.GetComponent<Enemy>());
            else
                targetCharacters[0] = target.GetComponent<Enemy>();
        }
        else
        {
            target = null;
            targetCharacters.Clear();
        }
    }

    protected override void Attack(List<Character> targets, Character target)
    {
        if(skillActive && selectedSkill == 2)
            Shoot(enemyTag, aoeRadius, GetAdjustedAttack() * 1.7f, DAMAGE_TYPE.ARTS, Constants.DISPLAY_DAMAGE_NUMBER);
        else
            Shoot(enemyTag, aoeRadius, GetAdjustedAttack(), DAMAGE_TYPE.PHYSICAL, Constants.DONT_DISPLAY_DAMAGE_NUMBER);
    }

    public void Shuriken()
    {
        if (sp >= maxSp)
        {
            range = Range.GetRangeFromBuildingBlocks(longRangeBuildingBlocks, transform.position.x, transform.position.z, direction);
            heroPanel.UpdateRangePreview();

            ResetAttackCooldown();
            GameObject effect = Instantiate(skillActiveSelfBuffEffect, transform.position, Quaternion.identity);
            StartCoroutine(StopVisualEffectAfterDelay(effect.GetComponent<VisualEffect>(), skillDuration));

            StartCoroutine(EndShuriken());
        }
    }

    public IEnumerator EndShuriken()
    {
        yield return new WaitForSeconds(skillDuration);

        range = Range.GetRangeFromBuildingBlocks(rangeBuildingBlocks, transform.position.x, transform.position.z, direction);
        heroPanel.UpdateRangePreview();

        skillActive = false;
    }

    public void FatalShuriken()
    {
        if(sp >= maxSp)
        {
            ResetAttackCooldown();
            GameObject effect = Instantiate(skillActiveSelfBuffEffect, transform.position, Quaternion.identity);
            StartCoroutine(StopVisualEffectAfterDelay(effect.GetComponent<VisualEffect>(), skillDuration));

            StartCoroutine(EndFatalShuriken());
        }
    }

    public IEnumerator EndFatalShuriken()
    {
        yield return new WaitForSeconds(skillDuration);
        skillActive = false;
    }

    protected override void SetAbilityButton()
    {
        if(selectedSkill == 1)
            HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(Shuriken);
        else
            HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(FatalShuriken);
    }
}
