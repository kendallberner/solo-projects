using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger : Enemy
{
    private bool hasBuffTriggered;

    private void Awake()
    {
        startHitpoints = 6000;
        baseAttack = 600;
        defense = 800;
        resistance = 0;
        attackInterval = 2.6f;
        range = 0f;
        moveSpeed = .75f;
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if (!hasBuffTriggered && hitpoints <= startHitpoints * .50)
        {
            ApplyBuff(new StatBuff("HeavyDefenderSpeedBuff1", STAT.MOVE_SPEED, .75f, 0f, 2f));
            hasBuffTriggered = true;
        }
    }
}
