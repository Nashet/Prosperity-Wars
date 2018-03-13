using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System;
using Nashet.UnityUIUtils;
using Nashet.Conditions;
using Nashet.ValueSpace;
using Nashet.Utils;
namespace Nashet.EconomicSimulation
{
    public class Army
    {
        static Modifier modifierInDefense = new Modifier(x => (x as Army).isInDefense(), "Is in defense", 0.5f, false);
        //static Modifier modifierDefenseInMountains = new Modifier(x => (x as Army).isInDefense() && (x as Army).getDestination()!=null && (x as Army).getDestination().getTerrain() == TerrainTypes.Mountains, "Defense in mountains", 0.2f, false);
        static Modifier modifierMorale = new Modifier(x => (x as Army).GetAverageCorps(y => y.getMorale()).get(), "Morale", 1f, true);

        static Modifier modifierHorses = new Modifier(x => (x as Army).getHorsesSupply(), "Horses", 0.5f, false);
        static Modifier modifierColdArms = new Modifier(x => (x as Army).getColdArmsSupply(), "Cold arms", 1f, false);
        static Modifier modifierFirearms = new Modifier(x => (x as Army).getEquippedFirearmsSupply(), "Charged Firearms", 2f, false);
        static Modifier modifierArtillery = new Modifier(x => (x as Army).getEquippedArtillerySupply(), "Charged Artillery", 1f, false);

        static Modifier modifierCars = new Modifier(x => (x as Army).getEquippedCarsSupply(), "Fueled Cars", 2f, false);
        static Modifier modifierTanks = new Modifier(x => (x as Army).getEquippedTanksSupply(), "Fueled & charged Tanks", 1f, false);
        static Modifier modifierAirplanes = new Modifier(x => (x as Army).getEquippedAirplanesSupply(), "Fueled & charged Airplanes", 1f, false);
        static Modifier modifierLuck = new Modifier(x => (float)Math.Round(UnityEngine.Random.Range(-0.5f, 0.5f), 2), "Luck", 1f, false);
        static Modifier modifierEducation = new Modifier(x => (x as Army).GetAverageCorps(y => y.getPopUnit().Education).RawUIntValue, "Education", 1f / Procent.Precision, false);


        private readonly Dictionary<PopUnit, Corps> personal;
        Province destination;
        private readonly Staff owner;

        private float getHorsesSupply()
        {
            if (getOwner().Country.Invented(Invention.Domestication))
                return new Procent(getConsumption(Product.Cattle), getNeeds(Product.Cattle), false).get();
            else return 0f;
        }
        private float getColdArmsSupply()
        {
            if (getOwner().Country.Invented(Product.ColdArms))
                return new Procent(getConsumption(Product.ColdArms), getNeeds(Product.ColdArms), false).get();
            else return 0f;
        }
        private float getEquippedFirearmsSupply()
        {
            if (getOwner().Country.Invented(Product.Firearms))
                return Mathf.Min(
             new Procent(getConsumption(Product.Firearms), getNeeds(Product.Firearms), false).get(),
             new Procent(getConsumption(Product.Ammunition), getNeeds(Product.Ammunition), false).get()
             );
            else return 0f;
        }
        private float getEquippedArtillerySupply()
        {
            if (getOwner().Country.Invented(Product.Artillery))
                return Mathf.Min(
             new Procent(getConsumption(Product.Artillery), getNeeds(Product.Artillery), false).get(),
             new Procent(getConsumption(Product.Ammunition), getNeeds(Product.Ammunition), false).get()
             );
            else return 0f;
        }
        private float getEquippedCarsSupply()
        {
            if (getOwner().Country.Invented(Product.Cars))
                return Mathf.Min(
             new Procent(getConsumption(Product.Cars), getNeeds(Product.Cars), false).get(),
             new Procent(getConsumption(Product.MotorFuel), getNeeds(Product.MotorFuel), false).get()
             );
            else return 0f;
        }
        private float getEquippedTanksSupply()
        {
            if (getOwner().Country.Invented(Product.Tanks))
                return Mathf.Min(
             new Procent(getConsumption(Product.Tanks), getNeeds(Product.Tanks), false).get(),
             new Procent(getConsumption(Product.MotorFuel), getNeeds(Product.MotorFuel), false).get(),
             new Procent(getConsumption(Product.Ammunition), getNeeds(Product.Ammunition), false).get()
             );
            else return 0f;
        }
        private float getEquippedAirplanesSupply()
        {
            if (getOwner().Country.Invented(Product.Airplanes))
                return Mathf.Min(
             new Procent(getConsumption(Product.Airplanes), getNeeds(Product.Airplanes), false).get(),
             new Procent(getConsumption(Product.MotorFuel), getNeeds(Product.MotorFuel), false).get(),
             new Procent(getConsumption(Product.Ammunition), getNeeds(Product.Ammunition), false).get()
             );
            else return 0f;
        }

        static ModifiersList modifierStrenght = new ModifiersList(new List<Condition>()
        {
        //modifierDefenseInMountains
            Modifier.modifierDefault1, modifierInDefense,  modifierMorale, modifierHorses, modifierColdArms,
        modifierFirearms, modifierArtillery, modifierCars, modifierTanks, modifierAirplanes, modifierLuck,
        modifierEducation
        });
        // private Army consolidatedArmy;

        public Army(Staff owner)
        {
            owner.addArmy(this);
            personal = new Dictionary<PopUnit, Corps>();
            this.owner = owner;
        }
        //public Army(Army consolidatedArmy) : this(consolidatedArmy.getOwner())
        //{ }    

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
                CorpsPool.ReleaseObject(corps);
            }
        }
        public void demobilize(Func<Corps, bool> predicate)
        {
            foreach (var corps in personal.Values.ToList())
                if (predicate(corps))
                {
                    personal.Remove(corps.getPopUnit());
                    CorpsPool.ReleaseObject(corps);
                }
        }
        internal void rebelTo(Func<Corps, bool> popSelector, Movement movement)
        {
            var takerArmy = movement.getDefenceForces();
            foreach (var corps in personal.Values.ToList())
                if (popSelector(corps))
                {
                    personal.Remove(corps.getPopUnit());
                    takerArmy.add(corps);
                }
        }
        public void consume()
        {
            personal.PerformAction(corps => corps.Value.consume(getOwner().Country));
        }
        public Procent GetAverageCorps(Func<Corps, Procent> selector)
        {
            Procent result = new Procent(0);
            int calculatedSize = 0;
            foreach (var item in personal)
            {
                result.AddPoportionally(calculatedSize, item.Value.getSize(), selector(item.Value));
                calculatedSize += item.Value.getSize();
            }
            return result;
        }
        //public Procent getAverageMorale()
        //{
        //    Procent result = new Procent(0);
        //    int calculatedSize = 0;
        //    foreach (var item in personal)
        //    {
        //        result.addPoportionally(calculatedSize, item.Value.getSize(), item.Value.getMorale());
        //        calculatedSize += item.Value.getSize();
        //    }
        //    return result;
        //}
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
                        CorpsPool.ReleaseObject(corpsToTransfert.Value);
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
        internal string getName()
        {
            StringBuilder sb = new StringBuilder();

            int size = getSize();
            if (size > 0)
            {
                //foreach (var next in getAmountByTypes())
                //    sb.Append(next.Value).Append(" ").Append(next.Key).Append(", ");
                sb.Append(getAmountByTypes().getString(": ", ", "));
                sb.Append(", Total size: ").Append(getSize());
                sb.Append(", Morale: ").Append(GetAverageCorps(x => x.getMorale()));
                sb.Append(", Provision: ").Append(getConsumption());
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
                res.Add(item.Value.getConsumption(prod));
            return res;
        }
        private StorageSet getConsumption()
        {
            var consumption = new StorageSet();
            foreach (var item in personal)
                consumption.Add(item.Value.getConsumption());


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
        public List<Storage> getNeeds()
        {
            // StorageSet used for faster calculation
            StorageSet res = new StorageSet();
            foreach (var item in personal)
                res.Add(item.Value.getRealNeeds(getOwner().Country));
            return res.ToList();
        }
        Value getNeeds(Product product)
        {
            Value res = new Value(0f);
            foreach (var item in personal)
                res.Add(item.Value.getRealNeeds(getOwner().Country, product));
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
                if (res.ContainsKey(next.Key.Type))
                    res[next.Key.Type] += next.Value.getSize();
                else
                    res.Add(next.Key.Type, next.Value.getSize());
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
                int secondArmyExpectedSize = howMuchShouldBeInSecondArmy.getProcentOf(this.getSize());

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
                int secondArmyExpectedSize = howMuchShouldBeInSecondArmy.getProcentOf(this.getSize());

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
        
        internal BattleResult attack(Province prov)
        {
            var enemy = prov.Country;
            if (enemy == World.UncolonizedLand)
                prov.mobilize();
            else
                enemy.mobilize(enemy.getAllProvinces());
            enemy.consolidateArmies();
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
            var attackerModifier = Army.modifierStrenght.getModifier(attacker, out attackerBonus);
            string defenderBonus;
            var defenderModifier = Army.modifierStrenght.getModifier(defender, out defenderBonus);

            if (attacker.getStrenght(attackerModifier) > defender.getStrenght(defenderModifier))
            {
                attackerWon = true;
                float winnerLossUnConverted;
                if (attacker.getStrenght(attackerModifier) > 0f)
                    winnerLossUnConverted = defender.getStrenght(defenderModifier) * defender.getStrenght(defenderModifier) / attacker.getStrenght(attackerModifier);
                else
                    winnerLossUnConverted = 0f;
                int attackerLoss = attacker.takeLossUnconverted(winnerLossUnConverted, defender.owner);
                int loserLoss = defender.takeLoss(defender.getSize(), attacker.owner);

                result = new BattleResult(attacker.getOwner(), defender.getOwner(), initialAttackerSize, attackerLoss
                , initialDefenderSize, loserLoss, attacker.destination, attackerWon, attackerBonus, defenderBonus);
            }
            else if (attacker.getStrenght(attackerModifier) == defender.getStrenght(defenderModifier))
            {
                attacker.takeLoss(attacker.getSize(), defender.owner);
                defender.takeLoss(defender.getSize(), attacker.owner);

                var r = new BattleResult(attacker.getOwner(), defender.getOwner(), attacker.getSize(), attacker.getSize(), defender.getSize(), defender.getSize(),
                    attacker.destination, false, attackerBonus, defenderBonus);
                return r;
            }
            else //defender.getStrenght() > attacker.getStrenght()  
            {

                attackerWon = false;
                float winnerLossUnConverted;
                if (defender.getStrenght(defenderModifier) > 0f)
                    winnerLossUnConverted = attacker.getStrenght(attackerModifier) * attacker.getStrenght(attackerModifier) / (defender.getStrenght(defenderModifier));
                else
                    winnerLossUnConverted = 0f;
                int defenderLoss = defender.takeLossUnconverted(winnerLossUnConverted, attacker.owner);

                int attackerLoss = attacker.takeLoss(attacker.getSize(), defender.owner);
                result = new BattleResult(attacker.getOwner(), defender.getOwner(), initialAttackerSize, attackerLoss
                , initialDefenderSize, defenderLoss, attacker.destination, attackerWon, attackerBonus, defenderBonus);
            }
            return result;
        }
        public Staff getOwner()
        {
            return owner;
        }
        //public void setOwner(Staff country)
        //{
        //    owner = country;
        //}

        private int takeLoss(int loss, IWayOfLifeChange reason)
        {
            int totalLoss = 0;
            if (loss > 0)
            {
                int totalSize = getSize();
                int currentLoss;
                foreach (var corp in personal.Values.ToList())
                    if (totalSize > 0)
                    {
                        currentLoss = Mathf.RoundToInt(corp.getSize() * loss / (float)totalSize);
                        corp.TakeLoss(currentLoss, reason);
                        totalLoss += currentLoss;
                    }
            }
            return totalLoss;
        }
        /// <summary>
        /// argument is NOT men, but they strength
        /// </summary>    
        private int takeLossUnconverted(float lossStrenght, IWayOfLifeChange reason)
        {
            int totalMenLoss = 0;
            if (lossStrenght > 0f)
            {
                float streghtLoss;
                int menLoss;
                var armyStrenghtModifier = getStrenghtModifier();
                float totalStrenght = getStrenght(armyStrenghtModifier);
                if (totalStrenght > 0f)
                {
                    foreach (var corp in personal.ToList())
                    {

                        var corpsStrenght = corp.Value.Type.getStrenght();
                        if (corpsStrenght * armyStrenghtModifier > 0)//(corp.Value.Type.getStrenght() > 0f)
                        {
                            streghtLoss = corp.Value.getStrenght(this, armyStrenghtModifier) * (lossStrenght / totalStrenght);
                            menLoss = Mathf.RoundToInt(streghtLoss / (corpsStrenght * armyStrenghtModifier)); // corp.Value.Type.getStrenght());

                            totalMenLoss += corp.Value.TakeLoss(menLoss, reason);
                        }
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
        private float getStrenght(float armyStrenghtModifier)
        {
            float result = 0;
            foreach (var c in personal)
                result += c.Value.getStrenght(this, armyStrenghtModifier);
            return result;
        }

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
}