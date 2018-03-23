using UnityEngine;

using Nashet.Utils;

namespace Nashet.ValueSpace
{
    public class MoneyView : ICopyable<Money>//IReadOnlyValue,
    {
        public static MoneyView Zero = new MoneyView(0m);
        protected decimal data;
        public MoneyView(decimal value, bool showMessageAboutNegativeValue = true)
        {
            if (value >= 0m)
                data = (decimal)value;
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
            if (value.Product != EconomicSimulation.Product.Gold)
                throw new System.Exception("THAT IS NOT REAL GOLD");
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

        public override string ToString()
        {
            if (EconomicSimulation.Game.devMode && data < 0.001m && data != 0m)
                return data.ToString("E") + " Gold";
            else
                return data.ToString("N3") + " Gold";
        }


        //public Money Copy()
        //{
        //    return new Money(this);
        //}
    }
}