using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System;
//May be that should be extension



public class Army
{
    Dictionary<PopUnit, Corps> personal;
    Province destination;
    Country owner;
    //List<Army> allArmies = new List<Army>();
    public Army(Country owner)
    {
        personal = new Dictionary<PopUnit, Corps>();
        this.owner = owner;
        owner.allArmies.Add(this);
    }
    //public Army(Army army)
    //{
    //    personal = new List<Corps>(army.personal);
    //    destination = army.destination;
    //    this.owner = army.owner;
    //}

    public void demobilize()
    {
        foreach (var corps in personal.Values.ToList())
        {
            personal.Remove(corps.getPopUnit());
            corps.demobilize();
        }
        if (this != getOwner().homeArmy && this != getOwner().sendingArmy)
            owner.allArmies.Remove(this);
        //personal.ForEach((pop, corps) =>
        //{

        //    personal.Remove(corps.getPopUnit());
        //    corps.demobilize();
        //}
        //);
    }

    public void add(Corps corpsToAdd)
    {
        if (corpsToAdd != null)
        {
            Corps found;
            if (personal.TryGetValue(corpsToAdd.getPopUnit(), out found)) // Returns true.
            {
                found.add(corpsToAdd.getSize());
            }
            else
                personal.Add(corpsToAdd.getPopUnit(), corpsToAdd);
        }
    }
    public void add(Army armyToAdd)
    {
        if (armyToAdd != this)
            this.AddRange(armyToAdd.personal);
    }
    internal void remove(Corps corps)
    {
        personal.Remove(corps.getPopUnit());
    }
    internal int getSize()
    {
        //uint result = 0;
        //foreach (var next in personal)
        //    result += next.getSize();

        return personal.Sum(x => x.Value.getSize());
        //return result;
    }
    internal IEnumerable<Corps> getCorps()
    {
        foreach (Corps corps in personal.Values)
            yield return corps;
    }
    override public string ToString()
    {
        StringBuilder sb = new StringBuilder();

        int size = getSize();
        if (size > 0)
        {
            foreach (var next in personal)
                sb.Append(next.Value).Append(", ");
            sb.Append("Total size is ").Append(getSize());
        }
        else
            sb.Append("None");
        return sb.ToString();
    }
    public void AddRange(Dictionary<PopUnit, Corps> source)
    {
        Corps found;
        foreach (var corpsToAdd in source)
            if (corpsToAdd.Value.getSize() > 0)
                if (this.personal.TryGetValue(corpsToAdd.Key, out found))
                    found.add(corpsToAdd.Value.getSize());
                else
                    this.personal.Add(corpsToAdd.Key, corpsToAdd.Value);

            else
                corpsToAdd.Value.demobilize();

    }
    internal void balance(Army secondArmy, Procent howMuchShouldBeInSecondArmy)
    {
        if (howMuchShouldBeInSecondArmy.get() == 1f)
        {
            secondArmy.AddRange(this.personal);
            this.personal.Clear();
        }
        else
        {
            //Army sumArmy = new Army();
            //sumArmy.add(this);
            this.add(secondArmy);
            int secondArmyExpectedSize = howMuchShouldBeInSecondArmy.getProcent(this.getSize());

            secondArmy.clear();

            int needToFullFill = secondArmyExpectedSize;
            while (needToFullFill > 0)
            {
                var corpsToBalance = this.getBiggestCorpsSmallerThan(needToFullFill);
                if (corpsToBalance == null)
                    break;
                else
                    this.personal.move(corpsToBalance, secondArmy.personal);
                needToFullFill = secondArmyExpectedSize - secondArmy.getSize();
            }

        }

    }
    internal BattleResult attack(Province prov)
    {
        var enemy = prov.getOwner();
        if (enemy == Country.NullCountry)
            prov.mobilize();
        else
            enemy.mobilize();
        return attack(enemy.homeArmy);
    }
    /// <summary>
    /// returns true if attacker is winner
    /// </summary>    
    internal BattleResult attack(Army defender)
    {
        Army attacker = this;
        int initialAttackerSize = attacker.getSize();
        int initialDefenderSize = defender.getSize();

        bool attackerWon;
        BattleResult result;
        if (attacker.getStrenght() > defender.getStrenght())
        {
            attackerWon = true;
            float winnerLossUnConverted = defender.getStrenght() * defender.getStrenght() / attacker.getStrenght();
            int attackerLoss = attacker.takeLossUnconverted(winnerLossUnConverted);
            int loserLoss = defender.takeLoss(defender.getSize());

            result = new BattleResult(attacker.getOwner(), defender.getOwner(), initialAttackerSize, attackerLoss
            , initialDefenderSize, loserLoss, attacker.destination, attackerWon);
        }
        else if (attacker.getStrenght() == defender.getStrenght())
        {
            attacker.takeLoss(attacker.getSize());
            defender.takeLoss(defender.getSize());
            var r = new BattleResult(attacker.getOwner(), defender.getOwner(), attacker.getSize(), attacker.getSize(), defender.getSize(), defender.getSize(), attacker.destination, false);
            return r;
        }
        else
        {

            attackerWon = false;

            float winnerLossUnConverted = attacker.getStrenght() * attacker.getStrenght() / defender.getStrenght();
            int defenderLoss = defender.takeLossUnconverted(winnerLossUnConverted);

            int attackerLoss = attacker.takeLoss(attacker.getSize());
            result = new BattleResult(attacker.getOwner(), defender.getOwner(), initialAttackerSize, attackerLoss
            , initialDefenderSize, defenderLoss, attacker.destination, attackerWon);
        }



        return result;
    }
    public Country getOwner()
    {
        return owner;
    }
    //private void takeLoss()
    //{
    //    takeLoss(this.getSize());
    //}

    private int takeLoss(int loss)
    {
        int totalLoss = 0;
        int totalSize = getSize();
        int currentLoss;
        foreach (var corp in personal)
            if (totalSize > 0)
            {
                currentLoss = Mathf.RoundToInt(corp.Value.getSize() * loss / (float)totalSize);
                corp.Value.TakeLoss(currentLoss);
                totalLoss += currentLoss;
            }
        return totalLoss;
    }
    private int takeLossUnconverted(float lossStrenght)
    {
        int totalMenLoss = 0;
        float streghtLoss;
        int menLoss;
        float totalStrenght = getStrenght();
        if (totalStrenght > 0f)

            foreach (var corp in personal)
                if (corp.Value.getType().getStrenght() > 0f)
                {
                    streghtLoss = corp.Value.getStrenght() * lossStrenght / totalStrenght;
                    menLoss = Mathf.RoundToInt(streghtLoss / corp.Value.getType().getStrenght());
                    corp.Value.TakeLoss(menLoss);
                    totalMenLoss += menLoss;
                }
        return totalMenLoss;
    }

    private float getStrenght()
    {
        float result = 0;
        float modifier = 0f;
        foreach (var c in personal)
        {
            //modifier = Options.armyDefenceBonus + UnityEngine.Random.Range(-0.1f, 01f);
            if (destination == null) // army at home
            {
                modifier = 1f;// + Options.armyDefenceBonus + UnityEngine.Random.Range(-0.1f, 01f);
                result += c.Value.getStrenght() * modifier;
            }
            else
            {
                modifier = 1f;// + UnityEngine.Random.Range(-0.1f, 01f);
                result += c.Value.getStrenght() * modifier;
            }
        }
        return result;
    }

    internal void clear()
    {
        personal.Clear();
        destination = null;
    }

    private Corps getBiggestCorpsSmallerThan(int secondArmyExpectedSize)
    {

        var smallerCorps = personal.Where(x => x.Value.getSize() < secondArmyExpectedSize);
        if (smallerCorps.Count() == 0)
            return null;
        else
            return smallerCorps.MaxBy(x => x.Value.getSize()).Value;
    }


    //internal Army split(Procent howMuchShouldBeInSecondArmy)
    //{
    //    if (personal.Count > 0)
    //    {
    //        Army newArmy = new Army();
    //        int newArmyExpectedSize = howMuchShouldBeInSecondArmy.getProcent(this.getSize());
    //        //personal= personal.OrderBy((x) => x.getSize()).ToList();
    //        personal.Sort((x, y) => x == null ? (y == null ? 0 : -1)
    //                    : (y == null ? 1 : x.getSize().CompareTo(y.getSize())));

    //        while (newArmy.getSize() < newArmyExpectedSize)
    //            personal.move(this.personal[0], newArmy.personal);
    //        return newArmy;
    //    }
    //    else
    //        return null;
    //}



    internal void moveTo(Province province)
    {
        destination = province;
    }

    internal Province getDestination()
    {
        return destination;
    }


}
public class BattleResult
{
    Country attacker, defender;
    //Army attackerArmy, attackerLoss, defenderArmy, defenderLoss;
    int attackerArmy, attackerLoss, defenderArmy, defenderLoss;
    bool result;
    Province place;
    StringBuilder sb = new StringBuilder();
    //public BattleResult(Country attacker, Country defender, Army attackerArmy, Army attackerLoss, Army defenderArmy, Army defenderLoss, bool result)
    public BattleResult(Country attacker, Country defender, int attackerArmy, int attackerLoss, int defenderArmy, int defenderLoss, Province place, bool result)
    {
        this.attacker = attacker; this.defender = defender;
        //this.attackerArmy = new Army(attackerArmy); this.attackerLoss = new Army(attackerLoss); this.defenderArmy = new Army(defenderArmy); this.defenderLoss = new Army(defenderLoss);
        this.attackerArmy = attackerArmy; this.attackerLoss = attackerLoss; this.defenderArmy = defenderArmy; this.defenderLoss = defenderLoss;
        this.result = result;
        this.place = place;
        //Game.allBattles.Add(this);

    }

    internal bool isAttackerWon()
    {
        return result;
    }
    internal bool isDefenderWon()
    {
        return !result;
    }

    internal void createMessage()
    {
        sb.Clear();

        if (attacker == Game.player && isAttackerWon())
        {
            sb.Append("Our glorious army has attacked ").Append(place).Append(" with army of ").Append(attackerArmy).Append(" men");
            sb.Append("\nWhile enemy had ").Append(defenderArmy).Append(" men");
            sb.Append("\n\nWe won, enemy lost all men and we lost ").Append(attackerLoss).Append(" men");
            sb.Append("\nProvince ").Append(place).Append(" is our now!");
            // sb.Append("\nDate is ").Append(Game.date);
            new Message("We won a battle!", sb.ToString(), "Fine");
        }
        else
        if (defender == Game.player && isDefenderWon())
        {
            sb.Append("Our glorious army has been attacked by evil ").Append(attacker).Append(" in province ").Append(place)
                .Append(" with army of ").Append(attackerArmy).Append(" men");
            sb.Append("\nWhile we had ").Append(defenderArmy).Append(" men");
            sb.Append("\n\nWe won, enemy lost all men and we lost ").Append(defenderLoss).Append(" men");
            // sb.Append("\nDate is ").Append(Game.date);
            new Message("We won a battle!", sb.ToString(), "Fine");
        }
        else
            if (attacker == Game.player && isDefenderWon())
        {
            sb.Append("Our glorious army has attacked ").Append(place).Append(" with army of ").Append(attackerArmy).Append(" men");
            sb.Append("\nWhile enemy had ").Append(defenderArmy).Append(" men");
            sb.Append("\n\nWe lost, our invasion army is destroyed, while enemy lost ").Append(defenderLoss).Append(" men");
            // sb.Append("\nDate is ").Append(Game.date);
            new Message("We lost a battle!", sb.ToString(), "Fine");
        }
        else
            if (defender == Game.player && isAttackerWon())

        {
            sb.Append("Our glorious army has been attacked by evil ").Append(attacker).Append(" in province ").Append(place)
                .Append(" with army of ").Append(attackerArmy).Append(" men");
            sb.Append("\nWhile we had ").Append(defenderArmy).Append(" men");
            sb.Append("\n\nWe lost, our home army is destroyed, while enemy lost  ").Append(attackerLoss).Append(" men");
            sb.Append("\nProvince ").Append(place).Append(" is not our anymore!");
            // sb.Append("\nDate is ").Append(Game.date);
            new Message("We lost a battle!", sb.ToString(), "Not fine really");
        }
    }

    internal Country getDefender()
    {
        return defender;
    }

    internal Country getAttacker()
    {
        return attacker;
    }
}