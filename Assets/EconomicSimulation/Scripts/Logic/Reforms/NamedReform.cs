using Nashet.Conditions;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    public abstract class NamedReform : AbstractReform
    {
        public NamedReform(string name, string indescription, Country country, List<IReformValue> possibleValues) : base(name, indescription, country, possibleValues)
        {
        }
    }

    public abstract class NamedReformValue : AbstrRefrmValue, INameable
    {
        protected readonly string description;
        protected readonly string name;

        public NamedReformValue(string name, string description, int id, DoubleConditionsList condition) : base(id, condition)
        {
            this.description = description;
            this.name = name;

        }

        public string FullName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string ShortName
        {
            get
            {
                throw new NotImplementedException();
            }
        }       

        public override bool IsAllowed(object firstObject, object secondObject, out string description)
        {
            return allowed.isAllTrue(firstObject, secondObject, out description);
        }

        public override bool IsAllowed(object firstObject, object secondObject)
        {
            throw new NotImplementedException();
        }
    }
    public interface IReformValue
    {
        int ID { get; }
        bool IsAllowed(object firstObject, object secondObject, out string description);
        bool IsAllowed(object firstObject, object secondObject);
        float getVotingPower(PopUnit forWhom);
        bool isMoreConservative(AbstractReform another);
        Procent howIsItGoodForPop(PopUnit pop);
        Procent LifeQualityImpact { get; }
    }

}