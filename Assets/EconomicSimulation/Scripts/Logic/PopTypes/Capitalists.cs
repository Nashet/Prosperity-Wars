using UnityEngine;
using UnityEditor;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Capitalists : GrainGetter
    {
        public Capitalists(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Capitalists, where, culture)
        { }
        public Capitalists(int iamount, Culture iculture, Province where) : base(iamount, PopType.Capitalists, iculture, where)
        { }
        public override bool canThisDemoteInto(PopType targetType)
        {
            if (targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
                || targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy)
                || targetType == PopType.Artisans
                )
                return true;
            else
                return false;
        }
        public override bool canThisPromoteInto(PopType targetType)
        {
            return false;
        }
        public override void produce()
        {
            // Caps don't produce products directly
        }
        internal override bool canTrade()
        {
            if (getCountry().economy.getValue() == Economy.PlannedEconomy)
                return false;
            else
                return true;
        }
        public override bool shouldPayAristocratTax()
        {
            return false;
        }
        internal override bool canVote(Government.ReformValue reform)
        {
            if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy
                || reform == Government.BourgeoisDictatorship)
                && (isStateCulture() || getCountry().minorityPolicy.getValue() == MinorityPolicy.Equality))
                return true;
            else
                return false;
        }
        internal override int getVotingPower(Government.ReformValue reformValue)
        {
            if (canVote(reformValue))
                if (reformValue == Government.WealthDemocracy)
                    return Options.PopRichStrataVotePower;
                else
                    return 1;
            else
                return 0;
        }
        internal override void invest()
        {
            if (Economy.isMarket.checkIftrue(getCountry()) && getCountry().isInvented(Invention.Manufactures))
            {
                //should I build?
                //province.getUnemployed() > Game.minUnemploymentToBuldFactory && 
                if (!getProvince().isThereFactoriesInUpgradeMoreThan(Options.maximumFactoriesInUpgradeToBuildNew))
                {
                    FactoryType proposition = FactoryType.getMostTeoreticalProfitable(getProvince());
                    if (proposition != null && proposition.canBuildNewFactory(getProvince()) &&
                        (getProvince().getUnemployedWorkers() > Options.minUnemploymentToBuldFactory || getProvince().getAverageFactoryWorkforceFullfilling() > Options.minFactoryWorkforceFullfillingToBuildNew))
                    {
                        //Value buildCost = Game.market.getCost(proposition.getBuildNeeds());
                        //buildCost.add(Options.factoryMoneyReservePerLevel);
                        Value buildCost = proposition.getMinimalMoneyToBuild();
                        if (canPay(buildCost))
                        {
                            Factory found = new Factory(getProvince(), this, proposition);
                            payWithoutRecord(found, buildCost);
                        }
                        else
                            if (getBank().giveLackingMoney(this, buildCost))
                        {
                            Factory found = new Factory(getProvince(), this, proposition);
                            payWithoutRecord(found, buildCost);
                        }
                    }
                    else
                    {
                        //upgrade section
                        Factory factory = FactoryType.getMostPracticlyProfitable(getProvince());
                        if (factory != null
                            && factory.canUpgrade() // don't change it to Modifier  - it would prevent loan takes
                            && factory.getMargin().get() >= Options.minMarginToUpgrade
                            && factory.getWorkForceFulFilling().isBiggerThan(Options.minWorkforceFullfillingToUpgradeFactory))
                        {
                            Value upgradeCost = Game.market.getCost(factory.getUpgradeNeeds());
                            if (canPay(upgradeCost))
                                factory.upgrade(this);
                            else if (getBank().giveLackingMoney(this, upgradeCost))
                                factory.upgrade(this);
                        }
                    }
                }

            }
            base.invest();
        }
    }
}