using System;

namespace Nashet.Conditions
{
    /// <summary>
    /// Supports second object to compare & dynamic string
    /// </summary>
    public class DoubleCondition : Condition
    {
        protected readonly Func<object, object, bool> checkingFunctionForTwoObjects;

        /// <summary>
        /// Supports second object to compare & dynamic string
        /// </summary>
        public DoubleCondition(Func<object, object, bool> checkingFunctionForTwoObjects, Func<object, string> dynamicString, bool showAchievedConditionDescribtion)
            : base(null, dynamicString, showAchievedConditionDescribtion)
        {
            this.checkingFunctionForTwoObjects = checkingFunctionForTwoObjects;
        }

        /// <summary>Returns bool result and description in out description, supports two objects</summary>
        public bool checkIftrue(object firstObject, object secondObject, out string description)
        {
            if (changeTargetObject != null)
                firstObject = changeTargetObject(firstObject);
            string result = null;
            bool answer = false;
            if (checkingFunctionForTwoObjects(firstObject, secondObject))
            {
                if (showAchievedConditionDescribtion) result += "\n(+) " + getName(firstObject);
                //if (base.getName() == null)
                //{
                //    //if (dynamicStringTarget == null)
                //    result += dynamicString(firstObject);
                //    //else
                //    // return dynamicString(dynamicStringTarget);
                //}
                //else
                //    result += name;
                answer = true;
            }
            else
            {
                result += "\n(-) " + getName(firstObject);
                answer = false;
            }
            description = result;
            return answer;
        }

        /// <summary>Returns bool result, fast version, without description, supports two objects</summary>
        public bool checkIftrue(object firstObject, object secondObject)
        {
            if (changeTargetObject != null)
                firstObject = changeTargetObject(firstObject);
            return checkingFunctionForTwoObjects(firstObject, secondObject);
        }
    }
}