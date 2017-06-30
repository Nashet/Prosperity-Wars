using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents any military commander structure
/// </summary>
public abstract class Staff : Consumer
{
    List<Army> allArmies = new List<Army>();
    protected Country place; //todo change class
    protected Staff(Country place) : base(null)
    {
        this.place = place;
    }
    /// <summary>
    /// Sum of existing armies men + unmobilized reserve
    /// </summary>
    /// <returns></returns>
    public float getStregth()
    {
        return howMuchCanMobilize() + getAllArmiesSize();
    }
    public Procent getRelativeStrength(Staff toWhom)
    {
        //var governmentHomeArmy = country.getDefenceForces();
        var thisStrenght = getStregth();
        var toWhomStrenght = toWhom.getStregth();

        if (toWhomStrenght == 0f && thisStrenght > 0f)
            return new Procent(999.999f);
        else
            return Procent.makeProcent(thisStrenght, toWhomStrenght, false);

    }
    public float howMuchCanMobilize()
    {
        float result = 0f;
        foreach (var pr in place.ownedProvinces)
            foreach (var po in pr.allPopUnits)
                if (po.popType.canMobilize())
                    result += po.howMuchCanMobilize(this);
        return result;
    }
    public float getAllArmiesSize()
    {
        int size = 0;
        var defArmy = getDefenceForces();
        if (defArmy != null)
            size = defArmy.getSize();
        return size;
    }
    public Country getPlaceDejure()
    {
        return place;
    }
    public bool isAI()
    {
        return this != Game.Player || (this == Game.Player && Game.isPlayerSurrended());
    }
    /// <summary>
    /// Unites all home armies in one. Assuming armies are alive, just needed to consolidate. If there is nothing to consolidate than returns empty army    
    /// </summary>
    public Army consolidateArmies()
    {
        Army consolidatedArmy = new Army(this);
        if (allArmies.Count == 1)
            return allArmies[0];
        else
        {
            if (allArmies.Count > 0)
            {

                foreach (Army next in allArmies)
                    if (next.getDestination() == null)
                    {
                        consolidatedArmy.setOwner(next.getOwner());
                        consolidatedArmy.joinin(next);
                    }
                //if (addConsolidatedArmyInList)
                allArmies.Add(consolidatedArmy);
                allArmies.RemoveAll(army => army.getSize() == 0);// && army != country.sendingArmy); // don't remove sending army. Its personal already transfered to Home army            

            }
        }
        return consolidatedArmy;

        //source.RemoveAll(armies => armies.getDestination() == null && armies != country.homeArmy && armies != country.sendingArmy);
        //allArmies.RemoveAll(army => army.getSize() == 0);// && army != country.sendingArmy); // don't remove sending army. Its personal already transfered to Home army
    }
    //internal void mobilize()
    //{
    //    foreach (var province in place.ownedProvinces)
    //    {
    //        Army newArmy = new Army(province.getCountry());
    //        foreach (var item in province.allPopUnits)
    //            //if (item.popType.canMobilize() && item.howMuchCanMobilize(this) > 0)
    //                newArmy.add(item.mobilize(this));
    //    }
    //    consolidateArmies();
    //}
    internal void mobilize(IEnumerable<Province> source)
    {
        foreach (var province in source)
        {
            Army newArmy = new Army(this);
            foreach (var item in province.allPopUnits)
                if (item.popType.canMobilize() && item.howMuchCanMobilize(this) > 0)
                    newArmy.add(item.mobilize(this));
        }
        consolidateArmies();
    }
    public void addArmy(Army army)
    {
        allArmies.Add(army);
    }
    internal void demobilize()
    {
        foreach (var item in allArmies)
        {
            item.demobilize();
        }
        allArmies.Clear();
    }
    internal void demobilize(Func<Corps, bool> predicate)
    {
        foreach (Army nextArmy in allArmies)
        {
            nextArmy.demobilize(predicate);
        }
        allArmies.RemoveAll(army => army.getSize() == 0);
    }

    //override public void buyNeeds()
    // {
    //     allArmies.ForEach(x => x.consume());
    // }

    internal PrimitiveStorageSet getNeeds()
    {
        PrimitiveStorageSet res = new PrimitiveStorageSet();
        foreach (var item in allArmies)
            res.add(item.getNeeds());
        return res;
    }

    virtual internal void sendArmy(Province possibleTarget, Procent procent)
    {
        consolidateArmies().balance(procent).sendTo(possibleTarget);

    }

    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        allArmies.ForEach(x => x.setStatisticToZero());
    }
    internal IEnumerable<Army> getAllArmies()
    {
        foreach (var army in allArmies)
            yield return army;
    }
    internal IEnumerable<Army> getAttackingArmies()
    {
        foreach (var army in allArmies)
            if (army.getDestination() != null)
                if (army.getDestination().getCountry() != army.getOwner())
                    yield return army;
                else
                    army.sendTo(null); // go home
    }

    /// <summary>
    /// returns Home army
    /// </summary>
    /// <returns></returns>
    internal Army getDefenceForces()
    {
        Army a = allArmies.Find(x => x.getSize() > 0 && x.getDestination() == null);
        if (a == null)
            return new Army(this);
        else
            return a;
    }

    internal static IEnumerable<Staff> getAllStaffs()
    {
        foreach (var country in Country.allCountries)
            if (country.isExist() && country != Country.NullCountry)
            {
                yield return country;
                foreach (var staff in country.movements)
                    yield return staff;
            }

    }



    //internal Army getVirtualArmy(Procent procent)
    //{
    //    Army virtualArmy = consolidateArmies(false).getVirtualArmy(procent);
    //    return virtualArmy;
    //}
}
