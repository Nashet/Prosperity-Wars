using System;
using System.Collections.Generic;



public class Product : Name
{
    //private static HashSet<Product> allProducts = new HashSet<Product>();
    internal static readonly List<Product> allProducts = new List<Product>();
    internal static readonly Product Food, Wood, Lumber, Furniture, Gold, Metal, MetallOre,
    Wool, Clothes, Stone, Cement, Fruit, Wine, ColdArms, Ammunition, Firearms, Artillery,
    Oil, Fuel, Cars, Tanks, Airplanes, Rubber, Machinery;
    private static int resourceCounter;

    private readonly bool resource;
    private readonly Value defaultPrice;

    static Product()
    {
        Food = new Product("Food", false, 0.04f);
        Wood = new Product("Wood", true, 2.7f);
        Lumber = new Product("Lumber", false, 8f);
        Gold = new Product("Gold", true, 4f);
        MetallOre = new Product("Metal ore", true, 3f);
        Metal = new Product("Metal", false, 6f);
        Wool = new Product("Wool", true, 1f);
        Clothes = new Product("Clothes", false, 3f);
        Furniture = new Product("Furniture", false, 7f);
        Stone = new Product("Stone", true, 1f);
        Cement = new Product("Cement", false, 2f);
        Fruit = new Product("Fruit", true, 1f);
        Wine = new Product("Wine", false, 3f);
        ColdArms = new Product("Cold arms", false, 13f);
        Ammunition = new Product("Ammunition", false, 13f);
        Firearms = new Product("Firearms", false, 13f);
        Artillery = new Product("Artillery", false, 13f);

        Oil = new Product("Oil", true, 10f);
        Fuel = new Product("Fuel", false, 15f);
        Machinery = new Product("Machinery", false, 8f);
        Cars = new Product("Cars", false, 15f);
        Tanks = new Product("Tanks", false, 20f);
        Airplanes = new Product("Airplanes", false, 20f);
        Rubber = new Product("Rubber", true, 10f);
    }
    private Product(string name, bool inlanded, float defaultPrice) : base(name)
    {
        this.defaultPrice = new Value(defaultPrice);
        resource = inlanded;
        if (resource) resourceCounter++;
        //this.name = name;
        allProducts.Add(this);
        Game.market.SetDefaultPrice(this, defaultPrice);

        //TODO checks for duplicates&
    }
    internal bool isResource()
    {
        return resource;
    }
    internal static Product getRandomResource(bool ignoreGold)
    {
        if (ignoreGold)
            return Product.Wood;
        return Product.allProducts.PickRandom(x => x.isResource());

    }

    public bool isInventedByAnyOne()
    {
        foreach (var country in Country.allCountries)
            if (this.isInvented(country))
                return true;
        return false;
    }
    public bool isInvented(Country country)
    {
        if (
            (
            (this == Metal || this == MetallOre || this == ColdArms) && !country.isInvented(Invention.Metal))
            || (!country.isInvented(Invention.SteamPower) && this == Machinery)
            || ((this == Artillery || this == Ammunition) && !country.isInvented(Invention.Gunpowder))
            || (this == Firearms && !country.isInvented(Invention.Firearms))
            || (!country.isInvented(Invention.CombustionEngine) && (this == Oil || this == Fuel || this == Rubber || this == Cars))
            || (!country.isInvented(Invention.Tanks) && this == Tanks)
            || (!country.isInvented(Invention.Airplanes) && this == Airplanes)
            || (!isResource() && !country.isInvented(Invention.Manufactories))
            )
            return false;
        else
            return true;
    }
    //bool isStorable()
    //{
    //    return storable;
    //}
    //void setStorable(bool isStorable)
    //{
    //    storable = isStorable;
    //}   

    internal Value getDefaultPrice()
    {
        if (isResource())
        {
            return defaultPrice.multipleOutside(Options.defaultPriceLimitMultiplier);
        }
        else
        {
            var type = FactoryType.whoCanProduce(this);
            if (type == null)
                return defaultPrice.multipleOutside(Options.defaultPriceLimitMultiplier);
            else
            {
                Value res = Game.market.getCost(type.resourceInput);
                res.multiple(Options.defaultPriceLimitMultiplier);
                res.divide(type.basicProduction);
                return res;
            }
        }
    }

}
