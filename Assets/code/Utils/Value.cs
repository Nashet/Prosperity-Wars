using UnityEngine;

public class Value
{
    ///<summary> storing as value as number * precision </summary>
    private uint value;
    internal static uint precision = 1000; // 0.01
    public Value(float number)
    {
        if (number < 0f)
            number = 0;
        set(number); // set already have multiplier
    }

    //TODO overflow checks?
    public void add(Value invalue)
    {
        value += invalue.value;
    }
    public void add(uint invalue)
    {
        value += invalue * precision;
    }
    public void add(float invalue)
    {
        if (invalue + value < 0)
        {
            Debug.Log("Value Add-float failed");
            set(0);
        }
        else
            value += (uint)Mathf.RoundToInt(invalue * precision);
    }
    public bool subtract(Value invalue)
    {
        if (invalue.value > value)
        {
            Debug.Log("Value subtract failed");
            set(0);
            return false;
        }
        else
        {
            value -= invalue.value;
            return true;
        }
    }
    public Value subtractOutside(Value invalue)
    {
        if (invalue.value > value)
        {
            Debug.Log("Value subtrackOutside failed");
            return new Value(0);
        }
        else
            return new Value(this.get() - invalue.get());
    }
    public void subtract(float invalue)
    {
        if (invalue > value)
            Debug.Log("Value subtract failed");
        value -= (uint)Mathf.RoundToInt(invalue * precision);
    }
    //public void multiple(Value invalue)
    //{
    //    if (invalue.get() < 0 )
    //        Debug.Log("Value multiple failed");
    //    value = (uint)(value * invalue.get());
    //}

    /// <summary>
    /// returns new value
    /// </summary>
    /// <param name="invalue"></param>
    /// <returns>new Value</returns>
    public Value multiple(Value invalue)
    {
        if (invalue.get() < 0)
            Debug.Log("Value multiple failed");
        return new Value(get() * invalue.get());
    }
    /// <summary>Keeps result inside</summary>    
    public void multipleInside(Value invalue)
    {
        if (invalue.get() < 0)
            Debug.Log("Value multiple failed");
        set(invalue.get() * this.get());
    }
    public void multipleInside(float invalue)
    {
        if (invalue < 0f)
            Debug.Log("Value multiple failed");
        set(invalue * this.get());
    }
    /// <summary>
    /// returns new value
    /// </summary>
    internal Value multiple(int invalue)
    {
        //if (invalue.get() < 0)
        //  Debug.Log("Value multiple failed");
        return new Value(get() * invalue);
    }
    public void divideInside(Value invalue)
    {
        if (invalue.get() < 0)
            Debug.Log("Value multiple failed");
        set(this.divide(invalue));
    }



    /// <summary>returns new value </summary>
    internal Value divide(int invalue)
    {
        if (invalue == 0) Debug.Log("Value divide by zero");

        return new Value(get() / invalue);
    }
    /// <summary>returns new value </summary>
    internal Value divide(Value invalue)
    {
        if (invalue.get() == 0) Debug.Log("Value divide by zero");

        return new Value(get() / invalue.get());
    }


    internal Procent HowMuchHaveOf(Value need)
    {
        if (need.value == 0)
            return new Procent(1f);
        else
            return Procent.makeProcent((int)this.value, (int)need.value);
    }
    public void send(Value another, uint amount)
    {
        if (this.get() >= amount)
        {
            this.subtract(amount);
            another.add(amount);
        }
        else Debug.Log("value payment failed");
    }
    public void send(Value another, float amount)
    {
        if (this.get() >= amount)
        {
            this.subtract(amount);
            another.add(amount);
        }
        else Debug.Log("value payment failed");
    }
    public void send(Value another, Value amount)
    {
        if (this.get() >= amount.get())
        {
            this.subtract(amount);
            another.add(amount);
        }
        else Debug.Log("value payment failed");
    }
    public bool canPay(Value HowMuch)
    {
        if (HowMuch.value >= this.value)
            return false;
        else return true;
    }
    public void sendAll(Value another)
    {
        another.add(this);
        this.set(0);
    }
    // toDO test that file properly
    public float get()
    {
        return (float)value / (float)precision; //TODO roundation fakup
    }
    virtual public void set(float invalue)
    {
        value = (uint)Mathf.RoundToInt(invalue * precision);
    }
    public void set(Value invalue)
    {
        value = invalue.value;
    }
    override public string ToString()
    {
        if (value > 0)
            return System.Convert.ToString(get());
        else return "0";
    }


}