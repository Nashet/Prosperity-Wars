using System.Collections.Generic;
using System.Text;

namespace Nashet.Conditions
{
    public class ModifiersList : ConditionsList
    {
        // basic constructor
        public ModifiersList(List<Condition> inlist) : base(inlist)
        {
        }

        //short constructor, allowing predicates of several types to be checked
        //public ModifiersList(List<AbstractCondition> inlist) : base(inlist)
        //{
        //}
        //public static ConditionsList AlwaysYes = new ConditionsList(new List<Condition>() { new Condition(delegate (Country forWhom) { return 2 == 2; }, "Always Yes condition", true) });
        //public static ConditionsList IsNotImplemented = new ConditionsList(new List<Condition>() { new Condition(delegate (Country forWhom) { return 2 == 0; }, "Feature is implemented", true) });

        public float getModifier(object forWhom, out string description)
        {
            StringBuilder text = new StringBuilder();
            //text.Clear();
            float summ = 0f;
            string accu;
            foreach (Modifier item in list)
            {
                summ += item.getModifier(forWhom, out accu);
                //if (item.checkIftrue(forWhom, out accu))

                if (accu != "")
                {
                    text.Append(accu);
                }
            }
            text.Append("\nTotal: ").Append(summ);
            //text.
            description = text.ToString();
            return summ;
        }

        public float getModifier(object forWhom)
        {
            float summ = 0f;
            foreach (Modifier item in list)
                summ += item.getModifier(forWhom);
            return summ;
        }

        //public float find(AbstractReform reformValue)
        //{
        //    var foundModifier = list.Find(x => reformValue.allowed.contains(x)) as Modifier;
        //    if (foundModifier == null)
        //        return 0f;
        //    else
        //        return foundModifier.getValue();
        //}

        public string GetDescription(object forWhom)
        {
            string res;
            getModifier(forWhom, out res);
            return res;
        }
    }
}