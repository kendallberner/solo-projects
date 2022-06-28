using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : Enemy
{
    private void Awake()
    {
        startHitpoints = 1650;
        baseAttack = 200;
        defense = 100;
        resistance = 0;
        attackInterval = 2f;
        range = 0f;
        moveSpeed = 1.1f;
    }
}
