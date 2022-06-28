using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public Image highlight;
    public Image lockIcon;
    public TextMeshPro textMesh;
    public GameObject cellExplosionEffect;

    public bool isReal;
    public CELL_STATE state = CELL_STATE.OPEN;
    public int x;
    public int y;
    public int value;

    private GameMaster gameMaster;
    private Cell[][] cellMatrix;
    private Transform displayObject;
    private float scaleSpeed = 10f;
    private Vector3 scaleTarget = new Vector3(1f, 1f, 1);
    private float fillSpeed = 15f;
    private float fillTarget = 0f;

    private void Awake()
    {
        displayObject = transform.GetChild(0);
        lockIcon.enabled = false;
        int.TryParse(textMesh.text, out value);
    }

    private void Start()
    {
        gameMaster = GameMaster.instance;
        cellMatrix = gameMaster.CellMatrix;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, scaleTarget, scaleSpeed * Time.deltaTime);
        if(state != CELL_STATE.LOCKED)
            highlight.fillAmount = Mathf.Lerp(highlight.fillAmount, fillTarget, fillSpeed * Time.deltaTime);
    }

    private void OnMouseEnter()
    {
        scaleTarget = new Vector3(.8f, .8f, 1);
        fillTarget = .5f;
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(state == CELL_STATE.OPEN)
            {
                if (isReal)
                    displayObject.GetComponent<SpriteRenderer>().color = Color.red;
                else
                {
                    GameObject cellExplosionEffectInstance = Instantiate(cellExplosionEffect, transform.position, transform.rotation);
                    Destroy(cellExplosionEffectInstance, 5f);
                    state = CELL_STATE.DESTROYED;
                    displayObject.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    highlight.enabled = false;
                    textMesh.enabled = false;
                }
            }
            gameMaster.UpdateHintLogicForCellXY(x, y);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if(state == CELL_STATE.LOCKED)
            {
                state = CELL_STATE.OPEN;
                displayObject.GetComponent<SpriteRenderer>().color = Color.white;
                lockIcon.enabled = false;
            }
            else if(state == CELL_STATE.OPEN)
            {
                state = CELL_STATE.LOCKED;
                displayObject.GetComponent<SpriteRenderer>().color = Color.yellow;
                highlight.fillAmount = .5f;
                lockIcon.enabled = true;
            }
            gameMaster.CheckForCompletedCellHints();
            gameMaster.UpdateHintLogicForCellXY(x, y);
        }
    }

    private void OnMouseExit()
    {
        scaleTarget = new Vector3(1f, 1f, 1);
        fillTarget = 0f;
    }
}

public enum CELL_STATE
{
    OPEN = 1,
    LOCKED = 2,
    DESTROYED = 3
}