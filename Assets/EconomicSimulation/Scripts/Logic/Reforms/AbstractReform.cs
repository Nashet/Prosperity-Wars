using Nashet.Conditions;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nashet.EconomicSimulation.Reforms
{
    public abstract class AbstractReform : Component<Country>, INameable, ISortableName, IClickable
    {
        protected readonly string description;
        protected readonly string name;
        protected readonly float nameWeight;
        public int ShowOrder { get; protected set; }

        protected readonly List<IReformValue> possibleValues;

        public IReformValue Value { get; protected set; }

        protected AbstractReform(string name, string description, Country country, int showOrder, List<IReformValue> possibleValues) : base(country)
        {
            this.name = name;
            if (name != null)
                nameWeight = name.GetWeight();
            this.ShowOrder = showOrder;
            this.description = description;
            country.Politics.RegisterReform(this);
            this.possibleValues = possibleValues;

            //foreach (var item in possibleValues)
            //{
            //    new Condition(x => (x as Country).taxationForPoor.isThatReformEnacted(nextID), "Previous reform enacted", true);
            //    var totalSteps = 11;
            //    var previousID = ID - 1;
            //    var nextID = ID + 1;
            //    if (previousID >= 0 && nextID < totalSteps)
            //        condition.add(new Condition(x => (x as Country).taxationForPoor.isThatReformEnacted(previousID)
            //        || (x as Country).taxationForPoor.isThatReformEnacted(nextID), "Previous reform enacted", true));
            //    else if (nextID < totalSteps)
            //        condition.add(new Condition(x => (x as Country).taxationForPoor.isThatReformEnacted(nextID), "Previous reform enacted", true));
            //    else if (previousID >= 0)
            //        condition.add(new Condition(x => (x as Country).taxationForPoor.isThatReformEnacted(previousID), "Previous reform enacted", true));
            //}
        }
        public virtual float getVotingPower(PopUnit forWhom)
        {
            return Value.getVotingPower(forWhom);
        }

        public virtual Procent LifeQualityImpact { get { return Value.LifeQualityImpact; } }
        //public abstract bool isAvailable(Country country);


        //public abstract bool canHaveThatValue(AbstractNamdRfrmValue abstractNamdRfrmValue);
        public virtual void OnReformEnactedInProvince(Province province)
        { }
        public override bool Equals(Object another)
        {
            if (ReferenceEquals(another, null))
                throw new ArgumentNullException();
            return another is IReformValue && this == (IReformValue)another
                || another is AbstractReform && this == (AbstractReform)another;
        }
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
        public static bool operator ==(AbstractReform x, IReformValue y)
        {
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                throw new ArgumentNullException();
            return x.Value == y;
        }
        public static bool operator !=(AbstractReform x, IReformValue y)
        {
            return !(x == y);
        }

        public static bool operator ==(AbstractReform x, AbstractReform y)
        {
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                throw new ArgumentNullException();
            return x.Value == y.Value;
        }

        public static bool operator !=(AbstractReform x, AbstractReform y)
        {

            return !(x == y);
        }

        public virtual void SetValue(AbstractReform reform)
        {
            SetValue(reform.Value);
        }
        public virtual void SetValue(IReformValue reformValue)
        {
            foreach (PopUnit pop in owner.Provinces.AllPops)
                if (pop.getSayingYes(reformValue))
                {
                    pop.loyalty.Add(Options.PopLoyaltyBoostOnDiseredReformEnacted);
                    pop.loyalty.clamp100();
                }
            var isThereSuchMovement = owner.Politics.AllMovements.FirstOrDefault(x => x.getGoal() == reformValue);
            if (isThereSuchMovement != null)
            {
                isThereSuchMovement.onRevolutionWon(false);
            }
            Value = reformValue;
        }

        public virtual string FullName
        {
            get { return description; }
        }

        public virtual string ShortName
        {
            get { return name; }
        }

        /// <summary>
        /// Gives value of reform. not type
        /// </summary>        
        public override string ToString()
        {
            return Value.ToString();
        }

        public virtual void OnClicked()
        {
            MainCamera.politicsPanel.selectReform(this);
        }

        public virtual float NameWeight
        {
            get
            {
                return nameWeight;
            }
        }
        public virtual IEnumerable<IReformValue> AllPossibleValues
        {
            get
            {
                foreach (var item in possibleValues)
                {
                    yield return item;
                }
            }
        }
        public bool IsMoreConservativeThan(AbstractReformValue anotherReform)
        {
            return Value.IsMoreConservativeThan(anotherReform);
        }
    }
}