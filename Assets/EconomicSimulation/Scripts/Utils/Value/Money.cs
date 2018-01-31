using UnityEngine;
using UnityEditor;
using Nashet.EconomicSimulation;

namespace Nashet.ValueSpace
{
    public class Money : Storage
    {
        public Money(float value) : base(Product.Gold, value)
        { }
    }
}