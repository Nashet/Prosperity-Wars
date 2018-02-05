using UnityEngine;
using UnityEditor;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class NewFactoryProject :IInvestable
    {
        public readonly FactoryType Type;
        public readonly Province Province;
        public NewFactoryProject(Province province, FactoryType type)
        {
            this.Type = type;
            this.Province = province;
        }
        public bool CanProduce(Product product)
        {
            return Type.CanProduce(product);
        }

        public Country GetCountry()
        {
            return Province.GetCountry();
        }

        public Value GetInvestmentCost()
        {
            return Type.GetBuildCost();
        }

        public Procent GetMargin()
        {
            return Type.GetPossibleMargin(Province);
        }

        public Province GetProvince()
        {
            return Province;
        }
    }
}