using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuardianObelisk : Hero
{
    [Header("Unity Setup Fields (Guardian Obelisk)")]
    public GameObject damageImpactEffect;
    public GameObject growingSandstormEffect;

    public Beeswax beeswax;

    private void Awake()
    {
        rangeBuildingBlocks = Range.GetAdjacentBuildingBlocks();
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        beeswax.OnGuardianObeliskSpawn();

        UpdateTarget();
        foreach (Character enemy in targetCharacters)
        {
            enemy.TakeDamage(beeswax.GetAdjustedAttack() / (1 + enemy.GetAdjustedResistance() / 100f));
        }
    }

    private new void Update()
    {
        base.Update();
    }

    protected override void UpdateTarget()
    {
        targetCharacters = GetCharactersInRange();
    }

    protected override void Attack(List<Character> targets, Character target)
    {

    }

    protected override void SetAbilityButton()
    {
        
    }
}
