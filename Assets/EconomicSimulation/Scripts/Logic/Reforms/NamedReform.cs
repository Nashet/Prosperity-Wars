using Nashet.Conditions;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    public abstract class NamedReform : AbstrRefrm
    {
        public NamedReform(string name, string indescription, Country country, List<IReformValue> possibleValues) : base(name, indescription, country, possibleValues)
        {
        }
    }

    public abstract class AbstrRefrmValue : IReformValue
    {
        public int ID { get; protected set; }
        protected readonly DoubleConditionsList allowed;

        public AbstrRefrmValue(string name, string description, int id, DoubleConditionsList condition)
        {
            ID = id;
            allowed = condition;
        }

        public abstract bool IsAllowed(object firstObject, object secondObject, out string description);
        public abstract bool IsAllowed(object firstObject, object secondObject);
    }
    public class NamdRfrmValue : AbstrRefrmValue, INameable
    {
        protected readonly string description;

        public NamdRfrmValue(string name, string description, int id, DoubleConditionsList condition) : base(name)
        {
            this.description = description;

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
    //

    public class SubsidyReform : AbstrRefrm
    {
        public SubsidyReform(string name, string indescription, Country country, List<IReformValue> possibleValues) : base(name, indescription, country, possibleValues)
        {
        }

        public override void OnReformEnactedInProvince(Province province)
        {
            throw new NotImplementedException();
        }

        protected override Procent howIsItGoodForPop(PopUnit pop)
        {
            throw new NotImplementedException();
        }
    }



    public class SeparRefrm : AbstrRefrm
    {
        protected readonly Country separatismTarget;


        public SeparRefrm(string name, string indescription, Country country, List<IReformValue> possibleValues) : base(name, indescription, country, possibleValues)
        {
        }

        public override void OnReformEnactedInProvince(Province province)
        {
            throw new NotImplementedException();
        }

        protected override Procent howIsItGoodForPop(PopUnit pop)
        {
            throw new NotImplementedException();
        }
    }

    public class ProcentRerfr : AbstrRefrm
    {
        public ProcentReformVal tax;
        public ProcentRerfr(string name, string description, Country country, List<IReformValue> possibleValues) : base(name, description, country, possibleValues)
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

        protected override Procent howIsItGoodForPop(PopUnit pop)
        {
            throw new NotImplementedException();
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

            public bool IsAllowed(object firstObject, object secondObject, out string description)
            {
                throw new NotImplementedException();
            }

            public bool IsAllowed(object firstObject, object secondObject)
            {
                throw new NotImplementedException();
            }
        }
    }
    public interface IReformValue
    {
        int ID { get; }
        bool IsAllowed(object firstObject, object secondObject, out string description);
        bool IsAllowed(object firstObject, object secondObject);
    }

}