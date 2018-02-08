using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.ValueSpace;
namespace Nashet.EconomicSimulation
{
    abstract public class GrainGetter : PopUnit
    {
        protected GrainGetter(PopUnit source, int sizeOfNewPop, PopType newPopType, Province where, Culture culture) : base(source, sizeOfNewPop, newPopType, where, culture)
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
    abstract public class CattleGetter : PopUnit
    {
        protected CattleGetter(PopUnit source, int sizeOfNewPop, PopType newPopType, Province where, Culture culture) : base(source, sizeOfNewPop, newPopType, where, culture)
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