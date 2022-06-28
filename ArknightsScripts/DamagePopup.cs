using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro textMesh;
    public Vector3 targetPosition;
    public Vector3 endPosition;
    public Vector3 velocity = new Vector3(0,0,0);

    public float lifetime = 1f;
    public float growthspan = .75f;
    public float fontSizeGrowthSpan = .75f;
    public float holdspan = 2.5f;
    public float deathspan = .25f;


    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    private void Start()
    {
        endPosition = transform.position;
    }

    private void Update()
    {
        if (growthspan > 0f)
        {
            if(fontSizeGrowthSpan > 0f)
            {
                fontSizeGrowthSpan -= Time.deltaTime;
                textMesh.fontSize += Time.deltaTime * 32;
            }
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, .25f);

            growthspan -= Time.deltaTime;
        }
        else if (holdspan > 0f)
        {
            holdspan -= Time.deltaTime;
        }
        else if (deathspan > 0f)
        {
            textMesh.fontSize = Mathf.Max(0f, textMesh.fontSize - Time.deltaTime * 128);
            transform.position = Vector3.SmoothDamp(transform.position, endPosition, ref velocity, .15f);

            deathspan -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
