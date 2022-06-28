using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Unity Setup")]
    public float speed = 70f;
    public GameObject impactEffect;

    private Character parent;
    private Transform target;
    private Character targetCharacter;
    private float explosionRadius = 0f;
    private string targetTag;
    private float damage;
    private DAMAGE_TYPE damageType;
    private bool displayDamagePopup;

    public void SetParent(Character turret)
    {
        parent = turret;
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
        targetCharacter = target.gameObject.GetComponent<Character>();
    }

    public void SetExplosionRadius(float aoe)
    {
        explosionRadius = aoe;
    }

    public void SetTargetTag(string tag)
    {
        targetTag = tag;
    }

    public void SetDamage(float _damage)
    {
        damage = _damage;
    }

    public void SetDamageType(DAMAGE_TYPE _damageType)
    {
        damageType = _damageType;
    }

    public void SetDisplayDamagePopup(bool _displayDamagePopup)
    {
        displayDamagePopup = _displayDamagePopup;
    }

    void Update()
    {
        if(target == null || !target.gameObject.activeSelf)
        {
            parent.ResetAttackCooldown();
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if(dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.LookAt(target);
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    private void HitTarget()
    {
        GameObject effectInstance = Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effectInstance, 5f);

        if(explosionRadius > 0f)
        {
            Explode();
        }
        else
        {
            //DamageTarget(target.gameObject.GetComponent<Character>());
            targetCharacter.TakeDamage(damage, damageType, displayDamagePopup);
        }

        Destroy(gameObject);
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            if(collider.CompareTag(targetTag))
            {
                //DamageTarget(collider.gameObject.GetComponent<Character>());
                collider.gameObject.GetComponent<Character>().TakeDamage(damage, damageType, displayDamagePopup);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
