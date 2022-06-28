using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoints : MonoBehaviour
{
    public Waypoint[] path1;
    public Waypoint[] path2;
    public Waypoint[] path3;

    private static Waypoint[][] paths;

    private void Awake()
    {
        paths = new Waypoint[3][];

        paths[0] = path1;
        paths[1] = path2;
        paths[2] = path3;
    }

    public static Waypoint[] GetPath(int index)
    {
        return paths[index];
    }

    public static Waypoint[] GetPath(string s)
    {
        switch (s){
            case "PATH_1":
                return paths[0];
            case "PATH_2":
                return paths[1];
            case "PATH_3":
                return paths[2];
            case "PATH_4":
                return paths[3];
            case "PATH_5":
                return paths[4];
            case "PATH_6":
                return paths[5];
            case "PATH_7":
                return paths[6];
            case "PATH_8":
                return paths[7];
            case "PATH_9":
                return paths[8];
            case "PATH_10":
                return paths[9];
            default:
                return paths[0];
        }
    }
}
