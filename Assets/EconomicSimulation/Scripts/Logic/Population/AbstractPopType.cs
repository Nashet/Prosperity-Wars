namespace Nashet.EconomicSimulation
{
    public abstract class GrainGetter : PopUnit
    {
        protected GrainGetter(PopUnit source, int sizeOfNewPop, PopType newPopType, Province where, Culture culture, IWayOfLifeChange oldLife) : base(source, sizeOfNewPop, newPopType, where, culture, oldLife)
        {
            changeProductionType(Product.Grain);
            //sentToMarket = new Storage(Product.Grain);
        }

        protected GrainGetter(int amount, PopType popType, Culture culture, Province where) : base(amount, popType, culture, where)
        {
            //storage = new Storage(Product.Grain);
            //gainGoodsThisTurn = new Storage(Product.Grain);
            changeProductionType(Product.Grain);
            //sentToMarket = new Storage(Product.Grain);
        }
    }

    public abstract class CattleGetter : PopUnit
    {
        protected CattleGetter(PopUnit source, int sizeOfNewPop, PopType newPopType, Province where, Culture culture, IWayOfLifeChange oldLife) : base(source, sizeOfNewPop, newPopType, where, culture, oldLife)
        {
            //storage = new Storage(Product.Cattle);
            //gainGoodsThisTurn = new Storage(Product.Cattle);
            //sentToMarket = new Storage(Product.Cattle);
            changeProductionType(Product.Cattle);
        }

        protected CattleGetter(int amount, PopType popType, Culture culture, Province where) : base(amount, popType, culture, where)
        {
            //storage = new Storage(Product.Cattle);
            //gainGoodsThisTurn = new Storage(Product.Cattle);
            //sentToMarket = new Storage(Product.Cattle);
            changeProductionType(Product.Cattle);
        }
    }
}