//using System;

using System;
using System.Collections.Generic;
using Nashet.Utils;
using UnityEngine;

//using System.Linq;

namespace Nashet.ValueSpace
{
    public class Procent : Value, ICopyable<Procent>
    {
        public static readonly Procent HundredProcent = new Procent(1f);
        public static readonly Procent _50Procent = new Procent(0.5f);
        public static readonly Procent ZeroProcent = new Procent(0f);
        public static readonly Procent Max999 = new Procent(999.999f);
        //public static readonly Procent Max = new Procent(int.MaxValue / 1000f);

        public Procent(float number, bool showMessageAboutNegativeValue = true) : base(number, showMessageAboutNegativeValue)
        {
        }

        protected Procent(Procent number) : base(number)
        {
        }

        public Procent(List<Storage> numerator, List<Storage> denominator, bool showMessageAboutOperationFails = true)
            : this(numerator.Sum(), denominator.Sum(), showMessageAboutOperationFails) { }

        public Procent(List<Storage> numerator, IEnumerable<Storage> denominator, bool showMessageAboutOperationFails = true)
            : this(numerator.Sum(), denominator.Sum(), showMessageAboutOperationFails) { }

        public Procent(StorageSet numerator, List<Storage> denominator, bool showMessageAboutOperationFails = true)
            : this(numerator.GetTotalQuantity(), denominator.Sum(), showMessageAboutOperationFails) { }

        public Procent(StorageSet numerator, IEnumerable<Storage> denominator, bool showMessageAboutOperationFails = true)
            : this(numerator.GetTotalQuantity(), denominator.Sum(), showMessageAboutOperationFails) { }

        public Procent(IEnumerable<Storage> numerator, IEnumerable<Storage> denominator, bool showMessageAboutOperationFails = true)
            : this(numerator.Sum(), denominator.Sum(), showMessageAboutOperationFails) { }

        public Procent(ReadOnlyValue numerator, ReadOnlyValue denominator, bool showMessageAboutOperationFails = true)
            : this(numerator.get(), denominator.get(), showMessageAboutOperationFails) { }

        public Procent(float numerator, float denominator, bool showMessageAboutOperationFails = true) : base(0f)
        {
            if (denominator == 0f)
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("Division by zero in new Procent(float)");
                Set(Max999);
            }
            else
                Set(numerator / denominator, showMessageAboutOperationFails);
        }

        public Procent(int numerator, int denominator, bool showMessageAboutOperationFails = true) : base(0f)
        {
            if (denominator == 0)
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("Division by zero in Percent.makeProcent(int)");
                Set(Max999);
            }
            else
                Set(numerator / (float)denominator, showMessageAboutOperationFails);
        }

        public Procent(MoneyView numerator, MoneyView denominator, bool showMessageAboutOperationFails = true) : base(0f)
        {
            if (denominator.isZero())
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("Division by zero in Percent.makeProcent(int)");
                Set(Max999);
            }
            else
                Set((float)(numerator.Get() / denominator.Get()), showMessageAboutOperationFails);
        }

        public float get50Centre()
        {
            return get() - 0.5f;
        }

        //public Procent add(Procent pro, bool showMessageAboutNegativeValue = true)
        //{
        //    base.Add(pro, showMessageAboutNegativeValue);
        //    return this;
        //}
        /// <summary>
        /// Calculates procent proportionally to int sizes of elements
        /// </summary>
        public void AddPoportionally(int totalValculatedValue, int nextElementValue, Procent elementProcent)
        {
            if ((totalValculatedValue + nextElementValue) != 0)
                Set(
                    (get() * totalValculatedValue + elementProcent.get() * nextElementValue) / (float)(totalValculatedValue + nextElementValue)
                    );
        }

        public override string ToString()
        {
            return (get()*100f).ToString("f1") + "%";
            //return get().ToString("P1");
            //return String.Format("{0:1%}", get());
        }

        /// <summary>
        /// new value
        /// </summary>
        public int getProcentOf(int value)
        {
            return Mathf.RoundToInt(get() * value);
        }

        //override public void set(float invalue)
        //{
        //    if (invalue < 0f)
        //        base.set(0f);
        //    //else
        //    //    if (invalue > 1f)
        //    //    base.set(1f);
        //    else
        //        base.set(invalue);
        //}

        public void clamp100()
        {
            if (isBiggerThan(HundredProcent))
                Set(1f);
        }

        [Obsolete("Don't use that for Procent")]
        public void SendAll(Value where)
        { }

        public Procent Copy()
        {
            return new Procent(this);
        }

        public Procent Subtract(ReadOnlyValue howMuch, bool showMessageAboutNegativeValue = true)
        {
            return base.Subtract(howMuch, showMessageAboutNegativeValue) as Procent;
        }
    }
}