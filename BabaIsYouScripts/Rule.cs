using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rule : Thing
{
    public bool isNoun, isVerb, isAdjective, isConjunction;

    public Color baseColor;

    public override bool IsPush()
    {
        return true;
    }

    public override bool IsStop()
    {
        return false;
    }
}
