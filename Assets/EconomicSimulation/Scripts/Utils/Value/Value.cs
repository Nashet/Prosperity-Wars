using Nashet.Utils;
using UnityEngine;

namespace Nashet.ValueSpace
{
    public class Value : ReadOnlyValue, ICopyable<Value>
    {
        public Value(float number, bool showMessageAboutNegativeValue = true) : base(number, showMessageAboutNegativeValue)
        {
        }

        //protected
        public Value(ReadOnlyValue number) : base(number)
        {
            //   set(number); // set already have multiplier
        }

        /// <summary>
        /// Converts dirty float into Value format
        /// </summary>
        public static float Convert(float invalue)
        {
            uint intermediate = (uint)Mathf.RoundToInt(invalue * Precision);
            return (float)intermediate / (float)Precision;
        }

        //TODO overflow checks?
        public Value Add(ReadOnlyValue howMuch, bool showMessageAboutNegativeValue = true)
        {
            if (rawUIntValue + howMuch.RawUIntValue < 0f)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value Add-Value failed");
                Set(0);
            }
            else
                rawUIntValue += howMuch.RawUIntValue;
            return this;
        }

        public Value Add(float howMuch, bool showMessageAboutNegativeValue = true)
        {
            if (howMuch + get() < 0f)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value Add-float failed");
                Set(0);
            }
            else
                rawUIntValue += (uint)Mathf.RoundToInt(howMuch * Precision);
            return this;
        }

        public Value Subtract(ReadOnlyValue howMuch, bool showMessageAboutNegativeValue = true)
        {
            if (howMuch.RawUIntValue > rawUIntValue)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value subtract gave negative result");
                Set(0);
            }
            else
                rawUIntValue -= howMuch.RawUIntValue;
            return this;
        }

        public Value Subtract(float howMuch, bool showMessageAboutNegativeValue = true)
        {
            if (howMuch > get())
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value subtract failed");
                rawUIntValue = 0;
            }
            else
                rawUIntValue -= (uint)Mathf.RoundToInt(howMuch * Precision);
            return this;
        }

        /// <summary>Keeps result inside</summary>
        public Value Multiply(ReadOnlyValue howMuch, bool showMessageAboutNegativeValue = true)
        {
            //if (howMuch.get() < 0)
            //{
            //    if (showMessageAboutNegativeValue)
            //        Debug.Log("Value multiply failed");
            //    value = 0;
            //}
            //else
            Set(howMuch.get() * get());
            return this;
        }

        public Value Multiply(float howMuch, bool showMessageAboutNegativeValue = true)
        {
            if (howMuch < 0f)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value multiply failed");
                rawUIntValue = 0;
            }
            else
                Set(howMuch * get());
            return this;
        }

        /// <summary>Keeps result inside</summary>
        public Value Divide(ReadOnlyValue divider, bool showMessageAboutNegativeValue = true)
        {
            //if (invalue.get() <= 0)
            //{
            //    if (showMessageAboutNegativeValue)
            //        Debug.Log("Value divide failed");
            //    value = 99999;
            //}
            //else
            Set(rawUIntValue / (float)divider.RawUIntValue);
            return this;
        }

        public Value Divide(int divider, bool showMessageAboutNegativeValue = true)
        {
            if (divider <= 0)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value divide failed");
                rawUIntValue = 99999;
            }
            else
                Set(get() / (float)divider);
            return this;
        }

        //public void send(Value another, float amount, bool showMessageAboutOperationFails = true)
        //{
        //    if (this.get() >= amount)
        //    {
        //        this.subtract(amount);
        //        another.add(amount);
        //    }
        //    else
        //    {
        //        if (showMessageAboutOperationFails) Debug.Log("No enough value to send");
        //        sendAll(another);
        //    }

        //}
        //public bool send(Value another, ReadOnlyValue amount, bool showMessageAboutOperationFails = true)
        //{
        //    if (this.get() >= amount.get())
        //    {
        //        subtract(amount);
        //        another.Add(amount);
        //        return true;
        //    }
        //    else
        //    {
        //        if (showMessageAboutOperationFails)
        //            Debug.Log("No enough value to send");
        //        sendAll(another);
        //        return false;
        //    }
        //}

        public void SendAll(Value where)
        {
            where.Add(this);
            SetZero();
        }

        public void SetZero()
        {
            rawUIntValue = 0;
        }

        public void Set(float newValue, bool showMessageAboutOperationFails = true)
        {
            if (newValue >= 0f)
                rawUIntValue = (uint)Mathf.RoundToInt(newValue * Precision);
            else
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("Can't set negative value");
                rawUIntValue = 0;
            }
        }

        public void Set(ReadOnlyValue invalue)
        {
            rawUIntValue = invalue.RawUIntValue;
        }

        //public int Compare(Value x, Value y)
        //{
        //    if (x.value == y.value)
        //        return 0;
        //    else
        //        if (x.value > y.value)
        //        return -1;
        //    else
        //        return 1;
        //}
    }
}