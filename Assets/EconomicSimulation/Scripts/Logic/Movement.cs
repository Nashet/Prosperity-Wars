using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Nashet.UnityUIUtils;
using Nashet.ValueSpace;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    //public class StaffOwner :Consumer
    //{
    //    protected readonly GeneralStaff staff;
    //}
    public class Movement : Staff, INameable
    {
        private readonly AbstractReformValue targetReformValue;
        private readonly AbstractReform targetReform;
        //private readonly Country separatism;
        private readonly List<PopUnit> members = new List<PopUnit>();
        private bool _isInRevolt;


        private Movement(PopUnit firstPop, Country place) : base(place)
        {
            members.Add(firstPop);
            Country.movements.Add(this);
        }
        private Movement(AbstractReform reform, AbstractReformValue goal, PopUnit firstPop, Country place) : this(firstPop, place)
        {
            this.targetReform = reform;
            this.targetReformValue = goal;
        }

        public static void join(PopUnit pop)
        {
            if (pop.getMovement() == null)
            {
                var goal = pop.getMostImportantIssue();
                if (!goal.Equals(default(KeyValuePair<AbstractReform, AbstractReformValue>)))
                {
                    //find reasonable goal and join
                    var found = pop.Country.movements.Find(x => x.getGoal() == goal.Value);
                    if (found == null)
                        pop.setMovement(new Movement(goal.Key, goal.Value, pop, pop.Country));
                    else
                    {
                        found.add(pop);
                        pop.setMovement(found);
                    }
                }
            }
            else // change movement
                if (Game.Random.Next(Options.PopChangeMovementRate) == 1)
            {
                leave(pop);
                join(pop);
            }
        }
        public static void leave(PopUnit pop)
        {
            if (pop.getMovement() != null)
            {
                pop.getMovement().demobilize(x => x.getPopUnit() == pop);
                pop.getMovement().members.Remove(pop);

                if (pop.getMovement().members.Count == 0)
                {
                    pop.getMovement().demobilize();
                    pop.Country.movements.Remove(pop.getMovement());
                }
                pop.setMovement(null);
            }
        }
        /// <summary>Need it for sorting</summary>
        public int getID()
        {
            if (getGoal() == null)//separatists
            {
                var separatists = targetReformValue as Separatism;
                return separatists.ID;
            }
            else
                return getGoal().ID;
        }
        void add(PopUnit pop)
        {
            members.Add(pop);
        }
        public bool isInRevolt()
        {
            return _isInRevolt;
        }
        public bool isValidGoal()
        {
            return targetReformValue.allowed.isAllTrue(Country, targetReformValue);
        }
        public AbstractReformValue getGoal()
        {
            return targetReformValue;
        }
        public override string ToString()
        {
            return "Movement for " + ShortName;
        }
        public string FullName
        {
            get
            {
                var sb = new StringBuilder(ShortName);
                sb.Append(", members: ").Append(getMembership()).Append(", avg. loyalty: ").Append(getAverageLoyalty()).Append(", rel. strength: ").Append(getRelativeStrength(Country));
                //sb.Append(", str: ").Append(getStregth(this));
                return sb.ToString();
            }
        }

        public string ShortName
        {
            get { return targetReformValue.ToString(); }
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


        //public bool canWinUprising()
        //{
        //    var defence = country.getDefenceForces();
        //    if (defence == null)
        //        return true;
        //    else
        //        return getMembership() > defence.getSize();
        //}


        private Procent getAverageLoyalty()
        {
            Procent result = new Procent(0);
            int calculatedSize = 0;
            foreach (var item in members)
            {
                result.AddPoportionally(calculatedSize, item.getPopulation(), item.loyalty);
                calculatedSize += item.getPopulation();
            }
            return result;
        }

        public override void consumeNeeds()
        {
            throw new NotImplementedException();
        }
        private void killMovement()
        {
            //foreach (var item in getAllArmies())
            //{
            //    item.demobilize();
            //}
            foreach (var pop in members.ToArray())
            {
                leave(pop);
                //pop.setMovement(null);
            }
            //members.Clear();
        }
        internal void onRevolutionWon()
        {
            //demobilize();
            //_isInRevolt = false;
            if (targetReform == null) // meaning separatism
            {
                var separatists = targetReformValue as Separatism;
                separatists.Country.onSeparatismWon(Country);
                if (!separatists.Country.isAI())
                    Message.NewMessage("", "Separatists won revolution - " + separatists.Country.FullName, "hmm", false);
            }
            else
                targetReform.setValue(targetReformValue);
            foreach (var pop in members)
            {
                pop.loyalty.Add(Options.PopLoyaltyBoostOnRevolutionWon);
                pop.loyalty.clamp100();
            }
            killMovement();
            //Country.movements.Remove(this);

        }

        internal void onRevolutionLost()
        {
            foreach (var pop in members)
            {
                pop.loyalty.Add(Options.PopLoyaltyBoostOnRevolutionLost);
                pop.loyalty.clamp100();
            }
            //_isInRevolt = false;
            //demobilize();
        }
        internal bool isEmpty()
        {
            return members.Count == 0;
        }
        public void simulate()
        {
            base.simulate();
            //assuming movement already won or lost
            if (isInRevolt())
            {
                _isInRevolt = false;
                demobilize();
            }
            if (!isValidGoal())
            {
                killMovement();
                return;
            }

            //&& canWinUprising())
            if (getRelativeStrength(Country).isBiggerOrEqual(Options.MovementStrenthToStartRebellion)
                    && getAverageLoyalty().isSmallerThan(Options.PopLoyaltyLimitToRevolt)
                    //&& getStrength(Country) > Options.PopMinStrengthToRevolt
                    )//&& isValidGoal()) do it in before battle
            {
                doRevolt();
            }
        }
        // clearing dead pops (0 population)
        //public void clearDeadPops()
        //{
        //    foreach (var item in members)
        //        if (!item.isAlive())
        //    {

        //    }
        //}
        private void doRevolt()
        {
            //revolt
            if (country == Game.Player && !Game.Player.isAI())
                Message.NewMessage("Revolution is coming", "People rebelled demanding " + targetReformValue + "\n\nTheir army is moving to our capital", "Ok", false);

            Country.rebelTo(x => x.getPopUnit().getMovement() == this, this);

            base.mobilize(country.ownedProvinces);

            sendArmy(country.Capital, Procent.HundredProcent);
            _isInRevolt = true;
        }
        /// <summary>
        /// new value
        /// </summary>
        /// <param name="toWhom"></param>
        /// <returns></returns>
        public Procent getRelativeStrength(Staff toWhom)
        {
            //var governmentHomeArmy = country.getDefenceForces();
            // Movement isToWhomMovement = toWhom as Movement;
            var thisStrenght = getStrengthExluding(toWhom); // null or not null

            // Movement isThisMovement = this as Movement;
            var toWhomStrenght = toWhom.getStrengthExluding(this);// null or not null

            if (toWhomStrenght == 0f)
            {
                if (thisStrenght == 0f)
                    return Procent.ZeroProcent.Copy();
                else
                    return Procent.Max999.Copy();
            }
            else
                return new Procent(thisStrenght, toWhomStrenght);
        }
    }
}

