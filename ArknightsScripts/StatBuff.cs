using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatBuff
{
    public string name;
    public STAT stat;
    public float flat;
    public float percent;
    public float duration;
    public float durationRemaining;

    public StatBuff(string name, STAT stat, float flat, float percent, float duration)
    {
        this.name = name;
        this.stat = stat;
        this.flat = flat;
        this.percent = percent;
        this.duration = duration;
        durationRemaining = duration;
    }

    public void ResetDuration()
    {
        durationRemaining = Mathf.Max(durationRemaining, duration);
    }
}
