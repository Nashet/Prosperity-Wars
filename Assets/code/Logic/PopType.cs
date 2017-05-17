using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public class PopType
{
    ///<summary> per 1000 men </summary>    
    protected PrimitiveStorageSet lifeNeeds = new PrimitiveStorageSet();
    protected PrimitiveStorageSet everyDayNeeds = new PrimitiveStorageSet();
    protected PrimitiveStorageSet luxuryNeeds = new PrimitiveStorageSet();

    private PrimitiveStorageSet militaryNeeds = new PrimitiveStorageSet();
    //List<Needs> lifeNeeds = new List<Needs>();
    //List<Needs> everyDayNeeds = new List<Needs>();
    //List<Needs> luxuryNeeds = new List<Needs>();

    //string name;
    public enum PopTypes { Tribemen, Aristocrats, Farmers, Artisans, Soldiers, Workers, Capitalists };
    public static PopType tribeMen, aristocrats, farmers, artisans, soldiers, workers, capitalists;
    public PopTypes type;
    public static List<PopType> allPopTypes = new List<PopType>();
    ///<summary> per 1000 men </summary>
    public Storage basicProduction;
    private string name;
    private float strenght;
    public PopType(PopTypes itype, Storage iproduces, string iname, float strenght, PrimitiveStorageSet militaryNeeds)
    {
        this.militaryNeeds = militaryNeeds;
        this.strenght = strenght;
        type = itype;
        name = iname;
        basicProduction = iproduces;
        allPopTypes.Add(this);
        switch (itype)
        {
            case PopTypes.Tribemen:
                tribeMen = this;
                lifeNeeds.Set(new Storage(Product.Food, 1));
                everyDayNeeds.Set(new Storage(Product.Food, 2));
                luxuryNeeds.Set(new Storage(Product.Food, 3));
                break;
            case PopTypes.Aristocrats:
                aristocrats = this;
                lifeNeeds.Set(new Storage(Product.Food, 1));

                everyDayNeeds.Set(new Storage(Product.Fruit, 1));


                luxuryNeeds.Set(new Storage(Product.Clothes, 1));
                luxuryNeeds.Set(new Storage(Product.Furniture, 1));

                luxuryNeeds.Set(new Storage(Product.Wine, 2));
                luxuryNeeds.Set(new Storage(Product.Metal, 1));
                luxuryNeeds.Set(new Storage(Product.Cement, 0.5f));
                break;
            case PopTypes.Capitalists:
                capitalists = this;
                lifeNeeds.Set(new Storage(Product.Food, 1));

                luxuryNeeds.Set(new Storage(Product.Fruit, 1));

                everyDayNeeds.Set(new Storage(Product.Clothes, 1));
                everyDayNeeds.Set(new Storage(Product.Furniture, 1));

                everyDayNeeds.Set(new Storage(Product.Wine, 2));
                everyDayNeeds.Set(new Storage(Product.Metal, 1));
                everyDayNeeds.Set(new Storage(Product.Cement, 0.5f));
                break;
            case PopTypes.Farmers:
                farmers = this;
                lifeNeeds.Set(new Storage(Product.Food, 1f));

                //everyDayNeeds.Set(new Storage(Product.Fruit, 1));
                everyDayNeeds.Set(new Storage(Product.Stone, 1));
                everyDayNeeds.Set(new Storage(Product.Wood, 1));
                everyDayNeeds.Set(new Storage(Product.Wool, 1));
                everyDayNeeds.Set(new Storage(Product.Lumber, 1));
                everyDayNeeds.Set(new Storage(Product.MetallOre, 1));

                luxuryNeeds.Set(new Storage(Product.Clothes, 1));
                luxuryNeeds.Set(new Storage(Product.Furniture, 1));

                luxuryNeeds.Set(new Storage(Product.Wine, 2));
                luxuryNeeds.Set(new Storage(Product.Metal, 1));
                luxuryNeeds.Set(new Storage(Product.Cement, 0.5f));

                break;
            case PopTypes.Artisans:
                artisans = this;
                break;
            case PopTypes.Soldiers:
                soldiers = this;
                break;
            case PopTypes.Workers:
                workers = this;
                lifeNeeds.Set(new Storage(Product.Food, 1));

                everyDayNeeds.Set(new Storage(Product.Fruit, 1));

                everyDayNeeds.Set(new Storage(Product.Clothes, 1));
                everyDayNeeds.Set(new Storage(Product.Furniture, 1));

                luxuryNeeds.Set(new Storage(Product.Wine, 2));
                everyDayNeeds.Set(new Storage(Product.Metal, 1));


                break;
            default:
                Debug.Log("Unknown PopType");
                break;
        }

    }
    public bool canMobilize()
    {
        if (this == PopType.capitalists)
            return false;
        else
            return true;
    }
    public PrimitiveStorageSet getMilitaryNeedsPer1000()
    {
        return militaryNeeds;
    }
    /////<summary> per 1000 men </summary>
    //public Storage getLifeNeedsPer1000(PopType popType)
    //{
    //    foreach (Needs next in lifeNeeds)
    //        if (next.popType == popType)
    //            return next.needs;
    //    return null;
    //}
    ///<summary> per 1000 men </summary>
    public List<Storage> getLifeNeedsPer1000()
    {
        List<Storage> result = new List<Storage>();
        foreach (Storage next in lifeNeeds)
            //if (next.popType == this)
            result.Add(next);
        return result;
    }
    ///<summary> per 1000 men </summary>
    public List<Storage> getEveryDayNeedsPer1000()
    {
        List<Storage> result = new List<Storage>();
        foreach (Storage next in everyDayNeeds)
            // (next.popType == this)
            result.Add(next);
        return result;
    }
    ///<summary> per 1000 men </summary>
    public List<Storage> getLuxuryNeedsPer1000()
    {
        List<Storage> result = new List<Storage>();
        foreach (Storage next in luxuryNeeds)
            // if (next.popType == this)
            result.Add(next);
        return result;
        // Needs
    }
    override public string ToString()
    {
        return name;
    }

    internal bool isPoorStrata()
    {
        return this == PopType.farmers || this == PopType.workers || this == PopType.tribeMen;
    }

    internal bool isRichStrata()
    {
        return this == PopType.aristocrats || this == PopType.capitalists;
    }

    internal float getStrenght()
    {
        return strenght;
    }
}
