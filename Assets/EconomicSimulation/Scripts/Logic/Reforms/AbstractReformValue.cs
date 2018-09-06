
using Nashet.Conditions;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation.Reforms
{
    public abstract class AbstractReformValue : IReformValue
    {
        public int ID { get; protected set; }
        protected readonly DoubleConditionsList allowed;
        protected readonly Modifier loyalty;
        protected readonly Modifier education = new Modifier(Condition.IsNotImplemented, 0f, false);
        protected readonly Modifier wantsReform;
        protected readonly ModifiersList modVoting;

        public Procent LifeQualityImpact { get; protected set; }

        protected AbstractReformValue(int id, DoubleConditionsList condition)
        {
            LifeQualityImpact = Procent.ZeroProcent.Copy();
            ID = id;
            allowed = condition;
            wantsReform = new Modifier(x => howIsItGoodForPop(x as PopUnit).get(), "Benefit to population", 1f, true);
            loyalty = new Modifier(x => loyaltyBoostFor(x as PopUnit), "Loyalty", 1f, false);
            modVoting = new ModifiersList(new List<Condition> { wantsReform, loyalty, education });
        }

        public abstract Procent howIsItGoodForPop(PopUnit pop);

        public virtual bool IsAllowed(object firstObject, object secondObject, out string description)
        {
            return allowed.isAllTrue(firstObject, secondObject, out description);
        }

        public virtual bool IsAllowed(object firstObject, object secondObject)
        {
            return allowed.isAllTrue(firstObject, secondObject);
        }

        public virtual float getVotingPower(PopUnit forWhom)
        {
            return modVoting.getModifier(forWhom);
        }

        /// <summary>
        /// Could be wrong for some reforms!  Assumes that reforms go in conservative-liberal order
        /// </summary>        
        public virtual bool IsMoreConservativeThan(IReformValue anotherReform)
        {
            return ID < anotherReform.ID;
        }

        public virtual int GetRelativeConservatism(IReformValue two)
        {
            return this.ID - two.ID;
        }

        protected float loyaltyBoostFor(PopUnit popUnit)
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