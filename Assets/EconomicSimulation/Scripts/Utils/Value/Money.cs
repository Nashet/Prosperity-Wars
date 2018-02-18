using UnityEngine;
using Nashet.EconomicSimulation;
using Nashet.Utils;
using System;

namespace Nashet.ValueSpace
{
    public class Money : Storage, ICopyable<Money>
    {
        public Money(float value) : base(Product.Gold, value)
        { }
        protected Money(Money value) : base(value)
        { }
        private Money(Storage value) : base(value)
        {
            if (value.Product != Product.Gold)
                Debug.Log("THAT IS NOT REAL GOLD"); 
        }


        public Money Copy()
        {
            return new Money(this);
        }
        public static Money CovertFromGold(Storage stor)
        {
            return new Money(stor);
        }
        internal Money Divide(int divider, bool showMessageAboutNegativeValue = true)
        {
            if (divider == 0)
            {
                Debug.Log("Can't divide by zero");
                Set(99999);
                return this;
            }
            else
                return this.Multiply(1f / divider, showMessageAboutNegativeValue);
        }
        public Money Multiply(Procent multiplier, bool showMessageAboutNegativeValue = true)
        {
            Multiply(multiplier.get());
            return this;
        }
        public Money Multiply(int multiplier, bool showMessageAboutNegativeValue = true)
        {
            return Multiply((float)multiplier, showMessageAboutNegativeValue);
        }
        public Money Multiply(float multiplier, bool showMessageAboutNegativeValue = true)
        {
            if (multiplier < 0f)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value multiply failed");
                Set(0);
            }
            else
                Set(multiplier * this.get());
            return this;
        }
        ///////////////////Add section
        public Money Add(float adding, bool showMessageAboutNegativeValue = true)
        {
            if (adding + get() < 0f)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Money can't be negative");
                Set(0);
            }
            else
                Set(get() + adding);
            return this;
        }

        
    }
}