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
    private readonly AbstractReformValue goal;
    private readonly AbstractReform reform;
    private readonly List<PopUnit> members = new List<PopUnit>();
    //private readonly Country country;

    Movement( AbstractReform reform, AbstractReformValue goal, PopUnit firstPop, Country place) : base(place)
    {
        this.reform = reform;
        this.goal = goal;
        members.Add(firstPop);
        //country = firstPop.getCountry();
        getPlaceDejure().movements.Add(this);
        //staff = new GeneralStaff(this);
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
            pop.getMovement().members.Remove(pop);

            if (pop.getMovement().members.Count == 0)
                pop.getCountry().movements.Remove(pop.getMovement());
            pop.setMovement(null);
        }
    }
    void add(PopUnit pop)
    {
        members.Add(pop);
    }
    
    public AbstractReformValue getGoal()
    {
        return goal;
    }
    public override string ToString()
    {
        return getName();
    }
    public string getName()
    {
        return goal.ToString();
    }
    public string getDescription()
    {
        return "members: " + getMembership() + ", mid. loyalty: " + getMiddleLoyalty() + ", rel. strength: " + getRelativeStrength(getPlaceDejure());
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
    public void simulate()
    {
        //if really angry and can win then revolt
        //if (getMiddleLoyalty().isSmallerThan(Options.PopLoyaltyLimitToRevolt) && canWinUprising())
        if (getRelativeStrength(getPlaceDejure()).isBiggerOrEqual(Procent.HundredProcent)
            && getMiddleLoyalty().isSmallerThan(Options.PopLoyaltyLimitToRevolt))
        {
            //revolt
            if (place == Game.Player)
                new Message("Revolution is coming", "People rebelled demanding " + goal, "Ok");
            mobilize(place.ownedProvinces);
            sendArmy(place.getCapital(), Procent.HundredProcent);
        }
    }

    //public bool canWinUprising()
    //{
    //    var defence = country.getDefenceForces();
    //    if (defence == null)
    //        return true;
    //    else
    //        return getMembership() > defence.getSize();
    //}

    //public Country getCountry()
    //{
    //    return country;
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
        reform.setValue(goal);
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
    }

    internal bool isEmpty()
    {
        return members.Count == 0;
    }
}
public static class MovementExtensions
{
    public static string getDescription(this List<Movement> list)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in list)
        {
            sb.Append(item.getName()).Append(" ").Append(item.getDescription()).Append("\n");
        }
        return sb.ToString();
    }

}