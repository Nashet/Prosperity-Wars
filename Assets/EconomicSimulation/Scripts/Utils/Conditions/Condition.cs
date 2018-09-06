using System;
using Nashet.Utils;

namespace Nashet.Conditions
{
    /// <summary>
    /// Represents condition, which can return bool value or string describing that value
    /// </summary>
    public class Condition : Name
    {
        public static readonly Condition IsNotImplemented
            //= new Condition(delegate { return 2 * 2 == 5 || Game.devMode; }, "Feature is implemented", true);
            = new Condition(delegate { return 2 * 2 == 5; }, "Feature is implemented", true);

        public static readonly Condition AlwaysYes = new Condition(x => 2 * 2 == 4, "Always Yes condition", true);

        protected readonly Func<object, bool> checkingFunction;

        /// <summary>to hide junk info /// </summary>
        protected readonly bool showAchievedConditionDescribtion;

        protected readonly Func<object, string> dynamicString;
        protected readonly Func<object, object> changeTargetObject;
        //private readonly object dynamicStringTarget;

        public Condition(Func<object, bool> checkingFunction, string conditionIsTrueText, bool showAchievedConditionDescribtion) : base(conditionIsTrueText)
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
        protected Condition(Condition another) : base(another.ToString())
        {
            checkingFunction = another.checkingFunction;
            showAchievedConditionDescribtion = another.showAchievedConditionDescribtion;
            dynamicString = another.dynamicString;
            changeTargetObject = another.changeTargetObject;
            //this.dynamicStringTarget = another.dynamicStringTarget;
        }

        /// <summary>
        /// Allows scope-changing
        /// </summary>
        /// <param name="changeTargetObject"> Select another scope</param>
        public Condition(Condition another, Func<object, object> changeTargetObject) : base(another.ToString())
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
        //    check3 = x => (x as Country).economy == economy;
        //    //this.text = "Economical policy is " + economy.ToString(); // invention.getInventedPhrase();
        //    this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
        //}
        public string getName(object some)
        {
            if (ToString() == null)
            {
                //if (dynamicStringTarget == null)
                return dynamicString(some);
                //else
                // return dynamicString(dynamicStringTarget);
            }
            else
                return ToString();
        }

        /// <summary>Returns bool result and description in out description</summary>
        public bool checkIftrue(object forWhom, out string description)
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
        public bool checkIfTrue(object forWhom)
        {
            if (changeTargetObject != null)
                forWhom = changeTargetObject(forWhom);
            return checkingFunction(forWhom);
        }
    }
}