using System.Collections.Generic;

namespace Nashet.ValueSpace
{
    /// <summary>
    /// represents procent value which can't go hire than 100%
    /// </summary>
    public class Procent100 : Procent
    {
        public Procent100(float number, bool showMessageAboutNegativeValue = true) : base(number, showMessageAboutNegativeValue)
        {
            clamp100();
        }

        public Procent100(List<Storage> numerator, List<Storage> denominator, bool showMessageAboutOperationFails = true) : base(numerator, denominator, showMessageAboutOperationFails)
        {
            clamp100();
        }

        public Procent100(StorageSet numerator, List<Storage> denominator, bool showMessageAboutOperationFails = true) : base(numerator, denominator, showMessageAboutOperationFails)
        {
            clamp100();
        }

        public Procent100(ReadOnlyValue numerator, ReadOnlyValue denominator, bool showMessageAboutOperationFails = true) : base(numerator, denominator, showMessageAboutOperationFails)
        {
            clamp100();
        }

        public Procent100(float numerator, float denominator, bool showMessageAboutOperationFails = true) : base(numerator, denominator, showMessageAboutOperationFails)
        {
            clamp100();
        }

        public Procent100(int numerator, int denominator, bool showMessageAboutOperationFails = true) : base(numerator, denominator, showMessageAboutOperationFails)
        {
            clamp100();
        }

        protected Procent100(Procent number) : base(number)
        {
            clamp100();
        }

        public void AddPoportionally(int totalValculatedValue, int nextElementValue, Procent elementProcent)
        {
            base.AddPoportionally(totalValculatedValue, nextElementValue, elementProcent);
            clamp100();
        }

        public Procent100 Add(float howMuch, bool showMessageAboutNegativeValue = true)
        {
            base.Add(howMuch, showMessageAboutNegativeValue);
            clamp100();
            return this;
        }

        public Procent100 Add(ReadOnlyValue howMuch, bool showMessageAboutNegativeValue = true)
        {
            base.Add(howMuch, showMessageAboutNegativeValue);
            clamp100();
            return this;
        }

        public Procent100 Subtract(ReadOnlyValue howMuch, bool showMessageAboutNegativeValue = true)
        {
            base.Subtract(howMuch, showMessageAboutNegativeValue);
            clamp100();
            return this;
        }

        public Procent100 Subtract(float howMuch, bool showMessageAboutNegativeValue = true)
        {
            base.Subtract(howMuch, showMessageAboutNegativeValue);
            clamp100();
            return this;
        }

        public Procent100 Multiply(ReadOnlyValue howMuch, bool showMessageAboutNegativeValue = true)
        {
            base.Multiply(howMuch, showMessageAboutNegativeValue);
            clamp100();
            return this;
        }

        public Procent100 Multiply(float howMuch, bool showMessageAboutNegativeValue = true)
        {
            base.Multiply(howMuch, showMessageAboutNegativeValue);
            clamp100();
            return this;
        }

        public Procent100 Divide(ReadOnlyValue divider, bool showMessageAboutOperationFails = true)
        {
            base.Divide(divider, showMessageAboutOperationFails);
            clamp100();
            return this;
        }

        public Procent100 Divide(int divider, bool showMessageAboutOperationFails = true)
        {
            base.Divide(divider, showMessageAboutOperationFails);
            clamp100();
            return this;
        }

        public Procent100 Set(float newValue, bool showMessageAboutOperationFails = true)
        {
            base.Set(newValue, showMessageAboutOperationFails);
            clamp100();
            return this;
        }
    }
}