using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Linq;

namespace Nashet.EconomicSimulation
{

    /// <summary>
    /// 
    /// </summary>
    //public struct StockShare
    //{
    //    private int amount;
    //    private int wantToSell;
    //}
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
    public class Record
    {
        int howMuchOwns = 1;//default value
        int howMuchWantsToSellOrBuy;
        public int Has
        {
            get { return howMuchOwns; }
            set { howMuchOwns = value; }
        }
        public int ToSell
        {
            get
            {
                if (howMuchWantsToSellOrBuy < 0)
                    return howMuchWantsToSellOrBuy * -1;
                else
                    return 0;
            }
        }
        public int ToBuy
        {
            get
            {
                if (howMuchWantsToSellOrBuy > 0)
                    return howMuchWantsToSellOrBuy;
                else
                    return 0;
            }
        }
        public void SetToSell(int add)
        {
            if (howMuchOwns + howMuchWantsToSellOrBuy - add < 0)
                Debug.Log("I don't have that much");
            else
                howMuchWantsToSellOrBuy -= add;
        }
        public void SetToBuy(int add)
        {
            howMuchWantsToSellOrBuy += add;
        }
    }

    public class Owners : IOwnerShip<IShareOwner, Record>, IInvestable
    {
        private readonly IShareable parent;
        // getPrice Modifier
        private readonly Dictionary<IShareOwner, Record> ownership = new Dictionary<IShareOwner, Record>();
        public Owners(IShareable parent)
        {
            this.parent = parent;
        }
        private bool IsCorrectData(int value)
        {
            if (value <= 0)
            {
                Debug.Log("Attempt to transfer negative amount");
                return false;
            }
            else
                return true;
        }
        public void Add(IShareOwner owner, int value)
        {
            if (IsCorrectData(value))
            {
                Record record;
                if (ownership.TryGetValue(owner, out record))
                    record.Has += value;
                else
                    ownership.Add(owner, new Record());
                //owner.GetOwnership().Add(parent, value);
            }
        }
        private void Remove(IShareOwner fromWho, int howMuchRemove)
        {
            Record found;
            if (ownership.TryGetValue(fromWho, out found))
            {
                if (found.Has > howMuchRemove)
                    found.Has -= howMuchRemove;
                else if (found.Has == howMuchRemove)
                    ownership.Remove(fromWho);
                else
                    Debug.Log("Doesn't have that much");
            }
            Debug.Log("No such owner");
        }
        /// <summary>
        /// Test it!!
        /// </summary>        
        public bool Transfer(IShareOwner oldOwner, IShareOwner newOwner, int valueToTransfer)
        {
            if (IsCorrectData(valueToTransfer))
            {
                Record record;
                if (ownership.TryGetValue(oldOwner, out record))
                {
                    if (record.Has >= valueToTransfer)
                    {
                        Add(newOwner, valueToTransfer);
                        Remove(oldOwner, valueToTransfer);
                        //oldOwner.GetOwnership().Transfer(oldOwner.GetOwnership().parent, newOwner, valueToTransfer);
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
            }
            else return false;
        }
        internal void TransferAll(IShareOwner oldOwner, IShareOwner newOwner, bool showMessageAboutOperationFails = true)
        {
            Record record;
            if (ownership.TryGetValue(oldOwner, out record))
            {
                Add(newOwner, record.Has);
                ownership.Remove(oldOwner);
            }
            else
                if (showMessageAboutOperationFails) Debug.Log("No such owner");
        }
        internal void Nationilize(Country byWhom)
        {
            foreach (var owner in GetAll().ToList())
                if (owner.Key != byWhom)
                {
                    TransferAll(owner.Key, Game.Player);
                    var isPop = owner.Key as PopUnit;
                    if (isPop != null)
                        isPop.loyalty.subtract(Options.PopLoyaltyDropOnNationalization, false);
                    else
                    {
                        //var isCountry = owner.Key as Country;
                        //if (isCountry != null)
                        //todo drop relations
                    }
                }
        }
        public IEnumerable<KeyValuePair<IShareOwner, Record>> GetAll()
        {
            foreach (var item in ownership)
            {
                yield return item;
            }
        }
        public IEnumerable<KeyValuePair<IShareOwner, Procent>> GetAllWithProcents()
        {
            var total = GetAllOwnership();
            foreach (var item in ownership)
            {
                yield return new KeyValuePair<IShareOwner, Procent>(item.Key, Procent.makeProcent(item.Value.Has, total));
            }
        }
        private int GetAllOwnership()
        {
            int res = 0;
            foreach (var item in ownership)
            {
                res += item.Value.Has;
            }
            return res;
        }
        internal bool HasOwner(IShareOwner pop)
        {
            return ownership.ContainsKey(pop);
        }
        internal bool IsCountryOwnsControlPacket()
        {
            int ownedByAnyCountry = 0;
            int total = 0;
            foreach (var item in GetAll())
            {
                if (item.Key is Country)
                    ownedByAnyCountry += item.Value.Has;
                total += item.Value.Has;
            }
            if ((float)ownedByAnyCountry / total >= 0.5f)
                return true;
            else
                return false;
        }
        internal bool IsOnlyOwner(IShareOwner owner)
        {
            return ownership.ContainsKey(owner) && ownership.Count == 1;
        }

        internal bool IsOnSale()
        {
            return ownership.Any(x => x.Value.ToSell > 0);
        }
        internal int HowMuchSelling(IShareOwner owner)
        {
            Record record;
            if (ownership.TryGetValue(owner, out record))
                return record.ToSell;
            else
                return 0;
        }
        internal int HowMuchBuying(IShareOwner owner)
        {
            Record record;
            if (ownership.TryGetValue(owner, out record))
                return record.ToBuy;
            else
                return 0;
        }

        public void SetToSell(IShareOwner owner, int howMuchSell)
        {
            Record record;
            if (ownership.TryGetValue(owner, out record))
                record.SetToSell(howMuchSell);
            else
                Debug.Log("No such owner");
        }
        public void SetToBuy(IShareOwner owner, int howMuchBuy)
        {
            Record record;
            if (ownership.TryGetValue(owner, out record))
            {
                record.SetToBuy(howMuchBuy);
            }
            else Debug.Log("No such owner");
        }

        internal KeyValuePair<IShareOwner, Record> GetRandomSale()
        {
            return ownership.Where(x => x.Value.ToSell > 0).Random();
        }
        internal Value GetPrice()
        { }
        internal void Buy(IShareOwner buyer, int howMuch)
        {
            var agent = buyer as Agent;
            var purhase = GetRandomSale();
            var cost = GetPrice();
            cost.multiply(howMuch);
            if (agent.pay(purhase.Key as Agent, cost))
            {
                Transfer(purhase.Key, buyer, howMuch);
                Debug.Log(buyer + " bough " + howMuch + " of " + parent);
            }
            
        }
    }

}