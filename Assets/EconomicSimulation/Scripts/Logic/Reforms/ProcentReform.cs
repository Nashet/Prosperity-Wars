using Nashet.ValueSpace;
using System;
using System.Collections.Generic;
using Nashet.Conditions;

namespace Nashet.EconomicSimulation.Reforms
{
    public abstract class ProcentReform : AbstractReform
    {
        public ProcentReformVal tax;
        public ProcentReform(string name, string description, Country country, List<IReformValue> possibleValues) : base(name, description, country, possibleValues)
        {
            //tax = new ProcentReformVal(0.1f);
        }

        //public override void SetValue(IReformValue reformValue)
        //{
        //    base.SetValue(reformValue);
        //    this.tax = reformValue as ProcentReformVal;
        //}
        public void SetValue(ProcentReformVal tax)
        {
            base.SetValue(tax);
            this.tax = tax;
        }

        public abstract class ProcentReformVal : AbstractReformValue
        {
            public Procent Procent { get; }
            internal ProcentReformVal(int ID, Procent procent, DoubleConditionsList condition) : base(ID, condition)
            {
                Procent = procent;
            }
            //internal ProcentReformVal(int ID, Procent procent) : this(ID, procent, new DoubleConditionsList(new List<Condition> { Condition.AlwaysYes }))
            //{ }

            internal float get()
            {
                return Procent.get();
            }
            public override string ToString()
            {
                return Procent.ToString() + " rate";
            } 
        }
    }
}