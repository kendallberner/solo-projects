using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.VFX;

public abstract class Character : MonoBehaviour
{
    [Header("Stats (Character)")]
    public float startHitpoints;
    public float baseAttack;
    public float defense;
    public float resistance;
    public float attackInterval;
    public float dodge;
    public float physicalDamageMultiplier = 1f;
    public float artsDamageMultiplier = 1f;
    public float damageMultiplier = 1f;

    [Header("Unity Setup Fields (Character)")]
    public GameObject model;
    public GameObject canvas;
    public Animator animator;
    public float delayForAttackAnimation;
    public float turnSpeed = 10f;
    public Transform partToRotate;
    public GameObject deathEffect;
    public Image healthbar;
    public Image healthbarWhite;
    public Image healthbarBackground;
    public Image spbar;
    public string enemyTag = "Enemy";
    public string playerTag = "Player";
    public Transform firePoint;
    public GameObject projectilePrefab;
    public AFFILIATION affiliation;


    [HideInInspector] public HEALTH_BAR_STATE healthBarState;
    [HideInInspector] public float hitpoints;
    [HideInInspector] public float maxSp;
    [HideInInspector] public float sp;
    [HideInInspector] public float skillSPCost;
    [HideInInspector] public float skillMaxCharges = 1f;
    [HideInInspector] public bool isDead;
    public DIRECTION spriteDirection = DIRECTION.RIGHT;
    protected float attackCountdown;
    protected List<StatBuff> statBuffs = new List<StatBuff>();
    protected List<FunctionBuff> functionBuffs = new List<FunctionBuff>();
    protected SP_RECOVERY_STYLE spRecoveryStyle;
    protected float spRecoveryCountdown = 1f;
    protected bool skillActive;
    protected float skillDuration;
    protected float spDrainDuration;
    protected float spDrainAmountToGo;
    protected bool spDraining;
    protected Transform target;
    protected List<Character> targetCharacters = new List<Character>();
    public int stunned;
    public int asleep;
    public int cold;
    public int invulnerable;

    private Color spbarColor;
    private Color spbarColorDraining;

    protected readonly System.Random rand = new System.Random();

    public bool CanAttack { get { return stunned <= 0 && asleep <= 0 && cold <= 1 && !isDead; } }

    public bool IsTargetable { get { return asleep <= 0 && invulnerable <= 0 && !isDead; } }

    public enum AFFILIATION
    {
        PLAYER = 1,
        ENEMY = 2,
        PLAYER_ALLY = 4,
        NEUTRAL_ANTAGONIST = 8,
        NEUTRAL_FRIENDLY = 16
    }

    public enum SP_RECOVERY_STYLE
    {
        AUTOMATIC = 1,
        OFFENSIVE = 2,
        DEFENSIVE = 4
    }

    public enum HEALTH_BAR_STATE
    {
        HEALING = 1,
        DYING = 2,
        NEUTRAL = 4
    }

    protected void Start()
    {
        ColorUtility.TryParseHtmlString("#00FF28", out spbarColor);
        ColorUtility.TryParseHtmlString("#FF8800", out spbarColorDraining);
        hitpoints = startHitpoints;
        InvokeRepeating("UpdateTarget", 0f, .2f);
    }

    protected void Update()
    {
        if(CanAttack)
            attackCountdown -= Time.deltaTime / (1 + cold);

        List<StatBuff> buffsToBeRemoved = new List<StatBuff>();
        foreach(StatBuff buff in statBuffs)
        {
            buff.durationRemaining -= Time.deltaTime;
            if (buff.durationRemaining <= 0)
                buffsToBeRemoved.Add(buff);
        }
        foreach(StatBuff buff in buffsToBeRemoved)
        {
            EndBuff(buff.name);
        }

        List<FunctionBuff> functionBuffsToBeRemoved = new List<FunctionBuff>();
        foreach (FunctionBuff buff in functionBuffs)
        {
            buff.Update(this);
            buff.durationRemaining -= Time.deltaTime;
            if (buff.durationRemaining <= 0)
                functionBuffsToBeRemoved.Add(buff);
        }
        foreach (FunctionBuff buff in functionBuffsToBeRemoved)
        {
            EndBuff(buff.name);
        }


        if (healthbar.fillAmount == healthbarWhite.fillAmount)
            healthBarState = HEALTH_BAR_STATE.NEUTRAL;

        if (healthBarState == HEALTH_BAR_STATE.DYING)
        {
            if (healthbar.fillAmount < healthbarWhite.fillAmount)
                healthbarWhite.fillAmount -= Math.Max(.001f, (healthbarWhite.fillAmount - healthbar.fillAmount) * Time.deltaTime * 2f);
            else
                healthbarWhite.fillAmount = healthbar.fillAmount;
        }
        else if (healthBarState == HEALTH_BAR_STATE.HEALING)
        {
            float amountHealedThisFrame = Math.Max(.001f, (healthbarWhite.fillAmount - healthbar.fillAmount) * Time.deltaTime * 2f);
            healthbar.fillAmount = Math.Min(healthbarWhite.fillAmount, healthbar.fillAmount + amountHealedThisFrame);
        }


        if (spDraining)
        {
            spbar.color = spbarColorDraining;
            spDrainAmountToGo = Math.Max(0, spDrainAmountToGo - skillSPCost / spDrainDuration * Time.deltaTime);
            spbar.fillAmount = spDrainAmountToGo / skillSPCost;
            if (spDrainAmountToGo == 0)
            {
                spDraining = false;
                spDrainAmountToGo = skillSPCost;
                spbar.fillAmount = sp / skillSPCost;
                spbar.color = spbarColor;
            }
        }
        if (spRecoveryStyle.HasFlag(SP_RECOVERY_STYLE.AUTOMATIC) && sp < maxSp)
        {
            GainSP(Time.deltaTime);
        }


        if (targetCharacters.Count > 0 && CanAttack)
        {
            LockOnTarget();
            if(attackCountdown <= 0f && IsFacingTarget())
            {
                attackCountdown = GetAdjustedAttackInterval();
                bool gainSp = spRecoveryStyle.HasFlag(SP_RECOVERY_STYLE.OFFENSIVE) && sp < maxSp;
                Attack(targetCharacters, targetCharacters[0]);
                if (gainSp)
                    GainSP(1f);
            }
        }
    }

    protected void LockOnTarget()
    {
        Vector3 dir = targetCharacters[0].transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        if (partToRotate != null)
        {
            Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
            partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        }
        else
        {
            if(spriteDirection == DIRECTION.LEFT && dir.x > 0f)
            {
                spriteDirection = DIRECTION.RIGHT;
                //transform.Rotate(new Vector3(0, 180, 0), Space.World);
                Vector3 oldRotation = model.transform.rotation.eulerAngles;
                model.transform.rotation = Quaternion.Euler(-oldRotation.x, oldRotation.y + 180f, oldRotation.z);
            }
            else if (spriteDirection == DIRECTION.RIGHT && dir.x < 0f)
            {
                spriteDirection = DIRECTION.LEFT;
                //transform.Rotate(new Vector3(0, 180, 0), Space.World);
                Vector3 oldRotation = model.transform.rotation.eulerAngles;
                model.transform.rotation = Quaternion.Euler(-oldRotation.x, oldRotation.y - 180f, oldRotation.z);
            }
        }
    }

    protected bool IsFacingTarget()
    {
        if(partToRotate != null)
        {
            Vector3 dir = targetCharacters[0].transform.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            return Math.Abs(partToRotate.rotation.eulerAngles.y - lookRotation.eulerAngles.y) < 35f;
        }
        else
        {
            return true;
        }
    }

    protected virtual void Shoot(string targetTag, float aoe, float damage, DAMAGE_TYPE damageType, bool displayDamagePopup)
    {
        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projectileGO.GetComponent<Projectile>();

        projectile.SetTarget(targetCharacters[0].transform);
        projectile.SetParent(this);
        projectile.SetExplosionRadius(aoe);
        projectile.SetTargetTag(targetTag);
        projectile.SetDamage(damage);
        projectile.SetDamageType(damageType);
        projectile.SetDisplayDamagePopup(displayDamagePopup);
    }

    protected virtual void Attack(List<Character> targets, Character target)
    {
        animator.Play("Attack");
        StartCoroutine(ActualAttack(targets, target, delayForAttackAnimation));
    }

    protected virtual IEnumerator ActualAttack(List<Character> targets, Character target, float delayForAttackAnimation)
    {
        yield return new WaitForSeconds(delayForAttackAnimation);

        target.TakeDamage(GetAdjustedAttack());
    }

    public virtual void TakeDamage(float damage)
    {
        TakeDamage(damage, DAMAGE_TYPE.PHYSICAL, Constants.DONT_DISPLAY_DAMAGE_NUMBER);
    }

    public virtual void TakeDamage(float damage, DAMAGE_TYPE damageType, bool displayDamageNumber)
    {
        if (isDead)
            return;

        if (spRecoveryStyle.HasFlag(SP_RECOVERY_STYLE.DEFENSIVE) && sp < maxSp)
            GainSP(1f);

        if (rand.NextDouble() < GetAdjustedDodge())
            return;

        float damageTaken = CalculateLifeLoss(damage, damageType);
        hitpoints -= damageTaken;

        if (displayDamageNumber)
        {
            DamagePopupFactory.Create(damageTaken, transform.position, damageType);
        }

        if (healthBarState == HEALTH_BAR_STATE.HEALING && hitpoints / startHitpoints > healthbar.fillAmount)
        {
            healthbarWhite.fillAmount = hitpoints / startHitpoints;
        }
        else
        {
            healthBarState = HEALTH_BAR_STATE.DYING;
            healthbar.fillAmount = hitpoints / startHitpoints;
        }

        if (hitpoints <= 0)
            Die();
    }

    public virtual float CalculateLifeLoss(float damage, DAMAGE_TYPE damageType)
    {
        if (damageType == DAMAGE_TYPE.PHYSICAL)
            return Math.Max(.05f * damage, (damage - GetAdjustedDefense()) * physicalDamageMultiplier * damageMultiplier);
        else if (damageType == DAMAGE_TYPE.ARTS)
            return damage * (1 - GetAdjustedResistance() / 100f) * artsDamageMultiplier * damageMultiplier;
        else //PURE DAMAGE
            return damage * damageMultiplier;
    }

    public virtual void GainHealth(float hitpointsRestored)
    {
        hitpoints = Math.Min(startHitpoints, hitpoints + hitpointsRestored);

        if (healthBarState == HEALTH_BAR_STATE.DYING && hitpoints / startHitpoints < healthbarWhite.fillAmount)
        {
            healthbar.fillAmount = hitpoints / startHitpoints;
        }
        else
        {
            healthBarState = HEALTH_BAR_STATE.HEALING;
            healthbarWhite.fillAmount = hitpoints / startHitpoints;
        }
    }

    public virtual void GainSP(float spRestored)
    {
        if (!skillActive)
        {
            sp = Math.Min(maxSp, sp + spRestored);

            if (spbar != null && !spDraining)
                spbar.fillAmount = sp / skillSPCost;
        }
    }

    public void ResetAttackCooldown()
    {
        attackCountdown = 0f;
    }

    public virtual float GetProjectileDamage(Character target)
    {
        return GetAdjustedAttack() - target.GetAdjustedDefense();
    }

    #region Buffs

    public void ApplyBuff(StatBuff newBuff)
    {
        foreach(StatBuff buff in statBuffs)
        {
            if (buff.name == newBuff.name)
            {
                buff.ResetDuration();
                return;
            }
        }

        if (newBuff.stat.HasFlag(STAT.HITPOINTS))
        {
            float oldRatio = GetAdjustedMaxHitpoints() / startHitpoints;
            statBuffs.Add(newBuff);
            float newRatio = GetAdjustedMaxHitpoints() / startHitpoints;
            hitpoints *= (newRatio / oldRatio);
        }
        else
        {
            statBuffs.Add(newBuff);
        }
    }

    public void ApplyBuff(FunctionBuff newBuff)
    {
        foreach (FunctionBuff buff in functionBuffs)
        {
            if (buff.name == newBuff.name)
            {
                buff.ResetDuration();
                return;
            }
        }

        functionBuffs.Add(newBuff);
    }

    public void EndBuff(string name)
    {
        foreach (StatBuff buff in statBuffs)
        {
            if (buff.name == name)
            {
                statBuffs.Remove(buff);

                if (buff.stat.HasFlag(STAT.HITPOINTS))
                {
                    if (hitpoints > GetAdjustedMaxHitpoints())
                        hitpoints = GetAdjustedMaxHitpoints();
                    GainHealth(0f);
                }
                return;
            }
        }
        foreach (FunctionBuff buff in functionBuffs)
        {
            if (buff.name == name)
            {
                functionBuffs.Remove(buff);
                return;
            }
        }
    }

    protected float GetAdjustedStat(float baseValue, STAT statType)
    {
        float adjustedStat = baseValue;
        float multiplier = 1f;
        float flatBuff = 0f;
        float debuffMultiplier = 1f;
        float flatDebuff = 0f;
        foreach (StatBuff buff in statBuffs)
        {
            if (buff.stat.HasFlag(statType))
            {
                if(buff.flat < 0f || buff.percent < 0f)
                {
                    flatDebuff += buff.flat;
                    debuffMultiplier *= (1 + buff.percent);
                }
                else
                {
                    flatBuff += buff.flat;
                    multiplier += buff.percent;
                }
            }
        }

        adjustedStat *= multiplier;
        adjustedStat += flatBuff;

        adjustedStat -= flatDebuff;
        adjustedStat *= debuffMultiplier;

        return adjustedStat;
    }

    public virtual float GetAdjustedMaxHitpoints()
    {
        return GetAdjustedStat(startHitpoints, STAT.HITPOINTS);
    }

    public virtual float GetAdjustedAttack()
    {
        return GetAdjustedStat(baseAttack, STAT.ATTACK);
    }

    public virtual float GetAdjustedDefense()
    {
        return Math.Max(0f, GetAdjustedStat(defense, STAT.DEFENSE));
    }

    public virtual float GetAdjustedResistance()
    {
        return Math.Min(95f, GetAdjustedStat(resistance, STAT.RESISTANCE));
    }

    public virtual float GetAdjustedAttackInterval()
    {
        //TODO Rework this
        float attackSpeed = 1f / attackInterval;
        float multiplier = 1f;
        foreach (StatBuff buff in statBuffs)
        {
            if (buff.stat.HasFlag(STAT.ATTACK_SPEED))
            {
                attackSpeed += buff.flat;
                multiplier += buff.percent;
            }
        }
        attackSpeed *= multiplier;
        float adjustedStat = 1f / attackSpeed;
        return adjustedStat;
    }

    public virtual float GetAdjustedDodge()
    {
        return GetAdjustedStat(dodge, STAT.DODGE);
    }

    public virtual float GetAdjustedPhysicalDamageMultiplier()
    {
        return GetAdjustedStat(physicalDamageMultiplier, STAT.PHYSICAL_DAMAGE_MULTIPLIER);
    }

    public virtual float GetAdjustedArtsDamageMultiplier()
    {
        return GetAdjustedStat(artsDamageMultiplier, STAT.ARTS_DAMAGE_MULTIPLIER);
    }

    public virtual float GetAdjustedDamageMultiplier()
    {
        return GetAdjustedStat(damageMultiplier, STAT.DAMAGE_MULTIPLIER);
    }

    public float ConvertASPDBuffToATKIntervalBuff(float aspdBuff)
    {
        return 1f / (1 + aspdBuff);
    }

    #endregion Buffs

    public IEnumerator StunForSeconds(float duration)
    {
        stunned++;
        yield return new WaitForSeconds(duration);
        stunned--;
    }

    public IEnumerator SleepForSeconds(float duration)
    {
        asleep++;
        yield return new WaitForSeconds(duration);
        asleep--;
    }

    public IEnumerator ColdForSeconds(float duration)
    {
        cold++;
        yield return new WaitForSeconds(duration);
        cold--;
    }

    public static IEnumerator StopVisualEffectAfterDelay(VisualEffect effect, float delay)
    {
        yield return new WaitForSeconds(delay);

        effect.Stop();
        Destroy(effect.gameObject, 5f);
    }

    protected void SkillSetup()
    {
        maxSp = skillSPCost * skillMaxCharges;
        spDrainDuration = skillDuration > 0f ? skillDuration : attackInterval;
        spDrainAmountToGo = skillSPCost;
    }

    public abstract void Die();

    protected abstract void UpdateTarget();

    protected abstract List<Character> GetCharactersInRange();
}
