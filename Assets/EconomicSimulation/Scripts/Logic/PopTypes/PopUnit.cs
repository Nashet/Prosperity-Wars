using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Nashet.UnityUIUtils;
using Nashet.Conditions;
using Nashet.ValueSpace;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    abstract public class PopUnit : Producer, IClickable
    {
        ///<summary>buffer popList. To avoid iteration breaks</summary>
        public readonly static List<PopUnit> PopListToAddToGeneralList = new List<PopUnit>();
        public static readonly Predicate<PopUnit> All = x => true;

        public readonly Procent loyalty;
        private int population;
        private int mobilized;

        public readonly PopType popType;
        public readonly Culture culture;
        public readonly Procent education;
        public readonly Procent needsFulfilled;

        private int daysUpsetByForcedReform;
        private bool didntGetPromisedUnemloymentSubsidy;
        protected bool didntGetPromisedSalary;

        public readonly static ModifiersList modifiersLoyaltyChange, modEfficiency;

        static readonly Modifier modifierLuxuryNeedsFulfilled, modifierCanVote, modifierCanNotVote, modifierEverydayNeedsFulfilled, modifierLifeNeedsFulfilled,
            modifierStarvation, modifierUpsetByForcedReform, modifierLifeNeedsNotFulfilled, modifierNotGivenUnemploymentSubsidies,
            modifierMinorityPolicy;
        static readonly Modifier modCountryIsToBig = new Modifier(x => (x as PopUnit).GetCountry().getSize() > (x as PopUnit).GetCountry().government.getTypedValue().getLoyaltySizeLimit(), "That country is too big for good management", -0.5f, false);

        public Value incomeTaxPayed = new Value(0);

        private readonly MyDate born;
        private Movement movement;
        private readonly KeyVal<IEscapeTarget, int> lastEscaped = new KeyVal<IEscapeTarget, int>();
        //if add new fields make sure it's implemented in second constructor and in merge()  


        static PopUnit()
        {
            //makeModifiers();
            modifierStarvation = new Modifier(x => (x as PopUnit).needsFulfilled.get() < 0.20f, "Starvation", -0.3f, false);
            modifierLifeNeedsNotFulfilled = new Modifier(x => (x as PopUnit).getLifeNeedsFullfilling().get() < 0.99f, "Life needs are not satisfied", -0.2f, false);
            modifierLifeNeedsFulfilled = new Modifier(x => (x as PopUnit).getLifeNeedsFullfilling().get() > 0.99f, "Life needs are satisfied", 0.1f, false);
            modifierEverydayNeedsFulfilled = new Modifier(x => (x as PopUnit).getEveryDayNeedsFullfilling().get() > 0.99f, "Everyday needs are satisfied", 0.15f, false);
            modifierLuxuryNeedsFulfilled = new Modifier(x => (x as PopUnit).getLuxuryNeedsFullfilling().get() > 0.99f, "Luxury needs are satisfied", 0.2f, false);

            //Game.threadDangerSB.Clear();
            //Game.threadDangerSB.Append("Likes that government because can vote with ").Append(this.province.getOwner().government.ToString());
            modifierCanVote = new Modifier(x => (x as PopUnit).canVote(), "Can vote with that government ", 0.1f, false);
            //Game.threadDangerSB.Clear();
            //Game.threadDangerSB.Append("Dislikes that government because can't vote with ").Append(this.province.getOwner().government.ToString());
            modifierCanNotVote = new Modifier(x => !(x as PopUnit).canVote(), "Can't vote with that government ", -0.1f, false);
            //Game.threadDangerSB.Clear();
            //Game.threadDangerSB.Append("Upset by forced reform - ").Append(daysUpsetByForcedReform).Append(" days");
            modifierUpsetByForcedReform = new Modifier(x => (x as PopUnit).daysUpsetByForcedReform > 0, "Upset by forced reform", -3.0f, false);
            modifierNotGivenUnemploymentSubsidies = new Modifier(x => (x as PopUnit).didntGetPromisedUnemloymentSubsidy, "Didn't got promised Unemployment Subsidies", -1.0f, false);
            modifierMinorityPolicy = //new Modifier(MinorityPolicy.IsResidencyPop, 0.02f);
            new Modifier(x => !(x as PopUnit).isStateCulture()
            && ((x as PopUnit).GetCountry().minorityPolicy.getValue() == MinorityPolicy.Residency
            || (x as PopUnit).GetCountry().minorityPolicy.getValue() == MinorityPolicy.NoRights), "Is minority", -0.05f, false);


            //MinorityPolicy.IsResidency
            modifiersLoyaltyChange = new ModifiersList(new List<Condition>
        {
           modifierStarvation, modifierLifeNeedsNotFulfilled, modifierLifeNeedsFulfilled, modifierEverydayNeedsFulfilled,
        modifierLuxuryNeedsFulfilled, modifierCanVote, modifierCanNotVote, modifierUpsetByForcedReform, modifierNotGivenUnemploymentSubsidies,
            modifierMinorityPolicy,
            new Modifier(x => (x as PopUnit).didntGetPromisedSalary, "Didn't got promised salary", -1.0f, false),
            new Modifier (x => !(x as PopUnit).isStateCulture() &&
            (x as PopUnit).getProvince().hasModifier(Mod.recentlyConquered), Mod.recentlyConquered.ToString(), -1f, false),
            modCountryIsToBig
});

            // can increase performance by making separate modifiers for different popTypes
            modEfficiency = new ModifiersList(new List<Condition> {
            Modifier.modifierDefault1,
            new Modifier(x=>(x as PopUnit).getProvince().getOverpopulationAdjusted(x as PopUnit), "Overpopulation", -1f, false),
            new Modifier(Invention.SteamPowerInvented, x=>(x as PopUnit).GetCountry(), 0.25f, false),
            new Modifier(Invention.CombustionEngineInvented, x=>(x as PopUnit).GetCountry(), 0.25f, false),

            new Modifier(Economy.isStateCapitlism, x=>(x as PopUnit).GetCountry(),  0.10f, false),
            new Modifier(Economy.isInterventionism, x=>(x as PopUnit).GetCountry(),  0.30f, false),
            new Modifier(Economy.isLF, x=>(x as PopUnit).GetCountry(),  0.50f, false),
            new Modifier(Economy.isPlanned, x=>(x as PopUnit).GetCountry(),  -0.10f, false),

            //new Modifier(Serfdom.Allowed,  -20f, false)

            // copied in Factory
             new Modifier(x => Government.isPolis.checkIfTrue((x as PopUnit).GetCountry())
             && (x as PopUnit).getProvince().isCapital(), "Capital of Polis", 1f, false),
             new Modifier(x=>(x as PopUnit).getProvince().hasModifier(Mod.recentlyConquered), Mod.recentlyConquered.ToString(), -0.20f, false),
             new Modifier(x=>(x as PopUnit).GetCountry().government.getValue() == Government.Tribal
             && (x as PopUnit).popType!=PopType.Tribesmen, "Government is Tribal", -0.5f, false),
             new Modifier(Government.isDespotism, x=>(x as PopUnit).GetCountry(), -0.30f, false) // remove this?
        });
        }



        /// <summary>
        ///  Constructor for population created on game startup
        /// </summary>    
        protected PopUnit(int amount, PopType popType, Culture culture, Province where) : base(where)
        {
            where.allPopUnits.Add(this);
            born = new MyDate(Game.date);
            population = amount;
            this.popType = popType;
            this.culture = culture;

            education = new Procent(0.00f);
            loyalty = new Procent(0.50f);
            needsFulfilled = new Procent(0.50f);
            //province = where;
        }
        /// <summary> Creates new PopUnit basing on part of other PopUnit.
        /// And transfers sizeOfNewPop population.
        /// </summary>    
        protected PopUnit(PopUnit source, int sizeOfNewPop, PopType newPopType, Province where, Culture culture) : base(where)
        {
            born = new MyDate(Game.date);
            PopListToAddToGeneralList.Add(this);
            // makeModifiers();

            // here should be careful copying of popUnit data
            //And careful editing of old unit
            Procent newPopShare = Procent.makeProcent(sizeOfNewPop, source.getPopulation());

            //Own PopUnit fields:
            loyalty = new Procent(source.loyalty.get());
            population = sizeOfNewPop;
            // if source pop is gonna be dead..
            if (source.population - sizeOfNewPop <= 0 && this.popType == PopType.Aristocrats || this.popType == PopType.Capitalists)
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
                        isSourceInvestor.getOwnedFactories().PerformAction(x => x.ownership.TransferAll(isSourceInvestor, GetCountry()));
                }
            }
            mobilized = 0;
            popType = newPopType;
            this.culture = culture;
            education = new Procent(source.education.get());
            needsFulfilled = new Procent(source.needsFulfilled.get());
            daysUpsetByForcedReform = 0;
            didntGetPromisedUnemloymentSubsidy = false;
            //incomeTaxPayed = newPopShare.sendProcentToNew(source.incomeTaxPayed);

            //Agent's fields:
            //wallet = new Wallet(0f, where.getCountry().bank); it's already set in constructor
            //bank - could be different, set in constructor
            //loans - keep it in old unit        
            //take deposit share?
            if (source.deposits.isNotZero())
            {
                Value takeDeposit = source.deposits.Copy().multiply(newPopShare);
                if (source.getBank().canGiveMoney(this, takeDeposit))
                {
                    source.getBank().giveMoney(source, takeDeposit);
                    source.payWithoutRecord(this, takeDeposit);
                }
            }
            source.payWithoutRecord(this, source.cash.Copy().multiply(newPopShare));


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
            //    gainGoodsThisTurn = new Storage(source.gainGoodsThisTurn.getProduct());
            //    sentToMarket = new Storage(source.sentToMarket.getProduct());
            //}

            //province = where;//source.province;

            //Consumer's fields:
            // Do I really need it?
            getConsumed().setZero();// = new PrimitiveStorageSet();
            getConsumedLastTurn().setZero();// = new PrimitiveStorageSet();
            getConsumedInMarket().setZero();// = new PrimitiveStorageSet();

            //kill in the end
            source.subtractPopulation(sizeOfNewPop);
        }
        /// <summary>
        /// Merging source into this pop
        /// assuming that both pops are in same province, and has same type
        /// culture defaults to this.culture
        /// </summary>    
        internal void mergeIn(PopUnit source)
        {
            //carefully summing 2 pops..                

            //Own PopUnit fields:
            loyalty.addPoportionally(this.getPopulation(), source.getPopulation(), source.loyalty);
            addPopulation(source.getPopulation());

            mobilized += source.mobilized;
            //type = newPopType; don't change that
            //culture = source.culture; don't change that
            education.addPoportionally(this.getPopulation(), source.getPopulation(), source.education);
            needsFulfilled.addPoportionally(this.getPopulation(), source.getPopulation(), source.needsFulfilled);
            //daysUpsetByForcedReform = 0; don't change that
            //didntGetPromisedUnemloymentSubsidy = false; don't change that

            //Agent's fields:        
            source.PayAllAvailableMoneyWithoutRecord(this); // includes deposits
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
            getConsumed().Add(source.getConsumed());
            getConsumedLastTurn().Add(source.getConsumedLastTurn());
            getConsumedInMarket().Add(source.getConsumedInMarket());

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
                    isSourceInvestor.getOwnedFactories().PerformAction(x => x.ownership.TransferAll(isSourceInvestor, GetCountry()));
            }

            // basically, killing that unit. Currently that object is linked in PopUnit.PopListToAddInGeneralList only so don't worry
            source.deleteData();
        }
        /// <summary>
        /// Sets population to zero as a mark to delete this Pop
        /// </summary>
        virtual protected void deleteData()
        {
            population = 0;
            //province.allPopUnits.Remove(this); // gives exception        
            //Game.popsToShowInPopulationPanel.Remove(this);
            if (MainCamera.popUnitPanel.whomShowing() == this)
                MainCamera.popUnitPanel.Hide();
            //remove from population panel.. Would do it automatically        

            PayAllAvailableMoney(getBank()); // just in case if there is something
            getBank().defaultLoaner(this);
            Movement.leave(this);
        }
        //public Culture getCulture()
        //{
        //    return culture;
        //}
        // have to be this way!
        internal abstract int getVotingPower(Government.ReformValue reformValue);
        internal int getVotingPower()
        {
            return getVotingPower(GetCountry().government.getTypedValue());
        }


        override public void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            incomeTaxPayed.setZero(); // need it because pop could stop paying taxes due to reforms for example
            needsFulfilled.setZero();
            didntGetPromisedUnemloymentSubsidy = false;
            lastEscaped.value = 0;
            // if (popType != PopType.Aristocrats)
            //    storage.setZero();  // may mess with aristocrats
            // makes too mush tribes -> failes economy
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
        public int getPopulation()
        {
            return population;
        }
        internal int howMuchCanMobilize(Staff byWhom, Staff againstWho)
        {
            int howMuchCanMobilizeTotal = 0;
            if (byWhom == GetCountry())
            {
                if (this.getMovement() == null || (!this.getMovement().isInRevolt() && this.getMovement() != againstWho))
                //if (this.loyalty.isBiggerOrEqual(Options.PopMinLoyaltyToMobilizeForGovernment))
                {
                    if (popType == PopType.Soldiers)
                        howMuchCanMobilizeTotal = (int)(getPopulation() * 0.5);
                    else
                        howMuchCanMobilizeTotal = (int)(getPopulation() * loyalty.get() * Options.ArmyMobilizationFactor);
                }
            }
            else
            {
                if (byWhom == getMovement())
                    howMuchCanMobilizeTotal = (int)(getPopulation() * (Procent.HundredProcent.get() - loyalty.get()) * Options.ArmyMobilizationFactor);
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
                    return GetCountry();
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
        internal void takeLoss(int loss)
        {
            //int newPopulation = getPopulation() - (int)(loss * Options.PopAttritionFactor);

            this.subtractPopulation((int)(loss * Options.PopAttritionFactor));
            mobilized -= loss;
            if (mobilized < 0) mobilized = 0;
        }
        internal void addDaysUpsetByForcedReform(int popDaysUpsetByForcedReform)
        {
            daysUpsetByForcedReform += popDaysUpsetByForcedReform;
        }

        /// <summary>
        /// Creates Pop in PopListToAddToGeneralList, later in will go to proper List
        /// </summary>    
        public static PopUnit makeVirtualPop(PopType targetType, PopUnit source, int sizeOfNewPop, Province where, Culture culture)
        {
            if (targetType == PopType.Tribesmen) return new Tribesmen(source, sizeOfNewPop, where, culture);
            else
                if (targetType == PopType.Farmers) return new Farmers(source, sizeOfNewPop, where, culture);
            else
                if (targetType == PopType.Aristocrats) return new Aristocrats(source, sizeOfNewPop, where, culture);
            else
                if (targetType == PopType.Workers) return new Workers(source, sizeOfNewPop, where, culture);
            else
                if (targetType == PopType.Capitalists) return new Capitalists(source, sizeOfNewPop, where, culture);
            else
                if (targetType == PopType.Soldiers) return new Soldiers(source, sizeOfNewPop, where, culture);
            else
                if (targetType == PopType.Artisans) return new Artisans(source, sizeOfNewPop, where, culture);
            else
            {
                Debug.Log("Unknown pop type!");
                return null;
            }
        }

        internal bool getSayingYes(AbstractReformValue reform)
        {
            return reform.modVoting.getModifier(this) > Options.votingPassBillLimit;
        }
        public static int getRandomPopulationAmount(int minGeneratedPopulation, int maxGeneratedPopulation)
        {
            int randomPopulation = minGeneratedPopulation + Game.Random.Next(maxGeneratedPopulation - minGeneratedPopulation);
            return randomPopulation;
        }

        internal bool isAlive()
        {
            return getPopulation() > 0;
        }
        /// <summary>
        /// makes new list of new elements
        /// </summary>
        //private List<Storage> getNeedsInCommon(List<Storage> needs)
        //{
        //    Value multiplier = new Value(this.getPopulation() / 1000f);            
        //    foreach (Storage next in needs)
        //    {
        //        Storage nStor = new Storage(next.getProduct(), next.get());
        //        nStor.multiply(multiplier);                
        //    }
        //    return needs;
        //}

        public List<Storage> getRealLifeNeeds()
        {
            return popType.getLifeNeedsPer1000Men().Multiply(new Value(this.getPopulation() / 1000f));
        }

        public List<Storage> getRealEveryDayNeeds()
        {
            return popType.getEveryDayNeedsPer1000Men().Multiply(new Value(this.getPopulation() / 1000f));
        }

        public List<Storage> getRealLuxuryNeeds()
        {
            return popType.getLuxuryNeedsPer1000Men().Multiply(new Value(this.getPopulation() / 1000f));
        }
        override public List<Storage> getRealAllNeeds()
        {
            return popType.getAllNeedsPer1000Men().Multiply(new Value(this.getPopulation() / 1000f));
        }

        internal Procent getUnemployedProcent()
        {
            if (popType == PopType.Workers)
            {
                int employed = 0;
                foreach (Factory factory in getProvince().getAllFactories())
                    employed += factory.howManyEmployed(this);
                if (getPopulation() - employed <= 0) //happening due population change by growth/demotion
                    return new Procent(0);
                return new Procent((getPopulation() - employed) / (float)getPopulation());
            }
            else if (popType == PopType.Farmers || popType == PopType.Tribesmen)
            {
                var overPopulation = getProvince().GetOverpopulation();
                if (overPopulation.isSmallerOrEqual(Procent.HundredProcent))
                    return new Procent(0f);
                else
                    return new Procent(1f - (1f / overPopulation.get()));
            }
            else return new Procent(0f);
        }


        //public void payTaxes() // should be abstract 
        //{
        //    if (Economy.isMarket.checkIftrue(GetCountry()) && popType != PopType.Tribesmen)
        //    {
        //        Value taxSize;
        //        if (this.popType.isPoorStrata())
        //        {
        //            taxSize = moneyIncomethisTurn.Copy().multiply((GetCountry().taxationForPoor.getValue() as TaxationForPoor.ReformValue).tax);
        //            if (canPay(taxSize))
        //            {
        //                incomeTaxPayed = taxSize;
        //                GetCountry().poorTaxIncomeAdd(taxSize);
        //                pay(GetCountry(), taxSize);
        //            }
        //            else
        //            {
        //                incomeTaxPayed.set(cash);
        //                GetCountry().poorTaxIncomeAdd(cash);
        //                sendAllAvailableMoney(GetCountry());

        //            }
        //        }
        //        else
        //        if (this.popType.isRichStrata())
        //        {
        //            taxSize = moneyIncomethisTurn.Copy().multiply((GetCountry().taxationForRich.getValue() as TaxationForRich.ReformValue).tax);
        //            if (canPay(taxSize))
        //            {
        //                incomeTaxPayed.set(taxSize);
        //                GetCountry().richTaxIncomeAdd(taxSize);
        //                pay(GetCountry(), taxSize);
        //            }
        //            else
        //            {
        //                incomeTaxPayed.set(cash);
        //                GetCountry().richTaxIncomeAdd(cash);
        //                sendAllAvailableMoney(GetCountry());
        //            }
        //        }

        //    }
        //    else// non market
        //        //if (this.popType != PopType.Aristocrats)
        //    {
        //        Storage howMuchSend;
        //        if (this.popType.isPoorStrata())
        //            howMuchSend = getGainGoodsThisTurn().multiplyOutside((GetCountry().taxationForPoor.getValue() as TaxationForPoor.ReformValue).tax);
        //        else
        //        {
        //            //if (this.popType.isRichStrata())
        //            howMuchSend = getGainGoodsThisTurn().multiplyOutside((GetCountry().taxationForRich.getValue() as TaxationForRich.ReformValue).tax);
        //        }
        //        if (storage.isBiggerOrEqual(howMuchSend))
        //            storage.send(GetCountry().countryStorageSet, howMuchSend);
        //        else
        //            storage.sendAll(GetCountry().countryStorageSet);
        //    }
        //}

        internal bool isStateCulture()
        {
            return this.culture == this.GetCountry().getCulture();
        }

        //virtual internal bool CanGainDividents()
        //{
        //    return false;
        //}

        public Procent getLifeNeedsFullfilling()
        {
            float need = needsFulfilled.get();
            if (need < Options.PopOneThird)
                return new Procent(needsFulfilled.get() * Options.PopStrataWeight.get());
            //return needsFullfilled;
            else
                return Procent.HundredProcent;
        }
        public Procent getEveryDayNeedsFullfilling()
        {
            float need = needsFulfilled.get();
            if (need <= Options.PopOneThird)
                return Procent.ZeroProcent;
            if (need < Options.PopTwoThird)
                return new Procent((needsFulfilled.get() - Options.PopOneThird) * Options.PopStrataWeight.get());
            //return needsFullfilled;
            else
                return Procent.HundredProcent;
        }

        public Procent getLuxuryNeedsFullfilling()
        {
            float need = needsFulfilled.get();
            if (need <= Options.PopTwoThird)
                return Procent.ZeroProcent;
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
        private void consumeEveryDayAndLuxury(List<Storage> needs, byte howDeep)
        {
            howDeep--;
            //List<Storage> needs = getEveryDayNeeds();
            foreach (Storage need in needs)
                if (storage.has(need))
                {
                    //storage.subtract(need);
                    //consumedTotal.add(need);
                    consumeFromItself(need); // todo fails if is abstract
                    needsFulfilled.set(2f / 3f);
                    if (howDeep != 0) consumeEveryDayAndLuxury(getRealLuxuryNeeds(), howDeep);
                }
                else
                {
                    float canConsume = storage.get();
                    //consumedTotal.add(storage);
                    //storage.set(0);
                    consumeFromItself(storage); // todo fails if is abstract
                    needsFulfilled.add(canConsume / need.get() / 3f);
                }
        }

        private void consumeNeedsWithMarket(List<Storage> lifeNeeds, bool skipLifeneeds)
        {
            //buy life needs
            Value moneyWasBeforeLifeNeedsConsumption = getMoneyAvailable();
            //if (!skipLifeneeds)
            foreach (Storage need in lifeNeeds)
            {
                if (storage.has(need))// don't need to buy on market
                {
                    //FixedJoint it 4 times
                    Storage realConsumption;
                    if (need.isAbstractProduct())
                        realConsumption = new Storage(storage.getProduct(), need);
                    else
                        realConsumption = need;
                    //storage.subtract(need); // danger moment - may subtracts different type of product
                    //consumedTotal.set(storage.getProduct(), need);
                    consumeFromItself(realConsumption);
                    needsFulfilled.set(Options.PopOneThird);
                }
                else
                    needsFulfilled.set(Game.market.buy(this, need, null).get() / need.get() / Options.PopStrataWeight.get());
                //needsFullfilled.set(Procent.makeProcent(getConsumed().getContainer(), getRealLifeNeeds()));
                //{
                //    needsFullfilled.set(Game.market.buy(this, need, null), need);
                //    needsFullfilled.divide(Options.PopStrataWeight);
                //}
            }

            // buy everyday needs
            if (getLifeNeedsFullfilling().isBiggerOrEqual(Procent.HundredProcent))
            {
                // save some money in reserve to avoid spending all money on luxury 
                //Agent reserve = new Agent(0f, null, null); // temporally removed for testing
                // payWithoutRecord(reserve, cash.multipleOutside(Options.savePopMoneyReserv));            

                Value moneyWasBeforeEveryDayNeedsConsumption = getMoneyAvailable();
                var everyDayNeedsConsumed = new List<Storage>();
                foreach (Storage need in getRealEveryDayNeeds())
                {
                    //NeedsFullfilled.set(0.33f + Game.market.Consume(this, need).get() / 3f);
                    var consumed = Game.market.buy(this, need, null);
                    if (consumed.isNotZero())
                        everyDayNeedsConsumed.Add(consumed);
                }
                //Value spentMoneyOnEveryDayNeeds = moneyWasBeforeEveryDayNeedsConsumption.subtractOutside(getMoneyAvailable(), false);// moneyWas - cash.get() could be < 0 due to taking money from deposits
                //if (spentMoneyOnEveryDayNeeds.isNotZero())
                //    needsFullfilled.add(spentMoneyOnEveryDayNeeds.get() / Game.market.getCost(everyDayNeeds).get() / 3f);
                var everyDayNeedsFulfilled = Procent.makeProcent(everyDayNeedsConsumed, getRealEveryDayNeeds());
                everyDayNeedsFulfilled.divide(Options.PopStrataWeight);
                needsFulfilled.add(everyDayNeedsFulfilled);

                // buy luxury needs
                if (getEveryDayNeedsFullfilling().isBiggerOrEqual(Procent.HundredProcent))
                {
                    var luxuryNeeds = getRealLuxuryNeeds();

                    Value moneyWasBeforeLuxuryNeedsConsumption = getMoneyAvailable();
                    bool someLuxuryProductUnavailable = false;
                    var luxuryNeedsConsumed = new List<Storage>();
                    foreach (Storage nextNeed in luxuryNeeds)
                    {
                        var consumed = Game.market.buy(this, nextNeed, null);
                        if (consumed.isZero())
                            someLuxuryProductUnavailable = true;
                        else
                            luxuryNeedsConsumed.Add(consumed);
                    }
                    Value luxuryNeedsCost = Game.market.getCost(luxuryNeeds);

                    // unlimited consumption
                    // unlimited luxury spending should be limited by money income and already spent money
                    // I also can limit regular luxury consumption but should I?:
                    if (!someLuxuryProductUnavailable
                        && cash.isBiggerThan(Options.PopUnlimitedConsumptionLimit))  // need that to avoid poor pops
                    {
                        Value spentOnUnlimitedConsumption = cash.Copy();
                        Value spentMoneyOnAllNeeds = moneyWasBeforeLifeNeedsConsumption.Copy().subtract(getMoneyAvailable(), false);// moneyWas - cash.get() could be < 0 due to taking money from deposits
                        Value spendingLimit = getSpendingLimit(spentMoneyOnAllNeeds);// reduce limit by income - already spent money

                        if (spentOnUnlimitedConsumption.isBiggerThan(spendingLimit))
                            spentOnUnlimitedConsumption.set(spendingLimit); // don't spent more than gained                    

                        if (spentOnUnlimitedConsumption.get() > 5f)// to avoid zero values
                        {
                            // how much pop wants to spent on unlimited consumption. Pop should spent cash only..
                            Value buyExtraGoodsMultiplier = spentOnUnlimitedConsumption.Copy().divide(luxuryNeedsCost);
                            foreach (Storage nextNeed in luxuryNeeds)
                            {
                                nextNeed.multiply(buyExtraGoodsMultiplier);
                                var consumed = Game.market.buy(this, nextNeed, null);
                                if (consumed.isNotZero())
                                    luxuryNeedsConsumed.Add(consumed);
                            }
                        }
                    }
                    var luxuryNeedsFulfilled = Procent.makeProcent(luxuryNeedsConsumed, getRealLuxuryNeeds());
                    luxuryNeedsFulfilled.divide(Options.PopStrataWeight);
                    needsFulfilled.add(luxuryNeedsFulfilled);
                    //Value spentMoneyOnLuxuryNeedsTotal = moneyWasBeforeLuxuryNeedsConsumption.subtractOutside(getMoneyAvailable(), false);// moneyWas - cash.get() could be < 0 due to taking money from deposits
                    //if (spentMoneyOnLuxuryNeedsTotal.isNotZero())
                    //    needsFullfilled.add(spentMoneyOnLuxuryNeedsTotal.get() / luxuryNeedsCost.get() / 3f);
                }
                // reserve.payWithoutRecord(this, reserve.cash);
            }

        }
        protected void consumeWithNaturalEconomy(List<Storage> lifeNeeds)
        {
            GetCountry().TakeIncomeTax(this, null, popType.isPoorStrata()); //payTaxes(); // that is here because pop should pay taxes from all income
            foreach (Storage need in lifeNeeds)
                if (storage.has(need))// don't need to buy on market
                {
                    Storage realConsumption;
                    if (need.isAbstractProduct())
                        realConsumption = new Storage(storage.getProduct(), need);
                    else
                        realConsumption = need;

                    consumeFromItself(realConsumption);
                    needsFulfilled.set(1f / 3f);
                    consumeEveryDayAndLuxury(getRealEveryDayNeeds(), 2);
                }
                else
                {
                    //its about lifeneeds only
                    float canConsume = storage.get();
                    consumeFromItself(storage); // todo fails if is abstract
                    needsFulfilled.set(canConsume / need.get() / 3f);
                }
        }
        /// <summary>
        /// Returns list of actually consumed
        /// </summary>    
        private List<Storage> consumeWithPlannedEconomy(List<Storage> needs)
        {
            var consumed = new List<Storage>();
            foreach (var item in needs)
            {
                if (GetCountry().countryStorageSet.hasMoreThan(item, Options.CountryPopConsumptionLimitPE))
                    if (item.isAbstractProduct())
                    {
                        var stor = GetCountry().countryStorageSet.convertToBiggestStorage(item);
                        consumeFromCountryStorage(stor, GetCountry());
                        consumed.Add(stor);
                        //getCountry().countryStorageSet.subtract(getCountry().countryStorageSet.convertToBiggestStorageProduct(item));            
                    }

                    else
                    {
                        consumeFromCountryStorage(item, GetCountry());
                        consumed.Add(item);
                        //getCountry().countryStorageSet.subtract(item);
                    }
            }
            return consumed;
        }
        /// <summary> !!! Overloaded for artisans and tribesmen </summary>
        public override void consumeNeeds()
        {
            //life needs First
            List<Storage> lifeNeeds = getRealLifeNeeds();
            if (canTrade())
            {
                consumeNeedsWithMarket(lifeNeeds, false);
            }
            else if (GetCountry().economy.getValue() == Economy.PlannedEconomy)//non - market consumption
            {
                // todo - !! - check for substitutes
                consumeWithPlannedEconomy(getRealLifeNeeds());
                //needsFullfilled.set(Procent.makeProcent(getConsumed().getContainer(), getRealLifeNeeds()));
                var lifeNeedsFulfilled = Procent.makeProcent(getConsumed(), getRealLifeNeeds());
                lifeNeedsFulfilled.divide(Options.PopStrataWeight);
                needsFulfilled.set(lifeNeedsFulfilled);

                var everyDayNeedsConsumed = consumeWithPlannedEconomy(getRealEveryDayNeeds());
                if (getLifeNeedsFullfilling().isBiggerOrEqual(Procent.HundredProcent))
                {
                    var everyDayNeedsFulfilled = Procent.makeProcent(everyDayNeedsConsumed, getRealEveryDayNeeds());
                    everyDayNeedsFulfilled.divide(Options.PopStrataWeight);
                    needsFulfilled.add(everyDayNeedsFulfilled);
                }

                var luxuryNeedsConsumed = consumeWithPlannedEconomy(getRealLuxuryNeeds());
                if (getEveryDayNeedsFullfilling().isBiggerOrEqual(Procent.HundredProcent))
                {
                    var luxuryNeedsFulfilled = Procent.makeProcent(luxuryNeedsConsumed, getRealLuxuryNeeds());
                    luxuryNeedsFulfilled.divide(Options.PopStrataWeight);
                    needsFulfilled.add(luxuryNeedsFulfilled);
                }
                //var consumed = Procent.makeProcent(getConsumed().getContainer(), getRealAllNeeds());
                //consumed.multiply(Options.PopStrataWeight);
                //needsFullfilled.set(consumed);
            }
            else
                consumeWithNaturalEconomy(lifeNeeds);
        }
        /// <summary>
        /// Overrode for some pop types
        /// </summary>      
        virtual internal bool canTrade()
        {
            if (Economy.isMarket.checkIfTrue(GetCountry()))
                return true;
            else
                return false;
        }
        virtual internal bool canSellProducts()
        {
            return false;
        }
        internal bool canVote()
        {
            return canVote(GetCountry().government.getTypedValue());
        }
        abstract internal bool canVote(Government.ReformValue reform);
        public Dictionary<AbstractReformValue, float> getIssues()
        {
            var result = new Dictionary<AbstractReformValue, float>();
            foreach (var reform in this.GetCountry().reforms)
                foreach (AbstractReformValue reformValue in reform)
                    if (reformValue.allowed.isAllTrue(GetCountry(), reformValue))
                    {
                        var howGood = reformValue.modVoting.getModifier(this);//.howIsItGoodForPop(this);
                                                                              //if (howGood.isExist())
                        if (howGood > 0f)
                            result.Add(reformValue, Value.Convert(howGood));
                    }
            var target = getPotentialSeparatismTarget();
            if (target != null)
            {
                var howGood = target.modVoting.getModifier(this);
                if (howGood > 0f)
                    result.Add(target, Value.Convert(howGood));
            }
            return result;
        }
        public KeyValuePair<AbstractReform, AbstractReformValue> getMostImportantIssue()
        {
            var list = new Dictionary<KeyValuePair<AbstractReform, AbstractReformValue>, float>();
            foreach (var reform in this.GetCountry().reforms)
                foreach (AbstractReformValue reformValue in reform)
                    if (reformValue.allowed.isAllTrue(GetCountry(), reformValue))
                    {
                        var howGood = reformValue.modVoting.getModifier(this);//.howIsItGoodForPop(this);
                                                                              //if (howGood.isExist())
                        if (howGood > 0f)
                            list.Add(new KeyValuePair<AbstractReform, AbstractReformValue>(reform, reformValue), howGood);
                    }
            var target = getPotentialSeparatismTarget();
            if (target != null)
            {
                var howGood = target.modVoting.getModifier(this);
                if (howGood > 0f)
                    list.Add(new KeyValuePair<AbstractReform, AbstractReformValue>(null, target), howGood);
            }
            return list.MaxByRandom(x => x.Value).Key;
        }
        private Separatism getPotentialSeparatismTarget()
        {
            foreach (var item in getProvince().getAllCores())
            {
                if (!item.isAlive() && item != GetCountry() && item.getCulture() == this.culture)//todo doesn't supports different countries for same culture
                {
                    return Separatism.find(item);
                }
            }
            return null;
        }



        public void calcLoyalty()
        {
            float newRes = loyalty.get() + modifiersLoyaltyChange.getModifier(this) / 100f;
            loyalty.set(Mathf.Clamp01(newRes));
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
            Value taxSize = getGainGoodsThisTurn().multiplyOutside(GetCountry().serfdom.status.getTax());
            getProvince().shareWithAllAristocrats(storage, taxSize);
        }
        abstract public bool shouldPayAristocratTax();

        public void calcPromotions()
        {
            int promotionSize = getPromotionSize();
            if (wantsToPromote() && promotionSize > 0 && this.getPopulation() >= promotionSize)
                promote(getRichestPromotionTarget(), promotionSize);
        }
        public int getPromotionSize()
        {
            int result = (int)(this.getPopulation() * Options.PopPromotionSpeed.get());
            if (result > 0)
                return result;
            else
            if (getProvince().hasAnotherPop(this.popType) && getAge() > Options.PopAgeLimitToWipeOut)
                return this.getPopulation();// wipe-out
            else
                return 0;
        }

        public bool wantsToPromote()
        {
            if (this.needsFulfilled.isBiggerThan(Options.PopNeedsPromotionLimit))
                return true;
            else
                return false;
        }

        public PopType getRichestPromotionTarget()
        {
            Dictionary<PopType, Value> list = new Dictionary<PopType, Value>();
            foreach (PopType nextType in PopType.getAllPopTypes())
                if (canThisPromoteInto(nextType))
                    list.Add(nextType, getProvince().getAverageNeedsFulfilling(nextType));
            var result = list.MaxBy(x => x.Value.get());
            if (result.Value != null && result.Value.get() > this.needsFulfilled.get())
                return result.Key;
            else
                return null;
        }
        abstract public bool canThisPromoteInto(PopType popType);

        private void promote(PopType targetType, int amount)
        {
            if (targetType != null)
            {
                PopUnit.makeVirtualPop(targetType, this, amount, this.getProvince(), this.culture);
            }
        }


        private void setPopulation(int newPopulation)
        {
            if (newPopulation > 0)
                population = newPopulation;
            else
                this.deleteData();
            //throw new NotImplementedException();
            //because pool aren't implemented yet
            //Pool.ReleaseObject(this);
        }
        private void subtractPopulation(int subtract)
        {
            setPopulation(getPopulation() - subtract);
            //population -= subtract; ;
        }
        private void addPopulation(int adding)
        {
            population += adding;
        }
        internal void takeUnemploymentSubsidies()
        {
            // no subsidies with PE
            // may replace by trigger
            if (GetCountry().economy.getValue() != Economy.PlannedEconomy)
            {
                var reform = GetCountry().unemploymentSubsidies.getValue();
                if (getUnemployedProcent().isNotZero() && reform != UnemploymentSubsidies.None)
                {
                    Value subsidy = getUnemployedProcent();
                    subsidy.multiply(getPopulation() / 1000f * (reform as UnemploymentSubsidies.ReformValue).getSubsidiesRate());
                    //float subsidy = population / 1000f * getUnemployedProcent().get() * (reform as UnemploymentSubsidies.LocalReformValue).getSubsidiesRate();
                    if (GetCountry().canPay(subsidy))
                    {
                        GetCountry().pay(this, subsidy);
                        GetCountry().unemploymentSubsidiesExpenseAdd(subsidy);
                    }
                    else
                        this.didntGetPromisedUnemloymentSubsidy = true;
                }
            }
        }
        public void calcGrowth()
        {
            addPopulation(getGrowthSize());
        }
        public int getGrowthSize()
        {
            int result = 0;
            if (this.needsFulfilled.get() >= 0.33f) // positive growth
                result = Mathf.RoundToInt(Options.PopGrowthSpeed.get() * getPopulation());
            else
                if (this.needsFulfilled.get() >= 0.20f) // zero growth
                result = 0;
            else if (popType != PopType.Farmers) //starvation  
            {
                result = Mathf.RoundToInt(Options.PopStarvationSpeed.get() * getPopulation() * -1);
                if (result * -1 >= getPopulation()) // total starvation
                    result = 0;
            }

            return result;
            //return (int)Mathf.RoundToInt(this.population * PopUnit.growthSpeed.get());
        }
        private IEscapeTarget findEscapeTarget(Predicate<IEscapeTarget> predicate)
        {
            var list = new List<KeyValuePair<IEscapeTarget, Value>>();
            list.AddIfNotNull(getRichestDemotionTarget(predicate));
            list.AddIfNotNull(getRichestMigrationTarget(predicate));
            list.AddIfNotNull(getRichestImmigrationTarget(predicate));
            return list.MaxBy(x => x.Value.get()).Key;
        }

        /// <summary>
        /// Splits pops. New pops changes life in richest way - by demotion, migration or immigration
        /// </summary>        
        public void EscapeForBetterLife(Predicate<IEscapeTarget> predicate)
        {
            int escapeSize = getEscapeSize();
            if (escapeSize > 0 && this.getPopulation() >= escapeSize)
            {
                var escapeTarget = findEscapeTarget(predicate);
                if (escapeTarget != null)
                {
                    var targetIsPopType = escapeTarget as PopType;
                    if (targetIsPopType != null)
                    {
                        // assuming its PopType
                        PopUnit.makeVirtualPop(targetIsPopType, this, escapeSize, this.getProvince(), this.culture);
                        lastEscaped.key = targetIsPopType;
                    }
                    else
                    {
                        // assuming its province
                        var targetIsProvince = escapeTarget as Province;
                        // its both migration and immigration
                        PopUnit.makeVirtualPop(popType, this, escapeSize, targetIsProvince, this.culture);
                        lastEscaped.key = targetIsProvince;
                    }
                    lastEscaped.value = escapeSize;
                }
            }
        }
        /// <summary>
        /// Returns amount of people who wants change their lives (by demotion\migration\immigration)
        /// </summary>    
        public int getEscapeSize()
        {
            int result = (int)(this.getPopulation() * Options.PopEscapingSpeed.get());
            if (result > 0)
                return result;
            else
            {
                if (getProvince().hasAnotherPop(this.popType) && getAge() > Options.PopAgeLimitToWipeOut)
                    return this.getPopulation();// wipe-out
                else
                    return 0;
            }
        }

        public List<PopType> getPossibeDemotionsList()
        {
            List<PopType> result = new List<PopType>();
            foreach (PopType nextType in PopType.getAllPopTypes())
                if (canThisDemoteInto(this.popType))
                    result.Add(nextType);
            return result;
        }
        abstract public bool canThisDemoteInto(PopType popType);

        //abstract public PopType getRichestDemotionTarget();
        /// <summary>
        /// return popType to demote
        /// </summary>   
        public KeyValuePair<IEscapeTarget, Value> getRichestDemotionTarget(Predicate<IEscapeTarget> predicate)
        {
            Dictionary<IEscapeTarget, Value> list = new Dictionary<IEscapeTarget, Value>();

            foreach (PopType type in PopType.getAllPopTypes())
                if (canThisDemoteInto(type) && predicate(type))
                    list.Add(type, getProvince().getAverageNeedsFulfilling(type));
            var result = list.MaxBy(x => x.Value.get());
            if (result.Value != null && result.Value.isBiggerThan(this.needsFulfilled, Options.PopNeedsEscapingBarrier))
                return result;
            else
                return default(KeyValuePair<IEscapeTarget, Value>);
        }
        /// <summary>
        /// return province to immigrate or null if there is no better place to live
        /// </summary>    
        public KeyValuePair<IEscapeTarget, Value> getRichestImmigrationTarget(Predicate<IEscapeTarget> predicate)
        {
            Dictionary<IEscapeTarget, Value> provinces = new Dictionary<IEscapeTarget, Value>();
            //where to g0?
            // where life is rich and I where I have some rights
            foreach (var country in Country.getAllExisting())
                if (country.getCulture() == this.culture || country.minorityPolicy.getValue() == MinorityPolicy.Equality)
                    if (country != this.GetCountry())
                        foreach (var province in country.ownedProvinces)
                            if (predicate(province))
                            {
                                var needsInTargetProvince = province.getAverageNeedsFulfilling(this.popType);
                                if (needsInTargetProvince.isBiggerThan(this.needsFulfilled, Options.PopNeedsEscapingBarrier))
                                    provinces.Add(province, needsInTargetProvince);
                            }
            return provinces.MaxBy(x => x.Value.get());
        }
        /// <summary>
        /// return province to migrate or null if there is no better place to live
        /// </summary>  
        public KeyValuePair<IEscapeTarget, Value> getRichestMigrationTarget(Predicate<IEscapeTarget> predicate)
        {
            Dictionary<IEscapeTarget, Value> provinces = new Dictionary<IEscapeTarget, Value>();
            //foreach (var pro in getCountry().ownedProvinces)            
            //if (pro != this.province)

            foreach (var province in getProvince().getNeigbors(x => x.GetCountry() == this.GetCountry() && predicate(x)))
            {
                var needsInProvince = province.getAverageNeedsFulfilling(this.popType);
                if (needsInProvince.isBiggerThan(needsFulfilled, Options.PopNeedsEscapingBarrier))
                    provinces.Add(province, needsInProvince);
            }
            return provinces.MaxBy(x => x.Value.get());
        }

        //**********************************************
        internal void calcAssimilations()
        {

            if (!this.isStateCulture())
            {
                int assimilationSize = getAssimilationSize();
                if (assimilationSize > 0 && this.getPopulation() >= assimilationSize)
                    assimilate(GetCountry().getCulture(), assimilationSize);
            }
        }
        private void assimilate(Culture toWhom, int assimilationSize)
        {
            //if (toWhom != null)
            //{
            PopUnit.makeVirtualPop(popType, this, assimilationSize, this.getProvince(), toWhom);
            //}
        }
        public int getAssimilationSize()
        {
            if (getProvince().isCoreFor(this))
                return 0;
            else
            {
                int assimilationSpeed;
                if (GetCountry().minorityPolicy.getValue() == MinorityPolicy.Equality)
                    assimilationSpeed = (int)(this.getPopulation() * Options.PopAssimilationSpeedWithEquality.get());
                else
                    assimilationSpeed = (int)(this.getPopulation() * Options.PopAssimilationSpeed.get());
                if (assimilationSpeed > 0)
                    return assimilationSpeed;
                else
                {
                    if (getAge() > Options.PopAgeLimitToWipeOut)
                        return this.getPopulation(); // wipe-out
                    else
                        return 0;
                }
            }
        }
        virtual internal void invest()
        {
            if (GetCountry().isInvented(Invention.Banking))
            {
                Value extraMoney = new Value(cash.get() - Game.market.getCost(this.getRealAllNeeds()).get() * Options.PopDaysReservesBeforePuttingMoneyInBak, false);
                if (extraMoney.isNotZero())
                    getBank().takeMoney(this, extraMoney);
            }
        }
        /// <summary>
        /// Returns last escape type - demotion, migration or immigration
        /// </summary>
        public IEscapeTarget getLastEscapeTarget()
        {
            return lastEscaped.key;
        }
        /// <summary>
        /// Returns last escape size (how much people)
        /// </summary>
        public int getLastEscapeSize()
        {
            return lastEscaped.value;
        }
        override public string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(culture).Append(" ").Append(popType).Append(" from ").Append(getProvince().GetDescription());
            //return popType + " from " + province;
            return sb.ToString();
        }
        public void OnClicked()
        {
            MainCamera.popUnitPanel.show(this);
        }
    }
    public class KeyVal<Key, Val>
    {
        public Key key { get; set; }
        public Val value { get; set; }

        public KeyVal() { }

        public KeyVal(Key key, Val val)
        {
            this.key = key;
            this.value = val;
        }
    }
}