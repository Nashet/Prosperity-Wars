using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
public class Condition
{
    List<ConditionString> list;
    // basic constructor
    public Condition(List<ConditionString> inlist)
    {
        list = inlist;
    }
    //short constructor, allowing predicats of several types to be checked
    public Condition(List<Condition_Invention_Interface> inlist)
    {
        list = new List<ConditionString>();
        foreach (var next in inlist)
            if (next is Government.ReformValue)
                list.Add(new ConditionString(next as Government.ReformValue, true));
            else
                if (next is Economy.ReformValue)
                list.Add(new ConditionString(next as Economy.ReformValue, true));
            else
                if (next is InventionType)
                list.Add(new ConditionString(next as InventionType, true));        
    }
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
public class ConditionString//:AbstractConditionString
{
    public string conditionDescription; //, conditionIsFalse;
    public Func<Owner, bool> check;
    public Func<Country, bool> check2;
    /// <summary>to hide juncky info /// </summary>
    bool showAchievedConditionDescribtion;
    public ConditionString(Func<Owner, bool> myMethodName, string conditionIsTrue, bool showAchievedConditionDescribtion)
    {
        check = myMethodName;
        this.conditionDescription = conditionIsTrue;
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
        //this.conditionIsFalse = conditionIsFalse;
    }
    public ConditionString(Func<Country, bool> myMethodName, string conditionIsTrue, bool showAchievedConditionDescribtion)
    {
        check2 = myMethodName;
        this.conditionDescription = conditionIsTrue;
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
        //this.conditionIsFalse = conditionIsFalse;
    }
    /// <summary>Checks if invention is invented</summary>    
    public ConditionString(InventionType invention, bool showAchievedConditionDescribtion)
    {
        check2 = delegate (Country forWhom)
        {
            return forWhom.isInvented(InventionType.individualRights);
        };
        this.conditionDescription = invention.getInventedPhrase();
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
        //this.conditionIsFalse = conditionIsFalse;
    }
    /// <summary>Checks if government == CheckingCountry.government</summary>    
    public ConditionString(Government.ReformValue government, bool showAchievedConditionDescribtion)
    {
        check2 = government.isGovernmentEqualsThat;
        this.conditionDescription = "Government is " + government.ToString(); // invention.getInventedPhrase();
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
        //this.conditionIsFalse = conditionIsFalse;
    }
    /// <summary>Checks if economy == CheckingCountry.economy</summary>    
    public ConditionString(Economy.ReformValue economy, bool showAchievedConditionDescribtion)
    {
        check2 = economy.isEconomyEqualsThat;
        this.conditionDescription = "Economical policy is " + economy.ToString(); // invention.getInventedPhrase();
        this.showAchievedConditionDescribtion = showAchievedConditionDescribtion;
    }

    /// <summary></summary>    
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
    /// <summary></summary>    
    internal bool checkIftrue(Owner forWhom)
    {
        return check(forWhom);
    }
    internal bool checkIftrue(Country forWhom, out string description)
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
    /// <summary></summary>    
    internal bool checkIftrue(Country forWhom)
    {
        return check2(forWhom);
    }
}