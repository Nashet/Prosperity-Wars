using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
using System.Text;

public class ConditionsList
{
    protected List<Condition> list;
    //public ConditionsList()
    //{
    //    list = new List<Condition>();
    //}
    // basic constructor
    public ConditionsList(List<Condition> inlist)
    {
        list = inlist;
    }

    //short constructor, allowing predicates of several types to be checked
    public ConditionsList(List<AbstractCondition> inlist)
    {
        list = new List<Condition>();
        foreach (var next in inlist)
            if (next is Government.ReformValue)
                list.Add(new Condition(next as Government.ReformValue, true));
            else
                if (next is Economy.ReformValue)
                list.Add(new Condition(next as Economy.ReformValue, true));
            else
                if (next is InventionType)
                list.Add(new Condition(next as InventionType, true));
            else
                if (next is Condition)
                list.Add(next as Condition);
    }



    internal static ConditionsList AlwaysYes = new ConditionsList(new List<Condition>() { new Condition(x => 2 == 2, "Always Yes condition", true) });
    internal static ConditionsList IsNotImplemented = new ConditionsList(new List<Condition>() { Condition.IsNotImplemented });
    //private List<Modifier> inlist;


    /// <summary>Return false if any of conditions is false</summary>    
    public bool isAllTrue(Owner forWhom, out string description)
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
        if (atLeastOneNoAnswer) return false;
        else
        {
            //description = "";
            return true;
        }
    }
    /// <summary>Return false if any of conditions is false</summary>    
    public bool isAllTrue(Country forWhom, out string description)
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
        if (atLeastOneNoAnswer) return false;
        else
        {
            //description = "";
            return true;
        }
    }
    public bool isAllTrue(Owner forWhom)
    {
        foreach (var item in list)
            if (!item.checkIftrue(forWhom))
                return false;
        return true;
    }
    public bool isAllTrue(Country forWhom)
    {
        foreach (var item in list)
            if (!item.checkIftrue(forWhom))
                return false;
        return true;
    }
}
//public abstract class AbstractConditionString
//{
//    //internal bool checkIftrue(Owner forWhom)
//    //{
//    //    throw new NotImplementedException();
//    //}

//    //internal bool checkIftrue(Owner forWhom, out string accu)
//    //{
//    //    throw new NotImplementedException();
//    //}
//}
//public class ConditionStringCountry //: AbstractConditionString
//{
//    public string conditionIsTrue; //, conditionIsFalse;
//    public Func<Country, bool> check;
//    /// <summary>to hide juncky info /// </summary>
//    bool showAchievedConditionDescribtion;

//    public ConditionStringCountry(Func<Country, bool> myMethodName, string conditionIsTrue, bool showAchievedConditionDescribtion)
//    {
//        check = myMethodName;
//        this.conditionIsTrue = conditionIsTrue;
//        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
//        //this.conditionIsFalse = conditionIsFalse;
//    }
//    /// <summary></summary>    
//    internal bool checkIftrue(Country forWhom, out string description)
//    {
//        string result = null;
//        bool answer = false;
//        if (check(forWhom))
//        {
//            if (showAchievedConditionDescribtion) result += "\n(+) " + conditionIsTrue;
//            answer = true;
//        }
//        else
//        {
//            result += "\n(-) " + conditionIsTrue;
//            answer = false;
//        }
//        description = result;
//        return answer;
//    }
//    /// <summary></summary>    
//    internal bool checkIftrue(Country forWhom)
//    {
//        return check(forWhom);
//    }
//}
public class Condition : AbstractCondition
{
    string text; //, conditionIsFalse;
    //protected Func<Owner, bool> check;
    //protected Func<Country, bool> check2;
    protected Func<System.Object, bool> check3;
    /// <summary>to hide junk info /// </summary>
    bool showAchievedConditionDescribtion;
    Func<string> dynamicString;


    internal static Condition IsNotImplemented = new Condition(delegate (System.Object forWhom) { return 2 == 0 || Game.devMode; }, "Feature is implemented", true);


    public Condition(Func<System.Object, bool> myMethodName, string conditionIsTrue, bool showAchievedConditionDescribtion)
    {
        check3 = myMethodName;
        this.text = conditionIsTrue;
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
    }
    public Condition(Func<System.Object, bool> myMethodName, Func<string> dynamicString, bool showAchievedConditionDescribtion)
    {
        check3 = myMethodName;
        this.dynamicString = dynamicString;
        //this.conditionDescription = conditionIsTrue;
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
    }

    public Condition(Condition another)
    {
        text = another.text;
        check3 = another.check3;
        showAchievedConditionDescribtion = another.showAchievedConditionDescribtion;
        dynamicString = another.dynamicString;
    }

    ////used in tax-like modifiers
    public Condition(string conditionIsTrue, bool showAchievedConditionDescribtion)
    {
        this.text = conditionIsTrue;
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
    }

    /// <summary>Checks if invention is invented</summary>    
    public Condition(InventionType invention, bool showAchievedConditionDescribtion)
    {
        check3 = x => (x as Country).isInvented(invention);

        //    delegate (Country forWhom)
        //{
        //    return forWhom.isInvented(invention);
        //};
        this.text = invention.getInventedPhrase();
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
        //this.conditionIsFalse = conditionIsFalse;
    }
    ///// <summary>Checks if government == CheckingCountry.government</summary>    
    public Condition(Government.ReformValue government, bool showAchievedConditionDescribtion)
    {
        //check3 = government.isGovernmentEqualsThat;
        check3 = x => (x as Country).government.status == government;
        this.text = "Government is " + government.ToString(); // invention.getInventedPhrase();
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;

    }
    ///// <summary>Checks if economy == CheckingCountry.economy</summary>    
    public Condition(Economy.ReformValue economy, bool showAchievedConditionDescribtion)
    {
        //check2 = economy.isEconomyEqualsThat;
        check3 = x => (x as Country).economy.status == economy;
        this.text = "Economical policy is " + economy.ToString(); // invention.getInventedPhrase();
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
    }
    internal string getDescription()
    {
        if (text == null)
            return dynamicString();
        else
            return text;

    }
    public override string ToString()
    {
        return getDescription();
    }

    /// <summary>Returns bool result and description in out description</summary>    
    internal bool checkIftrue(Owner forWhom, out string description)
    {
        string result = null;
        bool answer = false;
        if (check3(forWhom))
        {
            if (showAchievedConditionDescribtion) result += "\n(+) " + getDescription();
            answer = true;
        }
        else
        {
            result += "\n(-) " + getDescription();
            answer = false;
        }
        description = result;
        return answer;
    }
    /// <summary>Fast version, without description</summary>    
    internal bool checkIftrue(Owner forWhom)
    {
        return check3(forWhom);
    }
    /// <summary>Returns bool result and description in out description</summary>    
    virtual internal bool checkIftrue(Country forWhom, out string description)
    {
        string result = null;
        bool answer = false;

        if (check3(forWhom))
        {
            if (showAchievedConditionDescribtion) result += "\n(+) " + getDescription();
            answer = true;
        }
        else
        {
            result += "\n(-) " + getDescription();
            answer = false;
        }
        description = result;
        return answer;


        //if (check2(forWhom))
        //{
        //    if (showAchievedConditionDescribtion) result += "\n(+) " + getDescription();
        //    answer = true;
        //}
        //else
        //{
        //    result += "\n(-) " + getDescription();
        //    answer = false;
        //}
        //description = result;
        //return answer;
    }
    /// <summary>Fast version, without description</summary>    
    internal bool checkIftrue(Country forWhom)
    {
        return check3(forWhom);
    }

}
public class Modifier : Condition
{
    float value;
    Func<int> multiplierModifierFunction;
    Func<System.Object, float> floatModifierFunction;
    bool showZeroModifiers;
    public Modifier(Func<System.Object, bool> myMethodName, string conditionIsTrue, float value, bool showZeroModifiers) : base(myMethodName, conditionIsTrue, true)
    {
        this.value = value;
        this.showZeroModifiers = showZeroModifiers;
    }
    public Modifier(Func<System.Object, float> myMethodName, string conditionIsTrue, float value, bool showZeroModifiers) : base(conditionIsTrue, true)
    {
        this.value = value;
        floatModifierFunction = myMethodName;
        this.showZeroModifiers = showZeroModifiers;
    }
    /// <summary></summary>
    public Modifier(Func<int> myMethodName, string conditionIsTrue, float value, bool showZeroModifiers) : base(conditionIsTrue, true)
    {
        check3 = null;
        multiplierModifierFunction = myMethodName;

        //isMultiplier = true;
        this.value = value;
        this.showZeroModifiers = showZeroModifiers;
    }

    public Modifier(Condition condition, float value, bool showZeroModifiers) : base(condition)
    {
        this.value = value;
        this.showZeroModifiers = showZeroModifiers;
    }


    /// <summary>Checks if invention is invented.
    /// depreciated</summary>    
    public Modifier(InventionType invention, float value, bool showZeroModifiers) : base(invention, true)
    {
        this.value = value;
        this.showZeroModifiers = showZeroModifiers;
    }
    /// <summary>Checks if government == CheckingCountry.government/// depreciated</summary>  
    public Modifier(Government.ReformValue government, float value, bool showZeroModifiers) : base(government, true)
    {
        this.value = value;
        this.showZeroModifiers = showZeroModifiers;
    }
    /// <summary>Checks if economy == CheckingCountry.economy/// depreciated</summary>  
    public Modifier(Economy.ReformValue economy, float value, bool showZeroModifiers) : base(economy, true)
    {
        this.value = value;
        this.showZeroModifiers = showZeroModifiers;
    }
    public override string ToString()
    {
        return getDescription();
    }

    internal float getValue()
    {
        return value;
    }
    /// <summary>Returns bool result and description in out description
    /// Doesn't care about showZeroModifier</summary>        
    override internal bool checkIftrue(Country forWhom, out string description)
    {
        bool answer = false;
        if (floatModifierFunction != null)
        {
            StringBuilder str = new StringBuilder("\n(+) ");
            str.Append(getDescription());
            str.Append(": ").Append(floatModifierFunction(forWhom) * getValue());
            description = str.ToString();
            answer = true;
        }
        else
        if (multiplierModifierFunction != null)
        {
            StringBuilder str = new StringBuilder("\n(+) ");
            str.Append(getDescription());
            str.Append(" ").Append(multiplierModifierFunction() * getValue());
            description = str.ToString();
            answer = true;
        }
        else
        if (check3(forWhom))
        {
            answer = true;
            StringBuilder str = new StringBuilder("\n(+) ");
            str.Append(getDescription());
            str.Append(" ").Append(getValue());
            description = str.ToString();
        }
        else
        {
            answer = false;
            description = "";
        }
        return answer;
    }
    internal float getModifier(System.Object forWhom, out string description)
    {
        float result;
        if (floatModifierFunction != null)
        {
            result = floatModifierFunction(forWhom) * getValue();
            if (result != 0f || showZeroModifiers)
            {
                StringBuilder str = new StringBuilder("\n(+) ");
                str.Append(getDescription());
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
                str.Append(getDescription());
                str.Append(": ").Append(result);
                description = str.ToString();
            }
            else description = "";
        }
        else
        {
            if (check3(forWhom))
                result = getValue();
            else
                result = 0f;
            if (result != 0f || showZeroModifiers)
            {
                StringBuilder str = new StringBuilder("\n(+) ");
                str.Append(getDescription());
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
    internal float getModifier(System.Object forWhom)
    {
        float result;
        if (floatModifierFunction != null)
            result = floatModifierFunction(forWhom) * getValue();
        else
        if (multiplierModifierFunction != null)
            result = multiplierModifierFunction() * getValue();
        else
        if (check3(forWhom))
            result = getValue();
        else
            result = 0;
        return result;
    }
}
public class ModifiersList : ConditionsList
{
    //List<Modifier> list;
    // basic constructor
    public ModifiersList(List<Condition> inlist) : base(inlist)
    {

    }
    //short constructor, allowing predicates of several types to be checked
    public ModifiersList(List<AbstractCondition> inlist) : base(inlist)
    {

    }
    //internal static ConditionsList AlwaysYes = new ConditionsList(new List<Condition>() { new Condition(delegate (Country forWhom) { return 2 == 2; }, "Always Yes condition", true) });
    //internal static ConditionsList IsNotImplemented = new ConditionsList(new List<Condition>() { new Condition(delegate (Country forWhom) { return 2 == 0; }, "Feature is implemented", true) });

    internal float getModifier(System.Object forWhom, out string description)
    {
        StringBuilder text = new StringBuilder();
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
        description = text.ToString();
        return summ;
    }
    internal float getModifier(System.Object forWhom)
    {
        float summ = 0f;
        foreach (Modifier item in list)
            summ += item.getModifier(forWhom);
        return summ;
    }
}