using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Product : Name
{
    //private static HashSet<Product> allProducts = new HashSet<Product>();
    internal static readonly List<Product> allProducts = new List<Product>();
    internal static readonly Product Food, Wood, Lumber, Furniture, Gold, Metal, MetallOre,
    Wool, Clothes, Stone, Cement, Fruit, Wine, ColdArms, Ammunition, Firearms, Artillery,
    Oil, Fuel, Cars, Tanks, Airplanes, Rubber, Machinery, Fish, Grain, Cattle;
    private static int resourceCounter;

    private readonly bool _isResource;
    private readonly Value defaultPrice;
    private readonly bool _isAbstract;
    private readonly List<Product> substitutes;
    public bool isAbstract()
    {
        return _isAbstract;
    }
    public static IEnumerable<Product> getAllAbstract()
    {
        foreach (var item in allProducts)
            if (item.isAbstract())
                yield return item;
    }
    //public ReadOnlyCollection<Product> getSubstitutes()
    //{
    //    //if (!isAbstract())
    //    //    return null;
    //    //else
    //        return substitutes;
    //}
    public IEnumerable<Product> getSubstitutes()
    {
        //if (!isAbstract())
        //    return null;
        //else
        //return substitutes;
        foreach (var item in substitutes)
        {
            yield return item;
        }
    }
    static Product()
    {
        Gold = new Product("Gold", true, 4f);
        Fish = new Product("Fish", true, 0.04f);
        Grain = new Product("Grain", false, 0.04f);
        Cattle = new Product("Cattle", true, 0.04f);

        Fruit = new Product("Fruit", true, 1f);
        Wine = new Product("Wine", false, 3f);

        Food = new Product("Food", false, 0.04f, new List<Product> { Fish, Grain, Cattle, Fruit});

        Wood = new Product("Wood", true, 2.7f);
        Lumber = new Product("Lumber", false, 8f);
        Furniture = new Product("Furniture", false, 7f);

        Wool = new Product("Wool", true, 1f);
        Clothes = new Product("Clothes", false, 6f);

        Stone = new Product("Stone", true, 1f);
        Cement = new Product("Cement", false, 2f);

        MetallOre = new Product("Metal ore", true, 3f);
        Metal = new Product("Metal", false, 6f);

        ColdArms = new Product("Cold arms", false, 13f);
        Ammunition = new Product("Ammunition", false, 13f);
        Firearms = new Product("Firearms", false, 13f);
        Artillery = new Product("Artillery", false, 13f);

        Oil = new Product("Oil", true, 10f);
        Fuel = new Product("Fuel", false, 15f);
        Machinery = new Product("Machinery", false, 8f);
        Rubber = new Product("Rubber", true, 10f);
        Cars = new Product("Cars", false, 15f);
        Tanks = new Product("Tanks", false, 20f);
        Airplanes = new Product("Airplanes", false, 20f);

    }
    /// <summary>
    /// General constructor
    /// </summary>    
    private Product(string name, bool isResource, float defaultPrice) : base(name)
    {
        this.defaultPrice = new Value(defaultPrice);
        _isResource = isResource;
        if (_isResource)
            resourceCounter++;        
        allProducts.Add(this);
        Game.market.SetDefaultPrice(this, defaultPrice);
        //_isAbstract = false;
        //TODO checks for duplicates&
    }
    public static void sortSubstitutes()
    {
        foreach (var item in getAllAbstract())
        {
            item.substitutes.Sort(CostOrder);
        }
    }
    static public int CostOrder(Product x, Product y)
    {
        //eats less memory
        float sumX =  Game.market.findPrice(x).get();
        float sumY = Game.market.findPrice(y).get();
        return sumX.CompareTo(sumY);

        //return Game.market.getCost(x).get().CompareTo(Game.market.getCost(y).get());
    }
    /// <summary>
    /// Constructor for abstract products
    /// </summary>    
    private Product(string name, bool inlanded, float defaultPrice, List<Product> substitutes) : this(name, inlanded, defaultPrice)
    {
        _isAbstract = true;
        this.substitutes = substitutes;
    }
    internal bool isResource()
    {
        return _isResource;
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
            || (!country.isInvented(Invention.SteamPower) && (this == Machinery || this == Cement))
            || ((this == Artillery || this == Ammunition) && !country.isInvented(Invention.Gunpowder))
            || (this == Firearms && !country.isInvented(Invention.Firearms))
            || (!country.isInvented(Invention.CombustionEngine) && (this == Oil || this == Fuel || this == Rubber || this == Cars))
            || (!country.isInvented(Invention.Tanks) && this == Tanks)
            || (!country.isInvented(Invention.Airplanes) && this == Airplanes)
            //|| (!isResource() && !country.isInvented(Invention.Manufactories))
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
            return defaultPrice.multiplyOutside(Options.defaultPriceLimitMultiplier);
        }
        else
        {
            var type = FactoryType.whoCanProduce(this);
            if (type == null)
                return defaultPrice.multiplyOutside(Options.defaultPriceLimitMultiplier);
            else
            {
                Value res = Game.market.getCost(type.resourceInput);
                res.multiply(Options.defaultPriceLimitMultiplier);
                res.divide(type.basicProduction);
                return res;
            }
        }
    }

}
