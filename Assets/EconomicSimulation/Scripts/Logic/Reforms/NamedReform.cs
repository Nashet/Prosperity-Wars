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



    public class NamdRfrmValue : Name, IReformValue
    {
        protected readonly string description;
        protected readonly int ID;
        protected readonly DoubleConditionsList allowed;
        public NamdRfrmValue(string name, string description, int id, DoubleConditionsList condition) : base(name)
        {
            this.description = description;
            ID = id;
            allowed = condition;
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

    public class TaxRerfr : AbstrRefrm
    {
        public Procent tax;
        public TaxRerfr(string name, string description, Country country, List<IReformValue> possibleValues) : base(name, description, country, possibleValues)
        {
            tax = new Procent(0.1f);
        }

        public override void OnReformEnactedInProvince(Province province)
        {
            throw new NotImplementedException();
        }

        public void SetValue(Procent tax)
        {
            base.SetValue(tax);
            this.tax.Set(tax);
        }

        protected override Procent howIsItGoodForPop(PopUnit pop)
        {
            throw new NotImplementedException();
        }
    }
    public interface IReformValue
    {
        
    }

}