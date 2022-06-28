using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptedKnight : Enemy
{
    private void Awake()
    {
        startHitpoints = 40000;
        baseAttack = 1200;
        defense = 1200;
        resistance = 30;
        attackInterval = 4f;
        range = 0f;
        moveSpeed = .65f;

        sp = 5;
        maxSp = 5;
    }

    new void Start()
    {
        base.Start();
    }

    new void Update()
    {
        base.Update();
    }

    protected override void Attack(List<Character> targets, Character target)
    {
        if (sp >= 5)
        {
            sp -= 5;
            target.TakeDamage(GetAdjustedAttack() * 3 / (1 + target.GetAdjustedDefense() / 100));
            return;
        }

        sp++;
        target.TakeDamage(GetAdjustedAttack() / (1 + target.GetAdjustedDefense() / 100));
    }
}
