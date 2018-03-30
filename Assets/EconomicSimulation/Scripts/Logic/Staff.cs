﻿using System;
using System.Collections.Generic;
using Nashet.Utils;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represents any military commander structure
    /// </summary>
    public abstract class Staff : Consumer, IWayOfLifeChange
    {
        private List<Army> allArmies = new List<Army>();

        //protected Country place; //todo change class
        protected Staff(Country place) : base(place)
        {
            //this.place = place;
        }

        /// <summary>
        /// Sum of existing armies men + unmobilized reserve
        /// </summary>
        /// <returns></returns>
        public float getStrengthExluding(Staff againstWho)
        {
            return howMuchCanMobilize(againstWho) + getAllArmiesSize();
        }

        public override void simulate()
        {
            foreach (var item in getAllCorps())
            {
                item.reMobilize(this);
            }
            //if (Game.Random.Next(20) == 1)
            //    ;
        }

        public float howMuchCanMobilize(Staff againstWho)
        {
            float result = 0f;
            foreach (var province in country.getAllProvinces())
                foreach (var pop in province.GetAllPopulation())
                    if (pop.Type.canMobilize(this))
                        result += pop.howMuchCanMobilize(this, againstWho);
            return result;
        }

        public Procent getAverageMorale()
        {
            Procent result = new Procent(0);
            int calculatedSize = 0;
            foreach (var item in allArmies)
            {
                result.AddPoportionally(calculatedSize, item.getSize(), item.GetAverageCorps(x => x.getMorale()));
                calculatedSize += item.getSize();
            }
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

        //public Country Country
        //{
        //    return place;
        //}
        public bool isAI()
        {
            return this != Game.Player || (this == Game.Player && Game.isPlayerSurrended());
        }

        public bool IsHuman
        {
            get { return !isAI(); }
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
                            //consolidatedArmy.setOwner(next.getOwner());
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
        //        Army newArmy = new Army(Country);
        //        foreach (var item in province.allPopUnits)
        //            //if (item.Type.canMobilize() && item.howMuchCanMobilize(this) > 0)
        //                newArmy.add(item.mobilize(this));
        //    }
        //    consolidateArmies();
        //}
        internal void mobilize(IEnumerable<Province> source)
        {
            foreach (var province in source)
            {
                Army newArmy = new Army(this);
                foreach (var pop in province.GetAllPopulation())
                    if (pop.Type.canMobilize(this) && pop.howMuchCanMobilize(this, null) > 0)
                        //newArmy.add(item.mobilize(this));
                        newArmy.add(Corps.mobilize(this, pop));
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

        internal void rebelTo(Func<Corps, bool> popSelector, Movement movement)
        {
            allArmies.ForEach(x => x.rebelTo(popSelector, movement));
        }

        public override List<Storage> getRealAllNeeds()
        {
            //StorageSet res = new StorageSet();
            //foreach (var item in allArmies)
            //    res.Add(item.getNeeds());

            // assuming all corps has same needs
            var res = PopType.Soldiers.getMilitaryNeedsPer1000Men(Country);
            var multiplier = new Value(getAllArmiesSize() / 1000f);
            res.Multiply(multiplier);
            return res;
        }

        internal virtual void sendArmy(Province possibleTarget, Procent procent)
        {
            consolidateArmies().balance(procent).sendTo(possibleTarget);
        }

        public override void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            allArmies.ForEach(x => x.setStatisticToZero());
        }

        internal IEnumerable<Army> getAllArmies()
        {
            foreach (var army in allArmies)
                yield return army;
        }

        internal IEnumerable<Corps> getAllCorps()
        {
            foreach (var army in allArmies)
                foreach (var corps in army.getCorps())
                    yield return corps;
        }

        internal IEnumerable<Army> getAttackingArmies()
        {
            foreach (var army in allArmies)
                if (army.getDestination() != null)
                    if (army.getDestination().Country != army.getOwner())
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
            foreach (var country in World.getAllExistingCountries())
                if (country.isAlive() && country != World.UncolonizedLand)
                {
                    yield return country;
                    foreach (var staff in country.movements)
                        yield return staff;
                }
        }

        /// <summary>
        /// Just a plce holder, never intended to call
        /// </summary>

        public ReadOnlyValue getLifeQuality(PopUnit pop, PopType proposedType)
        {
            throw new NotImplementedException();
        }

        //public override void produce()
        //{
        //    throw new NotImplementedException();
        //}

        //public override void payTaxes()
        //{
        //    throw new NotImplementedException();
        //}

        //internal Army getVirtualArmy(Procent procent)
        //{
        //    Army virtualArmy = consolidateArmies(false).getVirtualArmy(procent);
        //    return virtualArmy;
        //}
    }
}