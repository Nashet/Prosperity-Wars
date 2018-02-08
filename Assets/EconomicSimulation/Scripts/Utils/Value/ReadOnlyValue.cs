using UnityEngine;
using Nashet.Utils;

namespace Nashet.ValueSpace
{
    public class ReadOnlyValue: ICopyable<Value>
    {
        ///<summary> storing as value as number * precision </summary>
        protected uint value;
        public uint RawValue
        {
            get { return value; }
        }
        internal readonly static uint precision = 1000; // 0.001
        internal static readonly ReadOnlyValue Zero = new ReadOnlyValue(0);
        internal static readonly ReadOnlyValue Max999 = new ReadOnlyValue(999.999f);
        internal static readonly ReadOnlyValue Max = new ReadOnlyValue(int.MaxValue / precision);

        public ReadOnlyValue(float number, bool showMessageAboutOperationFails = true)
        {
            if (number >= 0)
                value = (uint)Mathf.RoundToInt(number * precision);
            else
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("Can't set negative value");
                value = 0;
            }
        }
        protected ReadOnlyValue(ReadOnlyValue number) 
        {
            value = number.value;
        }
        public bool isBiggerThan(ReadOnlyValue invalue)
        {
            return this.value > invalue.value;
        }
        public bool IsEqual(ReadOnlyValue invalue)
        {
            return this.value == invalue.value;
        }
        /// <summary>
        /// Returns true if bigger than argument + barrier
        /// </summary>
        public bool isBiggerThan(ReadOnlyValue invalue, ReadOnlyValue barrier)
        {
            return this.value > invalue.value + barrier.value;
        }
        public bool isBiggerOrEqual(ReadOnlyValue invalue)
        {
            return this.value >= invalue.value;
        }
        public bool isSmallerThan(ReadOnlyValue invalue)
        {
            return this.value < invalue.value;
        }
        public bool isSmallerOrEqual(ReadOnlyValue invalue)
        {
            return this.value <= invalue.value;
        }
        // toDO test that file properly
        public float get()
        {
            return (float)value / (float)precision; //TODO roundation fakup
        }
        override public string ToString()
        {
            if (value > 0)
                return System.Convert.ToString(get());
            else
                return "0";
        }
        /// <summary>
        /// Bigger than 0
        /// </summary>        
        public bool isNotZero()
        {
            return value > 0;
        }
        public bool isZero()
        {
            return value == 0;
        }
        // new value
        internal Procent HowMuchHaveOf(Value need)
        {
            if (need.value == 0)
                return new Procent(1f);
            else
                return new Procent((int)this.value, (int)need.value);
        }
        public Value Copy()
        {            
            return new Value(this);
        }
    }
}