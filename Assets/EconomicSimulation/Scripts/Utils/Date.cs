using UnityEngine;
using UnityEditor;
namespace Nashet.Utils
{
    /// <summary>
    /// Hand made class to work with game date
    /// </summary>
    public class Date
    {
        internal static readonly Date Never = new Date(int.MinValue);
        static private Date today = new Date(0);
        static public Date Today
        {
            get { return today; }
        }

        public bool IsToday
        {
            get { return year == today.year; }
        }

        private int year;
        public Date(int year)
        {
            this.year = year;
        }
        public Date(Date date)
        {
            this.year = date.year;
        }
        public static void Simulate()
        {
            today.AddTick(1);
        }
        protected void AddTick(int v)
        {
            year += v;
        }

        internal Date getNewDate(int v)
        {
            return new Date(year + v);
        }
        /// <summary>
        /// How much time passed after stored here date
        /// </summary>    
        public int getYearsSince()
        {
            return today.year - this.year;
        }
        /// <summary>
        /// How much time before that date come
        /// </summary>    
        public int getYearsUntill()
        {
            return this.year - today.year;
        }
        /// <summary>
        /// Returns true if exactly passed years has passed, no more no less
        /// </summary>    
        public bool isDivisible(int passed)
        {
            return this.year % passed == 0;
        }
        public bool isDatePassed()
        {
            return today.year > this.year;
        }

        internal void set(Date newDate)
        {
            // Debug.Log("date set to "+ newDate.year);
            this.year = newDate.year;
        }
        //public static bool operator ==(Date d1, Date d2)
        //{

        //    if (object.ReferenceEquals(d1, null) && object.ReferenceEquals(d2, null)) // both null
        //        return true;
        //    else
        //    {
        //        if (object.ReferenceEquals(d1, null) || object.ReferenceEquals(d2, null))   //one null
        //            return false;
        //    }
        //    //no null
        //    return d1.year == d2.year;
        //}
        //public static bool operator !=(Date d1, Date d2)
        //{
        //    if (object.ReferenceEquals(d1, null) && object.ReferenceEquals(d2, null)) // both null
        //        return false;
        //    else
        //    {
        //        if (object.ReferenceEquals(d1, null) || object.ReferenceEquals(d2, null))   //one null
        //            return true;
        //    }
        //    //no null
        //    return d1.year != d2.year;
        //}
        public override string ToString()
        {
            return year.ToString();
        }
    }
}