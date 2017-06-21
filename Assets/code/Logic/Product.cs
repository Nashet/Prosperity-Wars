using System;
using System.Collections.Generic;



public class Product
{
    //private static HashSet<Product> allProducts = new HashSet<Product>();
    internal static List<Product> allProducts = new List<Product>();
    // bool storable = true;
    private string name;
    internal bool resource = false;
    static int resourceCounter = 0;
    Value defaultPrice;

    internal static Product Food, Wood, Lumber, Furniture, Gold, Metal, MetallOre,
     Wool, Clothes, Stone, Cement, Fruit, Wine, ColdArms, Ammunition, Firearms, Artillery;
    internal static Product Oil,
      Fuel,
      Cars,
   Tanks,
     Airplanes,
      Rubber, Machinery;

    public Product(string name, bool inlanded, float defaultPrice)
    {
        this.defaultPrice = new Value(defaultPrice);
        resource = inlanded;
        if (resource) resourceCounter++;
        this.name = name;
        allProducts.Add(this);
        Game.market.SetDefaultPrice(this, defaultPrice);
        if (name == "Food") Food = this;
        if (name == "Wood") Wood = this;
        if (name == "Lumber") Lumber = this;
        if (name == "Furniture") Furniture = this;
        if (name == "Gold") Gold = this;
        if (name == "Metal") Metal = this;
        if (name == "Metal ore") MetallOre = this;
        if (name == "Wool") Wool = this;
        if (name == "Clothes") Clothes = this;
        if (name == "Stone") Stone = this;
        if (name == "Cement") Cement = this;
        if (name == "Fruit") Fruit = this;
        if (name == "Wine") Wine = this;
        if (name == "Cold arms") ColdArms = this;
        if (name == "Ammunition") Ammunition = this;
        if (name == "Firearms") Firearms = this;
        if (name == "Artillery") Artillery = this;

        if (name == "Oil") Oil = this;
        if (name == "Fuel") Fuel = this;
        if (name == "Cars") Cars = this;
        if (name == "Tanks") Tanks = this;
        if (name == "Airplanes") Airplanes = this;
        if (name == "Rubber") Rubber = this;
        if (name == "Machinery") Machinery = this;
        //TODO checks for duplicates&
    }
    internal bool isResource()
    {
        return resource;
    }
    internal static Product getRandomResource(bool ignoreGold)
    {
        //int random = Game.Random.Next(resourceCounter);
        //int counter = 0;
        //foreach (Product pro in Product.allProducts)
        //{
        //    if (pro.isResource())
        //    {
        //        if (counter == random)
        //            return pro;
        //        counter++;
        //    }
        //}
        if (ignoreGold)
            return Product.Wood;
        return Product.allProducts.PickRandom(x=>x.isResource());
        
        //return null;
    }

    public static Product findByName(string name)
    {  //HashSet set = new HashSet();
        foreach (Product next in allProducts)
        {
            if (next.getName().Equals(name))
                return next;
        }
        return null;

    }
    public string getName()
    {
        return name;
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
            || (!country.isInvented(Invention.CombustionEngine) && (this == Oil || this == Fuel || this == Rubber  || this == Cars))            
            || (!country.isInvented(Invention.Tanks) && this == Tanks )
            || (!country.isInvented(Invention.Airplanes) && this == Airplanes)
            || (!isResource() && !country.isInvented(Invention.manufactories))
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
    override public string ToString()
    {
        return getName();
    }

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
