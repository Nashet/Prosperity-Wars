using Nashet.Conditions;
using Nashet.ValueSpace;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation.Reforms
{
    public class UBI : AbstractReform
    {
        public UBI(Country country) : base("Unconditional basic income", "", country, new List<IReformValue> { })
        {
        }
        public class UBIReformValue : NamedReformValue
        {
            public UBIReformValue(string name, string description, int id, DoubleConditionsList condition) : base(name, description, id, condition)
            {
            }

            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}