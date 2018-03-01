using UnityEngine;
using Nashet.Utils;

namespace Nashet.ValueSpace
{
    public class ReadOnlyValue: ICopyable<Value>
    {
        ///<summary> storing as value as number * precision </summary>
        protected uint rawUIntValue;
        public uint RawUIntValue
        {
            get { return rawUIntValue; }
        }
        internal readonly static int Precision = 1000; // 0.001
        internal static readonly ReadOnlyValue Zero = new ReadOnlyValue(0);
        internal static readonly ReadOnlyValue Max999 = new ReadOnlyValue(999.999f);
        internal static readonly ReadOnlyValue Max = new ReadOnlyValue(int.MaxValue / Precision);

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
            return this.rawUIntValue > invalue.rawUIntValue;
        }
        public bool IsEqual(ReadOnlyValue invalue)
        {
            return this.rawUIntValue == invalue.rawUIntValue;
        }
        /// <summary>
        /// Returns true if bigger than argument + barrier
        /// </summary>
        public bool isBiggerThan(ReadOnlyValue invalue, ReadOnlyValue barrier)
        {
            return this.rawUIntValue > invalue.rawUIntValue + barrier.rawUIntValue;
        }
        public bool isBiggerOrEqual(ReadOnlyValue invalue)
        {
            return this.rawUIntValue >= invalue.rawUIntValue;
        }
        public bool isSmallerThan(ReadOnlyValue invalue)
        {
            return this.rawUIntValue < invalue.rawUIntValue;
        }
        public bool isSmallerOrEqual(ReadOnlyValue invalue)
        {
            return this.rawUIntValue <= invalue.rawUIntValue;
        }
        // toDO test that file properly
        public float get()
        {
            return (float)rawUIntValue / (float)Precision; //TODO roundation fakup
        }
        override public string ToString()
        {
            if (rawUIntValue > 0)
                return System.Convert.ToString(get());
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
        internal Procent HowMuchHaveOf(Value need)
        {
            if (need.rawUIntValue == 0)
                return new Procent(1f);
            else
                return new Procent((int)this.rawUIntValue, (int)need.rawUIntValue);
        }
        public Value Copy()
        {            
            return new Value(this);
        }
    }
}