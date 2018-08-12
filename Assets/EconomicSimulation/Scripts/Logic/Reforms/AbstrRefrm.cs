using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    public abstract class AbstrRefrm : Component<Country>, INameable, ISortableName, IClickable
    {
        private readonly string description;
        protected readonly string name;
        protected readonly float nameWeight;

        protected IReformValue value;
        protected readonly List<IReformValue> possibleValues;

        public AbstrRefrm(string name, string indescription, Country country, List<IReformValue> possibleValues) : base(country)
        {
            this.name = name;
            if (name != null)
                nameWeight = name.GetWeight();
            description = indescription;
            //country.reforms.Add(this);
            this.possibleValues = possibleValues;

        }
        protected abstract Procent howIsItGoodForPop(PopUnit pop);

        public override bool Equals(Object another)
        {
            if (ReferenceEquals(another, null))
                throw new ArgumentNullException();
            return another is IReformValue && this == (IReformValue)another
                || another is AbstrRefrm && this == (AbstrRefrm)another;
        }
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
        public static bool operator ==(AbstrRefrm x, IReformValue y)
        {
            return x.value == y;
        }
        public static bool operator !=(AbstrRefrm x, IReformValue y)
        {
            return !(x.value == y);
        }

        public static bool operator ==(AbstrRefrm x, AbstrRefrm y)
        {
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                throw new ArgumentNullException();
            //return true;
            return x.value == y.value;
        }
        public static bool operator !=(AbstrRefrm x, AbstrRefrm y)
        {

            return !(x == y);
        }
        // private readonly Country country;
        //public abstract bool isAvailable(Country country);

        //public abstract IEnumerator GetEnumerator();

        //public abstract bool canHaveThatValue(AbstractNamdRfrmValue abstractNamdRfrmValue);
        public abstract void OnReformEnactedInProvince(Province province);

        public virtual void SetValue(AbstrRefrm reform)
        {
            SetValue(reform.value);
        }
        public virtual void SetValue(IReformValue reformValue)
        {
            // todo return it!!
            //foreach (PopUnit pop in owner.Provinces.AllPops)
            //    if (pop.getSayingYes(reformValue))
            //    {
            //        pop.loyalty.Add(Options.PopLoyaltyBoostOnDiseredReformEnacted);
            //        pop.loyalty.clamp100();
            //    }
            var isThereSuchMovement = owner.movements.Find(x => x.getGoal() == reformValue);
            if (isThereSuchMovement != null)
            {
                isThereSuchMovement.onRevolutionWon(false);
            }
        }

        public string FullName
        {
            get { return description; }
        }
        public string ShortName
        {
            get { return name; }
        }

        public bool Is(NamdRfrmValue value)
        {
            throw new System.NotImplementedException();
        }
        //public abstract AbstractNamdRfrmValue getValue();



        //abstract public AbstractNamdRfrmValue getValue(int value);
        //abstract public void setValue(int value);
        public void OnClicked()
        {
            //MainCamera.politicsPanel.selectReform(this);
            MainCamera.politicsPanel.Refresh();
        }

        public float GetNameWeight()
        {
            return nameWeight;
        }
        public IEnumerable<IReformValue> PossibleValues
        {
            get
            {
                foreach (var item in possibleValues)
                {
                    yield return item;
                }
            }
        }
    }

}