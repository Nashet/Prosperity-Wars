using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nashet.EconomicSimulation;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.Utils
{
    public static class ToStringExtensions
    {
        public static string ToString(this Dictionary<Product, Storage> collection, String lineBreaker)
        {
            if (collection.Any(x => x.Value.isNotZero()))
            {
                var sb = new StringBuilder();
                bool isFirstRow = true;
                foreach (var item in collection)
                    if (item.Value.isNotZero())
                    {
                        if (!isFirstRow)
                            sb.Append(lineBreaker);
                        isFirstRow = false;
                        sb.Append(item.Value.get()).Append(" ").Append(item.Key);
                    }
                return sb.ToString();
            }
            else
                return "none";
        }

        public static string ToString(this IEnumerable<Storage> list, string lineBreaker)
        {
            if (list.Any(x => x.isNotZero()))
            {
                var sb = new StringBuilder();
                bool isFirstRow = true;
                bool haveAnyNonZeroItem = false;
                foreach (var item in list)
                    if (item.isNotZero())
                    {
                        haveAnyNonZeroItem = true;
                        if (!isFirstRow)
                        {
                            sb.Append(lineBreaker);
                        }
                        isFirstRow = false;
                        sb.Append(item);
                    }
                if (haveAnyNonZeroItem)
                    return sb.ToString();
                else
                    return "none";
            }
            else
                return "none";
        }

        /// <summary>
        /// Uses FullName instead of standard ToString()
        /// </summary>
        public static string ToString(this IEnumerable<Movement> source)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in source)
            {
                sb.Append("  ").Append(item.FullName).Append(" \n");
            }
            return sb.ToString();
        }

        public static string ToString<TValue>(this IEnumerable<KeyValuePair<TValue, Procent>> list, string lineBreaker, int howMuchStringsToShow)
        {
            var sb = new StringBuilder();
            //if (list.Count() <= howMuchStringsToShow || howMuchStringsToShow == 0)
            //{
            //    bool isFirstRow = true;
            //    foreach (var item in list)
            //    {
            //        if (!isFirstRow)
            //            sb.Append(lineBreaker);
            //        isFirstRow = false;
            //        sb.Append(item.Key).Append(": ").Append(item.Value);
            //    }
            //}
            //else  // there is at least howMuchStringsToShow + 1 elements

            bool isFirstRow = true;
            bool isAddedAnyStrings = false;
            //for (int i = 0; i < howMuchStringsToShow; i++)
            int i = 0;
            var othersSum = new Procent(0f);
            foreach (var item in list)
            {
                if (i < howMuchStringsToShow || howMuchStringsToShow == 0)
                {
                    if (!isFirstRow)
                        sb.Append(lineBreaker);
                    isFirstRow = false;
                    sb.Append(item.Key).Append(": ").Append(item.Value);
                    isAddedAnyStrings = true;
                }
                else
                    othersSum.Add(item.Value);
                //  break;
                i++;
            }

            //for (int i = howMuchStringsToShow; i < list.Count; i++)
            //    othersSum.Add(list[i].Value);
            if (othersSum.isNotZero())
            {
                sb.Append(lineBreaker);
                sb.Append("Others: ").Append(othersSum);
            }

            if (isAddedAnyStrings)
                return sb.ToString();
            else

                return "none";
        }

        public static string ToString<TValue>(this IEnumerable<TValue> list, string lineBreaker)
        {
            if (list.Count() > 0)
            {
                var sb = new StringBuilder();
                bool isFirstRow = true;
                foreach (var item in list)
                {
                    if (!isFirstRow)
                    {
                        sb.Append(lineBreaker);
                    }
                    isFirstRow = false;
                    sb.Append(item);
                }
                return sb.ToString();
            }
            else
                return "none";
        }

        //public static string ToString<T, V>(this KeyValuePair<T, V> source, string separator)
        //{
        //    return source.Key + separator + source.Value;
        //}
        public static string ToString<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, string intermediateString, string lineBreaker)
        {
            if (source.Count() == 0)
                return "none";
            var sb = new StringBuilder();
            bool isFirstRow = true;
            foreach (var item in source)
            {
                if (!isFirstRow)
                {
                    sb.Append(lineBreaker);
                }
                isFirstRow = false;
                sb.Append(item.Key).Append(intermediateString).Append(item.Value);
            }
            return sb.ToString();
        }

        public static string ToString(this KeyValuePair<IWayOfLifeChange, int> source, PopUnit pop)
        {
            var sb = new StringBuilder();
            if (source.Key == null)
                if (source.Value > 0)
                    sb.Append("born ");
                else
                    sb.Append("starved to death ");
            else
            {
                String direction;
                if (source.Value > 0)
                    direction = " from ";
                else
                    direction = " to ";

                var isProvince = source.Key as Province;
                if (isProvince != null)
                {
                    if (pop == null)
                        sb.Append("moved").Append(direction).Append(isProvince.FullName);
                    else if (pop.Country == isProvince.Country)
                        sb.Append("migrated").Append(direction).Append(isProvince.ShortName);
                    else
                        sb.Append("immigrated").Append(direction).Append(isProvince.FullName);
                }
                else
                {
                    var isType = source.Key as PopType;
                    if (isType != null)
                    {
                        if (pop == null)
                            sb.Append("converted").Append(direction).Append(isType);
                        else
                        {
                            if (source.Value > 0) // we gain population
                            {
                                if (isType.CanDemoteTo(pop.Type, pop.Country))
                                    //if (pop.canThisDemoteInto(isType))
                                    sb.Append("demoted").Append(direction).Append(isType);
                                else
                                    sb.Append("promoted").Append(direction).Append(isType);
                            }
                            else
                            {
                                // assumed that If pop can demote into something than it can't promote into that
                                if (pop.Type.CanDemoteTo(isType, pop.Country))
                                    sb.Append("demoted").Append(direction).Append(isType);
                                else
                                    sb.Append("promoted").Append(direction).Append(isType);
                            }
                        }
                    }
                    else
                    {
                        var isCulture = source.Key as Culture;
                        if (isCulture != null)
                            sb.Append("assimilated").Append(direction).Append(isCulture);
                        else
                        {
                            var isStaff = source.Key as Staff;
                            if (isStaff != null)
                                sb.Append("killed in battle with ").Append(isStaff);
                            else
                                Debug.Log("Unknown WayOfLifeChange");
                        }
                    }
                }
            }
            return sb.ToString();
        }

        //public static string getString(this IEnumerable<IGrouping<IWayOfLifeChange, KeyValuePair<IWayOfLifeChange, int>>> source, string lineBreaker)
        public static string ToString(this IEnumerable<KeyValuePair<IWayOfLifeChange, int>> source, string lineBreaker, string totalString)
        {
            var sb = new StringBuilder();

            var query = source.GroupBy(
        toBeKey => toBeKey.Key,
        (group, element) => new
        {
            Key = group,
            Sum = element.Sum(everyElement => everyElement.Value)
            //.Sum(x => x.Value))
        }).OrderByDescending(x => Math.Abs(x.Sum));
            //only null or province
            if (query.Count() == 0)
                return "no changes";
            int total = 0;
            foreach (var item in query)
            {
                // value-migrated-to-key
                if (item.Sum != 0)
                {
                    if (item.Sum > 0)
                        sb.Append("+");
                    var toShow = new KeyValuePair<IWayOfLifeChange, int>(item.Key, item.Sum);
                    sb.Append(item.Sum).Append(" ").Append(toShow.ToString(null)).Append(lineBreaker);
                    total += item.Sum;
                }
            }
            //.Append(lineBreaker)
            sb.Append(totalString).Append(total);
            return sb.ToString();
        }

        public static string ToString(this IEnumerable<KeyValuePair<IWayOfLifeChange, int>> source, string lineBreaker, PopUnit pop, string totalString)
        {
            if (!source.Any(x => x.Value != 0))
                return "no changes";
            var sb = new StringBuilder();
            bool isFirstRow = true;
            int total = 0;
            foreach (var item in source.Reverse())
                if (item.Value != 0) // skip empty records
                {
                    if (!isFirstRow)
                        sb.Append(lineBreaker);
                    isFirstRow = false;
                    total += item.Value;

                    if (item.Value > 0)
                        sb.Append("+");

                    sb.Append(item.Value).Append(" ");
                    sb.Append(item.ToString(pop));
                }
            sb.Append(lineBreaker).Append(totalString).Append(total);
            return sb.ToString();
        }

        public static string ToString(IEnumerable<KeyValuePair<TemporaryModifier, Date>> dictionary)
        {
            if (dictionary.Count() == 0)
                return "none";
            var sb = new StringBuilder();
            bool isFirstRow = true;
            foreach (var item in dictionary)
            {
                if (!isFirstRow)
                {
                    sb.Append("\n");
                }
                isFirstRow = false;
                if (item.Value == null)
                    sb.Append(item.Key).Append(" (permanent)");
                else
                    sb.Append(item.Key).Append(" expires in ").Append(item.Value.getYearsUntill()).Append(" years");
            }
            return sb.ToString();
        }
    }
}