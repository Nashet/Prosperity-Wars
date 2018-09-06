using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.EconomicSimulation;
using Nashet.ValueSpace;
using Nashet.Utils;
/// <summary>
/// Remember that nUnity create persistent instance of that class
/// Don't forget to clear it before new test start
/// </summary>
[TestFixture]

public class TradeTests
{
    MockWorld world = new MockWorld();
    [SetUp]
    public void CommonSetup()
    {
        Game.devMode = true;
        Game.logInvestments = true;
        Game.logMarket = true;
        world.Clear();
    }
    [Test]
    public void OneMarketTest()
    {
        var province = new Province("test", 1, Color.black, null);
        var country = new Country("test", null, Color.black, province, 10f);

        var world = new MockWorld();
        world.RegisterCountry(country);

        var market = country.market;
        market.Initialize(country);

        var factory = new Factory(province, null, ProductionType.Orchard, new MoneyView(20));

        factory.SendToMarket(new Storage(Product.Fruit, 10f));

        var buyer = new MockAristocrats(1000, null, province);

        buyer.Cash = new Money(1000000);

        buyer.Buy(new Storage(Product.Fruit, 10f), null);

        //force DSB recalculation
        World.AllMarkets.PerformAction(x => x.getDemandSupplyBalance(null, true));

        Market.GiveMoneyForSoldProduct(factory);

        World.AllExistingCountries().PerformAction(x => Debug.Log(x + "\n"));
        Assert.AreEqual(new MoneyView(10).Get(), factory.Cash.Get());

    }
    [Test]
    public void TwoMarketTest()
    {
        var firstProvince = new Province("first province", 1, Color.black, null);
        var firstCountry = new Country("first", null, Color.black, firstProvince, 10f);

        var secondProvince = new Province("second province", 1, Color.black, null);
        var secondCountry = new Country("second", null, Color.black, secondProvince, 10f);


        world.RegisterCountry(firstCountry);
        world.RegisterCountry(secondCountry);

        firstCountry.market.Initialize(firstCountry);
        secondCountry.market.Initialize(secondCountry);

        var factory = new Factory(firstProvince, null, ProductionType.Orchard, new MoneyView(20));
        //c.open(null, false);
        //c.produce();
        factory.SendToMarket(new Storage(Product.Fruit, 10f));

        var buyer = new MockAristocrats(1000, null, secondProvince);



        buyer.Cash = new Money(1000000);

        buyer.Buy(new Storage(Product.Fruit, 10f), null);

        //force DSB recalculation
        World.AllMarkets.PerformAction(x => x.getDemandSupplyBalance(null, true));

        Market.GiveMoneyForSoldProduct(factory);
        Debug.Log("factory has " + factory.Cash + " default fruit price is 1 ");
        World.AllExistingCountries().PerformAction(x => Debug.Log(x));
        Assert.AreEqual(new MoneyView(10).Get(), factory.Cash.Get());

    }
}
