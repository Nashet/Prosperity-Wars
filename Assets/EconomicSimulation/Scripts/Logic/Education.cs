using System.Collections.Generic;
using Nashet.Utils;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Education : Procent100, ICopyable<Education>
    {
        public Education(float number, bool showMessageAboutNegativeValue = true) : base(number, showMessageAboutNegativeValue)
        {
        }

        public Education(List<Storage> numerator, List<Storage> denominator, bool showMessageAboutOperationFails = true) : base(numerator, denominator, showMessageAboutOperationFails)
        {
        }

        public Education(StorageSet numerator, List<Storage> denominator, bool showMessageAboutOperationFails = true) : base(numerator, denominator, showMessageAboutOperationFails)
        {
        }

        public Education(ReadOnlyValue numerator, ReadOnlyValue denominator, bool showMessageAboutOperationFails = true) : base(numerator, denominator, showMessageAboutOperationFails)
        {
        }

        public Education(float numerator, float denominator, bool showMessageAboutOperationFails = true) : base(numerator, denominator, showMessageAboutOperationFails)
        {
        }

        public Education(int numerator, int denominator, bool showMessageAboutOperationFails = true) : base(numerator, denominator, showMessageAboutOperationFails)
        {
        }

        protected Education(Procent number) : base(number)
        {
        }

        public void Learn()
        {
            if (Rand.Chance(HundredProcent.Copy().Subtract(this)))
                Add(Options.PopEducationGrowthRate);
        }

        public Education Copy()
        {
            return new Education(this);
        }
    }
}