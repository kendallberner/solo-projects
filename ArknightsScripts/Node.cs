using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Node : MonoBehaviour
{
    [Header("Unity Setup")]
    public Color canBeBuiltOnColor = Color.green;
    public Color errorColor = Color.red;
    public Vector3 positionOffset;
    public bool canPlaceMeleeHeroes;
    public bool canPlaceRangedHeroes;


    [HideInInspector] public GameObject hero;
    private Renderer rend;
    private Color startColor;

    BuildManager buildManager;

    private readonly int rightClick = 1;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        startColor = rend.material.color;

        buildManager = BuildManager.instance;
    }

    private void Update()
    {
        if (buildManager.state.HasFlag(BuildManager.STATE.PREVIEWING))
        {
            if (buildManager.heroToPlace.CanBeDeployedOn(this))
            {
                rend.material.color = canBeBuiltOnColor;
            }
        }
        else
            rend.material.color = startColor;
    }

    public Vector3 GetBuildPosition()
    {
        return transform.position + positionOffset;
    }

    private void OnMouseDown()
    {
        if (buildManager.CanBuild(this))
        {
            buildManager.EnterSelectingDirectionState();
            rend.material.color = startColor;
        }
    }

    private void OnMouseEnter()
    {
        if (buildManager.state.HasFlag(BuildManager.STATE.PREVIEWING))
        {
            if (buildManager.CanBuild(this))
            {
                buildManager.PreviewTurretOn(this);
            }
        }
    }

    private void OnMouseExit()
    {
        if(buildManager.state.HasFlag(BuildManager.STATE.PREVIEWING))
            buildManager.CeasePreview();
    }

    private void OnMouseOver()
    {
        if (buildManager.CanBuild(this) && !buildManager.state.HasFlag(BuildManager.STATE.SELECTING_DIRECTION))
            buildManager.PreviewTurretOn(this);
            
        if (Input.GetMouseButtonDown(rightClick))
        {
            rend.material.color = startColor;
        }
    }
}
