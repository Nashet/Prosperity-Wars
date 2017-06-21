using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System;

public class GeneralStaff
{
    //internal Army sendingArmy;
    List<Army> allArmies = new List<Army>();
    Country country;
    public GeneralStaff(Country country)
    {
        this.country = country;
    }
    /// <summary>
    /// Unites all home armies in one. Assuming armies are alive, just needed to consolidate. If there is nothing to consolidate than returns empty army    
    /// </summary>
    public Army consolidateArmies()
    {
        Army consolidatedArmy = new Army(country);
        if (allArmies.Count == 1)
            return allArmies[0];
        else
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
        return consolidatedArmy;

        //source.RemoveAll(armies => armies.getDestination() == null && armies != country.homeArmy && armies != country.sendingArmy);
        //allArmies.RemoveAll(army => army.getSize() == 0);// && army != country.sendingArmy); // don't remove sending army. Its personal already transfered to Home army
    }
    internal void mobilize(IEnumerable<Province> source)
    {
        foreach (var province in source)
        {
            Army newArmy = new Army(province.getCountry());
            foreach (var item in province.allPopUnits)
                if (item.popType.canMobilize() && item.howMuchCanMobilize() > 0)
                    newArmy.add(item.mobilize());
            //if (newArmy.getSize() > 0)
            //    addArmy(newArmy);
        }
        consolidateArmies();
    }
    public void addArmy(Army army)
    {
        allArmies.Add(army);
    }
    internal void demobilizeAllArmies()
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

    internal void consume()
    {
        allArmies.ForEach(x => x.consume());
    }

    internal PrimitiveStorageSet getNeeds()
    {
        PrimitiveStorageSet res = new PrimitiveStorageSet();
        foreach (var item in allArmies)
            res.add(item.getNeeds());
        return res;
    }

    internal void sendArmy(Province possibleTarget, Procent procent)
    {
        consolidateArmies().balance(procent).sendTo(possibleTarget);
        
    }

    internal void setStatisticToZero()
    {
        allArmies.ForEach(x => x.setStatisticToZero());
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

    internal Army getDefenceForces()
    {
        return allArmies.Find(x => x.getSize() > 0 && x.getDestination() == null);
        //return consolidateArmies();
    }



    //internal Army getVirtualArmy(Procent procent)
    //{
    //    Army virtualArmy = consolidateArmies(false).getVirtualArmy(procent);
    //    return virtualArmy;
    //}
}
public class Army
{
    Dictionary<PopUnit, Corps> personal;
    Province destination;
    Country owner;
    static Modifier modifierInDefense = new Modifier(x => (x as Army).isInDefense(), "Is in defense", 0.5f, false);
    static Modifier modifierMoral = new Modifier(x => (x as Army).getMoral().get(), "Moral", 1f, true);
    
    static Modifier modifierColdArms = new Modifier(x => (x as Army).getColdArmsSupply(), "Cold arms", 1f, false);
    static Modifier modifierFirearms = new Modifier(x => (x as Army).getEquippedFirearmsSupply(), "Equipped Firearms", 2f, false);
    static Modifier modifierArtillery = new Modifier(x => (x as Army).getEquippedArtillerySupply(), "Equipped Artillery", 1f, false);

    static Modifier modifierCars = new Modifier(x => (x as Army).getEquippedCarsSupply(), "Equipped Cars", 2f, false);
    static Modifier modifierTanks = new Modifier(x => (x as Army).getEquippedTanksSupply(), "Equipped Tanks", 1f, false);
    static Modifier modifierAirplanes = new Modifier(x => (x as Army).getEquippedAirplanesSupply(), "Equipped Airplanes", 1f, false);
    static Modifier modifierLuck = new Modifier(x => (float)Math.Round(UnityEngine.Random.Range(-0.5f, 0.5f), 2), "Luck", 1f, false);

    private float getColdArmsSupply()
    {
        if (Product.ColdArms.isInvented(getOwner()))
            return Procent.makeProcent(getConsumption(Product.ColdArms), getNeeds(Product.ColdArms), false).get();
        else return 0f;
    }
    private float getEquippedFirearmsSupply()
    {
        if (Product.Firearms.isInvented(getOwner()))
            return Mathf.Min(
         Procent.makeProcent(getConsumption(Product.Firearms), getNeeds(Product.Firearms), false).get(),
         Procent.makeProcent(getConsumption(Product.Ammunition), getNeeds(Product.Ammunition), false).get()
         );
        else return 0f;
    }
    private float getEquippedArtillerySupply()
    {
        if (Product.Artillery.isInvented(getOwner()))
            return Mathf.Min(
         Procent.makeProcent(getConsumption(Product.Artillery), getNeeds(Product.Artillery), false).get(),
         Procent.makeProcent(getConsumption(Product.Ammunition), getNeeds(Product.Ammunition), false).get()
         );
        else return 0f;
    }
    private float getEquippedCarsSupply()
    {
        if (Product.Cars.isInvented(getOwner()))
            return Mathf.Min(
         Procent.makeProcent(getConsumption(Product.Cars), getNeeds(Product.Cars), false).get(),
         Procent.makeProcent(getConsumption(Product.Fuel), getNeeds(Product.Fuel), false).get()
         );
        else return 0f;
    }
    private float getEquippedTanksSupply()
    {
        if (Product.Tanks.isInvented(getOwner()))
            return Mathf.Min(
         Procent.makeProcent(getConsumption(Product.Tanks), getNeeds(Product.Tanks), false).get(),
         Procent.makeProcent(getConsumption(Product.Fuel), getNeeds(Product.Fuel), false).get(),
         Procent.makeProcent(getConsumption(Product.Ammunition), getNeeds(Product.Ammunition), false).get()
         );
        else return 0f;
    }
    private float getEquippedAirplanesSupply()
    {
        if (Product.Airplanes.isInvented(getOwner()))
            return Mathf.Min(
         Procent.makeProcent(getConsumption(Product.Airplanes), getNeeds(Product.Airplanes), false).get(),
         Procent.makeProcent(getConsumption(Product.Fuel), getNeeds(Product.Fuel), false).get(),
         Procent.makeProcent(getConsumption(Product.Ammunition), getNeeds(Product.Ammunition), false).get()
         );
        else return 0f;
    }

    static ModifiersList modifierStrenght = new ModifiersList(new List<Condition>()
        {
            Modifier.modifierDefault, modifierInDefense, modifierMoral, modifierColdArms, modifierFirearms, modifierArtillery,
        modifierCars, modifierTanks, modifierAirplanes, modifierLuck
        });
    // private Army consolidatedArmy;

    public Army(Country owner)
    {
        owner.staff.addArmy(this);
        personal = new Dictionary<PopUnit, Corps>();
        this.owner = owner;
    }
    public Army(Army consolidatedArmy) : this(consolidatedArmy.getOwner())
    { }
    //public Army()
    //{
    //    //owner.staff.addArmy(this);
    //    personal = new Dictionary<PopUnit, Corps>();
    //    this.owner = null;
    //}

    //public static bool Any<TSource>(this IEnumerable<TSource> source);
    void move(Corps item, Army destination)
    {
        if (this.personal.Remove(item.getPopUnit())) // don't remove this
            destination.personal.Add(item.getPopUnit(), item);
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
            Pool.ReleaseObject(corps);
        }
    }
    public void demobilize(Func<Corps, bool> predicate)
    {

        foreach (var corps in personal.Values.ToList())
            if (predicate(corps))
            {
                personal.Remove(corps.getPopUnit());
                Pool.ReleaseObject(corps);
            }
    }
    public void consume()
    {
        //foreach (var corps in personal)
        //{
        //    corps.Value.consume(getOwner());
        //}
        personal.ForEach((x, corps) => corps.consume(getOwner()));
    }
    Procent getMoral()
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
    public void joinin(Army armyToAdd)
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
                {
                    armyToAdd.personal.Remove(corpsToTransfert.Key);
                    Pool.ReleaseObject(corpsToTransfert.Value);
                }
            armyToAdd.personal.Clear();
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
                if (next.Value.getSize() > 0)
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
    //private Procent getConsumption(Product prod)
    //{
    //    Procent res = new Procent(0f);
    //    int calculatedSize = 0;
    //    foreach (var item in personal)
    //    {
    //        res.addPoportionally(calculatedSize, item.Value.getSize(), item.Value.getConsumption( prod));
    //        calculatedSize += item.Value.getSize();
    //    }
    //    return res;
    //}
    private Value getConsumption(Product prod)
    {
        Value res = new Value(0f);
        foreach (var item in personal)
            res.add(item.Value.getConsumption(prod));
        return res;
    }
    private PrimitiveStorageSet getConsumption()
    {
        var consumption = new PrimitiveStorageSet();
        foreach (var item in personal)
            consumption.add(item.Value.getConsumption());


        //    Procent res = new Procent(0f);
        //int calculatedSize = 0;
        //foreach (var item in personal)
        //{
        //    res.addPoportionally(calculatedSize, item.Value.getSize(), item.Value.getConsumption());
        //    calculatedSize += item.Value.getSize();
        //}
        //return res;
        return consumption;
    }
    public PrimitiveStorageSet getNeeds()
    {
        PrimitiveStorageSet res = new PrimitiveStorageSet();
        foreach (var item in personal)
            res.add(item.Value.getRealNeeds(getOwner()));
        return res;
    }
    Value getNeeds(Product pro)
    {
        Value res = new Value(0f);
        foreach (var item in personal)
            res.add(item.Value.getRealNeeds(getOwner()).getStorage(pro));
        return res;
    }

    Dictionary<PopType, int> getAmountByTypes()
    {
        Dictionary<PopType, int> res = new Dictionary<PopType, int>();
        //int test;
        foreach (var next in personal)
        {
            //if (res.TryGetValue(next.Key.type, out test))
            //    test += next.Value.getSize();
            //else
            //    res.Add(next.Key.type, next.Value.getSize());
            if (res.ContainsKey(next.Key.popType))
                res[next.Key.popType] += next.Value.getSize();
            else
                res.Add(next.Key.popType, next.Value.getSize());
        }
        return res;
    }
    /// <summary>
    /// howMuchShouldBeInSecondArmy - procent of this army. Returns second army
    /// </summary>
    internal Army balance(Army secondArmy, Procent howMuchShouldBeInSecondArmy)
    {
        //if (howMuchShouldBeInSecondArmy.get() == 1f)
        //{
        //    secondArmy.joinin(this);
        //    //this.personal.Clear();
        //}
        //else
        {
            //Army sumArmy = new Army();
            //sumArmy.add(this);
            this.joinin(secondArmy);
            int secondArmyExpectedSize = howMuchShouldBeInSecondArmy.getProcent(this.getSize());

            //secondArmy.clear();

            int needToFullFill = secondArmyExpectedSize;
            while (needToFullFill > 0)
            {
                var corpsToBalance = this.getBiggestCorpsSmallerThan(needToFullFill);
                if (corpsToBalance == null)
                    break;
                else
                    this.move(corpsToBalance, secondArmy);
                needToFullFill = secondArmyExpectedSize - secondArmy.getSize();
            }

        }
        return secondArmy;
    }
    internal Army balance(Procent howMuchShouldBeInSecondArmy)
    {
        //if (howMuchShouldBeInSecondArmy.get() == 1f)
        //{
        //    return this;
        //    //this.personal.Clear();
        //}
        //else
        {
            Army secondArmy = new Army(this.getOwner());

            //Army sumArmy = new Army();
            //sumArmy.add(this);
            //this.joinin(secondArmy);
            int secondArmyExpectedSize = howMuchShouldBeInSecondArmy.getProcent(this.getSize());

            //secondArmy.clear();

            int needToFullFill = secondArmyExpectedSize;
            while (needToFullFill > 0)
            {
                var corpsToBalance = this.getBiggestCorpsSmallerThan(needToFullFill);
                if (corpsToBalance == null)
                    break;
                else
                    this.move(corpsToBalance, secondArmy);
                needToFullFill = secondArmyExpectedSize - secondArmy.getSize();
            }
            return secondArmy;
        }
    }
    //internal Army getVirtualArmy(Procent howMuchShouldBeInSecondArmy)
    //{
    //    if (howMuchShouldBeInSecondArmy.get() == 1f)
    //    {
    //        return this;
    //        //this.personal.Clear();
    //    }
    //    else
    //    {
    //        Army secondArmy = new Army(this.getOwner());
    //        //Army sumArmy = new Army();
    //        //sumArmy.add(this);
    //        //this.joinin(secondArmy);
    //        int secondArmyExpectedSize = howMuchShouldBeInSecondArmy.getProcent(this.getSize());

    //        //secondArmy.clear();

    //        int needToFullFill = secondArmyExpectedSize;
    //        while (needToFullFill > 0)
    //        {
    //            var corpsToBalance = this.getBiggestCorpsSmallerThan(needToFullFill);
    //            if (corpsToBalance == null)
    //                break;
    //            else
    //                //this.move(corpsToBalance, secondArmy);
    //                secondArmy.add(new Corps(corpsToBalance));
    //            needToFullFill = secondArmyExpectedSize - secondArmy.getSize();
    //        }
    //        return secondArmy;
    //    }

    //}
    internal BattleResult attack(Province prov)
    {
        var enemy = prov.getCountry();
        if (enemy == Country.NullCountry)
            prov.mobilize();
        else
            enemy.staff.mobilize(enemy.ownedProvinces);
        enemy.staff.consolidateArmies();
        return attack(enemy.getDefenceForces());
    }
    /// <summary>
    /// returns true if attacker is winner
    /// </summary>    
    BattleResult attack(Army defender)
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
    public void setOwner(Country country)
    {
        owner = country;
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
                {
                    var strenghtModifier = getStrenghtModifier();
                    var corpsStrenght = corp.Value.getType().getStrenght();
                    if (corpsStrenght * strenghtModifier > 0)//(corp.Value.getType().getStrenght() > 0f)
                    {
                        streghtLoss = corp.Value.getStrenght(this) * (lossStrenght / totalStrenght);
                        menLoss = Mathf.RoundToInt(streghtLoss / (corpsStrenght * strenghtModifier)); // corp.Value.getType().getStrenght());

                        totalMenLoss += corp.Value.TakeLoss(menLoss);
                    }
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
        return modifierStrenght.getModifier(this);
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


    //internal void clearEmpty()
    //{
    //    personal.
    //}
    private Corps getBiggestCorpsSmallerThan(int secondArmyExpectedSize)
    {

        var smallerCorps = personal.Where(x => x.Value.getSize() <= secondArmyExpectedSize);
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



    internal void sendTo(Province province)
    {
        destination = province;
    }

    internal Province getDestination()
    {
        return destination;
    }

    internal void setStatisticToZero()
    {
        foreach (var corps in personal)
            corps.Value.setStatisticToZero();
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

        if (!attacker.isAI() && isAttackerWon())
        {
            //.Append(" owned by ").Append(place.getCountry())
            sb.Append("Our glorious army attacked ").Append(place)
                .Append(" with army of ").Append(attackerArmy).Append(" men.");
            sb.Append(" Modifiers: ").Append(attackerBonus);
            sb.Append("\n\nWhile enemy had ").Append(defenderArmy).Append(" men. Modifiers:  ").Append(defenderBonus);
            sb.Append("\n\nWe won, enemy lost all men and we lost ").Append(attackerLoss).Append(" men");
            sb.Append("\nProvince ").Append(place).Append(" is our now!");
            // sb.Append("\nDate is ").Append(Game.date);
            new Message("We won a battle!", sb.ToString(), "Fine");
        }
        else
        if (!defender.isAI()&& isDefenderWon())
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
            if (!attacker.isAI() && isDefenderWon())
        {
            //.Append(" owned by ").Append(place.getCountry())
            sb.Append("Our glorious army attacked ").Append(place)
                .Append(" with army of ").Append(attackerArmy).Append(" men");
            sb.Append(" Modifiers: ").Append(attackerBonus);
            sb.Append("\n\nWhile enemy had ").Append(defenderArmy).Append(" men. Modifiers:  ").Append(defenderBonus);
            sb.Append("\n\nWe lost, our invasion army is destroyed, while enemy lost ").Append(defenderLoss).Append(" men");
            // sb.Append("\nDate is ").Append(Game.date);
            new Message("We lost a battle!", sb.ToString(), "Fine");
        }
        else
            if (!defender.isAI() && isAttackerWon())

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