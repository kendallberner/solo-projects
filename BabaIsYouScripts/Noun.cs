using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noun : Thing
{
    public bool isYou;
    public bool isWin;
    public bool isStop;
    public bool isPush;
    public bool isExplode;
    public bool isLose;
    public bool isLock;
    public bool isKey;
    public bool isMove;

    public override bool IsPush()
    {
        return isPush;
    }

    public override bool IsStop()
    {
        return isStop;
    }
}
