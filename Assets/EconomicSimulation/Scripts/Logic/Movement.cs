using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;

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
        private int siegeCapitalTurns;

        //private Movement(PopUnit firstPop, Country place) : base(place)
        //{
        //    members.Add(firstPop);
        //    Country.movements.Add(this);
        //}

        private Movement(AbstractReform reform, AbstractReformValue goal, PopUnit firstPop, Country place) : base(place)// : this(firstPop, place)
        {
            members.Add(firstPop);
            Country.movements.Add(this);
            targetReform = reform;
            targetReformValue = goal;
            Flag = Nashet.Flag.Rebels;
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
                if (Rand.Get.Next(Options.PopChangeMovementRate) == 1)
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

        private void add(PopUnit pop)
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
        public AbstractReform getReformType()
        {
            return targetReform;
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
                res += item.population.Get();
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
                result.AddPoportionally(calculatedSize, item.population.Get(), item.loyalty);
                calculatedSize += item.population.Get();
            }
            return result;
        }

        public override void consumeNeeds()
        {
            throw new NotImplementedException();
        }

        private void killMovement()
        {
            foreach (var item in getAllArmies().ToArray())
            {
                item.demobilize();
            }
            foreach (var pop in members.ToArray())
            {
                leave(pop);
                //pop.setMovement(null);
            }
            Country.movements.Remove(this);
            //members.Clear();
        }
        internal void OnSeparatistsWon()
        {
            var separatists = getGoal() as Separatism;
            separatists.Country.onSeparatismWon(country);
            if (!separatists.Country.isAI())
                Message.NewMessage("", "Separatists won revolution - " + separatists.Country.FullName, "hmm", false, separatists.Country.Capital.getPosition());
        }
        internal void onRevolutionWon(bool setReform)
        {
            siegeCapitalTurns = 0;
            _isInRevolt = false;
            if (targetReform == null) // meaning separatism
            {
                OnSeparatistsWon();
            }
            else
            {
                if (!Country.isAI())
                    Message.NewMessage("Rebels won", "Now you have " + targetReformValue, "Ok", false, Game.Player.Capital.getPosition());
                
                if (setReform)
                    targetReform.setValue(getGoal());//to avoid recursion            
            }
            foreach (var pop in members)
            {
                pop.loyalty.Add(Options.PopLoyaltyBoostOnRevolutionWon);
                pop.loyalty.clamp100();
            }
            killMovement();
        }

        internal void onRevolutionLost()
        {
            foreach (var pop in members)
            {
                pop.loyalty.Add(Options.PopLoyaltyBoostOnRevolutionLost);
                pop.loyalty.clamp100();
            }
            _isInRevolt = false;
            //demobilize();
        }

        internal bool isEmpty()
        {
            return members.Count == 0;
        }

        public void Simulate()
        {
            if (!isValidGoal())
            {
                killMovement();
                return;
            }
            base.simulate();
            //assuming movement already won or lost
            //if (isInRevolt())
            //{
            //    _isInRevolt = false;
            //    demobilize();
            //}



            //&& canWinUprising())
            if (isInRevolt())
            {
                if (getAllArmies().Count() == 0)
                    onRevolutionLost();
                if (getAllArmies().Any(x => x.Province == Country.Capital))
                    siegeCapitalTurns++;
                else
                    siegeCapitalTurns = 0;
                if (siegeCapitalTurns > Options.ArmyTimeToOccupy)
                {
                    
                    //if (targetReform == null) // meaning separatism
                        onRevolutionWon(true);
                    //else
                    //    getReformType().setValue(getGoal()); // just to avoid recursion
                }
            }
            else
            {
                if (getRelativeStrength(Country).isBiggerOrEqual(Options.MovementStrenthToStartRebellion)
                && getAverageLoyalty().isSmallerThan(Options.PopLoyaltyLimitToRevolt)
                    //&& getStrength(Country) > Options.PopMinStrengthToRevolt
                    )//&& isValidGoal()) do it in before battle
                {
                    StartUprising();
                }

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
        private void StartUprising()
        {
            //revolt
            if (country == Game.Player && !Game.Player.isAI())
                Message.NewMessage("Revolution is on", "People rebelled demanding " + targetReformValue + "\n\nTheir army is moving to our capital", "Ok", false, Game.Player.Capital.getPosition());

            Country.rebelTo(x => x.getPopUnit().getMovement() == this, this);

            mobilize(country.getAllProvinces());

            //if (targetReformValue is Separatism)
            //    ;
            //else
            sendAllArmies(country.Capital, Procent.HundredProcent);
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