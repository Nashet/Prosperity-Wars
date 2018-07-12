using System;
using Nashet.Utils;
using UnityEngine;

namespace Nashet.ValueSpace
{
    public class ReadOnlyValue : ICopyable<Value>, IReadOnlyValue
    {
        ///<summary> storing as value as number * precision </summary>
        protected uint rawUIntValue;

        public uint RawUIntValue
        {
            get { return rawUIntValue; }
        }

        public static readonly int Precision = 1000; // 0.001
        public static readonly ReadOnlyValue Zero = new ReadOnlyValue(0);
        public static readonly ReadOnlyValue Max999 = new ReadOnlyValue(999.999f);
        public static readonly ReadOnlyValue Max = new ReadOnlyValue(int.MaxValue / (float)Precision);

        public ReadOnlyValue(float number, bool showMessageAboutOperationFails = true)
        {
            if (number >= 0)
                rawUIntValue = (uint)Mathf.RoundToInt(number * Precision);
            else
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("Can't set negative value");
                rawUIntValue = 0;
            }
        }

        protected ReadOnlyValue(ReadOnlyValue number)
        {
            rawUIntValue = number.rawUIntValue;
        }

        public bool isBiggerThan(ReadOnlyValue invalue)
        {
            return rawUIntValue > invalue.rawUIntValue;
        }

        public bool IsEqual(ReadOnlyValue invalue)
        {
            return rawUIntValue == invalue.rawUIntValue;
        }

        /// <summary>
        /// Returns true if bigger than argument + barrier
        /// </summary>
        public bool isBiggerThan(ReadOnlyValue invalue, ReadOnlyValue barrier)
        {
            return rawUIntValue > invalue.rawUIntValue + barrier.rawUIntValue;
        }

        public bool isBiggerOrEqual(ReadOnlyValue invalue)
        {
            return rawUIntValue >= invalue.rawUIntValue;
        }

        public bool isSmallerThan(ReadOnlyValue invalue)
        {
            return rawUIntValue < invalue.rawUIntValue;
        }

        public bool isSmallerOrEqual(ReadOnlyValue invalue)
        {
            return rawUIntValue <= invalue.rawUIntValue;
        }

        // toDO test that file properly
        public float get()
        {
            return (float)rawUIntValue / (float)Precision; //TODO roundation fakup
        }

        public override string ToString()
        {
            if (rawUIntValue > 0)
                return Convert.ToString(get());
            else
                return "0";
        }

        /// <summary>
        /// Bigger than 0
        /// </summary>
        public bool isNotZero()
        {
            return rawUIntValue > 0;
        }

        public bool isZero()
        {
            return rawUIntValue == 0;
        }

        // new value
        public Procent HowMuchHaveOf(Value need)
        {
            if (need.rawUIntValue == 0)
                return new Procent(1f);
            else
                return new Procent((int)rawUIntValue, (int)need.rawUIntValue);
        }

        public Value Copy()
        {
            return new Value(this);
        }
    }
}