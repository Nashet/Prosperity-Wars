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

    //short constructor, allowing predicats of several types to be checked
    public ConditionsList(List<Condition_Invention_Interface> inlist)
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
    }



    internal static ConditionsList AlwaysYes = new ConditionsList(new List<Condition>() { new Condition(delegate (Country forWhom) { return 2 == 2; }, "Always Yes condition", true) });
    internal static ConditionsList IsNotImplemented = new ConditionsList(new List<Condition>() { new Condition(delegate (Country forWhom) { return 2 == 0; }, "Feature is implemented", true) });
    private List<Modifier> inlist;

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
                return true;
        return false;
    }
    public bool isAllTrue(Country forWhom)
    {
        foreach (var item in list)
            if (!item.checkIftrue(forWhom))
                return true;
        return false;
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
public class Condition//:AbstractConditionString
{
    public string conditionDescription; //, conditionIsFalse;
    public Func<Owner, bool> check;
    public Func<Country, bool> check2;
    /// <summary>to hide juncky info /// </summary>
    bool showAchievedConditionDescribtion;

    /// <summary>You better use shorter constructor, if possible </summary>
    public Condition(Func<Owner, bool> myMethodName, string conditionIsTrue, bool showAchievedConditionDescribtion)
    {
        check = myMethodName;
        this.conditionDescription = conditionIsTrue;
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
    }
    public Condition(string conditionIsTrue, bool showAchievedConditionDescribtion)
    {
        this.conditionDescription = conditionIsTrue;
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
    }
    /// <summary>You better use shorter constructor, if possible </summary>
    public Condition(Func<Country, bool> myMethodName, string conditionIsTrue, bool showAchievedConditionDescribtion)
    {
        check2 = myMethodName;
        this.conditionDescription = conditionIsTrue;
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
        //this.conditionIsFalse = conditionIsFalse;
    }

    /// <summary>Checks if invention is invented</summary>    
    public Condition(InventionType invention, bool showAchievedConditionDescribtion)
    {
        check2 = delegate (Country forWhom)
        {
            return forWhom.isInvented(invention);
        };
        this.conditionDescription = invention.getInventedPhrase();
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
        //this.conditionIsFalse = conditionIsFalse;
    }
    /// <summary>Checks if government == CheckingCountry.government</summary>    
    public Condition(Government.ReformValue government, bool showAchievedConditionDescribtion)
    {
        check2 = government.isGovernmentEqualsThat;
        this.conditionDescription = "Government is " + government.ToString(); // invention.getInventedPhrase();
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
        //this.conditionIsFalse = conditionIsFalse;
    }
    /// <summary>Checks if economy == CheckingCountry.economy</summary>    
    public Condition(Economy.ReformValue economy, bool showAchievedConditionDescribtion)
    {
        check2 = economy.isEconomyEqualsThat;
        this.conditionDescription = "Economical policy is " + economy.ToString(); // invention.getInventedPhrase();
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
    }

    /// <summary>Returns bool result and description in out description</summary>    
    internal bool checkIftrue(Owner forWhom, out string description)
    {
        string result = null;
        bool answer = false;
        if (check(forWhom))
        {
            if (showAchievedConditionDescribtion) result += "\n(+) " + conditionDescription;
            answer = true;
        }
        else
        {
            result += "\n(-) " + conditionDescription;
            answer = false;
        }
        description = result;
        return answer;
    }
    /// <summary>Fast version, without description</summary>    
    internal bool checkIftrue(Owner forWhom)
    {
        return check(forWhom);
    }
    /// <summary>Returns bool result and description in out description</summary>    
    virtual internal bool checkIftrue(Country forWhom, out string description)
    {
        string result = null;
        bool answer = false;
        if (check2 == null)
        {
            if (check(forWhom))
            {
                if (showAchievedConditionDescribtion) result += "\n(+) " + conditionDescription;
                answer = true;
            }
            else
            {
                result += "\n(-) " + conditionDescription;
                answer = false;
            }
            description = result;
            return answer;
        }
        if (check2(forWhom))
        {
            if (showAchievedConditionDescribtion) result += "\n(+) " + conditionDescription;
            answer = true;
        }
        else
        {
            result += "\n(-) " + conditionDescription;
            answer = false;
        }
        description = result;
        return answer;
    }
    /// <summary>Fast version, without description</summary>    
    internal bool checkIftrue(Country forWhom)
    {
        return check2(forWhom);
    }

}
public class Modifier : Condition
{
    ///// <summary>You better use shorter constructor, if possible </summary>
    //public Modifier(Func<Owner, bool> myMethodName, string conditionIsTrue, bool showAchievedConditionDescribtion): base (myMethodName,  conditionIsTrue, showAchievedConditionDescribtion)
    //{

    //}
    bool isMultiplier;
    float value;
    Func<byte> multiplierModifierFunction;

    /// <summary>You better use shorter constructor, if possible </summary>
    public Modifier(Func<Country, bool> myMethodName, string conditionIsTrue, bool showAchievedConditionDescribtion, float value) : base(myMethodName, conditionIsTrue, true)
    {
        this.value = value;
    }

    /// <summary></summary>
    public Modifier(Func<byte> myMethodName, string conditionIsTrue, bool showAchievedConditionDescribtion, float value) : base(conditionIsTrue, showAchievedConditionDescribtion)
    {
        check2 = null;
        multiplierModifierFunction = myMethodName;

        isMultiplier = true;
        this.value = value;
    }


    /// <summary>Checks if invention is invented</summary>    
    public Modifier(InventionType invention, bool showAchievedConditionDescribtion, float value) : base(invention, true)
    {
        this.value = value;
    }
    /// <summary>Checks if government == CheckingCountry.government</summary>    
    public Modifier(Government.ReformValue government, bool showAchievedConditionDescribtion, float value) : base(government, true)
    {
        this.value = value;
    }
    /// <summary>Checks if economy == CheckingCountry.economy</summary>    
    public Modifier(Economy.ReformValue economy, bool showAchievedConditionDescribtion, float value) : base(economy, true)
    {
        this.value = value;
    }
    internal float getValue()
    { return value; }
    /// <summary>Returns bool result and description in out description</summary>    
    override internal bool checkIftrue(Country forWhom, out string description)
    {
        bool answer = false;
        if (isMultiplier)
        {
            StringBuilder str = new StringBuilder("\n(+) ");
            str.Append(conditionDescription);
            str.Append(" ").Append(multiplierModifierFunction() * getValue());            
            description = str.ToString();
            answer = true;
        }
        else
        if (check2(forWhom))
        {
            answer = true;
            StringBuilder str = new StringBuilder("\n(+) ");
            str.Append(conditionDescription);
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
    internal float getModifier(Country forWhom, out string description)
    {
        float result;
        if (isMultiplier)
        {
            StringBuilder str = new StringBuilder("\n(+) ");
            str.Append(conditionDescription);
            result = multiplierModifierFunction() * getValue();
            str.Append(": ").Append(result);
            description = str.ToString();
           
        }
        else
        if (check2(forWhom))
        {
            
            StringBuilder str = new StringBuilder("\n(+) ");
            str.Append(conditionDescription);
            result =  getValue();
            str.Append(": ").Append(result);

            description = str.ToString();
        }
        else
        {
            result = 0;
            description = "";
        }
        return result;
    }
    internal float getModifier(Country forWhom)
    {
        float result;
        if (isMultiplier)
            result = multiplierModifierFunction() * getValue();
        else
        if (check2(forWhom))
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
    //short constructor, allowing predicats of several types to be checked
    public ModifiersList(List<Condition_Invention_Interface> inlist) : base(inlist)
    {

    }
    //internal static ConditionsList AlwaysYes = new ConditionsList(new List<Condition>() { new Condition(delegate (Country forWhom) { return 2 == 2; }, "Always Yes condition", true) });
    //internal static ConditionsList IsNotImplemented = new ConditionsList(new List<Condition>() { new Condition(delegate (Country forWhom) { return 2 == 0; }, "Feature is implemented", true) });

    internal float getModifier(Country forWhom, out string description)
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
    internal float getModifier(Country forWhom)
    {        
        float summ = 0f;        
        foreach (Modifier item in list)
            summ += item.getModifier(forWhom);
        return summ;
    }
}