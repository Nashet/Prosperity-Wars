using System.Collections.Generic;
using Nashet.Utils;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public abstract class Investor : GrainGetter, IShareOwner
    {
        protected Investor(int amount, PopType popType, Culture culture, Province where) : base(amount, popType, culture, where)
        {
        }

        protected Investor(PopUnit source, int sizeOfNewPop, PopType newPopType, Province where, Culture culture, IWayOfLifeChange oldLife) : base(source, sizeOfNewPop, newPopType, where, culture, oldLife)
        {
        }

        public override void Kill()
        {
            base.Kill();
            //secede property... to government
            getOwnedFactories().PerformAction(x => x.ownership.TransferAll(this, Country));
        }

        /// <summary>
        /// Should be reworked to multiple province support and performance
        /// </summary>
        public IEnumerable<Factory> getOwnedFactories()
        {
            foreach (var item in World.AllFactories)
                if (item.ownership.HasOwner(this))
                    yield return item;
        }

        public Procent getBusinessSecurity(IInvestable business)
        {
            var res = business.Country.OwnershipSecurity;

            if (business.Country != Country)
                res.Multiply(Options.InvestingForeignCountrySecurity);
            else if (business.Province != Province)
                res.Multiply(Options.InvestingAnotherProvinceSecurity);

            if (business is NewFactoryProject) // building, upgrading and opening requires hiring people which can be impossible
                res.Multiply(Options.InvestorEmploymentSafety);

            return res;
        }
    }
}