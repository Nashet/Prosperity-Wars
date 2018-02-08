using UnityEngine;

using System.Collections.Generic;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Linq;

namespace Nashet.EconomicSimulation
{

    /// <summary>
    /// Represent entity which can be owned by several owners as joint stock company
    /// </summary>
    public interface IShareable { }
    /// <summary>
    /// Represents ability to own enterprise shares
    /// </summary>
    public interface IShareOwner
    {
        //Properties GetOwnership();
    }
    public class Owners : IInvestable
    // IOwnerShip<IShareOwner, Record>,
    {
        private readonly Factory parent;
        private readonly Procent marketPriceModifier = Procent.HundredProcent.Copy();
        private readonly Dictionary<IShareOwner, Share> ownership = new Dictionary<IShareOwner, Share>();
        private readonly Money totallyInvested = new Money(0f);
        public Owners(IShareable parent)
        {
            this.parent = parent as Factory;
        }

        public void Add(IShareOwner owner, Value value)
        {
            //if (IsCorrectData(value.))
            {
                Share record;
                if (ownership.TryGetValue(owner, out record))
                    record.Increase(value);
                else
                    ownership.Add(owner, new Share(value));
                totallyInvested.Add(value);
                //owner.GetOwnership().Add(parent, value);
            }
        }
        private void Remove(IShareOwner fromWho, Value howMuchRemove)
        {
            Share record;
            if (ownership.TryGetValue(fromWho, out record))
            {
                if (record.GetShare().isBiggerThan(howMuchRemove))
                    record.Decrease(howMuchRemove);
                else if (record.GetShare().IsEqual(howMuchRemove))
                    ownership.Remove(fromWho);
                else
                    Debug.Log("Doesn't have that much");
            }
            else
                Debug.Log("No such owner");
        }
        /// <summary>
        /// Test it!!
        /// </summary>        
        public bool Transfer(IShareOwner oldOwner, IShareOwner newOwner, Value amount)
        {
            //if (IsCorrectData(share.get()))
            //{
            Share oldOwnerAsset;
            if (ownership.TryGetValue(oldOwner, out oldOwnerAsset))
            {
                if (oldOwnerAsset.GetShare().isBiggerOrEqual(amount))
                {
                    Share newOwnerAsset;
                    if (ownership.TryGetValue(newOwner, out newOwnerAsset))
                        newOwnerAsset.Increase(amount);
                    else
                        ownership.Add(newOwner, new Share(amount));

                    Remove(oldOwner, amount);
                    return true;
                }
                else
                {
                    TransferAll(oldOwner, newOwner);
                    Debug.Log("Not enough property to transfer");
                    return false;
                }
            }
            else
            {
                Debug.Log("No such owner");
                return false;
            }
            //}
            //else return false;
        }
        internal void TransferAll(IShareOwner oldOwner, IShareOwner newOwner, bool showMessageAboutOperationFails = true)
        {
            Share oldOwnerAsset;
            if (ownership.TryGetValue(oldOwner, out oldOwnerAsset))
            {
                Share newOwnerAsset;
                if (ownership.TryGetValue(newOwner, out newOwnerAsset))
                    newOwnerAsset.Increase(oldOwnerAsset.GetShare());
                else
                    ownership.Add(newOwner, new Share(oldOwnerAsset.GetShare()));

                ownership.Remove(oldOwner);
            }
            else
                if (showMessageAboutOperationFails) Debug.Log("No such owner");
        }
        /// <summary>
        /// Don't call it directly, call it from Country
        /// </summary>        
        //internal void Nationilize(Country nationalizator)
        //{
        //    foreach (var owner in GetAll().ToList())
        //        if (owner.Key != nationalizator)
        //        {
        //            TransferAll(owner.Key, Game.Player);
        //            var popOwner = owner.Key as PopUnit;
        //            if (popOwner != null && popOwner.GetCountry() == nationalizator)
        //                popOwner.loyalty.subtract(Options.PopLoyaltyDropOnNationalization, false);
        //            else
        //            {
        //                var countryOwner = owner.Key as Country;
        //                if (countryOwner != null)
        //                    countryOwner.changeRelation(nationalizator, Options.PopLoyaltyDropOnNationalization.get());
        //            }
        //        }
        //}
        public IEnumerable<KeyValuePair<IShareOwner, Share>> GetAll()
        {
            foreach (var item in ownership)
            {
                yield return item;
            }
        }
        public IEnumerable<KeyValuePair<IShareOwner, Procent>> GetAllShares()
        {
            var total = GetAllAssetsValue();
            foreach (var item in ownership)
            {
                yield return new KeyValuePair<IShareOwner, Procent>(item.Key, new Procent(item.Value.GetShare(), total));
            }
        }

        internal bool HasOwner(IShareOwner owner)
        {
            return ownership.ContainsKey(owner);
        }
        internal bool Has(IShareOwner owner, Procent share)
        {
            Share found;
            if (ownership.TryGetValue(owner, out found))
            {
                if (found.GetShare().isBiggerOrEqual(GetShareAssetsValue(share)))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        internal bool IsCountryOwnsControlPacket()
        {
            Value ownedByAnyCountry = new Value(0f);
            Value total = new Value(0f);
            foreach (var item in GetAll())
            {
                var value = item.Value.GetShare();
                if (item.Key is Country)
                    ownedByAnyCountry.Add(value);
                total.Add(value);
            }

            var res = new Procent(ownedByAnyCountry, total, false); // to avoid console spam with ghost factories
            if (res.isBiggerOrEqual(Procent._50Procent))
                return true;
            else
                return false;
        }

        internal Procent GetTotalOnSale()
        {
            var onSale = new Value(0f);
            foreach (var item in ownership)
                onSale.Add(item.Value.GetShareForSale());
            return new Procent(onSale, totallyInvested);
        }

        internal bool IsOnlyOwner(IShareOwner owner)
        {
            return ownership.ContainsKey(owner) && ownership.Count == 1;
        }

        internal bool IsOnSale()
        {
            return ownership.Any(x => x.Value.GetShareForSale().isNotZero());
        }
        /// <summary>
        /// Readonly !!
        /// </summary>        
        internal Procent HowMuchSelling(IShareOwner owner)
        {
            Share record;
            if (ownership.TryGetValue(owner, out record))
                return new Procent(record.GetShareForSale(), GetAllAssetsValue());
            else
                return Procent.ZeroProcent.Copy();
        }
        /// <summary>
        /// Readonly !!
        /// </summary>        
        internal Procent HowMuchOwns(IShareOwner owner)
        {
            Share record;
            if (ownership.TryGetValue(owner, out record))
                return new Procent(record.GetShare(), GetAllAssetsValue());
            else
                return Procent.ZeroProcent.Copy();
        }

        public void SetToSell(IShareOwner owner, Procent share, bool showMessageAboutOperationFails = true)
        {

            Share record;
            if (ownership.TryGetValue(owner, out record))
            {
                var value = GetShareAssetsValue(share);
                record.SetToSell(value);
            }
            else if (showMessageAboutOperationFails)
                Debug.Log("No such owner");
        }
        public void CancelBuyOrder(IShareOwner owner, Procent share)
        {
            Share record;
            if (ownership.TryGetValue(owner, out record))
            {
                var value = GetShareAssetsValue(share);
                record.CancelBuyOrder(value);
            }
            else
                Debug.Log("No such owner");
        }

        //internal KeyValuePair<IShareOwner, Record> GetRandomSaleBiggerThan(Value desireableValue)
        //{
        //    return ownership.Where(x => x.Value.GetAssetForSale().isBiggerOrEqual(desireableValue)).Random();
        //}
        /// <summary>
        /// Returns copy
        /// </summary>

        internal Value GetAllAssetsValue()
        {
            return totallyInvested.Copy();
        }
        /// <summary>
        /// New value
        /// </summary>        
        internal Money GetMarketValue()
        {
            return totallyInvested.Copy().Multiply(marketPriceModifier);            
        }
        internal Value GetShareMarketValue(Procent share)
        {
            return GetMarketValue().Multiply(share);            
            //return share.SendProcentOf(GetMarketValue());
        }
        internal Value GetShareAssetsValue(Procent share)
        {
            return GetAllAssetsValue().multiply(share);
            //return share.SendProcentOf(GetAllAssetsValue());
        }
        internal void CalcMarketPrice()
        {
            var isOnsale = IsOnSale();
            if (isOnsale || parent.IsClosed)
            {
                // reduce price
                marketPriceModifier.subtract(0.001f, false);
                if (marketPriceModifier.isZero())
                    marketPriceModifier.set(0.001f);
            }
            if (!isOnsale && parent.IsOpen) //rise price
                marketPriceModifier.add(0.01f);
        }
        /// <summary>
        /// Buy that share (or less). Assumes that there is something on sale. Assumes that buyer has enough money
        /// </summary>        
        internal void BuyStandardShare(IShareOwner buyer)
        {
            // what if buys from itself?
            if (HowMuchSelling(buyer).isNotZero())
                CancelBuyOrder(buyer, Options.PopBuyAssetsAtTime);
            else
            {

                //var purchaseValue = GetShareMarketValue(Options.PopBuyAssetsAtTime);
                var purchaseValue = GetInvestmentCost();
                var sharesToBuy = ownership.Where(x => x.Value.GetShareForSale().IsEqual(purchaseValue));

                if (sharesToBuy.Count() == 0)
                {
                    //if no equal sharesToBuy find smaller one
                    sharesToBuy = ownership.Where(x => x.Value.GetShareForSale().isSmallerThan(purchaseValue)
                    && x.Value.GetShareForSale().isNotZero());
                }
                if (sharesToBuy.Count() == 0)
                {
                    //if no smaller sharesToBuy find bigger one
                    sharesToBuy = ownership.Where(x => x.Value.GetShareForSale().isBiggerThan(purchaseValue)
                    && x.Value.GetShareForSale().isNotZero());
                }
                if (sharesToBuy.Count() != 0)
                {
                    var shareToBuy = sharesToBuy.Random();
                    var cost = shareToBuy.Value.GetShareForSale();
                    if (cost.isBiggerThan(purchaseValue))
                        cost.set(purchaseValue);
                    var buyingAgent = buyer as Agent;

                    if (buyingAgent.pay(shareToBuy.Key as Agent, cost))
                    {
                        Transfer(shareToBuy.Key, buyer, cost);
                        //reduce onSale amount on successful deal

                        shareToBuy.Value.ReduceSale(cost);

                        var boughtProcent = new Procent(cost, parent.ownership.totallyInvested);
                        //Debug.Log(buyer + " bough " + boughtProcent + " shares (" + cost + ") of " + parent + " from " + shareToBuy.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Margin per market value. New value, includes tax.
        /// </summary>        
        public Procent GetMargin()
        {
            if (parent.IsClosed)
                return Procent.ZeroProcent.Copy();
            else
                return parent.GetMargin();
            //var payToGovernment = parent.GetCountry().taxationForRich.getTypedValue().tax.getProcentOf(GetDividends());
            //return new Procent(payedDividends.Copy().subtract(payToGovernment), ownership.GetMarketValue(), false);
            //return new Procent(parent.GetDividends(), GetMarketValue(), false); 
        }
        /// <summary>
        /// Cost of standard share
        /// </summary>        
        public Value GetInvestmentCost()
        {
            return GetMarketValue().Multiply(Options.PopBuyAssetsAtTime);            
        }
        public bool CanProduce(Product product)
        {
            return parent.getType().CanProduce(product);
        }
        /// <summary>
        /// Should be in Investor class
        /// </summary>
        public void SellLowMarginShares()
        {
            if (parent.GetMargin().isSmallerThan(Options.PopMarginToSellShares))
                foreach (var item in ownership)
                {
                    var country = item.Key as Country;
                    if (country != null)
                    {
                        if (country.isAI())
                            Rand.Call(() =>
                            {
                                SetToSell(item.Key, Options.PopBuyAssetsAtTime, false);
                                //Debug.Log(item.Key + " put on sale shares of " + parent);
                            }, 10);
                    }
                    else
                        //var agent = item.Key as Agent;
                        Rand.Call(() =>
                        {
                            SetToSell(item.Key, Options.PopBuyAssetsAtTime, false);
                            //Debug.Log(item.Key + " put on sale shares of " + parent);
                        }, 10);
                }
        }
        public Country GetCountry()
        {
            return parent.GetCountry();
        }
        public Province GetProvince()
        {
            return parent.GetProvince();
        }
    }

}