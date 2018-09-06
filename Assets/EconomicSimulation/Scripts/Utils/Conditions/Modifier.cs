using System;
using System.Text;

namespace Nashet.Conditions
{
    public class Modifier : Condition
    {
        public static readonly Modifier modifierDefault1 = new Modifier(x => true, "Default", 1f, true);
        public static readonly Modifier modifierDefault100 = new Modifier(x => true, "Default", 100f, true);
        private readonly float value;
        private readonly Func<int> multiplierModifierFunction;
        private readonly Func<object, float> floatModifierFunction;
        private readonly bool showZeroModifiers;

        /// <summary>
        /// regular modifier
        /// </summary>
        public Modifier(Func<object, bool> myMethodName, string conditionIsTrue, float value, bool showZeroModifiers) : base(myMethodName, conditionIsTrue, true)
        {
            this.value = value;
            this.showZeroModifiers = showZeroModifiers;
        }

        /// <summary>
        /// regular modifier with description generated on run
        /// </summary>
        public Modifier(Func<object, bool> myMethodName, Func<object, string> dynamicString, float value, bool showZeroModifiers) : base(myMethodName, dynamicString, true)
        {
            this.value = value;
            this.showZeroModifiers = showZeroModifiers;
        }

        /// <summary>
        /// modifier based on float function
        /// </summary>
        public Modifier(Func<object, float> myMethodName, string conditionIsTrue, float value, bool showZeroModifiers) : base(conditionIsTrue, true)
        {
            this.value = value;
            floatModifierFunction = myMethodName;
            this.showZeroModifiers = showZeroModifiers;
        }

        /// <summary>
        /// modifier based on int function
        /// </summary>
        public Modifier(Func<int> myMethodName, string conditionIsTrue, float value, bool showZeroModifiers) : base(conditionIsTrue, true)
        {
            multiplierModifierFunction = myMethodName;
            this.value = value;
            this.showZeroModifiers = showZeroModifiers;
        }

        public Modifier(Condition condition, float value, bool showZeroModifiers) : base(condition)
        {
            this.value = value;
            this.showZeroModifiers = showZeroModifiers;
        }

        /// <summary>
        /// Modifier where you can change scope
        /// </summary>
        public Modifier(Condition condition, Func<object, object> changeScopeTo, float value, bool showZeroModifiers) : base(condition, changeScopeTo)
        {
            this.value = value;
            this.showZeroModifiers = showZeroModifiers;
        }

        public float getValue()
        {
            return value;
        }

        /// <summary>Returns bool result and description in out description
        /// Doesn't care about showZeroModifier</summary>
        //override public bool checkIftrue(Country forWhom, out string description)
        //{
        //    bool answer = false;
        //    if (floatModifierFunction != null)
        //    {
        //        StringBuilder str = new StringBuilder("\n(+) ");
        //        str.Append(FullName);
        //        str.Append(": ").Append(floatModifierFunction(forWhom) * getValue());
        //        description = str.ToString();
        //        answer = true;
        //    }
        //    else
        //    if (multiplierModifierFunction != null)
        //    {
        //        StringBuilder str = new StringBuilder("\n(+) ");
        //        str.Append(FullName);
        //        str.Append(" ").Append(multiplierModifierFunction() * getValue());
        //        description = str.ToString();
        //        answer = true;
        //    }
        //    else
        //    if (check3(forWhom))
        //    {
        //        answer = true;
        //        StringBuilder str = new StringBuilder("\n(+) ");
        //        str.Append(FullName);
        //        str.Append(" ").Append(getValue());
        //        description = str.ToString();
        //    }
        //    else
        //    {
        //        answer = false;
        //        description = "";
        //    }
        //    return answer;
        //}
        public float getModifier(object forWhom, out string description)
        {
            if (changeTargetObject != null)
                forWhom = changeTargetObject(forWhom);

            float result;
            if (floatModifierFunction != null)
            {
                result = floatModifierFunction(forWhom) * getValue();
                if (result != 0f || showZeroModifiers)
                {
                    StringBuilder str = new StringBuilder("\n(+) ");
                    str.Append(getName(forWhom));
                    str.Append(": ").Append(result);
                    description = str.ToString();
                }
                else description = "";
            }
            else
            if (multiplierModifierFunction != null)
            {
                result = multiplierModifierFunction() * getValue();
                if (result != 0f || showZeroModifiers)
                {
                    StringBuilder str = new StringBuilder("\n(+) ");
                    str.Append(getName(forWhom));
                    str.Append(": ").Append(result);
                    description = str.ToString();
                }
                else description = "";
            }
            else
            {
                if (checkingFunction(forWhom))
                    result = getValue();
                else
                    result = 0f;
                if (result != 0f || showZeroModifiers)
                {
                    StringBuilder str = new StringBuilder("\n(+) ");
                    str.Append(getName(forWhom));
                    str.Append(": ").Append(result);
                    description = str.ToString();
                }
                else description = "";
            }
            //else
            //{
            //    result = 0;
            //    description = "";
            //}
            return result;
        }

        public float getModifier(object forWhom)
        {
            if (changeTargetObject != null)
                forWhom = changeTargetObject(forWhom);
            float result;
            if (floatModifierFunction != null)
                result = floatModifierFunction(forWhom) * getValue();
            else
            if (multiplierModifierFunction != null)
                result = multiplierModifierFunction() * getValue();
            else
            if (checkingFunction(forWhom))
                result = getValue();
            else
                result = 0;
            return result;
        }
    }
}