using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;

public interface IShareOwner { }
namespace Nashet.EconomicSimulation
{
    public class Ownership
    {
        private readonly Dictionary<IShareOwner, int> ownership = new Dictionary<IShareOwner, int>();
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
                ownership.AddMy(owner, value);
        }
        private void Remove(IShareOwner fromWho, int howMuch)
        {
            int found;
            if (ownership.TryGetValue(fromWho, out found))
            {
                if (found > howMuch)
                    found -= howMuch;
                else
                    ownership.Remove(fromWho);
            }
            Debug.Log("No such owner");
        }
        public bool Transfer(IShareOwner oldOwner, IShareOwner newOwner, int valueToTransfer)
        {
            if (IsCorrectData(valueToTransfer))
            {
                int found;
                if (ownership.TryGetValue(oldOwner, out found))
                {
                    if (found >= valueToTransfer)
                    {
                        //ownership.AddMy(newOwner, valueToTransfer);
                        found += valueToTransfer;
                        Remove(oldOwner, valueToTransfer);
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
            int found;
            if (ownership.TryGetValue(oldOwner, out found))
            {
                ownership.AddMy(newOwner, found);
                ownership.Remove(oldOwner);
            }
            else
                if (showMessageAboutOperationFails) Debug.Log("No such owner");
        }
        internal void Nationilize(Country byWhom)
        {
            foreach (var owner in GetAll())
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
        public IEnumerable<KeyValuePair<IShareOwner, int>> GetAll()
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
                yield return new KeyValuePair<IShareOwner, Procent>(item.Key, Procent.makeProcent(item.Value, total));
            }
        }
        private int GetAllOwnership()
        {
            int res = 0;
            foreach (var item in ownership)
            {
                res += item.Value;
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
                    ownedByAnyCountry += item.Value;
                total += item.Value;
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
    }
}