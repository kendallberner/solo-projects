using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Caster : Enemy
{
    private void Awake()
    {
        startHitpoints = 1600;
        baseAttack = 200;
        defense = 50;
        resistance = 50;
        attackInterval = 4f;
        range = 1.8f;
        moveSpeed = .8f;
    }

    protected override IEnumerator ActualAttack(List<Character> targets, Character target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (projectilePrefab != null)
        {
            Shoot(playerTag, 0, GetAdjustedAttack(), DAMAGE_TYPE.ARTS, Constants.DONT_DISPLAY_DAMAGE_NUMBER);
        }
        else
        {
            target.TakeDamage(GetAdjustedAttack());
        }
    }
}
