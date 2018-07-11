using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class NewFactoryProject : IInvestable
    {
        public readonly ProductionType Type;
        private readonly Province province;

        public NewFactoryProject(Province province, ProductionType type)
        {
            Type = type;
            this.province = province;
        }

        public Country Country
        {
            get { return province.Country; }
        }

        public Province Province
        {
            get { return province; }
        }

        public bool CanProduce(Product product)
        {
            return Type.CanProduce(product);
        }

        public MoneyView GetInvestmentCost(Market market)
        {
            return Type.GetBuildCost(market);
        }

        public Procent GetMargin()
        {
            return Type.GetPossibleMargin(Province);
        }
    }
}