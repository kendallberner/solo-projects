using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    [HideInInspector] public static GameMaster instance;

    public GameObject NextLevelButton;

    public Cell[][] CellMatrix
    {
        get; set;
    }

    public List<Cell> CellsWithValues
    {
        get; set;
    }

    public List<Cell> Cells
    {
        get; set;
    }

    public int RowSize
    {
        get; set;
    }
    public int ColumnSize
    {
        get; set;
    }
    public bool AllCellsAreDestroyedOrLocked { get { 
            foreach (Cell cell in Cells) {
                if (cell.state == CELL_STATE.OPEN)
                    return false;
            }
            return true;
        } }

    private HintSequence[][] hintSequences = new HintSequence[2][];

    private void Awake()
    {
        instance = this;
        Cell[] cells = GameObject.FindObjectsOfType<Cell>();

        RowSize = ColumnSize = (int)(Math.Sqrt(cells.Length));

        CellMatrix = new Cell[RowSize][];
        for (int i = 0; i < RowSize; i++)
        {
            CellMatrix[i] = new Cell[RowSize];
        }
    }

    void Start()
    {
        Cell[] cells = GameObject.FindObjectsOfType<Cell>();
        Cells = new List<Cell>();
        CellsWithValues = new List<Cell>();
        foreach (Cell cell in cells)
        {
            CellMatrix[cell.x][cell.y] = cell;
            Cells.Add(cell);
            if (cell.value > 0)
                CellsWithValues.Add(cell);
        }

        StartCoroutine(UpdateCellsToBeRemovedText());


        HintSequence[] hintSequenceArray = GameObject.FindObjectsOfType<HintSequence>();
        hintSequences[0] = new HintSequence[RowSize];
        hintSequences[1] = new HintSequence[ColumnSize];
        foreach (HintSequence hintSequence in hintSequenceArray)
        {
            if(hintSequence.axis == AXIS.COL)
                hintSequences[0][hintSequence.order] = hintSequence;
            else
                hintSequences[1][hintSequence.order] = hintSequence;
        }
    }

    internal void UpdateHintLogicForCellXY(int x, int y)
    {
        if(hintSequences[0][x] != null)
            StartCoroutine(hintSequences[0][x].UpdateHintOpacity());
        if (hintSequences[1][y] != null)
            StartCoroutine(hintSequences[1][y].UpdateHintOpacity());
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            StartCoroutine(UpdateCellsToBeRemovedText());
        }
    }

    IEnumerator UpdateCellsToBeRemovedText()
    {
        yield return new WaitForEndOfFrame();

        int cellsToBeRemoved = 0;
        Cell[] cells = GameObject.FindObjectsOfType<Cell>();
        foreach (Cell cell in cells)
        {
            if (!cell.isReal && cell.state != CELL_STATE.DESTROYED)
                cellsToBeRemoved++;
        }

        GetComponentInChildren<TextMeshPro>().text = cellsToBeRemoved + " cells to be removed";

        if (cellsToBeRemoved == 0 && AllCellsAreDestroyedOrLocked)
            Victory();
    }

    private void Victory()
    {
        GetComponentInChildren<TextMeshPro>().text = "Victory!";
        NextLevelButton.SetActive(true);
    }

    internal void CheckForCompletedCellHints()
    {
        foreach(Cell cellWithValue in CellsWithValues)
        {
            List<Cell> contiguousCells = GetContiguousCells(cellWithValue, new List<Cell>());

            List<Cell> cellsWithvalues = new List<Cell>();
            foreach (Cell cell in contiguousCells)
            {
                if (cell.value > 0)
                    cellsWithvalues.Add(cell);
            }

            if (cellsWithvalues.Count == 1 && contiguousCells.Count == cellsWithvalues[0].value)
                cellWithValue.textMesh.color = new Color(0,0,0,.4f);
            else
            {
                foreach (Cell cell in cellsWithvalues)
                    cell.textMesh.color = Color.black;
                cellWithValue.textMesh.color = Color.black;
            }
        }
    }

    private List<Cell> GetContiguousCells(Cell cell, List<Cell> seenCells)
    {
        if (seenCells.Contains(cell) || cell.state != CELL_STATE.LOCKED)
            return seenCells;


        seenCells.Add(cell);
        Cell cellToCheck;
        if (cell.y - 1 >= 0)
        {
            cellToCheck = CellMatrix[cell.x][cell.y - 1];
            if (cellToCheck != null && cellToCheck.state == CELL_STATE.LOCKED && !seenCells.Contains(cellToCheck))
            {
                seenCells = GetContiguousCells(cellToCheck, seenCells);
            }
        }

        if (cell.y + 1 < CellMatrix.Length)
        {
            cellToCheck = CellMatrix[cell.x][cell.y + 1];
            if (cellToCheck != null && cellToCheck.state == CELL_STATE.LOCKED && !seenCells.Contains(cellToCheck))
            {
                seenCells = GetContiguousCells(cellToCheck, seenCells);
            }
        }

        if (cell.x - 1 >= 0)
        {
            cellToCheck = CellMatrix[cell.x - 1][cell.y];
            if (cellToCheck != null && cellToCheck.state == CELL_STATE.LOCKED && !seenCells.Contains(cellToCheck))
            {
                seenCells = GetContiguousCells(cellToCheck, seenCells);
            }
        }

        if (cell.x + 1 < CellMatrix.Length)
        {
            cellToCheck = CellMatrix[cell.x + 1][cell.y];
            if (cellToCheck != null && cellToCheck.state == CELL_STATE.LOCKED && !seenCells.Contains(cellToCheck))
            {
                seenCells = GetContiguousCells(cellToCheck, seenCells);
            }
        }

        return seenCells;
    }
}
