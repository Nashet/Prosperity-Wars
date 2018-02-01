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
            getOwnedFactories().PerformAction(x => x.ownership.TransferAll(this, GetCountry()));
        }
        
        /// <summary>
        /// Should be reworked to multiple province support
        /// </summary>        
        public IEnumerable<Factory> getOwnedFactories()
        {
            //List<Factory> result = new List<Factory>();
            if (popType == PopType.Aristocrats || popType == PopType.Capitalists)
            {
                foreach (var item in getProvince().getAllFactories())
                    if (item.ownership.HasOwner(this))
                        yield return item;            
            }            
        }

        //private readonly Properties stock = new Properties();
        //public Properties GetOwnership()
        //{
        //    return stock;
        //}
        //internal void universalInvest(Predicate<Factory> predicate)
        //{
        //    if (!getProvince().isThereFactoriesInUpgradeMoreThan(Options.maximumFactoriesInUpgradeToBuildNew)
        //        && (getProvince().howMuchFactories() == 0 || getProvince().getAverageFactoryWorkforceFulfilling() > Options.minFactoryWorkforceFulfillingToBuildNew)
        //        )
        //    {
        //        // if AverageFactoryWorkforceFulfilling isn't full you can get more workforce by raising salary (implement it later)

        //        var projects = getProvince().getAllInvestmentsProjects(x => x.getMargin().get() >= Options.minMarginToInvest && predicate(x));
        //        var project = projects.MaxBy(x => x.getMargin().get());

        //        if (project != null)
        //        {
        //            Value investmentCost = project.getInvestmentsCost();
        //            if (!canPay(investmentCost))
        //                getBank().giveLackingMoney(this, investmentCost);
        //            if (canPay(investmentCost))
        //            {
        //                Factory factory;
        //                var factoryToBuild = project as FactoryType;
        //                if (factoryToBuild != null)
        //                    factory = new Factory(getProvince(), this, factoryToBuild);
        //                else
        //                {
        //                    factory = project as Factory;
        //                    if (factory != null)
        //                        factory.upgrade(this);
        //                    else
        //                        Debug.Log("Unknown investment type");
        //                }
        //                payWithoutRecord(factory, investmentCost);
        //            }
        //        }
        //    }
        //    base.invest();
        //}
    }
}