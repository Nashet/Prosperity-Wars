using System.Collections.Generic;
using Nashet.Conditions;

namespace Nashet.EconomicSimulation
{
    //public static class ReformExtensions
    //{
    //    public static bool isEnacted(this List<AbstractReform> list, AbstractReformValue reformValue)
    //    {
    //        foreach (var item in list)
    //            if (item.getValue() == reformValue)
    //                return true;
    //        return false;
    //    }
    //}

    public abstract class AbstractReformStepValue : AbstractReformValue
    {
        //private readonly int totalSteps;
        protected AbstractReformStepValue(string name, string indescription, int ID, DoubleConditionsList condition, int totalSteps)
            : base(name, indescription, ID, condition)
        {
        }
    }
}