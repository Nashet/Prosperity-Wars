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
    static Modifier modifierInDefense = new Modifier(x => (x as Army).isInDefense(), "Is in defense", false, 0.5f);
    static Modifier modifierMoral = new Modifier(x => (x as Army).getMoral().get(), "Moral", true, 1f);
    static Modifier modifierDefault = new Modifier(x=>x==x, "Default", true, 1f);
    static ModifiersList modifierStrenght = new ModifiersList(new List<Condition>()
        {
            modifierDefault, modifierInDefense, modifierMoral
        });
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
            //personal.Remove(corps.getPopUnit());
            corps.demobilizeFrom(this);
        }
        //is it here problem with #83?
        if (this != getOwner().homeArmy && this != getOwner().sendingArmy)
            owner.allArmies.Remove(this);
        //personal.ForEach((pop, corps) =>
        //{

        //    personal.Remove(corps.getPopUnit());
        //    corps.demobilize();
        //}
        //);
    }
    public void consume()
    {
        //foreach (var corps in personal)
        //{
        //    corps.Value.consume(getOwner());
        //}
        personal.ForEach((x, corps) => corps.consume(getOwner()));
    }
    public Procent getMoral()
    {
        Procent result = new Procent(0);
        int calculatedSize = 0;
        foreach (var item in personal)
        {
            result.addPoportionally(calculatedSize, item.Value.getSize(), item.Value.getMoral());
            calculatedSize += item.Value.getSize();
        }
        return result;
    }
    public void add(Corps corpsToAdd)
    {
        if (corpsToAdd != null)
        {
            Corps found;
            if (personal.TryGetValue(corpsToAdd.getPopUnit(), out found)) // Returns true.
            {
                found.add(corpsToAdd);
            }
            else
                personal.Add(corpsToAdd.getPopUnit(), corpsToAdd);
        }
    }
    public void add(Army armyToAdd)
    {
        if (armyToAdd != this)
        {
            Corps found;
            foreach (var corpsToTransfert in armyToAdd.personal.ToList())
                if (corpsToTransfert.Value.getSize() > 0)
                    if (this.personal.TryGetValue(corpsToTransfert.Key, out found))
                        found.add(corpsToTransfert.Value);
                    else
                        this.personal.Add(corpsToTransfert.Key, corpsToTransfert.Value);
                else
                    corpsToTransfert.Value.demobilizeFrom(armyToAdd);
        }
    }
    internal void remove(Corps corps)
    {
        personal.Remove(corps.getPopUnit());
    }
    internal int getSize()
    {
        return personal.Sum(x => x.Value.getSize());
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
    internal string getShortName()
    {
        StringBuilder sb = new StringBuilder();

        int size = getSize();
        if (size > 0)
        {
            foreach (var next in getAmountByTypes())
                sb.Append(next.Value).Append(" ").Append(next.Key).Append(", ");
            sb.Append("Total size: ").Append(getSize());
            sb.Append(" Moral: ").Append(getMoral());
            sb.Append(" Provision: ").Append(getConsumption());
            //string str;
            //modifierStrenght.getModifier(this, out str);
            //sb.Append(" Bonuses: ").Append(str);
        }
        else
            sb.Append("None");
        return sb.ToString();
    }

    private Procent getConsumption()
    {
        Procent res = new Procent(0f);
        int calculatedSize = 0;
        foreach (var item in personal)
        {
            res.addPoportionally(calculatedSize, item.Value.getSize(), item.Value.getConsumption(getOwner()));
            calculatedSize += item.Value.getSize();
        }
        return res;
    }

    public Dictionary<PopType, int> getAmountByTypes()
    {
        Dictionary<PopType, int> res = new Dictionary<PopType, int>();
        //int test;
        foreach (var next in personal)
        {
            //if (res.TryGetValue(next.Key.type, out test))
            //    test += next.Value.getSize();
            //else
            //    res.Add(next.Key.type, next.Value.getSize());
            if (res.ContainsKey(next.Key.type))
                res[next.Key.type] += next.Value.getSize();
            else
                res.Add(next.Key.type, next.Value.getSize());
        }
        return res;
    }   
  
    internal void balance(Army secondArmy, Procent howMuchShouldBeInSecondArmy)
    {
        if (howMuchShouldBeInSecondArmy.get() == 1f)
        {
            secondArmy.add(this);
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
        string attackerBonus;
        Army.modifierStrenght.getModifier(attacker, out attackerBonus);
        string defenderBonus;
        Army.modifierStrenght.getModifier(defender, out defenderBonus);

        if (attacker.getStrenght() > defender.getStrenght())
        {
            attackerWon = true;
            float winnerLossUnConverted;
            if (attacker.getStrenght() > 0f)
                winnerLossUnConverted = defender.getStrenght() * defender.getStrenght() / attacker.getStrenght();
            else
                winnerLossUnConverted = 0f;
            int attackerLoss = attacker.takeLossUnconverted(winnerLossUnConverted);
            int loserLoss = defender.takeLoss(defender.getSize());

            result = new BattleResult(attacker.getOwner(), defender.getOwner(), initialAttackerSize, attackerLoss
            , initialDefenderSize, loserLoss, attacker.destination, attackerWon, attackerBonus, defenderBonus);
        }
        else if (attacker.getStrenght() == defender.getStrenght())
        {
            attacker.takeLoss(attacker.getSize());
            defender.takeLoss(defender.getSize());

            var r = new BattleResult(attacker.getOwner(), defender.getOwner(), attacker.getSize(), attacker.getSize(), defender.getSize(), defender.getSize(),
                attacker.destination, false, attackerBonus, defenderBonus);
            return r;
        }
        else //defender.getStrenght() > attacker.getStrenght()  
        {

            attackerWon = false;
            float winnerLossUnConverted;
            if (defender.getStrenght() > 0f)
                winnerLossUnConverted = attacker.getStrenght() * attacker.getStrenght() / (defender.getStrenght());
            else
                winnerLossUnConverted = 0f;
            int defenderLoss = defender.takeLossUnconverted(winnerLossUnConverted);

            int attackerLoss = attacker.takeLoss(attacker.getSize());
            result = new BattleResult(attacker.getOwner(), defender.getOwner(), initialAttackerSize, attackerLoss
            , initialDefenderSize, defenderLoss, attacker.destination, attackerWon, attackerBonus, defenderBonus);
        }
        return result;
    }
    public Country getOwner()
    {
        return owner;
    }

    private int takeLoss(int loss)
    {
        int totalLoss = 0;
        if (loss > 0)
        {
            int totalSize = getSize();
            int currentLoss;
            foreach (var corp in personal)
                if (totalSize > 0)
                {
                    currentLoss = Mathf.RoundToInt(corp.Value.getSize() * loss / (float)totalSize);
                    corp.Value.TakeLoss(currentLoss);
                    totalLoss += currentLoss;
                }
        }
        return totalLoss;
    }
    /// <summary>
    /// argument is NOT men, but they strength
    /// </summary>    
    private int takeLossUnconverted(float lossStrenght)
    {
        int totalMenLoss = 0;
        if (lossStrenght > 0f)
        {

            float streghtLoss;
            int menLoss;
            float totalStrenght = getStrenght();
            if (totalStrenght > 0f)
                foreach (var corp in personal)
                    if (corp.Value.getType().getStrenght() * getStrenghtModifier() > 0)//(corp.Value.getType().getStrenght() > 0f)
                    {
                        streghtLoss = corp.Value.getStrenght(this) * (lossStrenght / totalStrenght);
                        menLoss = Mathf.RoundToInt(streghtLoss / (corp.Value.getType().getStrenght()));// * getStrenghtModifier())); // corp.Value.getType().getStrenght());
                        
                        totalMenLoss += corp.Value.TakeLoss(menLoss);
                    }
        }
        return totalMenLoss;
    }
    public bool isInDefense()
    {
        return destination == null;
    }
    public float getStrenghtModifier()
    {      
        return  modifierStrenght.getModifier(this);
    }
    private float getStrenght()
    {
        float result = 0;
        //float modifier = 0f;
        foreach (var c in personal)
        {
            //todo optimize            
            result += c.Value.getStrenght(this);
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
    string attackerBonus; string defenderBonus;
    //public BattleResult(Country attacker, Country defender, Army attackerArmy, Army attackerLoss, Army defenderArmy, Army defenderLoss, bool result)
    public BattleResult(Country attacker, Country defender, int attackerArmy, int attackerLoss, int defenderArmy, int defenderLoss,
        Province place, bool result, string attackerBonus, string defenderBonus)
    {
        this.attacker = attacker; this.defender = defender;
        //this.attackerArmy = new Army(attackerArmy); this.attackerLoss = new Army(attackerLoss); this.defenderArmy = new Army(defenderArmy); this.defenderLoss = new Army(defenderLoss);
        this.attackerArmy = attackerArmy; this.attackerLoss = attackerLoss; this.defenderArmy = defenderArmy; this.defenderLoss = defenderLoss;
        this.result = result;
        this.place = place;
        this.defenderBonus = defenderBonus;
        this.attackerBonus = attackerBonus;
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
            sb.Append("Our glorious army attacked ").Append(place).Append(" with army of ").Append(attackerArmy).Append(" men.");
            sb.Append(" Modifiers: ").Append(attackerBonus);
            sb.Append("\n\nWhile enemy had ").Append(defenderArmy).Append(" men. Modifiers:  ").Append(defenderBonus);
            sb.Append("\n\nWe won, enemy lost all men and we lost ").Append(attackerLoss).Append(" men");
            sb.Append("\nProvince ").Append(place).Append(" is our now!");
            // sb.Append("\nDate is ").Append(Game.date);
            new Message("We won a battle!", sb.ToString(), "Fine");
        }
        else
        if (defender == Game.player && isDefenderWon())
        {
            sb.Append("Our glorious army attacked by evil ").Append(attacker).Append(" in province ").Append(place)
                .Append(" with army of ").Append(attackerArmy).Append(" men.");
            sb.Append(" Modifiers: ").Append(attackerBonus);
            sb.Append("\n\nWhile we had ").Append(defenderArmy).Append(" men. Modifiers: ").Append(defenderBonus);
            sb.Append("\n\nWe won, enemy lost all men and we lost ").Append(defenderLoss).Append(" men");
            // sb.Append("\nDate is ").Append(Game.date);
            new Message("We won a battle!", sb.ToString(), "Fine");
        }
        else
            if (attacker == Game.player && isDefenderWon())
        {
            sb.Append("Our glorious army attacked ").Append(place).Append(" with army of ").Append(attackerArmy).Append(" men");
            sb.Append(" Modifiers: ").Append(attackerBonus);
            sb.Append("\n\nWhile enemy had ").Append(defenderArmy).Append(" men. Modifiers:  ").Append(defenderBonus);
            sb.Append("\n\nWe lost, our invasion army is destroyed, while enemy lost ").Append(defenderLoss).Append(" men");
            // sb.Append("\nDate is ").Append(Game.date);
            new Message("We lost a battle!", sb.ToString(), "Fine");
        }
        else
            if (defender == Game.player && isAttackerWon())

        {
            sb.Append("Our glorious army attacked by evil ").Append(attacker).Append(" in province ").Append(place)
                .Append(" with army of ").Append(attackerArmy).Append(" men");
            sb.Append(" Modifiers: ").Append(attackerBonus);
            sb.Append("\n\nWhile we had ").Append(defenderArmy).Append(" men. Modifiers:  ").Append(defenderBonus);
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