using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
//using System;

public class Procent : Value
{
    internal static readonly Procent HundredProcent = new Procent(1f);
    internal static readonly Procent ZeroProcent = new Procent(0f);

   
    public Procent(float number) : base(number)
    {
    }
    public Procent(Procent number) : base(number.get())
    {

    }
    public static Procent makeProcent(Value numerator, Value denominator)
    {
        if (denominator.get() == 0f)
        {
            // Debug.Log("Division by zero in Procent.makeProcent(Storage)");
            return new Procent(0f);
        }
        else
            return new Procent(numerator.get() / denominator.get());
    }
    public static Procent makeProcent(int numerator, int denominator)
    {
        if (denominator == 0)
        {
            Debug.Log("Division by zero in Procent.makeProcent()");
            return new Procent(0f);
        }
        else
            return new Procent(numerator / (float)denominator);
    }
    //TODO check it
    public static Procent makeProcent(PrimitiveStorageSet numerator, PrimitiveStorageSet denominator)
    {
        float allGoodsAmount = numerator.sum();
        if (allGoodsAmount == 0f)
            return new Procent(1f);
        Dictionary<Product, float> dic = new Dictionary<Product, float>();
        //numerator / denominator
        float relation;
        foreach (var item in numerator)
        {
            Storage denom = denominator.findStorage(item.getProduct());
            if (denom == null) // no such goods
                relation = 0f;
            else
                if (denom.get() == 0f) // division by zero
                relation = 0f;
            else
            {
                relation = item.get() / denom.get();
                if (relation > 1f) relation = 1f;
            }
            dic.Add(item.getProduct(), relation);
        }
        float result = 0f;

        foreach (var item in dic)
        {
            result += item.Value * numerator.findStorage(item.Key).get() / allGoodsAmount;
        }
        return new Procent(result);
    }

    internal float get50Centre()
    {
        return get() - 0.5f;
    }


    public Value sendProcentToNew(Value source)
    {

        Value result = new Value(0f);
        source.send(result, source.multipleOutside(this));
        return result;
    }
    public Storage sendProcentToNew(Storage source)
    {
        Storage result = new Storage(source.getProduct(), 0f);
        source.send(result, source.multipleOutside(this));
        return result;
    }

    public void add(Procent pro)
    {
        base.add(pro);
        if (base.get() > 1f)
            set(1f);
    }
    public void addPoportionally(int baseValue, int secondValue, Procent secondProcent)
    {
        if ((baseValue + secondValue) != 0)
            set((this.get() * baseValue + secondProcent.get() * secondValue) / (float)(baseValue + secondValue));
    }
    public override string ToString()
    {
        if (get() > 0)
            return System.Convert.ToString(get() * 100f) + "%";
        else return "0%";
    }

    //internal uint getProcent(int population)
    //{
    //    return (uint)Mathf.RoundToInt(get() * population);
    //}
    internal int getProcent(int population)
    {
        return Mathf.RoundToInt(get() * population);
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
