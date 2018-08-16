using Nashet.ValueSpace;
using System;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    public abstract class ProcentReform : AbstractReform
    {
        public ProcentReformVal tax;
        public ProcentReform(string name, string description, Country country, List<IReformValue> possibleValues) : base(name, description, country, possibleValues)
        {
            //tax = new ProcentReformVal(0.1f);
        }

        

        public void SetValue(ProcentReformVal tax)
        {
            base.SetValue(tax);
            this.tax.Set(tax);
        }

       
        public  class ProcentReformVal : Procent, IReformValue//  AbstrRefrmValue
        {
            public ProcentReformVal(float number, bool showMessageAboutNegativeValue = true) : base(number, showMessageAboutNegativeValue)
            {
            }
            public bool IsMoreConservative(IReformValue anotherReform)
            {
                return ID < anotherReform.ID;
            }

            public Procent LifeQualityImpact
            {
                get
                {
                    return this;
                }
            }
          

            public float getVotingPower(PopUnit forWhom)
            {
                throw new NotImplementedException();
            }

            public bool IsAllowed(object firstObject, object secondObject, out string description)
            {
                throw new NotImplementedException();
            }

            public bool IsAllowed(object firstObject, object secondObject)
            {
                throw new NotImplementedException();
            }

            public virtual Procent howIsItGoodForPop(PopUnit pop)
            {
                throw new NotImplementedException();
            }
        }
    }

}