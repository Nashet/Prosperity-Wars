using Nashet.EconomicSimulation;
using Nashet.Utils;
using System;
using UnityEngine;

namespace Nashet.ValueSpace
{
    public class MoneyView : ICopyable<Money>//IReadOnlyValue,
    {
        public static MoneyView Zero = new MoneyView(0m);
        protected decimal data;

        public MoneyView(decimal value, bool showMessageAboutNegativeValue = true)
        {
            if (value >= 0m)
                data = value;
            else if (showMessageAboutNegativeValue)
            {
                Debug.Log("Can't set negative value");
                data = 0;
            }
        }

        protected MoneyView(MoneyView value)
        {
            data = value.data;
        }

        public MoneyView(Storage value) : this((decimal)value.get())
        {
            if (value.Product != Product.Gold)
                throw new Exception("THAT IS NOT REAL GOLD");
        }

        public static MoneyView CovertFromGold(Storage stor)
        {
            return new MoneyView((decimal)stor.get());
        }

        public Money Copy()
        {
            return new Money(this);
        }

        public decimal Get()
        {
            return data;
        }

        public bool isBiggerOrEqual(MoneyView value)
        {
            return data >= value.data;
        }

        public bool isBiggerThan(MoneyView value)
        {
            return data > value.data;
        }

        //public bool isBiggerThan(MoneyView invalue, MoneyView barrier)
        //{
        //    throw new System.NotImplementedException();
        //}

        public bool IsEqual(MoneyView value)
        {
            return data == value.data;
        }

        public bool isNotZero()
        {
            return data != 0m;
        }

        public bool isZero()
        {
            return data == 0m;
        }

        public bool isSmallerOrEqual(MoneyView value)
        {
            return data <= value.data;
        }

        public bool isSmallerThan(MoneyView value)
        {
            return data < value.data;
        }

        public static string DecimalToString(decimal data)
        {
            //Game.devMode &&
            if (data < 0.001m && data != 0m && data > -0.001m)
                return (data * 1000m).ToString("N3") + " Gold bites";
            else
                return data.ToString("N3") + " Gold";
        }
        public override string ToString()
        {
            return DecimalToString(data);
        }

        //public Money Copy()
        //{
        //    return new Money(this);
        //}
    }
}