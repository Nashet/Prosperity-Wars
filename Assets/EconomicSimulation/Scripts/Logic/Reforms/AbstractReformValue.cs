
using Nashet.Conditions;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{

    public abstract class AbstrRefrmValue : IReformValue
    {
        public int ID { get; protected set; }
        protected readonly DoubleConditionsList allowed;
        protected readonly Modifier loyalty;
        protected readonly Modifier education = new Modifier(Condition.IsNotImplemented, 0f, false);
        protected readonly Modifier wantsReform;
        protected readonly ModifiersList modVoting;

        public Procent LifeQualityImpact { get; }

        public AbstrRefrmValue(int id, DoubleConditionsList condition)
        {
            ID = id;
            allowed = condition;
            wantsReform = new Modifier(x => howIsItGoodForPop(x as PopUnit).get(),
                                    "Benefit to population", 1f, true);
            loyalty = new Modifier(x => loyaltyBoostFor(x as PopUnit),
                        "Loyalty", 1f, false);
            modVoting = new ModifiersList(new List<Condition>
            {
                        wantsReform, loyalty, education
                        });
        }

        public abstract bool IsAllowed(object firstObject, object secondObject, out string description);
        public abstract bool IsAllowed(object firstObject, object secondObject);

        public float getVotingPower(PopUnit forWhom)
        {
            return modVoting.getModifier(forWhom);
        }

        public bool isMoreConservative(AbstractReform another)
        {
            throw new NotImplementedException();
        }

        public abstract Procent howIsItGoodForPop(PopUnit pop);
       
        private float loyaltyBoostFor(PopUnit popUnit)
        {
            float result;
            if (howIsItGoodForPop(popUnit).get() > 0.5f)
                result = popUnit.loyalty.get() / 4f;
            else
                result = popUnit.loyalty.get50Centre() / 4f;
            return result;
        }
    }
}
//    public abstract class AbstractReform : Name, IClickable
//    {
//        private readonly string description;

//        protected AbstractReform(string name, string indescription, Country country) : base(name)
//        {
//            description = indescription;
//            //country.reforms.Add(this);
//            this.country = country;
//        }
//        private readonly Country country;
//        public abstract bool isAvailable(Country country);

//        public abstract IEnumerator GetEnumerator();


//        public virtual void setValue(IReformValue selectedReformValue)
//        {
//            foreach (PopUnit pop in country.Provinces.AllPops)
//                if (pop.getSayingYes(selectedReformValue))
//                {
//                    pop.loyalty.Add(Options.PopLoyaltyBoostOnDiseredReformEnacted);
//                    pop.loyalty.clamp100();
//                }
//            var isThereSuchMovement = country.movements.Find(x => x.getGoal() == selectedReformValue);
//            if (isThereSuchMovement != null)
//            {
//                isThereSuchMovement.onRevolutionWon(false);
//            }
//        }

//        public override string FullName
//        {
//            get { return description; }
//        }

//        public abstract AbstractReformValue getValue();

//        public abstract bool canHaveValue(AbstractReformValue abstractReformValue);

//        //abstract public AbstractReformValue getValue(int value);
//        //abstract public void setValue(int value);
//        public void OnClicked()
//        {
//            MainCamera.politicsPanel.selectReform(this);
//            MainCamera.politicsPanel.Refresh();
//        }
//    }
//}
//using System.Collections.Generic;
//using Nashet.Conditions;
//using Nashet.Utils;
//using Nashet.ValueSpace;

//namespace Nashet.EconomicSimulation
//{
//    public abstract class AbstractReformValue : Name
//    {
//        public static readonly DoubleCondition isNotLFOrMoreConservative = new DoubleCondition((country, newReform) => (country as Country).economy != Econ.LaissezFaire
//        || (newReform as IReformValue).isMoreConservative(
//            (country as Country).getReform(newReform as IReformValue).getValue()
//            ), x => "Economy policy is not Laissez Faire or that is reform rollback", true);

//        private readonly string description;
//        public readonly int ID; // covert inti liberal_weight
//        public readonly DoubleConditionsList allowed;
//        public readonly Condition isEnacted;// = new Condition(x => !(x as Country).reforms.isEnacted(this), "Reform is not enacted yet", true);

//        public abstract bool isAvailable(Country country);

//        protected abstract Procent howIsItGoodForPop(PopUnit pop);


//        protected AbstractReformValue(string name, string indescription, int ID, DoubleConditionsList condition) : base(name)
//        {
//            this.ID = ID;
//            description = indescription;
//            allowed = condition;
//            isEnacted = new Condition(x => !(x as Country).reforms.isEnacted(this), "Reform not enacted yet", false);
//            allowed.add(isEnacted);
//            wantsReform = new Modifier(x => howIsItGoodForPop(x as PopUnit).get(),
//                        "Benefit to population", 1f, true);
//            loyalty = new Modifier(x => loyaltyBoostFor(x as PopUnit),
//                        "Loyalty", 1f, false);
//            modVoting = new ModifiersList(new List<Condition>{
//        wantsReform, loyalty, education
//        });
//        }

//        public bool isMoreConservative(AbstractReformValue anotherReform)
//        {
//            return ID < anotherReform.ID;
//        }

//        private float loyaltyBoostFor(PopUnit popUnit)
//        {
//            float result;
//            if (howIsItGoodForPop(popUnit).get() > 0.5f)
//                result = popUnit.loyalty.get() / 4f;
//            else
//                result = popUnit.loyalty.get50Centre() / 4f;
//            return result;
//        }

//        public override string FullName
//        {
//            get { return description; }
//        }

//        private readonly Modifier loyalty;
//        private readonly Modifier education = new Modifier(Condition.IsNotImplemented, 0f, false);
//        private readonly Modifier wantsReform;
//        public readonly ModifiersList modVoting;
//    }
//}