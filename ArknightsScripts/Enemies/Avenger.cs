using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avenger : Enemy
{
    [SerializeField] ParticleSystem flamingSwordParticles;

    private AvengerState avengerState = AvengerState.NORMAL;

    private void Awake()
    {
        startHitpoints = 5000;
        baseAttack = 300;
        defense = 200;
        resistance = 50;
        attackInterval = 2.3f;
        range = 0f;
        moveSpeed = .65f;
    }

    private enum AvengerState
    {
        NORMAL,
        VENGEFUL
    }

    new void Update()
    {
        base.Update();
        if (avengerState == AvengerState.NORMAL && hitpoints <= startHitpoints / 2)
        {
            avengerState = AvengerState.VENGEFUL;
            GoBigMode();
        }
    }

    private void GoBigMode()
    {
        //flamingSwordParticles.Play();

        baseAttack *= 2f;
        attackInterval /= 2f;
    }
}
