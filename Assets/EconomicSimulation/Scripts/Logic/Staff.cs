using System;
using System.Collections.Generic;
using System.Linq;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represents any military commander structure
    /// </summary>
    public abstract class Staff : Consumer, IWayOfLifeChange
    {
        private List<Army> allArmies = new List<Army>();
        
        /// <summary>
        /// how much armies created ever
        /// </summary>
        public int armyCount;
        public Texture2D Flag { get; protected set; }
        //protected Country place; //todo change class
        protected Staff(Country place) : base(place)
        {
            
        }

        /// <summary>
        /// Sum of existing armies men + unmobilized reserve
        /// </summary>
        /// <returns></returns>
        //todo performance hit 7% 420 calls 1.4mb 82 ms
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
            //if (Rand.random2.Next(20) == 1)
            //    ;
        }

        public float howMuchCanMobilize(Staff againstWho)
        {
            float result = 0f;
            foreach (var province in Country.AllProvinces)
                foreach (var pop in province.AllPops)
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
            int size = getDefenceForces();
            //if (defArmy != null)
            //    size = defArmy.getSize();
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
        //public Army consolidateArmies()
        //{
        //    Army consolidatedArmy = new Army(this);
        //    if (allArmies.Count == 1)
        //        return allArmies[0];
        //    else
        //    {
        //        if (allArmies.Count > 0)
        //        {
        //            foreach (Army next in allArmies)
        //                if (next.getDestination() == null)
        //                {
        //                    //consolidatedArmy.setOwner(next.getOwner());
        //                    consolidatedArmy.joinin(next);
        //                }
        //            //if (addConsolidatedArmyInList)
        //            allArmies.Add(consolidatedArmy);
        //            allArmies.RemoveAll(army => army.getSize() == 0);// && army != country.sendingArmy); // don't remove sending army. Its personal already transfered to Home army
        //        }
        //    }
        //    return consolidatedArmy;

        //    //source.RemoveAll(armies => armies.getDestination() == null && armies != country.homeArmy && armies != country.sendingArmy);
        //    //allArmies.RemoveAll(army => army.getSize() == 0);// && army != country.sendingArmy); // don't remove sending army. Its personal already transfered to Home army
        //}

        //public void mobilize()
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
        public void mobilize(IEnumerable<Province> source)
        {
            foreach (var province in source)
            {
                // mirrored in Army
                if (province.AllPops.Any(x=>x.Type.canMobilize(this) && x.howMuchCanMobilize(this, null) > 0))
                    //if (pop.Type.canMobilize(this) && pop.howMuchCanMobilize(this, null) > 0) 
                    {
                        armyCount++;
                        Army newArmy = new Army(this, province, this + "'s " + armyCount.ToString() + "th");                        
                    }
            }
            //consolidateArmies();
        }

        public void addArmy(Army army)
        {
            allArmies.Add(army);
        }

        //public void demobilize()
        //{
        //    foreach (var item in allArmies.ToList())
        //    {
        //        item.demobilize();
        //    }
        //    //allArmies.Clear();
        //}

        public void demobilize(Func<Corps, bool> predicate=null)
        {
            foreach (Army nextArmy in allArmies.ToList())
            {
                nextArmy.demobilize(predicate);
            }
            //allArmies.RemoveAll(army => army.getSize() == 0);
        }

        public void rebelTo(Func<Corps, bool> popSelector, Movement movement)
        {
            allArmies.ForEach(x => x.rebelTo(popSelector, movement));
        }

        public override IEnumerable<Storage> getRealAllNeeds()
        {
            //StorageSet res = new StorageSet();
            //foreach (var item in allArmies)
            //    res.Add(item.getNeeds());

            // assuming all corps has same needs
            var res = PopType.Soldiers.getMilitaryNeedsPer1000Men(Country);
            var multiplier = getAllArmiesSize() / 1000f;
            res.Multiply(multiplier);
            return res;
        }

        public virtual void sendAllArmies(Province possibleTarget)
        {
            allArmies.PerformAction(x => x.SetPathTo(possibleTarget));
            //consolidateArmies().balance(procent).sendTo(possibleTarget);
        }

        public override void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            allArmies.ForEach(x => x.setStatisticToZero());
        }

        public IEnumerable<Army> AllArmies()
        {
            foreach (var army in allArmies)
                yield return army;
        }

        public IEnumerable<Corps> getAllCorps()
        {
            foreach (var army in allArmies)
                foreach (var corps in army.getCorps())
                    yield return corps;
        }

        //public IEnumerable<Army> getAttackingArmies()
        //{
        //    foreach (var army in allArmies)
        //        if (army.getDestination() != null)
        //            if (army.getDestination().Country != army.getOwner())
        //                yield return army;
        //            else
        //                army.sendTo(null); // go home
        //}

        /// <summary>
        /// returns Home army
        /// </summary>
        /// <returns></returns>
        public int getDefenceForces()
        {
            return allArmies.Sum(x => x.getSize());
            //Army a = allArmies.Find(x => x.getSize() > 0 && x.getDestination() == null);
            //if (a == null)
            //    return new Army(this);
            //else
            //    return a;
        }

       

        /// <summary>
        /// Just a place holder, never intended to call. Just need it to record battle deaths
        /// </summary>
        public ReadOnlyValue getLifeQuality(PopUnit pop)
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

        //public Army getVirtualArmy(Procent procent)
        //{
        //    Army virtualArmy = consolidateArmies(false).getVirtualArmy(procent);
        //    return virtualArmy;
        //}
        public void KillArmy(Army army)
        {
            army.Deselect();
            army.Province.RemoveArmy(army);
            allArmies.Remove(army);
            World.DayPassed -= army.OnMoveArmy;
            UnityEngine.Object.Destroy(army.unit.gameObject);
            Game.provincesToRedrawArmies.Add(army.Province);
            //Debug.Log("Killed army " + army);
        }
        
    }
}