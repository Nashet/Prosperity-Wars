using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Nashet.Utils
{
    /// <summary>
    /// Remembers and gives next element. Not used now
    /// </summary>    
    public class ExtendedList<T> : List<T>
    {
        private int nextElement = -1;
        public T Next
        {
            get
            {
                nextElement++;
                if (nextElement == this.Count)
                    nextElement = 0;
                return this[nextElement];
            }
        }
    }
}
