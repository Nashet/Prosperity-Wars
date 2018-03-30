using System;

namespace Nashet.Utils
{
    public class CashedData<T>
    {
        private readonly Date lastRecalculationDate = new Date(Date.Never);
        private readonly Func<T> method;
        private T lastResult;

        public CashedData(Func<T> method)
        {
            this.method = method;
        }

        public T Get()
        {
            if (lastRecalculationDate.IsToday)
                return lastResult;
            else
            {
                lastRecalculationDate.set(Date.Today);
                lastResult = method();
                return lastResult;
            }
        }
    }
}