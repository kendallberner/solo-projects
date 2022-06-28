using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    [HideInInspector] public static BuildManager instance;

    public GameObject rangePreviewPrefab;
    public STATE state = STATE.OFF;
    public Hero heroToPlace;

    private List<GameObject> rangePreviewGOs = new List<GameObject>();
    private Range rangeForHero;
    private DIRECTION oldDirection = DIRECTION.NONE;
    private DIRECTION directionForHero;
    private int placementIndex = 1;
    private Vector3 initialMousePosition;

    private int leftClick = 0;
    private int rightClick = 1;

    public enum STATE
    {
        OFF,
        PREVIEWING,
        SELECTING_DIRECTION
    }

    private void Awake()
    {
        instance = this;
        for (int i = 0; i < 5; i++)
        {
            rangePreviewGOs.Add(Instantiate(rangePreviewPrefab));
            rangePreviewGOs[i].SetActive(false);
        }
    }

    private void Update()
    {
        if (state.HasFlag(STATE.PREVIEWING) || state.HasFlag(STATE.SELECTING_DIRECTION))
        {
            if (Input.GetMouseButtonDown(rightClick))
            {
                CeasePreview();
                heroToPlace.gameObject.SetActive(false);
                heroToPlace = null;
                EnterOffState();
                oldDirection = DIRECTION.NONE;
                return;
            }
            else if (heroToPlace.node == null)
            {
                Ray castPoint = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(castPoint, out RaycastHit hit, Mathf.Infinity))
                {
                    heroToPlace.gameObject.transform.position = hit.point;
                    heroToPlace.model.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
                    heroToPlace.canvas.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
                }
            }
        }
        if (state.HasFlag(STATE.SELECTING_DIRECTION))
        {
            if (Input.GetMouseButtonUp(leftClick))
            {
                BuildTurretOn(heroToPlace.node);
            }
            else
            {
                float relativeX = Input.mousePosition.x - initialMousePosition.x;
                float relativeY = Input.mousePosition.y - initialMousePosition.y;
                if (Mathf.Pow(relativeX, 2) + Mathf.Pow(relativeY, 2) < 10f)
                    directionForHero = DIRECTION.NONE;
                else if (relativeX > Mathf.Abs(relativeY))
                    directionForHero = DIRECTION.RIGHT;
                else if (relativeY > Mathf.Abs(relativeX))
                    directionForHero = DIRECTION.UP;
                else if (-relativeX > Mathf.Abs(relativeY))
                    directionForHero = DIRECTION.LEFT;
                else if (-relativeY > Mathf.Abs(relativeX))
                    directionForHero = DIRECTION.DOWN;


                if (oldDirection != directionForHero && directionForHero != DIRECTION.NONE)
                {
                    oldDirection = directionForHero;
                    PrepareRangePreview(directionForHero);
                }
            }
        }
    }

    private void PrepareRangePreview(DIRECTION direction)
    {
        rangeForHero = Range.GetRangeFromBuildingBlocks(heroToPlace.rangeBuildingBlocks, heroToPlace.node.GetBuildPosition().x, heroToPlace.node.GetBuildPosition().z, direction);
        List<Vector3> centers = rangeForHero.GetPositionCenters();
        List<Vector3> scales = rangeForHero.GetScales();
        for (int i = 0; i < centers.Count; i++)
        {
            GameObject rangePreviewGO = rangePreviewGOs[i];
            rangePreviewGO.SetActive(true);
            rangePreviewGO.transform.position = centers[i];
            rangePreviewGO.transform.localScale = scales[i];
        }
    }

    public void EnterOffState()
    {
        state = STATE.OFF;
        Time.timeScale = 1f;
    }

    public void EnterPreviewingState()
    {
        state = STATE.PREVIEWING;
        Time.timeScale = Constants.SLOW_TIME_SCALE;
    }

    public void EnterSelectingDirectionState()
    {
        initialMousePosition = Input.mousePosition;
        state = STATE.SELECTING_DIRECTION;
    }

    public bool TryingToBuild { get { return heroToPlace != null; } }
    public bool CanBuild(Node node) {
        if (heroToPlace != null && PlayerStats.DP >= heroToPlace.cost && heroToPlace.CanBeDeployedOn(node))
            return true;
        else if (heroToPlace != null && node.hero != null && PlayerStats.DP + node.hero.GetComponent<Hero>().cost/2 >= heroToPlace.cost && heroToPlace.CanBeDeployedOn(node))
            return true;
        else
            return false;
    }

    public void PreviewTurretOn(Node node)
    {
        if (CanBuild(node))
        {
            if(node.hero != null)
            {
                //yikes
            }
            else
            {
                heroToPlace.node = node;
                heroToPlace.gameObject.transform.position = node.GetBuildPosition();
                PrepareRangePreview(DIRECTION.RIGHT);
            }
        }
    }

    public void BuildTurretOn(Node node)
    {
        if(CanBuild(node))
        {
            if (node.hero != null)
            {
                node.hero.GetComponent<Hero>().Retreat();
            }

            PlayerStats.LoseDP(heroToPlace.cost);

            heroToPlace.gameObject.SetActive(true);
            heroToPlace.enabled = true;
            heroToPlace.gameObject.GetComponent<BoxCollider>().enabled = true;
            heroToPlace.gameObject.transform.position = node.GetBuildPosition();
            heroToPlace.placementIndex = placementIndex++;
            heroToPlace.node = node;
            heroToPlace.range = rangeForHero;
            heroToPlace.OnSpawn();
            node.hero = heroToPlace.gameObject;

            for (int i = 0; i < rangePreviewGOs.Count; i++)
            {
                rangePreviewGOs[i].SetActive(false);
            }

            GameObject shopGO = GameObject.Find("Shop" + heroToPlace.GetType().Name);
            heroToPlace.shopGO = shopGO;
            if(shopGO != null)
                shopGO.SetActive(false);

            heroToPlace = null;
            EnterOffState();
            oldDirection = DIRECTION.NONE;
        }
    }

    public void SelectHeroToPlace(GameObject hero)
    {
        ClearHeroToPlace();
        heroToPlace = hero.GetComponent<Hero>(); 
        heroToPlace.gameObject.SetActive(true);
        heroToPlace.gameObject.GetComponent<Hero>().enabled = false;
        heroToPlace.gameObject.GetComponent<BoxCollider>().enabled = false;
        EnterPreviewingState();
    }

    public void ClearHeroToPlace()
    {
        if(heroToPlace != null)
        {
            heroToPlace.gameObject.SetActive(false);
            heroToPlace = null;
        }
    }

    public void CeasePreview()
    {
        heroToPlace.node = null;
        for (int i = 0; i < rangePreviewGOs.Count; i++)
        {
            rangePreviewGOs[i].SetActive(false);
        }
    }
}
