using UnityEngine;

using System;
using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    public abstract class Investor : GrainGetter, IShareOwner
    {
        protected Investor(int amount, PopType popType, Culture culture, Province where) : base(amount, popType, culture, where)
        {
        }

        protected Investor(PopUnit source, int sizeOfNewPop, PopType newPopType, Province where, Culture culture) : base(source, sizeOfNewPop, newPopType, where, culture)
        {
        }
        protected override void deleteData()
        {
            base.deleteData();
            //secede property... to government
            getOwnedFactories().PerformAction(x => x.ownership.TransferAll(this, Country));
        }

        /// <summary>
        /// Should be reworked to multiple province support and performance
        /// </summary>        
        public IEnumerable<Factory> getOwnedFactories()
        {
            foreach (var item in World.GetAllFactories())
                if (item.ownership.HasOwner(this))
                    yield return item;
        }
        public Procent getBusinessSecurity(IInvestable business)
        {
            var res = business.Country.OwnershipSecurity;
            if (business.Country != this.Country)
                res.Multiply(Options.InvestingForeignCountrySecurity);
            if (business.Province != this.Province)
                res.Multiply(Options.InvestingAnotherProvinceSecurity);
            if (!(business is Owners)) // building, upgrading and opening requires hiring people which can be impossible
                res.Multiply(Options.InvestorEmploymentRisk);

            return res;
        }
        
    }
}