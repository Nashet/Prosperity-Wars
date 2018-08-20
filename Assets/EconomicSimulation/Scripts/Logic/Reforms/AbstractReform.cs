using Nashet.Conditions;
using Nashet.EconomicSimulation;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;
namespace Nashet.EconomicSimulation.Reforms
{
    public abstract class AbstractReform : Component<Country>, INameable, ISortableName, IClickable
    {
        private readonly string description;
        protected readonly string name;
        protected readonly float nameWeight;

        public IReformValue value;
        protected readonly List<IReformValue> possibleValues;

        public AbstractReform(string name, string indescription, Country country, List<IReformValue> possibleValues) : base(country)
        {
            this.name = name;
            if (name != null)
                nameWeight = name.GetWeight();
            description = indescription;
            country.reforms.Add(this);
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
        public float getVotingPower(PopUnit forWhom)
        {
            return value.getVotingPower(forWhom);
        }

        public Procent LifeQualityImpact { get { return value.LifeQualityImpact; } }
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
            return x.value == y;
        }
        public static bool operator !=(AbstractReform x, IReformValue y)
        {
            return !(x == y);
        }

        public static bool operator ==(AbstractReform x, AbstractReform y)
        {
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                throw new ArgumentNullException();
            return x.value == y.value;
        }
        public static bool operator !=(AbstractReform x, AbstractReform y)
        {

            return !(x == y);
        }

        public virtual void SetValue(AbstractReform reform)
        {
            SetValue(reform.value);
        }
        public virtual void SetValue(IReformValue reformValue)
        {
            foreach (PopUnit pop in owner.Provinces.AllPops)
                if (pop.getSayingYes(reformValue))
                {
                    pop.loyalty.Add(Options.PopLoyaltyBoostOnDiseredReformEnacted);
                    pop.loyalty.clamp100();
                }
            var isThereSuchMovement = owner.movements.Find(x => x.getGoal() == reformValue);
            if (isThereSuchMovement != null)
            {
                isThereSuchMovement.onRevolutionWon(false);
            }
            value = reformValue;
        }

        public string FullName
        {
            get { return description; }
        }
        public string ShortName
        {
            get { return name; }
        }

        public void OnClicked()
        {
            MainCamera.politicsPanel.selectReform(this);
        }

        public float NameWeight
        {
            get
            {
                return nameWeight;
            }
        }
        public IEnumerable<IReformValue> AllPossibleValues
        {
            get
            {
                foreach (var item in possibleValues)
                {
                    yield return item;
                }
            }
        }
        public override string ToString()
        {
            return ShortName;
        }
    }
}