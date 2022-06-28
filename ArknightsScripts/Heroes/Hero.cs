using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Hero : Character
{
    [Header("Stats (Hero)")]
    public CLASS clazz;
    public int selectedSkill;
    public int cost;
    public int blockCount;
    public int redeploymentTime;

    [Header("Unity Setup Fields (Hero)")]
    public Sprite skillSprite1;
    public Sprite skillSprite2;
    public Sprite skillSprite3;

    public Dictionary<int, Enemy> enemiesBlocked = new Dictionary<int, Enemy>();
    [HideInInspector] public int placementIndex;
    [HideInInspector] public Node node;
    [HideInInspector] public GameObject shopGO;
    [HideInInspector] public List<Range.RangeBuildingBlock> rangeBuildingBlocks;
    [HideInInspector] public DIRECTION direction;
    [HideInInspector] public Range range;

    protected HeroInformationPanel heroPanel;
    protected int eliteLevel;
    protected int level;
    protected int trust;
    protected int potential;
    protected int skillLevel;
    protected int skillMastery1;
    protected int skillMastery2;
    protected int skillMastery3;
    
    

    protected new void Start()
    {
        base.Start();
        heroPanel = HeroInformationPanel.instance;
    }

    protected new void Update()
    {
        base.Update();
    }

    public virtual void OnSpawn()
    {
        hitpoints = startHitpoints;
        enemiesBlocked.Clear();
    }

    protected override void UpdateTarget()
    {
        //If you're fighting a target that you're blocking, keep fighting them
        if (target != null && enemiesBlocked.ContainsKey(target.gameObject.GetComponent<Enemy>().GetInstanceID()))
            return;

        List<Character> possibleTargets = GetCharactersInRange();

        Character nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;
        foreach (Character possibleTarget in possibleTargets)
        {
            float dist = possibleTarget.gameObject.GetComponent<EnemyMovement>().GetDistanceFromEndOfPath();
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                nearestEnemy = possibleTarget;
            }
        }

        if (nearestEnemy != null)
        {
            target = nearestEnemy.transform;
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

    public Sprite GetSkillSprite()
    {
        if (selectedSkill == 1)
            return skillSprite1;
        else if (selectedSkill == 2)
            return skillSprite2;
        else 
            return skillSprite3;
    }

    protected override List<Character> GetCharactersInRange()
    {
        List<Character> viableTargets = new List<Character>();

        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
        foreach(Enemy enemy in enemies)
        {
            Vector3 enemyPosition = enemy.transform.position;
            if (enemy.IsTargetable && range.AreCoordsWithinRange(enemyPosition.x, enemyPosition.z))
                viableTargets.Add(enemy);
        }

        viableTargets.AddRange(enemiesBlocked.Values);

        return viableTargets;
    }

    private void OnMouseUpAsButton()
    {
        heroPanel.SetHero(this);
        heroPanel.NewHeroSelectedUpdate();
        HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.RemoveAllListeners();
        SetAbilityButton();
        heroPanel.gameObject.SetActive(true);
        HeroInformationPanel.useAbilityButton.GetComponent<Button>().onClick.AddListener(ActivateSkill);
    }

    protected abstract void SetAbilityButton();

    protected void ActivateSkill()
    {
        if (sp >= skillSPCost)
        {
            heroPanel.gameObject.SetActive(false);

            sp -= skillSPCost;
            skillActive = true;
            spDraining = true;

            Time.timeScale = Constants.NORMAL_TIME_SCALE;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (!enemy.isDead && enemiesBlocked.Count < GetAdjustedBlock() && !enemiesBlocked.ContainsKey(enemy.GetInstanceID()))
        {
            enemiesBlocked.Add(enemy.GetInstanceID(), enemy);
            enemy.isBlocked = true;
            enemy.heroBlockingMe = this;
        }
            
        if (enemiesBlocked.Count >= GetAdjustedBlock())
            gameObject.layer = Constants.NO_COLLISION_LAYER;
    }

    public override void Die()
    {
        DieRetreatSharedLogic();
    }

    public virtual void Retreat()
    {
        PlayerStats.GainDP(cost / 2);
        DieRetreatSharedLogic();
    }

    private void DieRetreatSharedLogic()
    {
        foreach (Enemy enemy in enemiesBlocked.Values)
        {
            enemy.isBlocked = false;
        }
        enemiesBlocked.Clear();
        gameObject.layer = Constants.COLLISION_LAYER;

        node.hero = null;
        node = null;
        if(shopGO != null)
        {
            shopGO.SetActive(true);
            shopGO.GetComponentInParent<Shop>().BeginRedeployment(GetType().Name);
        }
        gameObject.SetActive(false);
    }

    public virtual float GetAdjustedBlock()
    {
        return GetAdjustedStat(blockCount, STAT.BLOCK);
    }

    public virtual bool CanBeDeployedOn(Node node)
    {
        if (node.canPlaceMeleeHeroes && (clazz & (CLASS.DEFENDER | CLASS.GUARD | CLASS.SPECIALIST | CLASS.VANGUARD)) > 0)
            return true;
        else if (node.canPlaceRangedHeroes && (clazz & (CLASS.CASTER | CLASS.MEDIC | CLASS.SNIPER | CLASS.SUPPORTER)) > 0)
            return true;
        else
            return false;
    }
}
