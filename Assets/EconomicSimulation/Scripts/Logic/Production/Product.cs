using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class Product : Name, IClickable
    {
        protected enum Type
        {
            military, industrial, consumerProduct
        }

        //protected static HashSet<Product> allProducts = new HashSet<Product>();
        protected static readonly List<Product> allProducts = new List<Product>();

        protected static int resourceCounter;

        public readonly MoneyView defaultPrice;
        protected readonly bool _isResource;
        protected readonly bool _isAbstract;
        protected readonly bool _isMilitary;
        protected readonly bool _isIndustrial;
        protected readonly bool _isConsumerProduct;
        protected readonly List<Product> substitutes;
        protected readonly Color color;
        protected bool _isStoreable = true;

        protected Invention[] requiredInventions;

        public static readonly Product
            //Fish, Grain, Cattle, Wood, Lumber, Furniture, Gold, Metal, MetalOre,
            //Cotton, Clothes, Stone, Cement, Fruit, Liquor, ColdArms, Ammunition, Firearms, Artillery,
            //Oil, MotorFuel, Cars, Tanks, Airplanes, Rubber, Machinery,
            Fish = new Product("Fish", 0.04f, Color.cyan, Type.consumerProduct),
            Grain = new Product("Grain", 0.04f, new Color(0.57f, 0.75f, 0.2f), Type.industrial),//greenish
            Cattle = new Product("Cattle", 0.04f, Type.military),

            Fruit = new Product("Fruit", 1f, new Color(1f, 0.33f, 0.33f), Type.consumerProduct),//pinkish
            Liquor = new Product("Liquor", 3f, Type.consumerProduct),

            Wood = new Product("Wood", 2.7f, new Color(0.5f, 0.25f, 0f), Type.industrial), // brown
            Lumber = new Product("Lumber", 8f, Type.industrial),
            Furniture = new Product("Furniture", 7f, Type.consumerProduct),

            Cotton = new Product("Cotton", 1f, Color.white, Type.consumerProduct),
            Clothes = new Product("Clothes", 6f, Type.consumerProduct),

            Stone = new Product("Stone", 1f, new Color(0.82f, 0.62f, 0.82f), Type.industrial),
            //Cement = new Product("Cement", 2f, type.industrial,Invention.SteamPower),

            MetalOre = new Product("Metal ore", 3f, Color.blue, Type.industrial, Invention.Metal),
            Metal = new Product("Metal", 6f, Type.industrial, Invention.Metal),

            ColdArms = new Product("Cold arms", 13f, Type.military, Invention.Metal),
            Ammunition = new Product("Ammunition", 13f, Type.military, Invention.Gunpowder),
            Firearms = new Product("Firearms", 13f, Type.military, Invention.Firearms),
            Artillery = new Product("Artillery", 13f, Type.military, Invention.Gunpowder),

            Oil = new Product("Oil", 10f, new Color(0.25f, 0.25f, 0.25f), Type.military, Invention.CombustionEngine),
            MotorFuel = new Product("Motor Fuel", 15f, Type.military, Invention.CombustionEngine),
            Machinery = new Product("Machinery", 8f, Type.industrial, Invention.SteamPower),
            Rubber = new Product("Rubber", 10f, new Color(0.67f, 0.67f, 0.47f), Type.industrial, Invention.CombustionEngine), //light grey
            Cars = new Product("Cars", 15f, Type.military, Invention.CombustionEngine),
            Tanks = new Product("Tanks", 20f, Type.military, Invention.Tanks),
            Airplanes = new Product("Airplanes", 20f, Type.military, Invention.Airplanes),
            Coal = new Product("Coal", 1f, Color.black, Type.industrial, Invention.Coal),
            Tobacco = new Product("Tobacco", 1f, Color.green, Type.consumerProduct, Invention.Tobacco),
            Electronics = new Product("Electronics", 1f, Type.consumerProduct, Invention.Electronics),
            Gold = new Product("Gold", 4f, Color.yellow, Type.industrial),
            Education = new Product("Education", 4f, Type.consumerProduct, false, Invention.Universities);

        public static readonly Product //Food, Sugar, Fibers, Fuel;
            Food = new Product("Food", 0.04f, new List<Product> { Fish, Grain, Cattle, Fruit }, Type.consumerProduct),
            Sugar = new Product("Sugar", 0.04f, new List<Product> { Grain, Fruit }, Type.consumerProduct),
            Fibers = new Product("Fibers", 0.04f, new List<Product> { Cattle, Cotton }, Type.consumerProduct),
            Fuel = new Product("Fuel", 0.04f, new List<Product> { Wood, Coal, Oil }, Type.industrial);

        public static void init()
        { }

        //static initialization
        static Product()
        {
            //// abstract products
            //foreach (var markets in World.AllMarkets)
            //{
            //    foreach (var item in getAll().Where(x => !x.isAbstract()))
            //        if (item != Gold)
            //        {
            //            markets.SetDefaultPrice(item, (float)item.defaultPrice.Get());
            //        }
            //}
        }

        public IEnumerable<Invention> AllRequiredInventions
        {
            get {
                foreach (var item in requiredInventions)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// General constructor
        /// </summary>        
        private Product(string name, float defaultPrice, Type productType, params Invention[] requiredInventions) : base(name)
        {
            this.requiredInventions = requiredInventions;
            this.defaultPrice = new MoneyView((decimal)defaultPrice);
            allProducts.Add(this);
            switch (productType)
            {
                case Type.military:
                    _isMilitary = true;
                    break;

                case Type.industrial:
                    _isIndustrial = true;
                    break;

                case Type.consumerProduct:
                    _isConsumerProduct = true;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Constructor for resource product
        /// </summary>
        private Product(string name, float defaultPrice, Color color, Type productType, params Invention[] requiredInventions) : this(name, defaultPrice, productType, requiredInventions)
        {
            this.color = color;
            _isResource = true;
            resourceCounter++;
        }

        /// <summary>
        /// Constructor for unstorable product
        /// </summary>
        private Product(string name, float defaultPrice, Type productType, bool isStorable, params Invention[] requiredInventions) : this(name, defaultPrice, productType, requiredInventions)
        {
            _isStoreable = false;
        }

        /// <summary>
        /// Constructor for abstract products
        /// </summary>
        private Product(string name, float defaultPrice, List<Product> substitutes, Type productType) : this(name, defaultPrice, productType)
        {
            _isAbstract = true;
            this.substitutes = substitutes;
        }

        public static IEnumerable<Product> All()
        {
            foreach (var item in allProducts)
                yield return item;
        }
        public static IEnumerable<Product> AllNonAbstract()
        {
            foreach (var item in allProducts)
                if (!item.isAbstract())
                    yield return item;
        }

        //public static IEnumerable<Product> getAll(Predicate<Product> selector)
        //{
        //    foreach (var item in allProducts)
        //        if (selector(item))
        //            yield return item;
        //}
        public bool IsStorable
        {
            get { return _isStoreable; }
        }

        /// <summary>
        /// Products go in industrial-military-consumer order
        /// </summary>
        public static IEnumerable<Product> AllNonAbstractTradableInPEOrder(Country country)
        {
            foreach (var item in AllNonAbstract().Where(x => x.isIndustrial() && country.Science.IsInvented(x)))
                yield return item;
            foreach (var item in AllNonAbstract().Where(x => x.isMilitary() && country.Science.IsInvented(x)))
                yield return item;
            foreach (var item in AllNonAbstract().Where(x => x.isConsumerProduct() && country.Science.IsInvented(x)))
                yield return item;
        }

        //public static IEnumerable<Product> getAllSpecificProductsInvented(Func<Product, bool> selector, Country country)
        //{
        //    foreach (var item in getAll().Where(x => !x.isAbstract()))
        //        if (selector(item) && Country.inventions.Invented(item))
        //            yield return item;
        //}

        //public static IEnumerable<Product> getAllSpecificProductsTradable(Func<Product, bool> selector)
        //{
        //    foreach (var item in getAll().Where(x => !x.isAbstract()))
        //        if (selector(item) && item.isTradable())
        //            yield return item;
        //}

        public static int howMuchProductsTotal()
        {
            return allProducts.Count;
        }

        public static int howMuchProducts(Predicate<Product> selector)
        {
            return allProducts.FindAll(x => selector(x)).Count;
        }

        //public static IEnumerable<Product> getAllMilitaryProductsInvented(Country country)
        //{
        //    foreach (var item in getAllNonAbstract())
        //        if (item.isMilitary() && item != Product.Gold && item.isInventedBy(country))
        //            yield return item;
        //}
        //public static IEnumerable<Product> getAllMilitaryProductsTradable()
        //{
        //    foreach (var item in getAllNonAbstract())
        //        if (item.isMilitary() && item.isTradable())
        //            yield return item;
        //}
        //public static IEnumerable<Product> getAllIndustrialProducts(Country country)
        //{
        //    foreach (var item in getAllNonAbstract())
        //        if (item.isIndustrial() && item.isInventedBy(country))
        //            yield return item;
        //}
        //public static IEnumerable<Product> getAllConsumerProducts(Country country)
        //{
        //    foreach (var item in getAllNonAbstract())
        //        if (item.isConsumerProduct() && item.isInventedBy(country))
        //            yield return item;
        //}
        /// <summary>
        /// Gives elements in cheap-expensive order
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Product> getSubstitutes()
        {
            foreach (var item in substitutes)
            {
                yield return item;
            }
        }

        public static Product getRandomResource(bool ignoreGold)
        {
            if (ignoreGold)
                return Wood;
            return allProducts.Where(x => x.isResource()).Random();
        }

        public static void sortSubstitutes(Market market)
        {
            foreach (var item in All().Where(x => x.isAbstract()))
            //if (item.isTradable())
            // Abstract are always invented and not gold
            {
                item.substitutes.Sort(delegate (Product x, Product y)
                {
                    //if (x == null && y == null) return 0;
                    //else 
                    //if (x.PartName == null) return -1;
                    //else if (y.PartName == null) return 1;
                    //else
                    return market.getCost(x).Get().CompareTo(market.getCost(y).Get());
                });
            }
        }

        //public static int CostOrder(Product x, Product y)//, Market market
        //{
        //    //eats less memory
        //    float sumX = (float)Country.market.getCost(x).Get();
        //    float sumY = (float)Country.market.getCost(y).Get();
        //    return sumX.CompareTo(sumY);
        //    //return Country.market.getCost(x).get().CompareTo(Country.market.getCost(y).get());
        //}

        /// <summary>
        /// Isn't Gold & Invested by anyone
        /// </summary>
        /// <returns></returns>
        public bool isTradable()
        {
            return this != Gold && IsInventedByAnyOne();
        }

        public bool isAbstract()
        {
            return _isAbstract;
        }

        public bool isMilitary()
        {
            return _isMilitary;
        }

        public bool isIndustrial()
        {
            return _isIndustrial;
        }

        public bool isConsumerProduct()
        {
            return _isConsumerProduct;
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

        /// <summary> Assuming product is abstract product</summary>
        public bool isSubstituteFor(Product product)
        {
            if (product.substitutes.Contains(this))
                return true;
            else
                return false;
        }

        public bool isResource()
        {
            return _isResource;
        }

        public bool IsInventedByAnyOne()
        {
            // including dead countries. Because dead country could organize production
            //of some freshly invented product
            if (isAbstract())
                return true;
            foreach (var country in World.AllExistingCountries())
                if (country.Science.IsInvented(this))
                    return true;
            return false;
        }

        //bool isStorable()
        //{
        //    return storable;
        //}
        //void setStorable(bool isStorable)
        //{
        //    storable = isStorable;
        //}

        //public MoneyView getDefaultPrice()
        //{
        //    if (isResource())
        //    {
        //        return defaultPrice.Copy().Multiply(Options.defaultPriceLimitMultiplier);
        //    }
        //    else
        //    {
        //        var type = ProductionType.whoCanProduce(this);
        //        if (type == null)
        //            return defaultPrice.Copy().Multiply(Options.defaultPriceLimitMultiplier);
        //        else
        //        {
        //            Money res = Country.market.getCost(type.resourceInput) .Copy();
        //            res.Multiply(Options.defaultPriceLimitMultiplier);
        //            res.Divide(type.basicProduction.get());
        //            return res;
        //        }
        //    }
        //}
        public string ToStringWithoutSubstitutes()
        {
            return base.ToString();
        }

        public override string ToString()
        {
            if (isAbstract())
            {
                var sb = new StringBuilder(base.ToString());
                sb.Append(" (");
                bool firstLine = true;
                foreach (var item in getSubstitutes())
                    if (item.IsInventedByAnyOne())
                    {
                        if (!firstLine)
                            sb.Append(" or ");
                        sb.Append(item);
                        firstLine = false;
                    }
                sb.Append(")");
                return sb.ToString();
                //getSubstitutes().ToList().getString(" or ");
            }
            else
                return base.ToString();
        }

        public Color getColor()
        {
            return color;
        }

        public void OnClicked()
        {
            MainCamera.goodsPanel.show(this);
        }
    }
}