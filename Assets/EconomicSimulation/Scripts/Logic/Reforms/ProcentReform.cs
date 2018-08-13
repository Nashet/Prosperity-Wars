using Nashet.ValueSpace;
using System;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    //   





    public class ProcentReform : AbstractReform
    {
        public ProcentReformVal tax;
        public ProcentReform(string name, string description, Country country, List<IReformValue> possibleValues) : base(name, description, country, possibleValues)
        {
            tax = new ProcentReformVal(0.1f);
        }

        public override void OnReformEnactedInProvince(Province province)
        {
            throw new NotImplementedException();
        }

        public void SetValue(ProcentReformVal tax)
        {
            base.SetValue(tax);
            this.tax.Set(tax);
        }

       
        public class ProcentReformVal : Procent, IReformValue//  AbstrRefrmValue
        {
            public ProcentReformVal(float number, bool showMessageAboutNegativeValue = true) : base(number, showMessageAboutNegativeValue)
            {
            }

            public int ID
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public float getVotingPower(PopUnit forWhom)
            {
                throw new NotImplementedException();
            }

            public Procent howIsItGoodForPop(PopUnit pop)
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

            public bool isMoreConservative(AbstractReform another)
            {
                throw new NotImplementedException();
            }
        }
    }

}