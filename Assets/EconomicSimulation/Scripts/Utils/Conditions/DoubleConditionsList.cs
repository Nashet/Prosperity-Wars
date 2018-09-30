using System;
using System.Collections.Generic;
using System.ComponentModel;
using Nashet.Utils;

namespace Nashet.Conditions
{
    /// <summary>
    /// Supports second object to compare & dynamic string
    /// </summary>
    public class DoubleConditionsList : ConditionsList
    {
        protected List<Condition> listForSecondObject;

        /// <summary>
        /// basic constructor
        /// </summary>
        public DoubleConditionsList(List<Condition> inlist) : base(inlist)
        {
        }

        /// <summary>
        /// basic constructor
        /// </summary>
        public DoubleConditionsList(Condition condition) : base(condition)
        {
        }        

        /// <summary>
        /// copy constructor
        /// </summary>
        public DoubleConditionsList(ConditionsList conditionsList) : base(conditionsList)
        {
        }

        public DoubleConditionsList(params Condition[] conditions) : base(conditions)
        {
        }

        public DoubleConditionsList addForSecondObject(List<Condition> toAdd)
        {
            listForSecondObject = toAdd;
            return this;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("use isAllTrue(object firstObject, object secondObject) instead", false)]
        public bool isAllTrue(object forWhom, out string description)
        {
            throw new DontUseThatMethod();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("use isAllTrue(object firstObject, object secondObject) instead", false)]
        public bool isAllTrue(object forWhom)
        {
            throw new DontUseThatMethod();
        }

        /// <summary>Return false if any of conditions is false, supports two objects, also makes description</summary>
        public bool isAllTrue(object firstObject, object secondObject, out string description)
        {
            string accu;
            description = "";
            bool atLeastOneNoAnswer = false;
            foreach (var item in list)
            {
                var doubleCondition = item as DoubleCondition;
                if (doubleCondition == null)
                {
                    if (!item.checkIftrue(firstObject, out accu))
                        atLeastOneNoAnswer = true;
                }
                else
                {
                    if (!doubleCondition.checkIftrue(firstObject, secondObject, out accu))
                        atLeastOneNoAnswer = true;
                }
                description += accu;
            }
            if (listForSecondObject != null)
                foreach (var item in listForSecondObject)
                {
                    if (!item.checkIftrue(secondObject, out accu))
                        atLeastOneNoAnswer = true;
                    description += accu;
                }
            if (atLeastOneNoAnswer)
                return false;
            else
                return true;
        }

        /// <summary>Return false if any of conditions is false, supports two objects</summary>
        public bool isAllTrue(object firstObject, object secondObject)
        {
            foreach (var item in list)
            {
                var doubleCondition = item as DoubleCondition;
                if (doubleCondition == null)
                {
                    if (!item.checkIfTrue(firstObject))
                        return false;
                }
                else
                {
                    if (!doubleCondition.checkIftrue(firstObject, secondObject))
                        return false;
                }
            }
            if (listForSecondObject != null)
                foreach (var item in listForSecondObject)
                {
                    if (!item.checkIfTrue(secondObject))
                        return false;
                }
            return true;
        }
    }
}