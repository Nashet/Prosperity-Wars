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

        public class ProcentReformVal : AbstractReformValue
        {
            public Procent Procent { get; }
            internal ProcentReformVal(int ID, Procent procent) : base(ID, new DoubleConditionsList(new List<Condition> { Condition.AlwaysYes }))
            {
                Procent = procent;
            }            

            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                return new Procent(0f);
            }

            public override bool IsAllowed(object firstObject, object secondObject, out string description)
            {
                description = "";
                return true;
            }

            public override bool IsAllowed(object firstObject, object secondObject)
            {
                return true;
                
            }

            internal float get()
            {
                return Procent.get();
            }
            public override string ToString()
            {
                return Procent.ToString();
            }
            //public bool IsMoreConservative(IReformValue anotherReform)
            //{
            //    return ID < anotherReform.ID;
            //}

            //public Procent LifeQualityImpact
            //{
            //    get
            //    {
            //        return this;
            //    }
            //}


            //public float getVotingPower(PopUnit forWhom)
            //{
            //    throw new NotImplementedException();
            //}

            //public bool IsAllowed(object firstObject, object secondObject, out string description)
            //{
            //    throw new NotImplementedException();
            //}

            //public bool IsAllowed(object firstObject, object secondObject)
            //{
            //    throw new NotImplementedException();
            //}

            //public virtual Procent howIsItGoodForPop(PopUnit pop)
            //{
            //    throw new NotImplementedException();
            //}
        }
    }

}