using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    public int xmax, ymax;

    private List<Thing>[,] grid;
    private Dictionary<int, List<Tuple<Thing, Vector2>>> moveHistory;

    private void Awake()
    {
        instance = this;

        grid = new List<Thing>[xmax+1,ymax+1];
        moveHistory = new Dictionary<int, List<Tuple<Thing, Vector2>>>();
    }

    private void Start()
    {
        for (int x = 0; x <= xmax; x++)
        {
            for (int y = 0; y <= ymax; y++)
            {
                grid[x, y] = new List<Thing>();
            }
        }

        Thing[] things = GameObject.FindObjectsOfType<Thing>();
        foreach (Thing thing in things)
        {
            List<Thing> list = grid[thing.X(), thing.Y()];
            list.Add(thing);
            grid[thing.X(),thing.Y()] = list;
        }
    }

    public void Undo(int turnCount)
    {
        List<Tuple<Thing, Vector2>> lastTurn = moveHistory[turnCount];
        moveHistory.Remove(turnCount);

        foreach(Tuple<Thing, Vector2> movement in lastTurn)
        {
            Thing thing = movement.Item1;
            thing.gameObject.SetActive(true);
            grid[thing.X(), thing.Y()].Remove(thing);
            thing.SetTargetCoords(Mathf.RoundToInt(movement.Item2.x), Mathf.RoundToInt(movement.Item2.y));

            grid[thing.X(), thing.Y()].Add(thing);
        }
    }

    public void AddToMoveHistory(int turn, Tuple<Thing, Vector2> movement)
    {
        List<Tuple<Thing, Vector2>> movements = new List<Tuple<Thing, Vector2>>();
        movements.Add(movement);
        AddToMoveHistory(turn, movements);
    }

    public void AddToMoveHistory(int turn, List<Tuple<Thing, Vector2>> movements)
    {
        moveHistory[turn].AddRange(movements);
    }

    public void RemoveFrom(Thing thing, int x, int y)
    {
        List<Thing> list = grid[x, y];
        list.Remove(thing);
    }

    public void RemoveList(int x, int y)
    {
        grid[x,y] = new List<Thing>();
    }

    public Rule GetRuleAt(int x, int y)
    {
        if (x < 0 || y < 0 || x > xmax || y > ymax) return null;

        return grid[x, y].Count > 0 && grid[x, y][0].isRule ? (Rule)grid[x,y][0] : null;
    }

    public List<Noun> GetNounsAt(int x, int y)
    {
        List<Noun> nouns = new List<Thing>(grid[x, y]).Where(thing => !thing.isRule).Cast<Noun>().ToList();

        return nouns;
    }

    public bool Move(Thing mover, KeyCode key, int turnCount)
    {
        List<Thing> moverList = new List<Thing>();
        moverList.Add(mover);
        return Move(moverList, key, turnCount);
    }

    public bool Move(List<Thing> movers, KeyCode key, int turnCount)
    {
        int startx = movers[0].X();
        int starty = movers[0].Y();

        int endx = 0;
        int endy = 0;
        if (key == KeyCode.LeftArrow)
        {
            endx = startx - 1;
            endy = starty;
        }
        else if (key == KeyCode.RightArrow)
        {
            endx = startx + 1;
            endy = starty;
        }
        else if (key == KeyCode.DownArrow)
        {
            endx = startx;
            endy = starty - 1;
        }
        else if (key == KeyCode.UpArrow)
        {
            endx = startx;
            endy = starty + 1;
        }

        if (endx < 0 || endy < 0 || endx > xmax || endy > ymax) return false;

        List<Thing> occupants = grid[endx, endy];
        List<Thing> pushableOccupants = occupants.Where(x => !x.IsStop() && x.IsPush()).ToList();

        if (occupants.Count == 0 || occupants.All(x => !x.IsPush() && !x.IsStop()) || occupants.All(x => !x.IsStop()) && Move(pushableOccupants, key, turnCount))
        {
            grid[startx, starty] = grid[startx, starty].Except(movers).ToList();

            List<Thing> newOccupants = occupants.Except(pushableOccupants).ToList();
            newOccupants.AddRange(movers);
            grid[endx, endy] = newOccupants;

            //movers.ForEach(mover => mover.transform.position = new Vector2(endx, endy));
            movers.ForEach(mover => mover.SetTargetCoords(Mathf.RoundToInt(endx), Mathf.RoundToInt(endy)));

            foreach (Thing thing in movers)
            {
                Tuple<Thing, Vector2> movement = new Tuple<Thing, Vector2>(thing, new Vector2(startx, starty));
                List<Tuple<Thing, Vector2>> movements = new List<Tuple<Thing, Vector2>>();
                if (moveHistory.ContainsKey(turnCount))
                {
                    movements = moveHistory[turnCount];
                }
                movements.Add(movement);
                moveHistory[turnCount] = movements;
            }


            return true;
        }
        else
        {
            return false;
        }
    }
}
