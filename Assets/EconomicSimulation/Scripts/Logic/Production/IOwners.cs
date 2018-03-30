using System.Collections.Generic;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    //public interface IOwnerShip<T>
    //{
    //    void Add(T owner, int value);
    //    void Remove(T owner);
    //    IEnumerable<KeyValuePair<T, int>> GetAll();
    //    IEnumerable<KeyValuePair<T, Procent>> GetAllWithProcents();
    //    bool Transfer(T oldOwner, T newOwner, int valueToTransfer);
    //}
    public interface IOwnerShip<T, V>
    {
        void Add(T owner, int value);

        //void Remove(T owner); You can't just remove property to no where
        IEnumerable<KeyValuePair<T, V>> GetAll();

        IEnumerable<KeyValuePair<T, Procent>> GetAllWithProcents();

        bool Transfer(T oldOwner, T newOwner, int valueToTransfer);

        void SetToSell(T owner, int howMuch);

        void SetToBuy(IShareOwner owner, int howMuchBuy);
    }
}