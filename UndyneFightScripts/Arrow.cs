using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    public SpriteRenderer sprite;
    public GameObject explosionEffect;

    protected Vector2 velocity;

    public void Setup(Vector2 direction, float speed)
    {
        velocity = direction * speed;
    }

    public void Shoot()
    {
        GetComponent<Rigidbody2D>().velocity = velocity;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Gamemanager.instance.RemoveArrow(this);

        if (collision.gameObject.name.Equals("Heart"))
        {
            GameObject explosionEffectInstance = Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(explosionEffectInstance, 5f);
        }
        
        Destroy(gameObject);
    }

    public virtual void Highlight()
    {
        sprite.color = Color.red;
    }
}
