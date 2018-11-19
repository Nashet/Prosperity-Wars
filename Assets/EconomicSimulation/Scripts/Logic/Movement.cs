using Nashet.EconomicSimulation.Reforms;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nashet.EconomicSimulation
{
    //public class StaffOwner :Consumer
    //{
    //    protected readonly GeneralStaff staff;
    //}
    public class Movement : Staff, INameable
    {
        private readonly IReformValue targetReformValue;
        private readonly AbstractReform targetReformType;

        //private readonly Country separatism;
        private readonly List<PopUnit> members = new List<PopUnit>();

        private bool _isInRevolt;
        private int siegeCapitalTurns;

        //private Movement(PopUnit firstPop, Country place) : base(place)
        //{
        //    members.Add(firstPop);
        //    Country.movements.Add(this);
        //}

        private Movement(AbstractReform reform, IReformValue goal, PopUnit firstPop, Country place) : base(place)// : this(firstPop, place)
        {
            members.Add(firstPop);
            Country.Politics.RegisterMovement(this);
            targetReformType = reform;
            targetReformValue = goal;
            Flag = Nashet.Flag.Rebels;
        }

        public static void join(PopUnit pop)
        {
            if (pop.getMovement() == null)
            {
                var goal = pop.getMostImportantIssue();// getIssues().MaxByRandom(x => x.Value);
                //todo if it's null it should throw exception early
                //if (!goal.Equals(default(KeyValuePair<AbstractReform, IReformValue>)))
                //if (!ReferenceEquals(goal, null))
                {
                    //find reasonable goal and join
                    var found = pop.Country.Politics.AllMovements.FirstOrDefault(x => x.getGoal() == goal.Value);
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
                    pop.Country.Politics.RemoveMovement(pop.getMovement());
                }
                pop.setMovement(null);
            }
        }

        /// <summary>Need it for sorting</summary>
        //public int getID()
        //{
        //    if (getGoal() == null)//separatists
        //    {
        //        var separatists = targetReformValue as Separatism;
        //        return separatists.ID;
        //    }
        //    else
        //        return getGoal().ID;
        //}

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
            return targetReformValue.IsAllowed(Country, targetReformValue);
        }

        public IReformValue getGoal()
        {
            return targetReformValue;
        }
        public AbstractReform getReformType()
        {
            return targetReformType;
        }

        public override string ToString()
        {
            return "Movement for " + ShortName;
        }

        public string FullName
        {
            get
            {
                var sb = new StringBuilder();
                //.Append(targetReformType).Append(" ") adds reform type 
                sb.Append(ShortName).Append(", members: ").Append(getMembership()).Append(", avg. loyalty: ").Append(getAverageLoyalty()).Append(", rel. strength: ").Append(getRelativeStrength(Country));
                //sb.Append(", str: ").Append(getStregth(this));
                return sb.ToString();
            }
        }

        public string ShortName
        {
            get
            {
                var isUnempValue = targetReformValue as UnemploymentSubsidies.UnemploymentReformValue;
                if (isUnempValue == null)
                    return targetReformValue.ToString();
                else
                    return isUnempValue.ToString(Country.market);

            }
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
            foreach (var item in AllArmies().ToArray())
            {
                item.demobilize();
            }
            foreach (var pop in members.ToArray())
            {
                leave(pop);
                //pop.setMovement(null);
            }
            Country.Politics.RemoveMovement(this);
            //members.Clear();
        }
        public void OnSeparatistsWon()
        {
            var separatists = getGoal() as Separatism.Goal;
            separatists.separatismTarget.onSeparatismWon(Country);
            if (!Country.isAI())//separatists.C
                MessageSystem.Instance.NewMessage("", "Separatists won revolution - " + separatists.separatismTarget.FullName, "hmm", false, separatists.separatismTarget.Capital.Position);
        }
        public void onRevolutionWon(bool setReform)
        {
            siegeCapitalTurns = 0;
            _isInRevolt = false;
            if (ReferenceEquals(targetReformType, null)) // meaning separatism
            {
                OnSeparatistsWon();
            }
            else
            {
                if (setReform)
                {
                    targetReformType.SetValue(getGoal());//to avoid recursion            
                    if (!Country.isAI())
                        MessageSystem.Instance.NewMessage("Rebels won", "Now you have " + targetReformValue, "Ok", false, Game.Player.Capital.Position);
                }

            }
            foreach (var pop in members)
            {
                pop.loyalty.Add(Options.PopLoyaltyBoostOnRevolutionWon);
                pop.loyalty.clamp100();
            }
            killMovement();
        }

        public void onRevolutionLost()
        {
            foreach (var pop in members)
            {
                pop.loyalty.Add(Options.PopLoyaltyBoostOnRevolutionLost);
                pop.loyalty.clamp100();
            }
            _isInRevolt = false;
            //demobilize();
        }

        public bool isEmpty()
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
                if (AllArmies().Count() == 0)
                    onRevolutionLost();
                if (AllArmies().Any(x => x.Province == Country.Capital))
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
        //        if (!item.IsAlive)
        //    {
        //    }
        //}
        private void StartUprising()
        {
            //revolt
            if (Country == Game.Player && !Game.Player.isAI())
                MessageSystem.Instance.NewMessage("Revolution is on", "People rebelled demanding " + targetReformValue + "\n\nTheir army is moving to our capital", "Ok", false, Game.Player.Capital.Position);

            Country.rebelTo(x => x.getPopUnit().getMovement() == this, this);

            mobilize(Country.AllProvinces);

            //if (targetReformValue is Separatism)
            //    ;
            //else
            sendAllArmies(Country.Capital);
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