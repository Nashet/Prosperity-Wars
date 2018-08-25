using System;

namespace Nashet.Utils
{
    public class CashedData<T>
    {
        private readonly Date lastRecalculationDate = new Date(Date.Never);
        private readonly Func<T> calculationMethod;
        private T lastResult;

        public CashedData(Func<T> method)
        {
            this.calculationMethod = method;
        }

        public T Get()
        {
            if (lastRecalculationDate.IsToday)
                return lastResult;
            else
            {
                Recalculate();
                return lastResult;
            }
        }
        public void Recalculate()
        {
            lastRecalculationDate.set(Date.Today);
            lastResult = calculationMethod();            
        }

        public override string ToString()
        {
            return Get().ToString();
        }
    }
}