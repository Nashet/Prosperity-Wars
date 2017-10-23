using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;

public class Product : Name
{
    //private static HashSet<Product> allProducts = new HashSet<Product>();
    private static readonly List<Product> allProducts = new List<Product>();
    private static int resourceCounter;

    private readonly bool _isResource;
    private readonly Value defaultPrice;
    private readonly bool _isAbstract;
    private readonly List<Product> substitutes;
    private readonly Color color;

    internal static readonly Product Fish, Grain, Cattle, Wood, Lumber, Furniture, Gold, Metal, MetallOre,
    Cotton, Clothes, Stone, Cement, Fruit, Liquor, ColdArms, Ammunition, Firearms, Artillery,
    Oil, MotorFuel, Cars, Tanks, Airplanes, Rubber, Machinery,
        Coal = new Product("Coal", 1f, Color.black),
        Tobacco = new Product("Tobacco", 1f, Color.green),
        Electronics = new Product("Electronics", 1f);
    // abstract products
    internal static readonly Product Food, Sugar, Fibres, Fuel;

    static Product()
    {
        Gold = new Product("Gold", 4f, Color.yellow);
        Fish = new Product("Fish", 0.04f, Color.cyan);
        Grain = new Product("Grain", 0.04f, new Color(0.57f, 0.75f, 0.2f));//greenish
        Cattle = new Product("Cattle", 0.04f);

        Fruit = new Product("Fruit", 1f, new Color(1f, 0.33f, 0.33f));//pinkish
        Liquor = new Product("Liquor", 3f);

        Wood = new Product("Wood", 2.7f, new Color(0.5f, 0.25f, 0f)); // brown
        Lumber = new Product("Lumber", 8f);
        Furniture = new Product("Furniture", 7f);

        Cotton = new Product("Cotton", 1f, Color.white);
        Clothes = new Product("Clothes",  6f);

        Stone = new Product("Stone",  1f, new Color(0.82f, 0.62f, 0.82f));//light grey
        Cement = new Product("Cement",  2f);

        MetallOre = new Product("Metal ore", 3f, Color.blue);
        Metal = new Product("Metal",  6f);

        ColdArms = new Product("Cold arms",  13f);
        Ammunition = new Product("Ammunition", 13f);
        Firearms = new Product("Firearms",  13f);
        Artillery = new Product("Artillery", 13f);

        Oil = new Product("Oil",  10f, new Color(0.25f, 0.25f, 0.25f));
        MotorFuel = new Product("Motor Fuel", 15f);
        Machinery = new Product("Machinery", 8f);
        Rubber = new Product("Rubber",  10f, new Color (0.67f, 0.67f, 0.47f)); //light grey
        Cars = new Product("Cars",  15f);
        Tanks = new Product("Tanks", 20f);
        Airplanes = new Product("Airplanes", 20f);

        // abstract products
        Food = new Product("Food", 0.04f, new List<Product> { Fish, Grain, Cattle, Fruit });
        Sugar = new Product("Sugar",  0.04f, new List<Product> { Grain, Fruit });
        Fibres = new Product("Fibres", 0.04f, new List<Product> { Cattle, Cotton });
        Fuel = new Product("Fuel",  0.04f, new List<Product> { Wood, Coal, Oil });

        foreach (var item in getAllNonAbstract())
        {
            Game.market.SetDefaultPrice(item, item.defaultPrice.get());
        }
    }
    /// <summary>
    /// General constructor
    /// </summary>    
    private Product(string name, float defaultPrice) : base(name)
    {
        this.defaultPrice = new Value(defaultPrice);
        allProducts.Add(this);
        //_isAbstract = false;
        //TODO checks for duplicates&
    }
    /// <summary>
    /// Constructor for resorce product
    /// </summary>                     
    private Product(string name, float defaultPrice, Color color) : this(name, defaultPrice)
    {
        this.color = color;
        _isResource = true;
        resourceCounter++;
    }
    /// <summary>
    /// Constructor for abstract products
    /// </summary>    
    private Product(string name, float defaultPrice, List<Product> substitutes) : this(name, defaultPrice)
    {
        _isAbstract = true;
        this.substitutes = substitutes;
    }
    public static IEnumerable<Product> getAllAbstract()
    {
        foreach (var item in allProducts)
            if (item.isAbstract())
                yield return item;
    }
    public static IEnumerable<Product> getAllNonAbstract()
    {
        foreach (var item in allProducts)
            if (!item.isAbstract())
                yield return item;
    }
    public static IEnumerable<Product> getAll()
    {
        foreach (var item in allProducts)
            yield return item;
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
        float sumX = Game.market.getPrice(x).get();
        float sumY = Game.market.getPrice(y).get();
        return sumX.CompareTo(sumY);
        //return Game.market.getCost(x).get().CompareTo(Game.market.getCost(y).get());
    }
    //public static bool operator ==(Product a, Product b)
    //{
    //    // If both are null, or both are same instance, return true.
    //    if (System.Object.ReferenceEquals(a, b))
    //    {
    //        return true;
    //    }

    //    // If one is null, but not both, return false.
    //    if (((object)a == null) || ((object)b == null))
    //    {
    //        return false;
    //    }

    //    // Return true if its substitute product:
    //    if (a.isAbstract() && a.substitutes.Contains(b)
    //     || b.isAbstract() && b.substitutes.Contains(a))
    //        return true;
    //    else
    //        return false;
    //}

    //public static bool operator !=(Product a, Product b)
    //{
    //    return !(a == b);
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
    public bool isAbstract()
    {
        return _isAbstract;
    }
    /// <summary> Returns true if products exactly same or this is substitute for anotherProduct</summary>
    public bool isSameProduct(Product anotherProduct)
    {
        if (this == anotherProduct)
            return true;
        if (anotherProduct.isAbstract() && anotherProduct.substitutes.Contains(this))
            return true;
        else
            return false;
    }
    //public bool isSubstituteFor(Product x)
    //{
    //    if (x.isAbstract() && x.substitutes.Contains(this))
    //        return true;
    //    else
    //        return false;
    //}

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
            || (this == Coal && !country.isInvented(Invention.Coal))
            || (this == Cattle && !country.isInvented(Invention.Domestication))
            || (!country.isInvented(Invention.CombustionEngine) && (this == Oil || this == MotorFuel || this == Rubber || this == Cars))
            || (!country.isInvented(Invention.Tanks) && this == Tanks)
            || (!country.isInvented(Invention.Airplanes) && this == Airplanes)
            || (this == Tobacco && !country.isInvented(Invention.Tobacco))
            || (this == Electronics && !country.isInvented(Invention.Electronics))
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
    public override string ToString()
    {
        if (isAbstract())
        {
            var sb = new StringBuilder(base.ToString());
            sb.Append(" (");
            bool firstLine = true;
            foreach (var item in getSubstitutes())
                if (item.isInventedByAnyOne())
                {
                    if (!firstLine)
                        sb.Append(" or ");
                    sb.Append(item);
                    firstLine = false;
                }
            sb.Append(")");
            return sb.ToString();
        }
        else
            return base.ToString();
    }

    internal Color getColor()
    {
        return color;
    }
}
