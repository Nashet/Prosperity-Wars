using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
//public class StaffOwner :Consumer
//{
//    protected readonly GeneralStaff staff;
//}
public class Movement : Staff
{
    private readonly AbstractReformValue targetReformValue;
    private readonly AbstractReform targetReform;
    private readonly Country separatism;
    private readonly List<PopUnit> members = new List<PopUnit>();
    private bool _isInRevolt;
    //private readonly Country country;
    Movement(PopUnit firstPop, Country place) : base(place)
    {
        members.Add(firstPop);
        getPlaceDejure().movements.Add(this);
    }
    Movement(AbstractReform reform, AbstractReformValue goal, PopUnit firstPop, Country place) : this(firstPop, place)
    {
        this.targetReform = reform;
        this.targetReformValue = goal;
    }
    Movement(Country separatism, PopUnit firstPop, Country place) : this(firstPop, place)
    {
        this.separatism = separatism;
    }
    public static void join(PopUnit pop)
    {
        if (pop.getMovement() == null)
        {
            var goal = pop.getMostImportantIssue();
            //find reasonable goal and join
            var found = pop.getCountry().movements.Find(x => x.getGoal() == goal.Value);
            if (found == null)
                pop.setMovement(new Movement(goal.Key, goal.Value, pop, pop.getCountry()));
            else
            {
                found.add(pop);
                pop.setMovement(found);
            }
        }
    }
    public static void leave(PopUnit pop)
    {
        if (pop.getMovement() != null)
        {
            pop.getMovement().demobilize(x => x.getPopUnit() == pop);
            pop.getMovement().members.Remove(pop);

            if (pop.getMovement().members.Count == 0)
            {
                pop.getMovement().demobilize();
                pop.getCountry().movements.Remove(pop.getMovement());
            }
            pop.setMovement(null);
        }
    }
    void add(PopUnit pop)
    {
        members.Add(pop);
    }
    public bool isInRevolt()
    {
        return _isInRevolt;
    }
    public bool isValidGoal()
    {
        return targetReformValue.allowed.isAllTrue(getPlaceDejure());
    }
    public AbstractReformValue getGoal()
    {
        return targetReformValue;
    }
    public override string ToString()
    {
        return getName();
    }
    public string getName()
    {
        return "Movement for " + targetReformValue.ToString();
    }
    public string getShortName()
    {
        return targetReformValue.ToString();
    }
    public string getDescription()
    {
        return targetReformValue + ". Members: " + getMembership() + ", mid. loyalty: " + getMiddleLoyalty() + ", rel. strength: " + getRelativeStrength(getPlaceDejure());
    }
    /// <summary>
    /// Size of all members
    /// </summary>
    /// <returns></returns>
    public int getMembership()
    {
        int res = 0;
        foreach (var item in members)
        {
            res += item.getPopulation();
        }
        return res;
    }


    //public bool canWinUprising()
    //{
    //    var defence = country.getDefenceForces();
    //    if (defence == null)
    //        return true;
    //    else
    //        return getMembership() > defence.getSize();
    //}


    private Procent getMiddleLoyalty()
    {
        Procent result = new Procent(0);
        int calculatedSize = 0;
        foreach (var item in members)
        {
            result.addPoportionally(calculatedSize, item.getPopulation(), item.loyalty);
            calculatedSize += item.getPopulation();
        }
        return result;
    }

    public override void buyNeeds()
    {
        throw new NotImplementedException();
    }
    private void removeAllMembers()
    {
        foreach (var item in getAllArmies())
        {
            item.demobilize();
        }
        foreach (var pop in members)
        {
            //leave(pop);
            pop.setMovement(null);
        }
        members.Clear();
    }
    internal void onRevolutionWon()
    {
        //demobilize();
        //_isInRevolt = false;
        targetReform.setValue(targetReformValue);
        foreach (var pop in members)
        {
            pop.loyalty.add(Options.PopLoyaltyBoostOnRevolutionWon);
            pop.loyalty.clamp100();
        }
        removeAllMembers();
        //getPlaceDejure().movements.Remove(this);
        
    }

    internal void onRevolutionLost()
    {
        foreach (var pop in members)
        {
            pop.loyalty.add(Options.PopLoyaltyBoostOnRevolutionLost);
            pop.loyalty.clamp100();
        }
        //_isInRevolt = false;
        //demobilize();
    }
    internal bool isEmpty()
    {
        return members.Count == 0;
    }
    public void simulate()
    {
        //assuming movement already won or lost
        if (isInRevolt())
        {
            _isInRevolt = false;
            demobilize();
        }
        if (!isValidGoal())
        {
            removeAllMembers();
            return;
        }
     
    //&& canWinUprising())
        if (getRelativeStrength(getPlaceDejure()).isBiggerOrEqual(Procent.HundredProcent)
                && getMiddleLoyalty().isSmallerThan(Options.PopLoyaltyLimitToRevolt))
        {
            //revolt
            if (place == Game.Player)
                new Message("Revolution is coming", "People rebelled demanding " + targetReformValue + "\n\nTheir army is moving to our capital", "Ok");
            mobilize(place.ownedProvinces);
            sendArmy(place.getCapital(), Procent.HundredProcent);
            _isInRevolt = true;
        }
    }
    internal void mobilize(IEnumerable<Province> source)
    {
        getPlaceDejure().demobilize(x => x.getPopUnit().getMovement() == this);
        base.mobilize(source);
    }
}

public static class MovementExtensions
{
    public static string getDescription(this List<Movement> list)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in list)
        {
            sb.Append(" ").Append(item.getDescription()).Append("\n");
        }
        return sb.ToString();
    }

}