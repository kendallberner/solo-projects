using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionBuff
{
    public string name;
    public Function function;
    public float duration;
    public float durationRemaining;

    public FunctionBuff(string name, Function function, float duration)
    {
        this.name = name;
        this.function = function;
        this.duration = duration;
        durationRemaining = duration;
    }

    public void ResetDuration()
    {
        durationRemaining = Mathf.Max(durationRemaining, duration);
    }

    public void EndBuff()
    {
        durationRemaining = 0f;
    }

    public delegate void Function(Character character);

    public void Update(Character character)
    {
        function.Invoke(character);
    }
}
