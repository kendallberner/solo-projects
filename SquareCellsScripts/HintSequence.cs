using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HintSequence : MonoBehaviour
{
    public AXIS axis;
    public int order;
    public List<int> values;

    private GameMaster gameMaster;
    private Cell[][] cellMatrix;
    private int axisLength;

    private List<int> lockedCellSequences = new List<int>();

    void Start()
    {
        gameMaster = GameMaster.instance;
        cellMatrix = gameMaster.CellMatrix;
        axisLength = gameMaster.RowSize;
        lockedCellSequences = new List<int>();

        for(int i = 0; i < transform.childCount; i++)
        {
            values.Add(int.Parse(transform.GetChild(i).GetComponent<TextMeshPro>().text));
        }
    }

    void Update()
    {

    }

    public IEnumerator UpdateHintOpacity()
    {
        yield return new WaitForEndOfFrame();

        lockedCellSequences.Clear();

        int fixedCoordinate = order;
        int seq = 0;
        bool seqActive = true;
        bool allCellsInRowDestroyed = true;
        for (int scalingCoordinate = 0; scalingCoordinate < axisLength; scalingCoordinate++)
        {
            if (GetCellStateByAxis(axis, fixedCoordinate, scalingCoordinate) == CELL_STATE.LOCKED)
            {
                seq++;
                allCellsInRowDestroyed = false;
            }
            else if (GetCellStateByAxis(axis, fixedCoordinate, scalingCoordinate) == CELL_STATE.OPEN)
            {
                seq = 0;
                seqActive = false;
                allCellsInRowDestroyed = false;
            }
            else if (GetCellStateByAxis(axis, fixedCoordinate, scalingCoordinate) == CELL_STATE.DESTROYED)
            {
                if(seq > 0 && seqActive)
                {
                    lockedCellSequences.Add(seq);
                    seq = 0;
                    //seqActive = true;
                }
            }
        }
        if (seq > 0 && seqActive)
            lockedCellSequences.Add(seq);


        for (int i = 0; i < values.Count; i++)
        {
            if(i < lockedCellSequences.Count && values[i] == lockedCellSequences[i] || allCellsInRowDestroyed && values[i] == 0)
            {
                transform.GetChild(i).GetComponent<TextMeshPro>().color = new Color(1, 1, 1,.1f);
            }
            else
            {
                transform.GetChild(i).GetComponent<TextMeshPro>().color = Color.white;
            }
        }
    }

    private CELL_STATE GetCellStateByAxis(AXIS axis, int fixedCoordinate, int scalingCoordinate)
    {
        if (axis == AXIS.COL)
            return cellMatrix[fixedCoordinate][scalingCoordinate].state;
        else
            return cellMatrix[scalingCoordinate][fixedCoordinate].state;
    }

    public void backwards()
    {
        if (axis == AXIS.COL)
        {
            int x = order;
            int seq = 0;
            for (int y = axisLength - 1; y >= 0; y--)
            {
                if (cellMatrix[x][y].state == CELL_STATE.LOCKED)
                {
                    seq++;
                }
                else if (cellMatrix[x][y].state == CELL_STATE.OPEN)
                {
                    seq = 0;
                }
                else if (cellMatrix[x][y].state == CELL_STATE.DESTROYED && seq > 0)
                {
                    lockedCellSequences.Add(seq);
                    seq = 0;
                }
            }
            if (seq > 0)
                lockedCellSequences.Add(seq);
        }
    }
}

public enum AXIS
{
    ROW = 1,
    COL = 2
}