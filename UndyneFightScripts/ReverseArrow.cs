using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ReverseArrow : Arrow
{
    private Transform pivotPoint;
    private bool rotated = false;

    private void Start()
    {
        pivotPoint = FindObjectOfType<Heart>().transform;
    }

    private void Update()
    {
        if(!rotated && Vector3.Distance(transform.position, pivotPoint.position) <= 3f)
        {
            rotated = true;
            GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
            Flip();
        }
    }

    private async void Flip()
    {
        float degreesLeftToTurn = 180f;
        while(degreesLeftToTurn > 0f)
        {
            float degreesTurningThisFrame = Mathf.Min(Time.deltaTime * velocity.magnitude * 50f, degreesLeftToTurn);
            degreesLeftToTurn -= degreesTurningThisFrame;
            transform.RotateAround(pivotPoint.position, Vector3.forward, degreesTurningThisFrame);
            await Task.Yield();
        }
        GetComponent<Rigidbody2D>().velocity = -velocity;
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

    public override void Highlight()
    {
        sprite.color = Color.blue;
    }
}
