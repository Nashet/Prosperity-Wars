using System.Collections.Generic;
using Nashet.Conditions;
using Nashet.Utils;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public abstract class AbstractReformValue : Name
    {
        public static readonly DoubleCondition isNotLFOrMoreConservative = new DoubleCondition((country, newReform) => (country as Country).economy != Econ.LaissezFaire
        || (newReform as IReformValue).isMoreConservative(
            (country as Country).getReform(newReform as IReformValue).getValue()
            ), x => "Economy policy is not Laissez Faire or that is reform rollback", true);

        private readonly string description;
        public readonly int ID; // covert inti liberal_weight
        public readonly DoubleConditionsList allowed;
        public readonly Condition isEnacted;// = new Condition(x => !(x as Country).reforms.isEnacted(this), "Reform is not enacted yet", true);

        public abstract bool isAvailable(Country country);

        protected abstract Procent howIsItGoodForPop(PopUnit pop);


        protected AbstractReformValue(string name, string indescription, int ID, DoubleConditionsList condition) : base(name)
        {
            this.ID = ID;
            description = indescription;
            allowed = condition;
            isEnacted = new Condition(x => !(x as Country).reforms.isEnacted(this), "Reform not enacted yet", false);
            allowed.add(isEnacted);
            wantsReform = new Modifier(x => howIsItGoodForPop(x as PopUnit).get(),
                        "Benefit to population", 1f, true);
            loyalty = new Modifier(x => loyaltyBoostFor(x as PopUnit),
                        "Loyalty", 1f, false);
            modVoting = new ModifiersList(new List<Condition>{
        wantsReform, loyalty, education
        });
        }

        public bool isMoreConservative(AbstractReformValue anotherReform)
        {
            return ID < anotherReform.ID;
        }

        private float loyaltyBoostFor(PopUnit popUnit)
        {
            float result;
            if (howIsItGoodForPop(popUnit).get() > 0.5f)
                result = popUnit.loyalty.get() / 4f;
            else
                result = popUnit.loyalty.get50Centre() / 4f;
            return result;
        }

        public override string FullName
        {
            get { return description; }
        }

        private readonly Modifier loyalty;
        private readonly Modifier education = new Modifier(Condition.IsNotImplemented, 0f, false);
        private readonly Modifier wantsReform;
        public readonly ModifiersList modVoting;
    }
}