using Nashet.Utils;
using System;
using UnityEngine;

namespace Nashet.ValueSpace
{
    public class Money : MoneyView, ICopyable<Money>
    {
        public Money(decimal value, bool showMessageAboutNegativeValue = true) : base(value, showMessageAboutNegativeValue)
        { }

        public Money(Storage value) : this((decimal)value.get())
        {
            if (value.Product != EconomicSimulation.Product.Gold)
                throw new Exception("THAT IS NOT REAL GOLD");
        }

        public Money(MoneyView value) : base(value)
        { }

        //public Money Copy()
        //{
        //    return new Money(this);
        //}
        public Money Divide(decimal divider, bool showMessageAboutNegativeValue = true)
        {
            if (divider == 0m)
            {
                Debug.Log("Can't divide by zero");
                data = 99999m;
                return this;
            }
            else
                return Multiply(1m / divider, showMessageAboutNegativeValue);
        }

        public Money Multiply(Procent multiplier, bool showMessageAboutNegativeValue = true)
        {
            Multiply((decimal)multiplier.get());
            return this;
        }

        public Money Multiply(decimal multiplier, bool showMessageAboutNegativeValue = true)
        {
            if (multiplier < 0m)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value multiply failed");
                SetZero();
            }
            else
                data = multiplier * Get();
            return this;
        }

        ///////////////////Add section
        public Money Add(MoneyView storage, bool showMessageAboutNegativeValue = true)
        {
            decimal newData = data + storage.Get();
            if (newData < 0m)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Money can't be negative");
                data = 0m;
            }
            else
                data = newData;
            return this;
        }

        public Money Add(decimal adding, bool showMessageAboutNegativeValue = true)
        {
            decimal newData = data + adding;
            if (newData < 0m)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Money can't be negative");
                data = 0m;
            }
            else
                data = newData;
            return this;
        }

        public Money Subtract(MoneyView storage, bool showMessageAboutNegativeValue = true)
        {
            if (storage.isBiggerThan(this))
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Money subtract failed");
                SetZero();
            }
            else
                data = Get() - storage.Get();
            return this;
        }

        public void Set(MoneyView value)
        {
            data = (value.Copy()).data;// shit
        }

        //public void Set(float value)
        //{
        //    this.data = (value .Copy()).data;
        //}
        public void SetZero()
        {
            data = 0;
        }
    }
}