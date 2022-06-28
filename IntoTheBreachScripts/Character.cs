using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public abstract class Character : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer flamesSpriteRenderer;
    [SerializeField] private GameObject displayHealthBar;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private Image healthPoint;

    public bool CanMove { get { return !hasMoved; } }

    public bool CanAct { get { return !hasActed; } }

    public int movement, maxHitpoints;
    public bool isMassive;
    public bool isFlying;
    [HideInInspector] public bool hasMoved;
    [HideInInspector] public bool hasActed;
    public bool isArmored;
    [HideInInspector] public bool isBurning;
    [HideInInspector] public bool isAcidic;
    [HideInInspector] public bool isFrozen;
    public bool isFlameproof;
    public Attack attack1, attack2, repair;

    public FACTION faction;
    public string displayName;

    private int threatenedDamage;
    [HideInInspector] public Color targetColor;
    [HideInInspector] public int hitpoints;
    protected List<Image> hitpointImages = new List<Image>();
    [HideInInspector] public Tile occupiedTile;
    private Tile shiftTargetTile;
    private bool isShifting, isShiftingTowardsCollision, isFalling;
    protected bool isSelected;
    private Vector3 originalPosition;

    private void Awake()
    {
        if (attack1 != null)
        {
            attack1 = Instantiate(attack1, transform);
            attack1.owner = this;
        }
        if (attack2 != null)
        {
            attack2 = Instantiate(attack2, transform);
            attack2.owner = this;
        }
        if (repair != null)
        {
            repair = Instantiate(repair, transform);
            repair.owner = this;
        }
    }

    protected virtual void Start()
    {
        hitpoints = maxHitpoints;
        for (int i = 0; i < maxHitpoints; i++)
        {
            hitpointImages.Add(Instantiate(healthPoint, healthBar.transform));
        }
        if (hitpoints > 4) healthBar.transform.parent.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(2,1);

        displayHealthBar.SetActive(false);
    }

    public void Refresh()
    {
        hitpoints = maxHitpoints;
        isBurning = false;
    }

    protected virtual void Update()
    {
        for (int i = 0; i < hitpoints - threatenedDamage; i++)
        {
            hitpointImages[i].color = Color.Lerp(hitpointImages[i].color, Color.green, Time.deltaTime * 5f);
        }
        for (int i = hitpoints; i < maxHitpoints; i++)
        {
            hitpointImages[i].color = Color.Lerp(hitpointImages[i].color, Color.black, Time.deltaTime * 5f);
        }

        for(int i = hitpoints-threatenedDamage; i < hitpoints; i++)
        {
            hitpointImages[i].color = Color.Lerp(hitpointImages[i].color, targetColor, Time.deltaTime * 5f);
            if (hitpointImages[i].color.a < .25f) targetColor = new Color(0, 1, 0, 1);
            if (hitpointImages[i].color.a > .95f) targetColor = new Color(0, 1, 0, 0);
        }

        if (isShifting)
        {
            transform.position = Vector3.Lerp(transform.position, shiftTargetTile.transform.position, Time.deltaTime * 6f);
            if (Vector3.Distance(transform.position, shiftTargetTile.transform.position) < .05f) {
                isShifting = false;
                if(occupiedTile.IsSubmerged() && !isMassive)
                {
                    isFalling = true;
                }
            } 
        }

        if (isShiftingTowardsCollision)
        {
            transform.position = Vector3.Lerp(transform.position, shiftTargetTile.transform.position, Time.deltaTime * 6f);
            if (Vector3.Distance(transform.position, shiftTargetTile.transform.position) < .5f)
            {
                transform.position = originalPosition;
                TakeDamage(1, DAMAGE_TYPE.SHIFT);
                shiftTargetTile.TakeDamage(1, DAMAGE_TYPE.SHIFT);
                isShiftingTowardsCollision = false;
            }
        }

        if (isFalling)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(.1f, .1f), Time.deltaTime * 6f);
            if(transform.localScale.x < .15f)
            {
                isFalling = false;
                Die();
            }
        }
    }

    public virtual void Select()
    {
        if (CanMove) GridManager.Instance.HighlightTilesForMovement(GridManager.Instance.GetReachableTiles(this));
        DisplayHealthBar(true);
        isSelected = true;
    }

    public virtual void LoseSelection()
    {
        isSelected = false;
        DisplayHealthBar(false);
    }

    public void Threaten(int damage)
    {
        DisplayHealthBar(true);
        threatenedDamage = Mathf.Min(damage, hitpoints);
    }

    public void UnThreaten()
    {
        threatenedDamage = 0;
        for (int i = 0; i < hitpoints; i++)
        {
            hitpointImages[i].color = new Color(0, 1, 0, 1);
        }
    }

    public virtual void DisplayHealthBar(bool display)
    {
        displayHealthBar.SetActive(display);
    }

    public virtual void TakeDamage(int damage)
    {
        TakeDamage(damage, DAMAGE_TYPE.STANDARD);
    }

    public virtual void TakeDamage(int damage, DAMAGE_TYPE damageType)
    {
        switch (damageType)
        {
            case DAMAGE_TYPE.STANDARD:
                if (isAcidic)
                    hitpoints -= 2 * damage;
                else if (isArmored)
                    hitpoints -= damage - 1;
                else
                    hitpoints -= damage;
                break;
            case DAMAGE_TYPE.SHIFT:
                hitpoints--;
                break;
            case DAMAGE_TYPE.FIRE:
                if (!isFlameproof) hitpoints--;
                break;
            case DAMAGE_TYPE.BURROW:
                hitpoints--;
                break;
        }
        if (hitpoints <= 0) Die();
    }

    public virtual void Die()
    {
        occupiedTile.occupant = null;
        Destroy(gameObject);
    }

    public abstract Task Move(Tile originTile, Tile destinationTile);

    public virtual void Attack(Tile targetTile)
    {
        if (attack1.prepared)
            attack1.Execute(targetTile);
        else if (attack2 != null && attack2.prepared)
            attack2.Execute(targetTile);
        else if (repair.prepared)
            repair.Execute(targetTile);
    }

    public virtual void PrepareRepair()
    {
        repair.prepared = true;
        repair.Telegraph();
    }
    public virtual void PrepareAttack1()
    {
        attack1.prepared = true;
        attack1.Telegraph();
    }
    public virtual void PrepareAttack2()
    {
        attack2.prepared = true;
        attack2.Telegraph();
    }

    public async virtual Task Upkeep()
    {
        hasMoved = false;
        hasActed = false;
    }

    public void SetShiftPosition(Tile targetTile, bool onCollisionCourse)
    {
        isShifting = !onCollisionCourse;
        isShiftingTowardsCollision = onCollisionCourse;
        shiftTargetTile = targetTile;
        originalPosition = transform.position;
    }

    public void OnHover()
    {
        displayHealthBar.SetActive(true);
    }

    public void LoseHover()
    {
        if(!isSelected) displayHealthBar.SetActive(false);
    }

    public bool IsReadyForTurnEnd()
    {
        return !isShifting && !isFalling;
    }

    public void SetAflame()
    {
        isBurning = true;
        flamesSpriteRenderer.enabled = true;
    }

    public void DouseFlames()
    {
        isBurning = false;
        flamesSpriteRenderer.enabled = false;
    }
}

public enum FACTION
{
    PLAYER,
    ENEMY
}

public enum DAMAGE_TYPE
{
    STANDARD,
    SHIFT,
    FIRE,
    BURROW
}