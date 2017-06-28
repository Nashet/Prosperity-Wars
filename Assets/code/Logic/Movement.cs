using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
//public class StaffOwner :Consumer
//{
//    protected readonly GeneralStaff staff;
//}
public class Movement : GeneralStaff
{
    private readonly AbstractReformValue goal;
    private readonly List<PopUnit> members = new List<PopUnit>();
    private readonly Country country;
    
    Movement(AbstractReformValue goal, PopUnit firstPop, Country place) : base(place)
    {
        this.goal = goal;
        members.Add(firstPop);
        country = firstPop.getCountry();
        country.movements.Add(this);
        //staff = new GeneralStaff(this);
    }
    public static void join(PopUnit pop)
    {
        if (pop.getMovement() == null)
        {
            var goal = pop.getMostImportantIssue();
            //find reasonable goal and join
            var found = pop.getCountry().movements.Find(x => x.getGoal() == goal);
            if (found == null)
                pop.setMovement(new Movement(goal, pop, pop.getCountry()));
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
            pop.setMovement(null);
        }
    }
    void add(PopUnit pop)
    {
        members.Add(pop);
    }
    void remove(PopUnit pop)
    {
        members.Remove(pop);
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
        return "Movement for " + goal;
    }
    public string getDescription()
    {
        return getMembership() + " members, mid. loyalty: " + getMiddleLoyalty();
    }
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
        if (getMiddleLoyalty().isSmallerThan(Options.PopLoyaltyLimitToRevolt) && canWinUprising())
            ;//revolt
    }
    public bool canWinUprising()
    {
        var defence = country.getDefenceForces();
        if (defence == null)
            return true;
        else
            return getMembership() > defence.getSize();
    }
   
    public Country getCountry()
    {
        return country;
    }
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