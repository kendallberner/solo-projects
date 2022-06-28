using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Character
{
    [Header("Stats (Enemy)")]
    public float range;
    public float moveSpeed;
    public int weight;
    public bool isBlocked;
    public bool isWaiting;
    public float slow;

    [HideInInspector] public Text waveProgressText;

    public Hero heroBlockingMe;

    private float amountDissolved = 0f;

    public bool CanMove { get { return stunned <= 0 && asleep <= 0 && cold <= 1 && !isBlocked && !isDead && !isWaiting; } }

    protected new void Start()
    {
        base.Start();
    }

    public void OnSpawn()
    {
        if (model != null)
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }

    protected new void Update()
    {
        if (isDead)
        {
            amountDissolved += Time.deltaTime;
            SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.material.SetFloat("Amount_Dissolved", amountDissolved);
                spriteRenderer.material.SetFloat("Blackness", amountDissolved * 2);
            }
            if (amountDissolved >= 1)
                Destroy(gameObject);
        }

        base.Update();
    }

    protected override void UpdateTarget()
    {
        List<Character> possibleTargets = GetCharactersInRange();
        

        Character mostRecentlyDeployedHero = null;
        int earliestDeploymentIndex = 0;
        foreach (Character possibleTarget in possibleTargets)
        {
            int index = possibleTarget.GetComponent<Hero>().placementIndex;
            if (index > earliestDeploymentIndex)
            {
                mostRecentlyDeployedHero = possibleTarget;
                earliestDeploymentIndex = index;
            }
        }

        if (mostRecentlyDeployedHero != null)
        {
            target = mostRecentlyDeployedHero.transform;
            if (targetCharacters.Count == 0)
                targetCharacters.Add(target.GetComponent<Hero>());
            else
                targetCharacters[0] = target.GetComponent<Hero>();
        }
        else
        {
            target = null;
            targetCharacters.Clear();
        }
    }

    public override void Die()
    {
        isDead = true;

        UpdateWaveProgressText();

        if(heroBlockingMe != null)
        {
            heroBlockingMe.enemiesBlocked.Remove(GetInstanceID());
            heroBlockingMe.gameObject.layer = Constants.COLLISION_LAYER;
            heroBlockingMe = null;
        }

        //healthbarBackground.color = new Color(healthbarBackground.color.r, healthbarBackground.color.g, healthbarBackground.color.b, 0f);
        canvas.SetActive(false);

        if(animator != null)
            animator.Play("Die");

        //GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        //Destroy(effect, 3f);
    }

    public void Escape()
    {
        UpdateWaveProgressText();
        PlayerStats.LoseLife(1);
        Destroy(gameObject);
    }

    private void UpdateWaveProgressText()
    {
        if (waveProgressText != null)
        {
            String[] oldText = Regex.Split(waveProgressText.text, "/");
            waveProgressText.text = (int.Parse(oldText[0]) + 1) + "/" + oldText[1];
        }
    }

    protected override List<Character> GetCharactersInRange()
    {
        List<Character> targets = new List<Character>();

        if(range == 0f && heroBlockingMe != null)
            targets.Add(heroBlockingMe);
        else
        {
            GameObject[] charactersOnField = GameObject.FindGameObjectsWithTag(playerTag);
            foreach (GameObject playerCharacter in charactersOnField)
            {
                if (Vector3.Distance(base.gameObject.transform.position, playerCharacter.transform.position) <= range)
                {
                    if (playerCharacter.GetComponent<Character>().enabled)
                        targets.Add(playerCharacter.GetComponent<Character>());
                }
            }
        }

        return targets;
    }

    protected override void Attack(List<Character> targets, Character target)
    {
        if(animator != null)
            animator.Play("Attack");
        StartCoroutine(ActualAttack(targets, target, delayForAttackAnimation));
    }

    protected override IEnumerator ActualAttack(List<Character> targets, Character target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (projectilePrefab != null)
        {
            Shoot(playerTag, 0, GetAdjustedAttack(), DAMAGE_TYPE.PHYSICAL, Constants.DONT_DISPLAY_DAMAGE_NUMBER);
        }
        else
        {
            target.TakeDamage(GetAdjustedAttack());
        }
    }

    public virtual float GetAdjustedMoveSpeed()
    {
        return GetAdjustedStat(moveSpeed, STAT.MOVE_SPEED);
    }
}
