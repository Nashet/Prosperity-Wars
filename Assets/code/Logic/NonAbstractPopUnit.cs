using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tribemen : PopUnit
{
    public Tribemen(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.TribeMen, where, culture)
    {
    }
    public Tribemen(int iamount, Culture iculture, Province where) : base(iamount, PopType.TribeMen, iculture, where)
    {
    }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == this.popType
            || targetType == PopType.Farmers && !province.getCountry().isInvented(Invention.Farming)
            || targetType == PopType.Capitalists
            || targetType == PopType.Aristocrats
            || targetType == PopType.Soldiers)
            return false;
        else
            return true;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == this.popType
            || targetType == PopType.Farmers && !province.getCountry().isInvented(Invention.Farming)
            || targetType == PopType.Workers
            || targetType == PopType.Soldiers && !province.getCountry().isInvented(Invention.ProfessionalArmy))
            return false;
        else
            return true;
    }
    public override void produce()
    {
        Value producedAmount;
        float overpopulation = province.getOverpopulation();
        if (overpopulation <= 1) // all is OK
            producedAmount = new Value(getPopulation() * popType.getBasicProduction().get() / 1000f);
        else
            producedAmount = new Value(getPopulation() * popType.getBasicProduction().get() / 1000f / overpopulation);
        storageNow.add(producedAmount);
        gainGoodsThisTurn.set(producedAmount);
    }
    internal override bool canTrade()
    {
        return false;
    }
    public override bool ShouldPayAristocratTax()
    {
        return true;
    }
    //internal override bool getSayingYes(AbstractReformValue reform)
    //{
    //    if (reform == Government.Tribal)
    //    {
    //        var baseOpinion = new Procent(1f);
    //        baseOpinion.add(this.loyalty);
    //        //return baseOpinion.getProcent(this.population);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.Aristocracy)
    //    {
    //        var baseOpinion = new Procent(0f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.Democracy)
    //    {
    //        var baseOpinion = new Procent(0.8f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.Despotism)
    //    {
    //        var baseOpinion = new Procent(0.1f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.ProletarianDictatorship)
    //    {
    //        var baseOpinion = new Procent(0.2f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else
    //        return false;

    //}

    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Tribal || reform == Government.Democracy)
            && (isStateCulture() || getCountry().minorityPolicy.status == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }

    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            return 1;
        else
            return 0;
    }
}
public class Farmers : PopUnit
{
    public Farmers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Farmers, where, culture)
    { }
    public Farmers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Farmers, iculture, where)
    { }

    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == this.popType
            || targetType == PopType.Capitalists
            || targetType == PopType.Aristocrats
            || targetType == PopType.Soldiers && !province.getCountry().isInvented(Invention.ProfessionalArmy)
            )
            return false;
        else
            return true;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == this.popType
            || targetType == PopType.TribeMen
            || targetType == PopType.Workers
            || targetType == PopType.Soldiers
            )
            return false;
        else
            return true;
    }
    public override void produce()
    {
        Value producedAmount;
        float overpopulation = province.getOverpopulation();

        //if (overpopulation <= 1) // all is OK
        producedAmount = new Value(getPopulation() * popType.getBasicProduction().get() / 1000f);
        //else
        //  producedAmount = new Value(getPopulation() * popType.basicProduction.get() / 1000 / overpopulation);

        producedAmount.multiple(modEfficiency.getModifier(this));
        gainGoodsThisTurn.set(producedAmount);


        if (Economy.isMarket.checkIftrue(province.getCountry()))
        {
            sentToMarket.set(gainGoodsThisTurn);
            Game.market.sentToMarket.add(gainGoodsThisTurn);
        }
        else
            storageNow.add(gainGoodsThisTurn);
    }
    internal override bool canTrade()
    {
        if (Economy.isMarket.checkIftrue(province.getCountry()))
            return true;
        else
            return false;
    }
    public override bool ShouldPayAristocratTax()
    {
        return true;
    }
    //internal override bool getSayingYes(AbstractReformValue reform)
    //{
    //    if (reform is Government.ReformValue)
    //    {
    //        if (reform == Government.Tribal)
    //        {
    //            var baseOpinion = new Procent(0f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else if (reform == Government.Aristocracy)
    //        {
    //            var baseOpinion = new Procent(0.2f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else if (reform == Government.Democracy)
    //        {
    //            var baseOpinion = new Procent(1f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else if (reform == Government.Despotism)
    //        {
    //            var baseOpinion = new Procent(0.2f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else if (reform == Government.ProletarianDictatorship)
    //        {
    //            var baseOpinion = new Procent(0.3f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else
    //            return false;
    //    }
    //    else if (reform is TaxationForPoor.ReformValue)
    //    {
    //        TaxationForPoor.ReformValue taxReform = reform as TaxationForPoor.ReformValue;
    //        var baseOpinion = new Procent(1f);
    //        baseOpinion.set(baseOpinion.get() - taxReform.tax.get() * 2);
    //        baseOpinion.set(baseOpinion.get() + loyalty.get() - 0.5f);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else
    //        return false;
    //}
    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.AnticRespublic || reform == Government.WealthDemocracy)
            && (isStateCulture() || getCountry().minorityPolicy.status == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }

    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            return 1;
        else
            return 0;
    }
}
public class Aristocrats : PopUnit
{
    public Aristocrats(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Aristocrats, where, culture)
    { }
    public Aristocrats(int iamount, Culture iculture, Province where) : base(iamount, PopType.Aristocrats, iculture, where)
    { }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == this.popType
            || targetType == PopType.Farmers && !province.getCountry().isInvented(Invention.Farming)
            || targetType == PopType.Soldiers && !province.getCountry().isInvented(Invention.ProfessionalArmy)
            || targetType == PopType.Capitalists)// && Economy.isNotMarket.checkIftrue(province.getCountry()))
            return false;
        else
            return true;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        return false;
    }
    internal void dealWithMarket()
    {
        if (storageNow.get() > Options.aristocratsFoodReserv)
        {
            Storage howMuchSend = new Storage(storageNow.getProduct(), storageNow.get() - Options.aristocratsFoodReserv);
            storageNow.send(sentToMarket, howMuchSend);
            //sentToMarket.set(howMuchSend);
            Game.market.sentToMarket.add(howMuchSend);
        }
    }
    public override void produce()
    { }
    internal override bool canTrade()
    {
        if (Economy.isMarket.checkIftrue(province.getCountry()))
            return true;
        else
            return false;
    }
    public override bool ShouldPayAristocratTax()
    {
        return false;
    }

    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.AnticRespublic || reform == Government.WealthDemocracy
            || reform == Government.Aristocracy || reform == Government.Tribal)
            && (isStateCulture() || getCountry().minorityPolicy.status == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }
    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            if (reformValue == Government.WealthDemocracy)
                return 5;
            else
                return 1;
        else
            return 0;
    }

}
public class Soldiers : PopUnit
{
    public Soldiers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Soldiers, where, culture)
    { }
    public Soldiers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Soldiers, iculture, where)
    { }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == this.popType
            || targetType == PopType.Farmers && !province.getCountry().isInvented(Invention.Farming)
            || targetType == PopType.Capitalists
            || targetType == PopType.Aristocrats)
            return false;
        else
            return true;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == this.popType
             || targetType == PopType.TribeMen
             || targetType == PopType.Workers
             || targetType == PopType.Farmers)
            return false;
        else
            return true;
    }
    internal override bool canTrade()
    {
        if (Economy.isMarket.checkIftrue(province.getCountry()))
            return true;
        else
            return false;
    }
    public override bool ShouldPayAristocratTax()
    {
        return false;
    }

    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.MilitaryJunta)
            && (isStateCulture() || getCountry().minorityPolicy.status == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }
    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            return 1;
        else
            return 0;
    }

    public override void produce()
    {

    }
}
public class Capitalists : PopUnit
{
    public Capitalists(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Capitalists, where, culture)
    { }
    public Capitalists(int iamount, Culture iculture, Province where) : base(iamount, PopType.Capitalists, iculture, where)
    { }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == this.popType
            || targetType == PopType.Farmers && !province.getCountry().isInvented(Invention.Farming)
            || targetType == PopType.Soldiers && !province.getCountry().isInvented(Invention.ProfessionalArmy)
            || targetType == PopType.Aristocrats
            )
            return false;
        else
            return true;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        return false;
    }
    public override void produce()
    { }
    internal override bool canTrade()
    {
        if (Economy.isMarket.checkIftrue(province.getCountry()))
            return true;
        else
            return false;
    }
    public override bool ShouldPayAristocratTax()
    {
        return false;
    }
    //internal override bool getSayingYes(AbstractReformValue reform)
    //{
    //    if (reform == Government.Tribal)
    //    {
    //        var baseOpinion = new Procent(0f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.Aristocracy)
    //    {
    //        var baseOpinion = new Procent(0f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.Democracy)
    //    {
    //        var baseOpinion = new Procent(0.8f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.Despotism)
    //    {
    //        var baseOpinion = new Procent(0.3f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.ProletarianDictatorship)
    //    {
    //        var baseOpinion = new Procent(0.1f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else
    //        return false;
    //}
    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.AnticRespublic || reform == Government.WealthDemocracy
            || reform == Government.BourgeoisDictatorship)
            && (isStateCulture() || getCountry().minorityPolicy.status == MinorityPolicy.Equality))
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
}
public class Workers : PopUnit
{
    public Workers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Workers, where, culture)
    { }
    public Workers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Workers, iculture, where)
    { }

    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == this.popType
            || targetType == PopType.Farmers && !province.getCountry().isInvented(Invention.Farming)
            || targetType == PopType.Capitalists
            || targetType == PopType.Aristocrats
            || targetType == PopType.Soldiers)
            return false;
        else
            return true;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == this.popType
            || targetType == PopType.Farmers
            || targetType == PopType.TribeMen
            || targetType == PopType.Soldiers && !province.getCountry().isInvented(Invention.ProfessionalArmy))
            return false;
        else
            return true;
    }
    public override void produce()
    { }
    internal override bool canTrade()
    {
        if (Economy.isMarket.checkIftrue(province.getCountry()))
            return true;
        else
            return false;
    }
    public override bool ShouldPayAristocratTax()
    {
        return true;
    }
    //internal override bool getSayingYes(AbstractReform reform)
    //{
    //    return reform.modVoting.getModifier(this) > Options.votingPassBillLimit;


    //if (reform == Government.Tribal)
    //{
    //    var baseOpinion = new Procent(0f);
    //    baseOpinion.add(this.loyalty);
    //    return baseOpinion.get() > Options.votingPassBillLimit;
    //}
    //else if (reform == Government.Aristocracy)
    //{
    //    var baseOpinion = new Procent(0f);
    //    baseOpinion.add(this.loyalty);
    //    return baseOpinion.get() > Options.votingPassBillLimit;
    //}
    //else if (reform == Government.Democracy)
    //{
    //    var baseOpinion = new Procent(0.6f);
    //    baseOpinion.add(this.loyalty);
    //    return baseOpinion.get() > Options.votingPassBillLimit;
    //}
    //else if (reform == Government.Despotism)
    //{
    //    var baseOpinion = new Procent(0.3f);
    //    baseOpinion.add(this.loyalty);
    //    return baseOpinion.get() > Options.votingPassBillLimit;
    //}
    //else if (reform == Government.ProletarianDictatorship)
    //{
    //    var baseOpinion = new Procent(0.8f);
    //    baseOpinion.add(this.loyalty);
    //    return baseOpinion.get() > Options.votingPassBillLimit;
    //}
    //else
    //    return false;
    // }
    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy)
            && (isStateCulture() || getCountry().minorityPolicy.status == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }
    //public class UnknownReform : Exception
    //{
    //}
    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            return 1;
        else
            return 0;
    }
}
//public class PopLinkageValue
//{
//    public PopType type;
//    public Value amount;
//    internal PopLinkageValue(PopType p, Value a)
//    {
//        type = p;
//        amount = a;
//    }
//}