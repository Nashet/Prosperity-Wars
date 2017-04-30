using UnityEngine;
using System.Collections;
using System;
//using System;

public class Procent : Value
{
    //uint value;
    public static bool GetChance(uint procent)
    {
        //TODO fix that GetChance shit
        float realLuck = UnityEngine.Random.value * 100; // (0..100 including)
        if (procent >= realLuck)
            return true;
        else
            return false;
    }
    //uint get() { return value.g; }
    public Procent(float number) : base(number)
    {

    }
    public void add(Procent pro)
    {
        base.add(pro);
        if (base.get() > 1f)
            set(1f);
    }
    public override string ToString()
    {
        if (get() > 0)
            return System.Convert.ToString(get() * 100f) + "%";
        else return "0%";
    }

    internal uint getProcent(uint population)
    {
        return (uint)Mathf.RoundToInt(get() * population);
    }
    internal int getProcent(int population)
    {
        return (int)Mathf.RoundToInt(get() * population);
    }
    override public void set(float invalue)
    {
        if (invalue < 0f)
            base.set(0f);
        //else
        //    if (invalue > 1f)
        //    base.set(1f);
        else
            base.set(invalue);
    }
}
