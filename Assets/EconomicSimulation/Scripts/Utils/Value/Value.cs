using System;
using UnityEngine;
using Nashet.Utils;
namespace Nashet.ValueSpace
{

    public class Value : ReadOnlyValue, ICopyable<Value>
    {
        public Value(float number, bool showMessageAboutNegativeValue = true) : base(number, showMessageAboutNegativeValue)
        {            
        }
        //protected
        public Value(ReadOnlyValue number):base (number)
        {
         //   set(number); // set already have multiplier
        }
        /// <summary>
        /// Converts dirty float into Value format
        /// </summary>        
        public static float Convert(float invalue)
        {
            uint intermediate = (uint)Mathf.RoundToInt(invalue * precision);
            return (float)intermediate / (float)precision;
        }

        //TODO overflow checks?
        virtual public Value Add(ReadOnlyValue invalue, bool showMessageAboutNegativeValue = true)
        {
            if (value + invalue.RawValue < 0f)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value Add-Value failed");
                set(0);
            }
            else
                value += invalue.RawValue;
            return this;
        }

        virtual public Value add(float invalue, bool showMessageAboutNegativeValue = true)
        {
            if (invalue + get() < 0f)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value Add-float failed");
                set(0);
            }
            else
                value += (uint)Mathf.RoundToInt(invalue * precision);
            return this;
        }
        
        public Value subtract(ReadOnlyValue invalue, bool showMessageAboutNegativeValue = true)
        {
            if (invalue.RawValue > value)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value subtract gave negative result");
                set(0);
                return this;
            }
            else
            {
                value -= invalue.RawValue;
                return this;
            }
        }
        public void subtract(float invalue, bool showMessageAboutNegativeValue = true)
        {
            if (invalue > get())
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value subtract failed");
                value = 0;
            }
            else
                value -= (uint)Mathf.RoundToInt(invalue * precision);
        }
        

        /// <summary>Keeps result inside</summary>    
        public Value multiply(ReadOnlyValue invalue, bool showMessageAboutNegativeValue = true)
        {
            if (invalue.get() < 0)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value multiply failed");
                value = 0;
            }
            else
                set(invalue.get() * this.get());
            return this;
        }

        public Value multiply(float invalue, bool showMessageAboutNegativeValue = true)
        {
            if (invalue < 0f)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value multiply failed");
                value = 0;
            }
            else
                set(invalue * this.get());
            return this;
        }
        
        /// <summary>Keeps result inside</summary>    
        public Value divide(ReadOnlyValue invalue, bool showMessageAboutNegativeValue = true)
        {
            if (invalue.get() <= 0)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value divide failed");
                value = 99999;
            }
            else
                set(this.value / (float)invalue.RawValue);
            return this;
        }

        internal Value divide(int v, bool showMessageAboutNegativeValue = true)
        {
            if (v <= 0)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value divide failed");
                value = 99999;
            }
            else
                set(this.get() / (float)v);
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
        
        public void sendAll(Value another)
        {
            another.Add(this);
            this.setZero();
        }
        

        internal void setZero()
        {
            value = 0;
        }

        public void set(float invalue, bool showMessageAboutOperationFails = true)
        {
            if (invalue >= 0)
                value = (uint)Mathf.RoundToInt(invalue * precision);
            else
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("Can't set negative value");
                value = 0;
            }
        }
        public void set(ReadOnlyValue invalue)
        {
            value = invalue.RawValue;
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