using Nashet.Conditions;
using Nashet.EconomicSimulation.Reforms;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represents population unit which can live, consume, produce, invest
    /// </summary>
    public abstract class PopUnit : Producer, IClickable, INameable
    {
        ///<summary>buffer popList. To avoid iteration breaks</summary>
        public static readonly List<PopUnit> PopListToAddToGeneralList = new List<PopUnit>();

        public static readonly Predicate<PopUnit> All = x => true;

        public readonly Procent loyalty;
        public readonly Population population;
        private int mobilized;

        private readonly PopType type;

        public PopType Type
        {
            get { return type; }
        }

        public readonly Culture culture;
        private readonly Education education;

        /// <summary>new Value, read only</summary>
        public Education Education
        {
            get { return education.Copy(); }
        }

        public readonly Procent needsFulfilled;

        protected int daysUpsetByForcedReform;
        protected bool didntGetPromisedSalary;

        protected bool lastTurnDidntGetPromisedSocialBenefits;
        protected bool didntGetPromisedSocialBenefits;
        public bool LastTurnDidntGetPromisedSocialBenefits { get { return lastTurnDidntGetPromisedSocialBenefits; } }


        public static readonly ModifiersList modifiersLoyaltyChange, modEfficiency;

        private static readonly Modifier modifierLuxuryNeedsFulfilled, modifierCanVote, modifierCanNotVote, modifierEverydayNeedsFulfilled, modifierLifeNeedsFulfilled,
            modifierStarvation, modifierUpsetByForcedReform, modifierLifeNeedsNotFulfilled, modifierNotGivenUnemploymentSubsidies,
            modifierMinorityPolicy, modifierSomeEverydayNeedsFulfilled;


        private static readonly Modifier modCountryIsToBig = new Modifier(x => (x as PopUnit).Country.Provinces.Count > (x as PopUnit).Country.government.LoyaltySizeLimit, "That country is too big for good management", -0.5f, false);

        private readonly Date born;
        private Movement movement;

        /// <summary> PopType means promotion/demotion, Province means migration/immigration, null means growth/starvation</summary>
        private readonly FixedSizeQueue<KeyValuePair<IWayOfLifeChange, int>> populationChanges = new FixedSizeQueue<KeyValuePair<IWayOfLifeChange, int>>(12, new KeyValuePair<IWayOfLifeChange, int>(null, 0));

        //private KeyValuePair<IEscapeTarget, int> lastEscaped = new KeyValuePair<IEscapeTarget, int>();

        //if add new fields make sure it's implemented in second constructor and in merge()



        static PopUnit()
        {
            //makeModifiers();
            modifierStarvation = new Modifier(x => (x as PopUnit).needsFulfilled.get() < 0.20f, "Starvation", -0.3f, false);
            modifierLifeNeedsNotFulfilled = new Modifier(x => (x as PopUnit).getLifeNeedsFullfilling().get() < 0.99f, "Life needs are not satisfied", -0.2f, false);
            modifierLifeNeedsFulfilled = new Modifier(x => (x as PopUnit).getLifeNeedsFullfilling().get() > 0.99f, "Life needs are satisfied", 0.1f, false);
            modifierSomeEverydayNeedsFulfilled = new Modifier(x => (x as PopUnit).getEveryDayNeedsFullfilling().get() > 0.25f, "Some everyday needs are satisfied", 0.15f, false);
            modifierEverydayNeedsFulfilled = new Modifier(x => (x as PopUnit).getEveryDayNeedsFullfilling().get() > 0.99f, "Everyday needs are satisfied", 0.15f, false);
            modifierLuxuryNeedsFulfilled = new Modifier(x => (x as PopUnit).getLuxuryNeedsFullfilling().get() > 0.99f, "Luxury needs are satisfied", 0.2f, false);

            //Game.threadDangerSB.Clear();
            //Game.threadDangerSB.Append("Likes that government because can vote with ").Append(this.province.getOwner().government.ToString());
            modifierCanVote = new Modifier(x => (x as PopUnit).CanVoteInOwnCountry(), "Can vote with that government ", 0.1f, false);
            //Game.threadDangerSB.Clear();
            //Game.threadDangerSB.Append("Dislikes that government because can't vote with ").Append(this.province.getOwner().government.ToString());
            modifierCanNotVote = new Modifier(x => !(x as PopUnit).CanVoteInOwnCountry(), "Can't vote with that government ", -0.1f, false);
            //Game.threadDangerSB.Clear();
            //Game.threadDangerSB.Append("Upset by forced reform - ").Append(daysUpsetByForcedReform).Append(" days");
            modifierUpsetByForcedReform = new Modifier(x => (x as PopUnit).daysUpsetByForcedReform > 0, "Upset by forced reform", -3.0f, false);
            modifierNotGivenUnemploymentSubsidies = new Modifier(x => (x as PopUnit).didntGetPromisedSocialBenefits, "Didn't got promised social benefits", -1.0f, false);
            modifierMinorityPolicy = //new Modifier(MinorityPolicy.IsResidencyPop, 0.02f);
            new Modifier(x => !(x as PopUnit).isStateCulture()
            && ((x as PopUnit).Country.minorityPolicy == MinorityPolicy.Residency
            || (x as PopUnit).Country.minorityPolicy == MinorityPolicy.NoRights), "Is minority", -0.05f, false);

            //MinorityPolicy.IsResidency
            modifiersLoyaltyChange = new ModifiersList(new List<Condition>
        {
           modifierStarvation, modifierLifeNeedsNotFulfilled, modifierLifeNeedsFulfilled, modifierEverydayNeedsFulfilled,
        modifierLuxuryNeedsFulfilled, modifierCanVote, modifierCanNotVote, modifierUpsetByForcedReform, modifierNotGivenUnemploymentSubsidies,
            modifierMinorityPolicy,
            new Modifier(x => (x as PopUnit).didntGetPromisedSalary, "Didn't got promised salary", -1.0f, false),
            new Modifier (x => !(x as PopUnit).isStateCulture() &&
            (x as PopUnit).Province.hasModifier(TemporaryModifier.recentlyConquered), TemporaryModifier.recentlyConquered.ToString(), -1f, false),
            modCountryIsToBig, modifierSomeEverydayNeedsFulfilled
});

            // can increase performance by making separate modifiers for different popTypes
            modEfficiency = new ModifiersList(new List<Condition> {
            Modifier.modifierDefault1,
            new Modifier(x=>(x as PopUnit).Province.getOverpopulationAdjusted(x as PopUnit), "Overpopulation", -1f, false),
            new Modifier(Invention.SteamPower.Invented, x=>(x as PopUnit).Country, 0.25f, false),
            new Modifier(Invention.CombustionEngine.Invented, x=>(x as PopUnit).Country, 0.5f, false),

            new Modifier(Economy.isStateCapitlism, x=>(x as PopUnit).Country,  0.10f, false),
            new Modifier(Economy.isInterventionism, x=>(x as PopUnit).Country,  0.30f, false),
            new Modifier(Economy.isLF, x=>(x as PopUnit).Country,  0.50f, false),
            new Modifier(Economy.isPlanned, x=>(x as PopUnit).Country,  -0.10f, false),
            new Modifier(x=>(x as PopUnit).Education.RawUIntValue, "Education",  2f / Procent.Precision, true),

            //new Modifier(Serfdom.Allowed,  -20f, false)

            // copied in Factory
             new Modifier(x => Government.isPolis.checkIfTrue((x as PopUnit).Country)
             && (x as PopUnit).Country.Capital == (x as PopUnit).Province, "Capital of Polis", 0.5f, false),
             new Modifier(x=>(x as PopUnit).Province.hasModifier(TemporaryModifier.recentlyConquered), TemporaryModifier.recentlyConquered.ToString(), -0.20f, false),
             new Modifier(x=>(x as PopUnit).Country.government == Government.Tribal
             && (x as PopUnit).type!=PopType.Tribesmen, "Government is Tribal", -0.3f, false),
             new Modifier(Government.isDespotism, x=>(x as PopUnit).Country, -0.20f, false) // remove this?
        });
        }

        /// <summary>
        ///  Constructor for population created on game startup
        /// </summary>
        protected PopUnit(int amount, PopType popType, Culture culture, Province where) : base(where)
        {
            where.RegisterPop(this);
            born = new Date(Date.Today);

            type = popType;
            population = new Population(amount, this);
            this.culture = culture;

            education = new Education(0f);
            loyalty = new Procent(0.50f);
            needsFulfilled = new Procent(0.00f);
            //province = where;
        }

        /// <summary> Creates new PopUnit basing on part of other PopUnit.
        /// And transfers sizeOfNewPop population.
        /// </summary>
        protected PopUnit(PopUnit source, int sizeOfNewPop, PopType newPopType, Province where, Culture culture, IWayOfLifeChange oldLife)
            : base(where)
        {
            populationChanges.Enqueue(new KeyValuePair<IWayOfLifeChange, int>(oldLife, sizeOfNewPop));
            born = new Date(Date.Today);
            PopListToAddToGeneralList.Add(this);
            // makeModifiers();

            // here should be careful copying of popUnit data
            //And careful editing of old unit
            Procent newPopShare = new Procent(sizeOfNewPop, source.population.Get());

            //Own PopUnit fields:
            loyalty = new Procent(source.loyalty.get());
            type = newPopType;
            population = new Population(sizeOfNewPop, this);
            // if source pop is gonna be dead..
            if (source.population.Get() - sizeOfNewPop <= 0 && type == PopType.Aristocrats || type == PopType.Capitalists)
            //secede property... to new pop..
            //todo - can optimize it, double run on List
            {
                var isSourceInvestor = source as Investor;
                var isThisInvestor = this as Investor;
                if (isSourceInvestor != null)
                {
                    if (isThisInvestor != null)
                        isSourceInvestor.getOwnedFactories().PerformAction(x => x.ownership.TransferAll(isSourceInvestor, isThisInvestor));
                    else
                        isSourceInvestor.getOwnedFactories().PerformAction(x => x.ownership.TransferAll(isSourceInvestor, Country));
                }
            }
            mobilized = 0;

            this.culture = culture;
            education = new Education(source.education.get());
            needsFulfilled = new Procent(source.needsFulfilled.get());
            daysUpsetByForcedReform = 0;
            didntGetPromisedSocialBenefits = false;
            //incomeTaxPayed = newPopShare.sendProcentToNew(source.incomeTaxPayed);

            //Agent's fields:
            //wallet = new Wallet(0f, where.Country.bank); it's already set in constructor
            //bank - could be different, set in constructor
            //loans - keep it in old unit

            //take deposit share
            if (source.deposits.isNotZero())
            {
                MoneyView returnDeposit = source.deposits.Copy().Multiply(newPopShare);
                source.PayWithoutRecord(this, source.Bank.ReturnDeposit(source, returnDeposit), Register.Account.Rest);
            }
            //take Cash
            source.PayWithoutRecord(this, source.Cash.Copy().Multiply(newPopShare), Register.Account.Rest);

            //Producer's fields:
            //if convert from artisan to non-artisan
            //if (source.popType == PopType.Artisans && newPopType != PopType.Artisans)
            //{
            if (newPopType == PopType.Tribesmen)
            {
                //storage = new Storage(Product.Cattle);
                //gainGoodsThisTurn = new Storage(Product.Cattle);
                //sentToMarket = new Storage(Product.Cattle);
                changeProductionType(Product.Cattle);
            }
            else
            {
                //storage = new Storage(Product.Grain);
                //gainGoodsThisTurn = new Storage(Product.Grain);
                //sentToMarket = new Storage(Product.Grain);
                changeProductionType(Product.Grain);
            }
            //}
            //else
            //{
            //    storage = newPopShare.sendProcentToNew(source.storage);
            //    gainGoodsThisTurn = new Storage(source.gainGoodsThisTurn.Product);
            //    sentToMarket = new Storage(source.sentToMarket.Product);
            //}

            //province = where;//source.province;

            //Consumer's fields:
            // Do I really need it?
            //getConsumed().setZero(); //cleared in Consumer
            getConsumedLastTurn().setZero();// = new PrimitiveStorageSet();
            //getConsumedInMarket().setZero();// cleared in Consumer

            //kill in the end
            source.population.Change(-1 * sizeOfNewPop);
        }

        /// <summary>
        /// Merging source into this pop
        /// assuming that both pops are in same province, and has same type
        /// culture defaults to this.culture
        /// </summary>
        public void mergeIn(PopUnit source)
        {
            //carefully summing 2 pops..
            populationChanges.Add(source.populationChanges);
            //Own PopUnit fields:
            loyalty.AddPoportionally(population.Get(), source.population.Get(), source.loyalty);


            mobilized += source.mobilized;
            //type = newPopType; don't change that
            //culture = source.culture; don't change that
            education.AddPoportionally(population.Get(), source.population.Get(), source.education);
            needsFulfilled.AddPoportionally(population.Get(), source.population.Get(), source.needsFulfilled);
            //daysUpsetByForcedReform = 0; don't change that
            //didntGetPromisedUnemloymentSubsidy = false; don't change that

            //Agent's fields:
            source.PayAllAvailableMoneyWithoutRecord(this, Register.Account.Rest); // includes deposits
            loans.Add(source.loans);
            // Bank - stays same

            //Producer's fields:
            if (storage.isExactlySameProduct(source.storage))
                storage.add(source.storage);
            // looks I don't need - it erases every tick anyway
            //if (gainGoodsThisTurn.isSameProduct(source.gainGoodsThisTurn))
            //    gainGoodsThisTurn.add(source.gainGoodsThisTurn);
            // looks I don't need - it erases every tick anyway
            //if (sentToMarket.isSameProduct(source.sentToMarket))
            //    sentToMarket.add(source.sentToMarket);

            //province - read header

            //consumer's fields
            //isn't that important. That is fucking important
            //getConsumed().Add(source.getConsumed());
            consumed.Add(source.consumed);

            getConsumedLastTurn().Add(source.getConsumedLastTurn());
            consumedInMarket.AddRange(source.consumedInMarket);

            //province = source.province; don't change that

            //if (source.population - sizeOfNewPop <= 0)// if source pop is gonna be dead..It gonna be, for sure
            //secede property... to new pop..
            //todo - can optimize it, double run on List. Also have point only in Consolidation, not for PopUnit.PopListToAddToGeneralList
            //that check in not really needed as it this pop supposed to be same type as source
            //if (this.type == PopType.aristocrats || this.type == PopType.capitalists)
            var isSourceInvestor = source as Investor;
            var isThisInvestor = this as Investor;
            if (isSourceInvestor != null)
            {
                if (isThisInvestor != null)
                    isSourceInvestor.getOwnedFactories().PerformAction(x => x.ownership.TransferAll(isSourceInvestor, isThisInvestor));
                else
                    isSourceInvestor.getOwnedFactories().PerformAction(x => x.ownership.TransferAll(isSourceInvestor, Country));
            }

            population.Change(source.population.Get());
            // basically, killing that unit. Currently that object is linked in PopUnit.PopListToAddInGeneralList only so don't worry
            source.Kill();
        }

        /// <summary>
        /// Sets population to zero as a mark to delete this Pop
        /// </summary>
        public virtual void Kill()
        {
            population.Delete();
            //province.allPopUnits.Remove(this); // gives exception
            //Game.popsToShowInPopulationPanel.Remove(this);
            //remove from population panel.. Would do it automatically
            if (MainCamera.popUnitPanel.whomShowing() == this)
                MainCamera.popUnitPanel.Hide();


            PayAllAvailableMoney(Bank, Register.Account.Rest); // just in case if there is something
            Bank.OnLoanerRefusesToPay(this);
            Movement.leave(this);
        }

        //public Culture Culture
        //{
        //    return culture;
        //}
        // have to be this way!
        public abstract int getVotingPower(Government.GovernmentReformValue reformValue);

        //public abstract int getVotingPower();
        //{
        //    return getVotingPower(Country.government);
        //}

        public override void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            needsFulfilled.SetZero();
            lastTurnDidntGetPromisedSocialBenefits = didntGetPromisedSocialBenefits;
            didntGetPromisedSocialBenefits = false;

            // sets in ConsumeNeeds now
            //if (type != PopType.Aristocrats)
            //    storage.SetZero();  // may mess with aristocrats

            Rand.Call(() => education.Subtract(0.001f, false), Options.PopEducationRegressChance);
        }

        public int getMobilized()
        {
            return mobilized;
        }

        //abstract public Procent howIsItGoodForMe(AbstractReformValue reform);

        public int getAge()
        {
            //return Game.date - born;
            return born.getYearsSince();
        }

        public int howMuchCanMobilize(Staff byWhom, Staff againstWho)
        {
            int howMuchCanMobilizeTotal = 0;
            if (byWhom == Country)
            {
                if (getMovement() == null || (!getMovement().isInRevolt() && getMovement() != againstWho))
                //if (this.loyalty.isBiggerOrEqual(Options.PopMinLoyaltyToMobilizeForGovernment))
                {
                    if (type == PopType.Soldiers)
                        howMuchCanMobilizeTotal = (int)(population.Get() * 0.5);
                    else
                        howMuchCanMobilizeTotal = (int)(population.Get() * loyalty.get() * Options.ArmyMobilizationFactor);
                }
            }
            else
            {
                if (byWhom == getMovement())
                    //howMuchCanMobilizeTotal = (int)(population.Get() * (Procent.HundredProcent.get() - loyalty.get()) * Options.ArmyMobilizationFactor);
                    howMuchCanMobilizeTotal = (int)Procent.HundredProcent.Copy().Subtract(loyalty).Multiply(population.Get()).Multiply(Options.ArmyMobilizationFactor).get();
                else
                    howMuchCanMobilizeTotal = 0;
            }
            howMuchCanMobilizeTotal -= mobilized; //shouldn't mobilize more than howMuchCanMobilize
            if (howMuchCanMobilizeTotal < Options.PopMinimalMobilazation) // except if it's remobilization
                howMuchCanMobilizeTotal = 0;
            return howMuchCanMobilizeTotal;
        }

        public Staff whoMobilized()
        {
            if (getMobilized() == 0)
                return null;
            else
            {
                if (getMovement() != null && getMovement().isInRevolt())
                    return getMovement();
                else
                    return Country;
            }
        }

        public int mobilize(Staff byWho)
        {
            int amount = howMuchCanMobilize(byWho, null);
            if (amount > 0)
            {
                mobilized += amount;
                return amount;// CorpsPool.GetObject(this, amount);
            }
            else
                return 0;// null;
        }

        public void demobilize()
        {
            mobilized = 0;
        }

        public void takeLoss(int loss, IWayOfLifeChange reason)
        {
            //int newPopulation = population.Get() - (int)(loss * Options.PopAttritionFactor);
            var change = -1 * (int)(loss * Options.PopAttritionFactor);
            population.Change(change);
            populationChanges.Enqueue(new KeyValuePair<IWayOfLifeChange, int>(reason, change));
            mobilized -= loss;
            if (mobilized < 0)
                mobilized = 0;
        }

        public void addDaysUpsetByForcedReform(int popDaysUpsetByForcedReform)
        {
            daysUpsetByForcedReform += popDaysUpsetByForcedReform;
        }

        /// <summary>
        /// Creates Pop in PopListToAddToGeneralList, later in will go to proper List
        /// </summary>
        public static PopUnit makeVirtualPop(PopType targetType, PopUnit source, int sizeOfNewPop, Province where, Culture culture, IWayOfLifeChange newLife)
        {
            if (targetType == PopType.Tribesmen) return new Tribesmen(source, sizeOfNewPop, where, culture, newLife);
            else
                if (targetType == PopType.Farmers) return new Farmers(source, sizeOfNewPop, where, culture, newLife);
            else
                if (targetType == PopType.Aristocrats) return new Aristocrats(source, sizeOfNewPop, where, culture, newLife);
            else
                if (targetType == PopType.Workers) return new Workers(source, sizeOfNewPop, where, culture, newLife);
            else
                if (targetType == PopType.Capitalists) return new Capitalists(source, sizeOfNewPop, where, culture, newLife);
            else
                if (targetType == PopType.Soldiers) return new Soldiers(source, sizeOfNewPop, where, culture, newLife);
            else
                if (targetType == PopType.Artisans) return new Artisans(source, sizeOfNewPop, where, culture, newLife);
            else
            {
                Debug.Log("Unknown pop type!");
                return null;
            }
        }

        public bool getSayingYes(IReformValue reform)
        {
            return reform.getVotingPower(this) > Options.votingPassBillLimit;
        }

        public static int getRandomPopulationAmount(int minGeneratedPopulation, int maxGeneratedPopulation)
        {
            int randomPopulation = minGeneratedPopulation + Rand.Get.Next(maxGeneratedPopulation - minGeneratedPopulation);
            return randomPopulation;
        }

        public bool IsAlive
        {
            get
            {
                return population.Get() > 0;
            }
        }

        /// <summary>
        /// makes new list of new elements
        /// </summary>
        //private List<Storage> getNeedsInCommon(List<Storage> needs)
        //{
        //    Value multiplier = new Value(this.population.Get() / 1000f);
        //    foreach (Storage next in needs)
        //    {
        //        Storage nStor = new Storage(next.Product, next.get());
        //        nStor.multiply(multiplier);
        //    }
        //    return needs;
        //}



        //public void payTaxes() // should be abstract
        //{
        //    if (Econ.isMarket.checkIftrue(Country) && popType != PopType.Tribesmen)
        //    {
        //        Value taxSize;
        //        if (this.popType.isPoorStrata())
        //        {
        //            taxSize = moneyIncomethisTurn.Copy().multiply((Country.taxationForPoor.getValue() as TaxationForPoor.ReformValue).tax);
        //            if (canPay(taxSize))
        //            {
        //                incomeTaxPayed = taxSize;
        //                Country.poorTaxIncomeAdd(taxSize);
        //                pay(Country, taxSize);
        //            }
        //            else
        //            {
        //                incomeTaxPayed.set(Cash);
        //                Country.poorTaxIncomeAdd(Cash);
        //                sendAllAvailableMoney(Country);

        //            }
        //        }
        //        else
        //        if (this.popType.isRichStrata())
        //        {
        //            taxSize = moneyIncomethisTurn.Copy().multiply((Country.taxationForRich.getValue() as TaxationForRich.ReformValue).tax);
        //            if (canPay(taxSize))
        //            {
        //                incomeTaxPayed.set(taxSize);
        //                Country.richTaxIncomeAdd(taxSize);
        //                pay(Country, taxSize);
        //            }
        //            else
        //            {
        //                incomeTaxPayed.set(Cash);
        //                Country.richTaxIncomeAdd(Cash);
        //                sendAllAvailableMoney(Country);
        //            }
        //        }

        //    }
        //    else// non market
        //        //if (this.popType != PopType.Aristocrats)
        //    {
        //        Storage howMuchSend;
        //        if (this.popType.isPoorStrata())
        //            howMuchSend = getGainGoodsThisTurn().multiplyOutside((Country.taxationForPoor.getValue() as TaxationForPoor.ReformValue).tax);
        //        else
        //        {
        //            //if (this.popType.isRichStrata())
        //            howMuchSend = getGainGoodsThisTurn().multiplyOutside((Country.taxationForRich.getValue() as TaxationForRich.ReformValue).tax);
        //        }
        //        if (storage.isBiggerOrEqual(howMuchSend))
        //            storage.send(Country.countryStorageSet, howMuchSend);
        //        else
        //            storage.sendAll(Country.countryStorageSet);
        //    }
        //}

        public bool isStateCulture()
        {
            return culture == Country.Culture;
        }

        //virtual public bool CanGainDividents()
        //{
        //    return false;
        //}
        /// <summary>
        /// new value
        /// </summary>
        public Procent getLifeNeedsFullfilling()
        {
            float need = needsFulfilled.get();
            if (need < Options.PopOneThird)
                return new Procent(needsFulfilled.get() * Options.PopStrataWeight.get());
            else
                return Procent.HundredProcent.Copy();
        }

        /// <summary>
        /// new value
        /// </summary>
        public Procent getEveryDayNeedsFullfilling()
        {
            float need = needsFulfilled.get();
            if (need <= Options.PopOneThird)
                return Procent.ZeroProcent.Copy();
            if (need < Options.PopTwoThird)
                return new Procent((needsFulfilled.get() - Options.PopOneThird) * Options.PopStrataWeight.get());
            //return needsFullfilled;
            else
                return Procent.HundredProcent.Copy();
        }

        /// <summary>
        /// new value
        /// </summary>
        public Procent getLuxuryNeedsFullfilling()
        {
            float need = needsFulfilled.get();
            if (need <= Options.PopTwoThird)
                return Procent.ZeroProcent.Copy();
            //if (need == 0.999f)
            //    return Procent.HundredProcent;
            //else
            //    return new Procent((needsFullfilled.get() - Options.PopTwoThird) * Options.PopStrataWeight.get());
            //if (needsFullfilled.isSmallerThan(Procent.HundredProcent))
            return new Procent((needsFulfilled.get() - Options.PopTwoThird) * Options.PopStrataWeight.get());
            //else
            //    return new
        }

        /// <summary>
        /// !!Recursion is here!! Used for NE consumption
        /// </summary>
        private void consumeEveryDayAndLuxury(IEnumerable<Storage> needs, byte howDeep)
        {
            howDeep--;
            //List<Storage> needs = getEveryDayNeeds();
            foreach (Storage need in needs)
                if (storage.has(need))
                {
                    //storage.subtract(need);
                    //consumedTotal.add(need);
                    consumeFromItself(need); // todo fails if is abstract
                    needsFulfilled.Set(2f / 3f);
                    if (howDeep != 0) consumeEveryDayAndLuxury(population.getRealLuxuryNeeds(), howDeep);
                }
                else
                {
                    float canConsume = storage.get();
                    //consumedTotal.add(storage);
                    //storage.set(0);
                    consumeFromItself(storage); // todo fails if is abstract
                    needsFulfilled.Add(canConsume / need.get() / 3f);
                }
        }

        private void consumeNeedsWithMarket()
        {
            //buy life needs
            MoneyView moneyWasBeforeLifeNeedsConsumption = getMoneyAvailable();
            foreach (Storage need in population.getRealLifeNeeds())
            {
                if (storage.has(need))// don't need to buy on market
                {
                    Storage realConsumption;
                    if (need.isAbstractProduct())
                        realConsumption = new Storage(storage.Product, need);
                    else
                        realConsumption = need;
                    consumeFromItself(realConsumption);
                    needsFulfilled.Set(Options.PopOneThird);
                }
                else
                    needsFulfilled.Set(Buy(need).Divide(need).Divide(Options.PopStrataWeight));
            }
            if (type != PopType.Aristocrats)
                storage.SetZero();

            // buy everyday needs
            if (getLifeNeedsFullfilling().isBiggerOrEqual(Procent.HundredProcent))
            {
                // save some money in reserve to avoid spending all money on luxury
                Money reserve = new Money(0m);
                //PutInStash(Cash.Copy().Multiply(Options.savePopMoneyReserv));
                PayWithoutRecord(reserve, Cash.Copy().Multiply(Options.savePopMoneyReserv));

                //Value moneyWasBeforeEveryDayNeedsConsumption = getMoneyAvailable();
                var everyDayNeedsConsumed = new List<Storage>();
                foreach (Storage need in population.getRealEveryDayNeeds())
                {
                    var consumed = Buy(need);
                    if (consumed.isNotZero())
                    {
                        everyDayNeedsConsumed.Add(consumed);
                        if (consumed.Product == Product.Education && consumed.isBiggerOrEqual(need))
                            education.Learn();
                    }
                }
                var everyDayNeedsFulfilled = new Procent(everyDayNeedsConsumed, population.getRealEveryDayNeeds());
                everyDayNeedsFulfilled.Divide(Options.PopStrataWeight);
                needsFulfilled.Add(everyDayNeedsFulfilled);

                // buy luxury needs
                bool hasAnyLuxuryNeeds = false;
                if (getEveryDayNeedsFullfilling().isBiggerOrEqual(Procent.HundredProcent))
                {
                    var luxuryNeeds = population.getRealLuxuryNeeds();

                    //Value moneyWasBeforeLuxuryNeedsConsumption = getMoneyAvailable();
                    bool someLuxuryProductUnavailable = false;
                    List<Storage> luxuryNeedsConsumed = null;
                    if (!luxuryNeeds.IsEmpty())
                        luxuryNeedsConsumed = new List<Storage>();
                    foreach (Storage nextNeed in luxuryNeeds)
                    {
                        var consumed = Buy(nextNeed);
                        if (consumed.isZero())
                            someLuxuryProductUnavailable = true;
                        else
                        {
                            luxuryNeedsConsumed.Add(consumed);
                            hasAnyLuxuryNeeds = true;
                            if (consumed.Product == Product.Education && consumed.isBiggerOrEqual(nextNeed))
                                education.Learn();
                        }
                    }
                    MoneyView luxuryNeedsCost;
                    if (luxuryNeedsConsumed != null && luxuryNeedsConsumed.Count > 0)
                        luxuryNeedsCost = Market.GetCheapestMarket(luxuryNeedsConsumed[0]).getCost(luxuryNeeds);
                    else
                        luxuryNeedsCost = MoneyView.Zero.Copy();

                    // unlimited consumption
                    // unlimited luxury spending should be limited by money income and already spent money
                    // I also can limit regular luxury consumption but should I?:
                    if (!someLuxuryProductUnavailable && hasAnyLuxuryNeeds)
                    //&& Cash.isBiggerThan(Options.PopUnlimitedConsumptionLimit))  // need that to avoid poor pops
                    {
                        MoneyView spentMoneyOnAllNeeds = moneyWasBeforeLifeNeedsConsumption.Copy().Subtract(getMoneyAvailable(), false);// moneyWas - Cash.get() could be < 0 due to taking money from deposits
                        MoneyView spendingLimit = Register.IncomeLastTurn.Copy().Subtract(spentMoneyOnAllNeeds, false);//limit is income minus expenses minus reserves
                        // if gain more than consumed then spent it on extra luxury consumption                        

                        MoneyView spentOnUnlimitedConsumption;
                        if (Cash.isBiggerThan(spendingLimit))
                            spentOnUnlimitedConsumption = spendingLimit; // don't spent more than gained
                        else
                            spentOnUnlimitedConsumption = Cash; // don't spent savings on extra luxury consumption

                        if (spentOnUnlimitedConsumption.isNotZero())// to avoid zero values
                        {
                            // how much pop wants to spent on unlimited consumption. Pop should spent Cash only..
                            float buyExtraGoodsMultiplier = (float)(spentOnUnlimitedConsumption.Get() / luxuryNeedsCost.Get());
                            foreach (Storage nextNeed in luxuryNeeds)
                            {
                                nextNeed.Multiply(buyExtraGoodsMultiplier);
                                var consumed = Buy(nextNeed);
                                if (consumed.isNotZero())
                                    luxuryNeedsConsumed.Add(consumed);
                            }
                        }
                    }

                    if (luxuryNeedsConsumed != null)
                    {
                        var luxuryNeedsFulfilled = new Procent(luxuryNeedsConsumed, population.getRealLuxuryNeeds(), false);
                        luxuryNeedsFulfilled.Divide(Options.PopStrataWeight);
                        needsFulfilled.Add(luxuryNeedsFulfilled);
                    }
                }
                cash.Add(reserve);
            }
        }

        protected void consumeWithNaturalEconomy(IEnumerable<Storage> lifeNeeds)
        {
            Country.TakeNaturalTax(this, Country.taxationForPoor.tax); //payTaxes(); // that is here because pop should pay taxes from all income
            foreach (Storage need in lifeNeeds)
                if (storage.has(need))// don't need to buy on market
                {
                    Storage realConsumption;
                    if (need.isAbstractProduct())
                        realConsumption = new Storage(storage.Product, need);
                    else
                        realConsumption = need;

                    consumeFromItself(realConsumption);
                    needsFulfilled.Set(1f / 3f);
                    consumeEveryDayAndLuxury(population.getRealEveryDayNeeds(), 2);
                }
                else
                {
                    //its about lifeneeds only
                    float canConsume = storage.get();
                    consumeFromItself(storage); // todo fails if is abstract
                    needsFulfilled.Set(canConsume / need.get() / 3f);
                }
        }

        /// <summary>
        /// Returns list of actually consumed
        /// </summary>
        private List<Storage> consumeWithPlannedEconomy(IEnumerable<Storage> needs)
        {
            var consumed = new List<Storage>();
            foreach (var need in needs)
            {
                if (Country.countryStorageSet.hasMoreThan(need, Options.CountryPopConsumptionLimitPE)
                    || (Country.countryStorageSet.has(need) && !need.IsStorabe))
                    if (need.isAbstractProduct())
                    {
                        var stor = Country.countryStorageSet.convertToBiggestStorage(need);
                        consumeFromCountryStorage(stor, Country);
                        consumed.Add(stor);
                    }
                    else
                    {
                        consumeFromCountryStorage(need, Country);
                        consumed.Add(need);
                        if (need.Product == Product.Education)
                            education.Learn();
                    }
            }
            return consumed;
        }

        /// <summary> !!! Overloaded for artisans and tribesmen </summary>
        public override void consumeNeeds()
        {
            //life needs First
            if (canTrade())
            {
                consumeNeedsWithMarket();
            }
            else if (Country.economy == Economy.PlannedEconomy)//non - market consumption
            {
                // todo - !! - check for substitutes
                consumeWithPlannedEconomy(population.getRealLifeNeeds());
                var lifeNeedsFulfilled = new Procent(getConsumed(), population.getRealLifeNeeds());
                lifeNeedsFulfilled.Divide(Options.PopStrataWeight);
                needsFulfilled.Set(lifeNeedsFulfilled);

                var everyDayNeedsConsumed = consumeWithPlannedEconomy(population.getRealEveryDayNeeds());
                if (getLifeNeedsFullfilling().isBiggerOrEqual(Procent.HundredProcent))
                {
                    var everyDayNeedsFulfilled = new Procent(everyDayNeedsConsumed, population.getRealEveryDayNeeds());
                    everyDayNeedsFulfilled.Divide(Options.PopStrataWeight);
                    needsFulfilled.Add(everyDayNeedsFulfilled);
                }

                var luxuryNeedsConsumed = consumeWithPlannedEconomy(population.getRealLuxuryNeeds());
                if (getEveryDayNeedsFullfilling().isBiggerOrEqual(Procent.HundredProcent))
                {
                    var luxuryNeedsFulfilled = new Procent(luxuryNeedsConsumed, population.getRealLuxuryNeeds());
                    luxuryNeedsFulfilled.Divide(Options.PopStrataWeight);
                    needsFulfilled.Add(luxuryNeedsFulfilled);
                }
            }
            else
                consumeWithNaturalEconomy(population.getRealLifeNeeds());
        }

        /// <summary>
        /// Overrode for some pop types
        /// </summary>
        public virtual bool canTrade()
        {
            if (Economy.isMarket.checkIfTrue(Country))
                return true;
            else
                return false;
        }

        public virtual bool canSellProducts()
        {
            return false;
        }

        public bool CanVoteInOwnCountry()
        {
            return CanVoteWithThatGovernment(Country.government.typedValue);
        }
        //public bool canVote(Government reform)
        //{
        //    return canVote(reform);
        //}
        public abstract bool CanVoteWithThatGovernment(Government.GovernmentReformValue reform);

        public IEnumerable<KeyValuePair<IReformValue, float>> getIssues()
        {
            foreach (var reform in Country.Politics.AllReforms)
                foreach (IReformValue reformValue in reform.AllPossibleValues)
                    if (reformValue.IsAllowed(Country, reformValue))
                    {
                        var howGood = reformValue.getVotingPower(this);//.howIsItGoodForPop(this);
                                                                       //if (howGood.isExist())
                        if (howGood > 0f)
                            yield return new KeyValuePair<IReformValue, float>(reformValue, Value.Convert(howGood));
                    }
            var separatismTarget = getPotentialSeparatismTarget();
            if (!ReferenceEquals(separatismTarget, null))
            {
                var howGood = separatismTarget.getVotingPower(this);
                if (howGood > 0f)
                    yield return new KeyValuePair<IReformValue, float>(separatismTarget, Value.Convert(howGood));
            }
        }

        public KeyValuePair<AbstractReform, IReformValue> getMostImportantIssue()
        {
            var list = new Dictionary<KeyValuePair<AbstractReform, IReformValue>, float>();
            foreach (var reform in Country.Politics.AllReforms)
                foreach (IReformValue reformValue in reform.AllPossibleValues)
                    if (reformValue.IsAllowed(Country, reformValue))
                    {
                        var howGood = reformValue.getVotingPower(this);//.howIsItGoodForPop(this);
                                                                       //if (howGood.isExist())
                        if (howGood > 0f)
                            list.Add(new KeyValuePair<AbstractReform, IReformValue>(reform, reformValue), howGood);
                    }
            var separatismTarget = getPotentialSeparatismTarget();
            if (!(ReferenceEquals(separatismTarget, null)))
            {
                var howGood = separatismTarget.getVotingPower(this);
                if (howGood > 0f)
                    list.Add(new KeyValuePair<AbstractReform, IReformValue>(null, separatismTarget), howGood);
            }
            return list.MaxByRandom(x => x.Value).Key;
        }

        private Separatism.Goal getPotentialSeparatismTarget()
        {
            foreach (var country in Province.AllCores())
            {
                if (!country.IsAlive && country != Country && country.Culture == culture)//todo doesn't supports different countries for same culture
                {
                    return Separatism.Get(country);
                }
            }
            return null;
        }

        public void calcLoyalty()
        {
            float newRes = loyalty.get() + modifiersLoyaltyChange.getModifier(this) / 100f;
            loyalty.Set(Mathf.Clamp01(newRes));
            if (daysUpsetByForcedReform > 0)
                daysUpsetByForcedReform--;

            if (loyalty.isSmallerThan(Options.PopLowLoyaltyToJoinMovevent))
                Movement.join(this);
            else
            {
                if (loyalty.isBiggerThan(Options.PopHighLoyaltyToleaveMovevent))
                    Movement.leave(this);
            }
        }

        public void setMovement(Movement movement)
        {
            this.movement = movement;
        }

        public Movement getMovement()
        {
            return movement;
        }

        public override void simulate()
        {
            // it's in game.simulate
        }

        // Not called in capitalism
        public void payTaxToAllAristocrats()
        {
            Value taxSize = getGainGoodsThisTurn().Multiply(Country.serfdom.AristocratTax);
            Province.shareWithAllAristocrats(storage, taxSize);
        }

        public abstract bool shouldPayAristocratTax();

        public void Promote()
        {
            int promotionSize = GetPopulationChangeAmount(Options.PopPromotionSpeed);
            bool isPromoted = false;
            if (wantsToPromote() && promotionSize > 0 && population.Get() >= promotionSize)
            {
                var promoteTo = getRichestPromotionTarget();
                //promote(promotedTo, promotionSize);
                if (promoteTo != null)
                {
                    makeVirtualPop(promoteTo, this, promotionSize, Province, culture, type);
                    populationChanges.Enqueue(new KeyValuePair<IWayOfLifeChange, int>(promoteTo, promotionSize * -1));
                    isPromoted = true;
                }
            }
            if (!isPromoted)
                populationChanges.EnqueueEmpty();
        }

        /// <summary>
        /// Returns amount of people who wants change their lives (by demotion\migration\immigration)
        /// Result could be zero
        /// </summary>
        public int GetPopulationChangeAmount(ReadOnlyValue procent)
        {
            int result = (int)(population.Get() * procent.get());
            if (result > 0)
                return result;
            else if (result < 0)
            {
                Debug.Log("Population change Can't be negative"); //todo what about dead pops?
                return 0;
            }
            else
            {
                if (//Province.hasAnotherPop(this.type) &&
                    getAge() > Options.PopAgeLimitToWipeOut)
                    return population.Get();// wipe-out
                else
                    return 0;
            }
        }

        public bool wantsToPromote()
        {
            if (needsFulfilled.isBiggerThan(Options.PopNeedsPromotionLimit))
                return true;
            else
                return false;
        }

        public PopType getRichestPromotionTarget()
        {
            Dictionary<PopType, ReadOnlyValue> list = new Dictionary<PopType, ReadOnlyValue>();
            foreach (PopType nextType in PopType.getAllPopTypes())
                if (canThisPromoteInto(nextType))
                    list.Add(nextType, Province.getAverageNeedsFulfilling(nextType));
            var result = list.MaxBy(x => x.Value.get());
            if (result.Value != null && result.Value.get() > needsFulfilled.get())
                return result.Key;
            else
                return null;
        }

        public abstract bool canThisPromoteInto(PopType popType);

        //private void promote(PopType targetType, int amount)
        //{
        //    if (targetType != null)
        //    {
        //        PopUnit.makeVirtualPop(targetType, this, amount, this.Province, this.culture, type);
        //    }
        //}

        //private void ChangePopulation(int change)
        //{            
        //    var newPopulation = population.Get() + change;
        //    if (newPopulation > 0)
        //        population.Change(change);
        //    else
        //        deleteData();
        //}
        public virtual Procent GetSeekingJob()
        {
            return Procent.ZeroProcent.Copy();
        }

        public virtual Procent GetUnemployment()
        {
            return Procent.ZeroProcent.Copy();
        }

        public void takeUnemploymentSubsidies()
        {
            // no subsidies with PE
            // maybe replace by Condition?
            var reform = Country.unemploymentSubsidies;
            if (Type == PopType.Workers && Country.economy != Economy.PlannedEconomy && reform != UnemploymentSubsidies.None
                && (isStateCulture() || Country.minorityPolicy == MinorityPolicy.Equality))
            {
                var unemployment = GetUnemployment();
                if (unemployment.isNotZero())
                {
                    var rate = reform.SubsizionSize.Get();
                    MoneyView subsidy = rate.Copy().Multiply(population.Get()).Divide(1000);

                    if (!Country.Pay(this, subsidy, Register.Account.UnemploymentSubsidies))
                    {
                        didntGetPromisedSocialBenefits = true;
                        //Country.Politics.RegisterDefaultedSocialObligations(subsidy);
                    }
                }
            }
        }

        public void TakeUBISubsidies()
        {
            // no subsidies with PE
            // maybe replace by Condition?
            var reform = Country.UBI;
            if (canTrade() && Country.economy != Economy.PlannedEconomy && reform != UBI.None
                && (isStateCulture() || Country.minorityPolicy == MinorityPolicy.Equality))
            {
                var rate = reform.UBISize.Get();
                MoneyView subsidy = rate.Copy().Multiply(population.Get()).Divide(1000);

                if (!Country.Pay(this, subsidy, Register.Account.UBISubsidies))
                {
                    didntGetPromisedSocialBenefits = true;
                }
            }
        }
        public void TakePovertyAid()
        {
            // no subsidies with PE
            var reform = Country.PovertyAid;
            if (canTrade() && Country.economy != Economy.PlannedEconomy && reform != PovertyAid.None
                && (isStateCulture() || Country.minorityPolicy == MinorityPolicy.Equality))
            {
                var rate = reform.PovertyAidSize.Get();
                MoneyView subsidy = rate.Copy().Multiply(population.Get()).Divide(1000);
                var haveToPay = (subsidy as Money).Subtract(Register.Income, false); // subsidy - income
                if (haveToPay.isNotZero())
                {
                    if (!Country.Pay(this, subsidy, Register.Account.PovertyAid))
                    {
                        didntGetPromisedSocialBenefits = true;
                        //Country.Politics.RegisterDefaultedSocialObligations(subsidy);
                    }
                }
            }
        }

        public void Growth()
        {
            var growth = getGrowthSize();

            if (growth == 0)
                populationChanges.EnqueueEmpty();
            else
            {
                population.Change(growth);
                populationChanges.Enqueue(new KeyValuePair<IWayOfLifeChange, int>(null, growth));
            }
        }

        public int getGrowthSize()
        {
            int result = 0;
            if (needsFulfilled.get() >= 0.33f) // positive growth
            {
                result = Mathf.RoundToInt(Country.FamilyPlanning.GrowthRate.get() * population.Get());//* Options.PopGrowthSpeed.get()
            }
            else if (needsFulfilled.get() >= 0.20f) // zero growth
                result = 0;
            else if (type != PopType.Farmers) //starvation
            {
                result = Mathf.RoundToInt(Options.PopStarvationSpeed.get() * population.Get() * -1);
                if (result * -1 > population.Get()) // total starvation
                    result = population.Get(); // wipe out
            }

            return result;
            //return (int)Mathf.RoundToInt(this.population * PopUnit.growthSpeed.get());
        }
        /// <summary>
        /// 
        /// </summary>
        public void Migrate()
        {

        }
        /// <summary>
        /// Splits pops. New pops changes life in richest way - by demotion, migration or immigration
        /// </summary>
        public void ChangeLife(IWayOfLifeChange lifeChange, ReadOnlyValue procent)
        {
            bool FoundBetterLife = false;
            int escapeSize = GetPopulationChangeAmount(procent);
            if (escapeSize > 0)
            {
                if (lifeChange != null)
                {
                    FoundBetterLife = true;
                    populationChanges.Enqueue(new KeyValuePair<IWayOfLifeChange, int>(lifeChange, escapeSize * -1));
                    var targetIsPopType = lifeChange as PopType;
                    if (targetIsPopType != null)
                    {
                        // assuming its PopType
                        makeVirtualPop(targetIsPopType, this, escapeSize, Province, culture, type);
                    }
                    else
                    {
                        // assuming its province
                        var targetIsProvince = lifeChange as Province;
                        // its both migration and immigration
                        makeVirtualPop(type, this, escapeSize, targetIsProvince, culture, Province);
                    }
                }
            }
            if (!FoundBetterLife)
                populationChanges.EnqueueEmpty();// register time passed
        }



        //private IEnumerable<KeyValuePair<IWayOfLifeChange, ReadOnlyValue>> GetAllPossibleLifeChanges()
        //{
        //    //***********migration inside country***********
        //    if (type == PopType.Farmers || type == PopType.Workers || type == PopType.Tribesmen)
        //        foreach (var proposedNewProvince in Province.getAllNeighbors().Where(x => x.Country == Country))
        //        //foreach (var proposedNewProvince in Country.getAllProvinces())
        //        {
        //            var targetPriority = proposedNewProvince.getLifeQuality(this, Type);//province.getAverageNeedsFulfilling(this.type);

        //            if (targetPriority.isNotZero())
        //                yield return new KeyValuePair<IWayOfLifeChange, ReadOnlyValue>(proposedNewProvince, targetPriority);
        //        }
        //    // ***********immigration***********
        //    //where to g0?
        //    // where life is rich and I where I have some rights
        //    if (type != PopType.Aristocrats && type != PopType.Capitalists) // redo

        //        foreach (var country in World.getAllExistingCountries())
        //            //if (
        //            //(country.Culture == this.culture || country.minorityPolicy.getValue() == MinorityPolicy.Equality)
        //            //&& country != this.Country)
        //            //foreach (var proposedNewProvince in country.getAllProvinces())
        //            //foreach (var proposedNewProvince in World.GetAllProvinces().Where(
        //            //province =>
        //            //province.Country != this.Country && province.Country != World.UncolonizedLand
        //            //&& (province.Country.Culture == this.culture || province.Country.minorityPolicy.getValue() == MinorityPolicy.Equality)
        //            //))

        //            foreach (var proposedNewProvince in Province.getAllNeighbors().Where(x => x.Country != Country))
        //            {
        //                var targetPriority = proposedNewProvince.getLifeQuality(this, Type);
        //                if (targetPriority.isNotZero())
        //                    yield return new KeyValuePair<IWayOfLifeChange, ReadOnlyValue>(proposedNewProvince, targetPriority);
        //            }
        //    // ***********demotion***********
        //    foreach (PopType proposedNewType in PopType.getAllPopTypes().Where(x => type.CanDemoteTo(x, Country)))
        //    {
        //        var targetPriority = Province.getLifeQuality(this, proposedNewType);
        //        if (targetPriority.isNotZero())
        //            yield return new KeyValuePair<IWayOfLifeChange, ReadOnlyValue>(proposedNewType, targetPriority);//.getAverageNeedsFulfilling(type));
        //    }
        //    // ***********promotion***********
        //    //foreach (PopType proposedNewType in PopType.getAllPopTypes().Where(x => canThisPromoteInto(x)))
        //    //{
        //    //    var targetPriority = Province.getEscapeValueFor(this, proposedNewType);
        //    //    if (targetPriority.isNotZero())
        //    //        yield return new KeyValuePair<IEscapeTarget, ReadOnlyValue>(proposedNewType, targetPriority);//.getAverageNeedsFulfilling(type));
        //    //}
        //}

        public IEnumerable<KeyValuePair<IWayOfLifeChange, ReadOnlyValue>> GetAllPossibleMigrations()
        {
            //***********migration inside country***********
            if (type == PopType.Farmers || type == PopType.Workers || type == PopType.Tribesmen)
                foreach (var proposedNewProvince in Province.AllNeighbors().Where(x => x.Country == Country))
                //foreach (var proposedNewProvince in Country.getAllProvinces())
                {
                    var targetPriority = proposedNewProvince.getLifeQuality(this);//province.getAverageNeedsFulfilling(this.type);

                    if (targetPriority.isNotZero())
                        yield return new KeyValuePair<IWayOfLifeChange, ReadOnlyValue>(proposedNewProvince, targetPriority);
                }
            // ***********immigration***********
            //where to g0?
            // where life is rich and I where I have some rights
            if (type != PopType.Aristocrats && type != PopType.Capitalists) // redo

                foreach (var country in World.AllExistingCountries())
                    //if (
                    //(country.Culture == this.culture || country.minorityPolicy.getValue() == MinorityPolicy.Equality)
                    //&& country != this.Country)
                    //foreach (var proposedNewProvince in country.getAllProvinces())
                    //foreach (var proposedNewProvince in World.GetAllProvinces().Where(
                    //province =>
                    //province.Country != this.Country && province.Country != World.UncolonizedLand
                    //&& (province.Country.Culture == this.culture || province.Country.minorityPolicy.getValue() == MinorityPolicy.Equality)
                    //))

                    foreach (var proposedNewProvince in Province.AllNeighbors().Where(x => x.Country != Country))
                    {
                        var targetPriority = proposedNewProvince.getLifeQuality(this);
                        if (targetPriority.isNotZero())
                            yield return new KeyValuePair<IWayOfLifeChange, ReadOnlyValue>(proposedNewProvince, targetPriority);
                    }
        }

        public IEnumerable<KeyValuePair<IWayOfLifeChange, ReadOnlyValue>> GetAllPossibleDemotions()
        {
            // ***********demotion***********
            foreach (PopType proposedNewType in PopType.getAllPopTypes().Where(x => type.CanDemoteTo(x, Country)))
            {
                var targetPriority = proposedNewType.getLifeQuality(this);
                if (targetPriority.isNotZero())
                    yield return new KeyValuePair<IWayOfLifeChange, ReadOnlyValue>(proposedNewType, targetPriority);//.getAverageNeedsFulfilling(type));
            }
        }

        public void Assimilate()
        {
            bool isAssimilated = false;
            if (!isStateCulture())
            {
                int assimilationSize = getAssimilationSize();
                if (assimilationSize > 0)
                {
                    makeVirtualPop(type, this, assimilationSize, Province, Country.Culture, culture);
                    populationChanges.Enqueue(new KeyValuePair<IWayOfLifeChange, int>(Country.Culture, assimilationSize * -1));
                    isAssimilated = true;
                }
            }
            if (!isAssimilated)
                populationChanges.EnqueueEmpty();
        }

        public int getAssimilationSize()
        {
            if (Province.isCoreFor(this))
                return 0;
            else
            {
                int assimilationSize;
                if (Country.minorityPolicy == MinorityPolicy.Equality)
                    assimilationSize = (int)(population.Get() * Options.PopAssimilationSpeedWithEquality.get());
                else
                    assimilationSize = (int)(population.Get() * Options.PopAssimilationSpeed.get());

                if (assimilationSize > 0)
                    return assimilationSize;
                else
                {
                    if (getAge() > Options.PopAgeLimitToWipeOut)
                        return population.Get(); // wipe-out
                    else
                        return 0;
                }
            }
        }

        public virtual void invest()
        {
            if (Country.Science.IsInvented(Invention.Banking))
            {
                //Hmm.. Here it's about some world average price..
                MoneyView extraMoney = Cash.Copy().Subtract(
                   // introduce some factual price?
                   Country.market.getCost(getRealAllNeeds()).Copy().Multiply(Options.PopDaysReservesBeforePuttingMoneyInBak)
                    , false);
                if (extraMoney.isNotZero())
                    Bank.ReceiveMoney(this, extraMoney);
            }
        }

        /// <summary>
        /// Returns last escape type - demotion, migration or immigration
        /// </summary>
        public IEnumerable<KeyValuePair<IWayOfLifeChange, int>> getAllPopulationChanges()
        {
            foreach (var item in populationChanges)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Returns last escape size (how much people)
        /// </summary>
        //public int getLastEscapeSize()
        //{
        //    return lastEscaped.Value;
        //}
        public string FullName
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(culture).Append(" ").Append(type).Append(" from ").Append(Province.FullName);
                //return popType + " from " + province;
                return sb.ToString();
            }
        }

        public string ShortName
        {
            get { return type.ToString(); }
        }

        public override string ToString()
        {
            return FullName;
        }

        public void OnClicked()
        {
            MainCamera.popUnitPanel.show(this);
        }

        public Procent GetEmployedOnProcessingEnterprise()
        {
            //foreach  Province.getAllFactories()

            var employed = Province.AllFactories.Where(x => !x.Type.isResourceGathering() && x.IsOpen).Sum(x => x.HowManyEmployed(this));
            return new Procent(employed, population.Get());

            //Province.getAllFactories().PerformAction(x =>
            //{
            //    thisPop += x.howManyEmployed(this);
            //});
            //        Procent result = new Procent(0f);
            //int calculatedPopulation = 0;
            //foreach (var factory in Province.getAllFactories())
            //{
            //    var thisPop= factory.howManyEmployed(this);
            //    result.AddPoportionally(calculatedPopulation, thisPop, selector(factory));
            //    calculatedPopulation += thisPop;
            //}
            //return result;
        }

        public void LearnByWork()
        {
            //if (Rand.Chance(education) || education.isZero()
            if (Rand.Chance(Options.PopLearnByWorkingChance)
                && education.isSmallerThan(Options.PopLearnByWorkingLimit)
                && GetEmployedOnProcessingEnterprise().get() > 0.9f)
                education.Add(0.001f);
        }
        public override IEnumerable<Storage> getRealAllNeeds()
        {
            return population.getRealAllNeeds();
        }
    }
}