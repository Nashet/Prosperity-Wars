using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System;
//May be that should be extension



public class Army
{
    
    List<Corps> personal = new List<Corps>();

    Province destination;
    public void demobilize()
    {
    }

    public void add(Corps corps)
    {
        if (corps != null)
            personal.Add(corps);

    }
    internal void add(Army armyToAdd)
    {
        this.personal.AddRange(armyToAdd.personal);
    }
    private void remove(Corps corps)
    {
        personal.Remove(corps);
    }
    internal int getSize()
    {
        //uint result = 0;
        //foreach (var next in personal)
        //    result += next.getSize();

        return personal.Sum(x => x.getSize());
        //return result;
    }
    override public string ToString()
    {
        StringBuilder sb = new StringBuilder();

        int size = getSize();
        if (size > 0)
        {
            foreach (var next in personal)
                sb.Append(next).Append(", ");
            sb.Append("Total size is ").Append(getSize());
        }
        else
            sb.Append("None");
        return sb.ToString();
    }

    internal void balance( Army secondArmy, Procent howMuchShouldBeInSecondArmy)
    {
        if (howMuchShouldBeInSecondArmy.get() == 1f)
        {
            secondArmy.personal.AddRange(this.personal);
            this.personal.Clear();
        }
        else
        {
            Army sumArmy = new Army();
            sumArmy.add(this);
            sumArmy.add(secondArmy);
            int secondArmyExpectedSize = howMuchShouldBeInSecondArmy.getProcent(sumArmy.getSize());
            
            secondArmy.clear();

            int needToFullFill = secondArmyExpectedSize;
            while (needToFullFill > 0)
            {
                var corpsToBalance = sumArmy.getBiggestCorpsSmallerThan(needToFullFill);
                if (corpsToBalance == null)
                    break;
                else
                    sumArmy.personal.move(corpsToBalance, secondArmy.personal);
                needToFullFill = secondArmyExpectedSize - secondArmy.getSize();
            }            
            this.personal = sumArmy.personal;
        }

    }

    private void clear()
    {
        personal.Clear();
        
    }

    private Corps getBiggestCorpsSmallerThan(int secondArmyExpectedSize)
    {
        var smallerCorps = personal.FindAll((x) => x.getSize() < secondArmyExpectedSize);
        if (smallerCorps == null || smallerCorps.Count == 0)
            return null;
        else
            return smallerCorps.MaxBy(x => x.getSize());
    }

    /// <summary>
    /// Likely not working correctly
    /// </summary>
    /// <param name="howMuchShouldBeInSecondArmy"></param>
    /// <returns></returns>
    internal Army split(Procent howMuchShouldBeInSecondArmy)
    {
        if (personal.Count > 0)
        {
            Army newArmy = new Army();
            int newArmyExpectedSize = howMuchShouldBeInSecondArmy.getProcent(this.getSize());
            //personal= personal.OrderBy((x) => x.getSize()).ToList();
            personal.Sort((x, y) => x == null ? (y == null ? 0 : -1)
                        : (y == null ? 1 : x.getSize().CompareTo(y.getSize())));

            while (newArmy.getSize() < newArmyExpectedSize)
                personal.move(this.personal[0], newArmy.personal);
            return newArmy;
        }
        else
            return null;
    }



    internal void send(Province province)
    {
        destination = province;
    }


}
