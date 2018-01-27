using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
using System.Text;
using System.ComponentModel;
using Nashet.EconomicSimulation;
using Nashet.Utils;
namespace Nashet.Conditions
{
    public class ConditionsList
    {
        //internal readonly static ConditionsList AlwaysYes = new ConditionsList(new List<Condition> { new Condition(x => 2 * 2 == 4, "Always Yes condition", true) });
        //internal readonly static ConditionsList IsNotImplemented = new ConditionsList(new List<Condition> { Condition.IsNotImplemented });

        protected List<Condition> list;
        /// <summary>
        /// Only for descendants
        /// </summary>
        public ConditionsList()
        {
            list = new List<Condition>();
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
            list = new List<Condition>();
            list.Add(condition);
        }
        /// <summary>
        /// copy constructor
        /// </summary>    
        public ConditionsList(ConditionsList conditionsList)
        {
            list = new List<Condition>(conditionsList.list);
        }

        internal void add(Condition condition)
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
                if (!item.checkIftrue(forWhom))
                    return false;
            return true;
        }

    }

    /// <summary>
    /// Represents condition, which can return bool value or string describing that value
    /// </summary> 
    public class Condition : Name
    {
        internal static readonly Condition IsNotImplemented
            //= new Condition(delegate { return 2 * 2 == 5 || Game.devMode; }, "Feature is implemented", true);
            = new Condition(delegate { return 2 * 2 == 5; }, "Feature is implemented", true);
        internal static readonly Condition AlwaysYes = new Condition(x => 2 * 2 == 4, "Always Yes condition", true);
        
        protected readonly Func<object, bool> checkingFunction;
        /// <summary>to hide junk info /// </summary>
        protected readonly bool showAchievedConditionDescribtion;
        protected readonly Func<object, string> dynamicString;
        protected readonly Func<object, object> changeTargetObject;
        //private readonly object dynamicStringTarget;

        public Condition(Func<object, bool> checkingFunction, string conditionIsTrue, bool showAchievedConditionDescribtion) : base(conditionIsTrue)
        {
            this.checkingFunction = checkingFunction;
            this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
        }

        /// <summary>
        /// Supports dynamic string
        /// </summary>    
        public Condition(Func<object, bool> checkingFunction, Func<object, string> dynamicString, bool showAchievedConditionDescribtion) : base(null)
        {

            this.checkingFunction = checkingFunction;
            this.dynamicString = dynamicString;
            this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
        }

        /// <summary>
        /// Used to build Modifier on Condition (Copy constructor)
        /// </summary>    
        protected Condition(Condition another) : base(another.getName())
        {
            checkingFunction = another.checkingFunction;
            showAchievedConditionDescribtion = another.showAchievedConditionDescribtion;
            dynamicString = another.dynamicString;
            this.changeTargetObject = another.changeTargetObject;
            //this.dynamicStringTarget = another.dynamicStringTarget;
        }

        /// <summary>
        /// Allows scope-changing
        /// </summary>    
        /// <param name="changeTargetObject"> Select another scope</param>
        public Condition(Condition another, Func<object, object> changeTargetObject) : base(another.getName())
        {
            this.changeTargetObject = changeTargetObject;
            checkingFunction = another.checkingFunction;
            showAchievedConditionDescribtion = another.showAchievedConditionDescribtion;
            dynamicString = another.dynamicString;
        }
        /// <summary>
        /// used in tax-like list based Modifiers
        /// </summary>
        /// <param name="conditionIsTrue"></param>
        /// <param name="showAchievedConditionDescribtion"></param>
        public Condition(string conditionIsTrue, bool showAchievedConditionDescribtion) : base(conditionIsTrue)
        {
            this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
        }

        /// <summary>Checks if invention is invented</summary>    
        //public Condition(Invention invention, bool showAchievedConditionDescribtion)
        //{
        //    check3 = x => (x as Country).isInvented(invention);

        //    //this.text = invention.getInventedPhrase();
        //    this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;        
        //}
        ///// <summary>Checks if government == CheckingCountry.government</summary>    
        //public Condition(Government.ReformValue government, bool showAchievedConditionDescribtion): base ("Government is " + government.ToString())
        //{
        //    //check3 = government.isGovernmentEqualsThat;
        //    check3 = x => (x as Country).government.getValue() == government;
        //    //this.text = "Government is " + government.ToString(); // invention.getInventedPhrase();
        //    this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;

        //}
        /////// <summary>Checks if economy == CheckingCountry.economy</summary>    
        //public Condition(Economy.ReformValue economy, bool showAchievedConditionDescribtion):base("Economical policy is " + economy.ToString())
        //{
        //    //check2 = economy.isEconomyEqualsThat;
        //    check3 = x => (x as Country).economy.status == economy;
        //    //this.text = "Economical policy is " + economy.ToString(); // invention.getInventedPhrase();
        //    this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
        //}
        public string getName(object some)
        {
            if (getName() == null)
            {
                //if (dynamicStringTarget == null)
                return dynamicString(some);
                //else
                // return dynamicString(dynamicStringTarget);
            }
            else
                return getName();
        }

        /// <summary>Returns bool result and description in out description</summary>    
        internal bool checkIftrue(object forWhom, out string description)
        {
            if (changeTargetObject != null)
                forWhom = changeTargetObject(forWhom);
            string result = null;
            bool answer = false;
            if (checkingFunction(forWhom))
            {
                if (showAchievedConditionDescribtion) result += "\n(+) " + getName(forWhom);
                answer = true;
            }
            else
            {
                result += "\n(-) " + getName(forWhom);
                answer = false;
            }
            description = result;
            return answer;
        }
        /// <summary>Returns bool result, fast version, without description</summary>    
        internal bool checkIftrue(object forWhom)
        {
            if (changeTargetObject != null)
                forWhom = changeTargetObject(forWhom); ;
            return checkingFunction(forWhom);
        }

    }
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
        public DoubleConditionsList() : base()
        {

        }
        /// <summary>
        /// copy constructor
        /// </summary>    
        public DoubleConditionsList(ConditionsList conditionsList) : base(conditionsList)
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
            description = "";
            return false;
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("use isAllTrue(object firstObject, object secondObject) instead", false)]
        public bool isAllTrue(object forWhom)
        {
            throw new DontUseThatMethod();
            return false;
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
                    if (!item.checkIftrue(firstObject))
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
                    if (!item.checkIftrue(secondObject))
                        return false;
                }
            return true;
        }
    }
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
        internal bool checkIftrue(object firstObject, object secondObject, out string description)
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
        internal bool checkIftrue(object firstObject, object secondObject)
        {
            if (changeTargetObject != null)
                firstObject = changeTargetObject(firstObject); ;
            return checkingFunctionForTwoObjects(firstObject, secondObject);
        }
    }
    public class Modifier : Condition
    {
        static public readonly Modifier modifierDefault1 = new Modifier(x => true, "Default", 1f, true);
        static public readonly Modifier modifierDefault100 = new Modifier(x => true, "Default", 100f, true);
        readonly float value;
        readonly Func<int> multiplierModifierFunction;
        readonly Func<object, float> floatModifierFunction;
        readonly bool showZeroModifiers;

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

        public override string ToString()
        {
            return getName();
        }

        internal float getValue()
        {
            return value;
        }
        /// <summary>Returns bool result and description in out description
        /// Doesn't care about showZeroModifier</summary>        
        //override internal bool checkIftrue(Country forWhom, out string description)
        //{
        //    bool answer = false;
        //    if (floatModifierFunction != null)
        //    {
        //        StringBuilder str = new StringBuilder("\n(+) ");
        //        str.Append(getDescription());
        //        str.Append(": ").Append(floatModifierFunction(forWhom) * getValue());
        //        description = str.ToString();
        //        answer = true;
        //    }
        //    else
        //    if (multiplierModifierFunction != null)
        //    {
        //        StringBuilder str = new StringBuilder("\n(+) ");
        //        str.Append(getDescription());
        //        str.Append(" ").Append(multiplierModifierFunction() * getValue());
        //        description = str.ToString();
        //        answer = true;
        //    }
        //    else
        //    if (check3(forWhom))
        //    {
        //        answer = true;
        //        StringBuilder str = new StringBuilder("\n(+) ");
        //        str.Append(getDescription());
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
        internal float getModifier(object forWhom, out string description)
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
        internal float getModifier(object forWhom)
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
        //internal static ConditionsList AlwaysYes = new ConditionsList(new List<Condition>() { new Condition(delegate (Country forWhom) { return 2 == 2; }, "Always Yes condition", true) });
        //internal static ConditionsList IsNotImplemented = new ConditionsList(new List<Condition>() { new Condition(delegate (Country forWhom) { return 2 == 0; }, "Feature is implemented", true) });

        internal float getModifier(object forWhom, out string description)
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
        internal float getModifier(object forWhom)
        {
            float summ = 0f;
            foreach (Modifier item in list)
                summ += item.getModifier(forWhom);
            return summ;
        }

        internal float find(AbstractReformValue reformValue)
        {
            var foundModifier = list.Find(x => reformValue.allowed.contains(x)) as Modifier;
            if (foundModifier == null)
                return 0f;
            else
                return foundModifier.getValue();
        }

        internal string GetDescription(object forWhom)
        {
            string res;
            getModifier(forWhom, out res);
            return res;
        }
    }
}