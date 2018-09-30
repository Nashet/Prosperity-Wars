using System.Collections.Generic;
using System.Linq;

namespace Nashet.Conditions
{
    public class ConditionsList
    {
        //public readonly static ConditionsList AlwaysYes = new ConditionsList(new List<Condition> { new Condition(x => 2 * 2 == 4, "Always Yes condition", true) });
        //public readonly static ConditionsList IsNotImplemented = new ConditionsList(new List<Condition> { Condition.IsNotImplemented });

        protected List<Condition> list;

        /// <summary>
        /// Only for descendants
        /// </summary>
        public ConditionsList(params Condition[] conditions)
        {
            list = conditions.ToList();
        }

        /// <summary>
        /// basic constructor
        /// </summary>
        public ConditionsList(List<Condition> inlist)
        {
            list = inlist;
        }

        /// <summary>
        /// basic constructor
        /// </summary>
        public ConditionsList(Condition condition)
        {
            list = new List<Condition> { condition };
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        public ConditionsList(ConditionsList conditionsList)
        {
            list = new List<Condition>(conditionsList.list);
        }

        public void add(Condition condition)
        {
            list.Add(condition);
        }

        public bool contains(Condition condition)
        {
            return list.Contains(condition);
        }

        /// <summary>Return false if any of conditions is false, also makes description</summary>
        public bool isAllTrue(object forWhom, out string description)
        {
            string accu;
            description = "";
            bool atLeastOneNoAnswer = false;
            foreach (var item in list)
            {
                if (!item.checkIftrue(forWhom, out accu))
                    atLeastOneNoAnswer = true;
                description += accu;
            }
            if (atLeastOneNoAnswer)
                return false;
            else
                return true;
        }

        /// <summary>Return false if any of conditions is false</summary>
        public bool isAllTrue(object forWhom)
        {
            foreach (var item in list)
                if (!item.checkIfTrue(forWhom))
                    return false;
            return true;
        }
    }
}