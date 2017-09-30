using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public class PopType: IEscapeTarget
{
    private readonly static List<PopType> allPopTypes = new List<PopType>();
    public static readonly PopType TribeMen, Aristocrats, Farmers, Artisans, Soldiers, Workers, Capitalists;


    ///<summary> per 1000 men </summary>    
    private readonly PrimitiveStorageSet lifeNeeds = new PrimitiveStorageSet();
    private readonly PrimitiveStorageSet everyDayNeeds = new PrimitiveStorageSet();
    private readonly PrimitiveStorageSet luxuryNeeds = new PrimitiveStorageSet();
    private readonly PrimitiveStorageSet militaryNeeds = new PrimitiveStorageSet();

    ///<summary> per 1000 men </summary>
    private readonly Storage basicProduction;
    private readonly string name;
    /// <summary>
    /// SHOULD not be zero!
    /// </summary>
    private readonly float strenght;
    static PopType() // can't be private
    {
        var tribemenLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 1) });
        var tribemenEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 2) });
        var tribemenLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 3) });
        TribeMen = new PopType("Tribesmen", new Storage(Product.Food, 1.0f), 2f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
            tribemenLifeNeeds, tribemenEveryDayNeeds, tribemenLuxuryNeeds);
        //***************************************next type***************************
        var aristocratsLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 1) });
        var aristocratsEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> {

            new Storage(Product.ColdArms, 1),
            new Storage(Product.Clothes, 1),
            new Storage(Product.Furniture, 1),
            new Storage(Product.Wine, 2),});
        var aristocratsLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Fruit, 1),
            new Storage(Product.Cars, 1f),
            new Storage(Product.Fuel, 1f),
            new Storage(Product.Airplanes, 1f) });
        Aristocrats = new PopType("Aristocrats", null, 4f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
            aristocratsLifeNeeds, aristocratsEveryDayNeeds, aristocratsLuxuryNeeds);
        //***************************************next type***************************
        var capitalistsLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 1) });
        var capitalistsEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Clothes, 1f),
            new Storage(Product.Furniture, 1f),
            new Storage(Product.Fruit, 1f) });
        var capitalistsLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Wine, 2f),
            new Storage(Product.Firearms, 1f),
            new Storage(Product.Ammunition, 0.5f),
            new Storage(Product.Cars, 1f),
            new Storage(Product.Fuel, 1f),
            new Storage(Product.Airplanes, 1f)});
        Capitalists = new PopType("Capitalists", null, 1f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
            capitalistsLifeNeeds, capitalistsEveryDayNeeds, capitalistsLuxuryNeeds);
        //***************************************next type***************************
        {
            var artisansLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 1) });
            var artisansEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Clothes, 1f),
            new Storage(Product.Furniture, 1f),
            new Storage(Product.Metal, 1f) });
            var artisansLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Fish, 1f),
            new Storage(Product.Wine, 1f),
            new Storage(Product.Cars, 1f),
            new Storage(Product.Fuel, 1f),
            });
            Artisans = new PopType("Artisans", null, 1f,
                new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
                artisansLifeNeeds, artisansEveryDayNeeds, artisansLuxuryNeeds);
        }
        //***************************************next type***************************
        var farmersLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 1) });
        var farmersEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> {
           //everyDayNeeds.Set(new Storage(Product.Fruit, 1),
            new Storage(Product.Stone, 1),
            new Storage(Product.Wood, 1),
            //everyDayNeeds.set(new Storage(Product.Wool, 1),
            new Storage(Product.Lumber, 1),
            new Storage(Product.Cars, 0.5f),
            new Storage(Product.Fuel, 0.5f)});
        var farmersLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Clothes, 1),
            new Storage(Product.Furniture, 1),
            new Storage(Product.Wine, 2)
            //new Storage(Product.Metal, 1),
            //new Storage(Product.Cement, 0.5f)
                                            });
        Farmers = new PopType("Farmers", new Storage(Product.Food, 1.5f), 1f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
            farmersLifeNeeds, farmersEveryDayNeeds, farmersLuxuryNeeds);
        //***************************************next type***************************
        var workersLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 1) });
        var workersEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Clothes, 1f),
            new Storage(Product.Wine, 2f),
            new Storage(Product.Furniture, 1f)
             });
        var workersLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Fish, 1),
            new Storage(Product.Cars, 0.5f),
            new Storage(Product.Fuel, 0.5f)
            });
        Workers = new PopType("Workers", null, 1f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
            workersLifeNeeds, workersEveryDayNeeds, workersLuxuryNeeds);
        //***************************************next type***************************
        var soldiersLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 2) });
        var soldiersEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Fruit, 2),
            new Storage(Product.Wine, 5),
            new Storage(Product.Clothes, 4),
            new Storage(Product.Furniture, 2),
            //new Storage(Product.Wood, 1)
        });
        var soldiersLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> {
             new Storage(Product.Firearms, 1f),
            new Storage(Product.Ammunition, 0.5f),
            new Storage(Product.Cars, 1f), // temporally
            new Storage(Product.Fuel, 1f),// temporally
            new Storage(Product.Airplanes, 1f) // temporally
            });
        Soldiers = new PopType("Soldiers", null, 2f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
            soldiersLifeNeeds, soldiersEveryDayNeeds, soldiersLuxuryNeeds);
    }
    private PopType(string name, Storage produces, float strenght, PrimitiveStorageSet militaryNeeds,
        PrimitiveStorageSet lifeNeeds, PrimitiveStorageSet everyDayNeeds, PrimitiveStorageSet luxuryNeeds)
    {
        this.militaryNeeds = militaryNeeds;
        this.strenght = strenght;

        this.name = name;
        basicProduction = produces;
        this.lifeNeeds = lifeNeeds;
        this.everyDayNeeds = everyDayNeeds;
        this.luxuryNeeds = luxuryNeeds;
        allPopTypes.Add(this);
    }
    public static IEnumerable<PopType> getAllPopTypes()
    {
        foreach (var item in allPopTypes)
            yield return item;
    }

    public Storage getBasicProduction()
    {
        return basicProduction;
    }
    public bool canMobilize(Staff byWhom)
    {
        if (byWhom is Country)
        {
            if (this == PopType.Capitalists)
                return false;
            else
                return true;
        }
        else // movement
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
    ///<summary> per 1000 men. Be careful, its direct links </summary>
    public List<Storage> getLifeNeedsPer1000()
    {
        //List<Storage> result = new List<Storage>();
        //foreach (Storage next in lifeNeeds)        
        //    result.Add(next);
        //return result;
        return lifeNeeds.getContainer();
    }
    ///<summary> per 1000 men. Be careful, its direct links </summary>
    public List<Storage> getEveryDayNeedsPer1000()
    {
        //List<Storage> result = new List<Storage>();
        //foreach (Storage next in everyDayNeeds)            
        //    result.Add(next);
        //return result;
        return everyDayNeeds.getContainer();
    }
    ///<summary> per 1000 men. Be careful, its direct links </summary>
    public List<Storage> getLuxuryNeedsPer1000()
    {
        //List<Storage> result = new List<Storage>();
        //foreach (Storage next in luxuryNeeds)            
        //    result.Add(next);
        //return result;        
        return luxuryNeeds.getContainer();
    }
    ///<summary> per 1000 men. Be careful, its direct links </summary>
    public List<Storage> getAllNeedsPer1000()
    {
        List<Storage> result = new List<Storage>(getLifeNeedsPer1000());
        result.AddRange(getEveryDayNeedsPer1000());
        result.AddRange(getLuxuryNeedsPer1000());
        return result;
    }
    override public string ToString()
    {
        return name;
    }

    internal bool isPoorStrata()
    {
        return this == PopType.Farmers || this == PopType.Workers || this == PopType.TribeMen || this == PopType.Soldiers;
    }

    internal bool isRichStrata()
    {
        return this == PopType.Aristocrats || this == PopType.Capitalists || this == PopType.Artisans;
    }

    internal float getStrenght()
    {
        return strenght;
    }
    public bool canBeUnemployed()
    {
        return this == PopType.Farmers || this == PopType.Workers || this == PopType.TribeMen;
    }
    /// <summary>
    /// Returns true if can produce something by himself
    /// </summary>    
    internal bool isProducer()
    {
        return this == PopType.Farmers || this == PopType.TribeMen || this == PopType.Artisans;
    }
    /// <summary>
    /// Makes sure that pops consume product in cheap-first order
    /// </summary>
    internal static void sortNeeds()
    {
        foreach (var item in allPopTypes)
        {
            item.everyDayNeeds.sort(Storage.CostOrder);
            item.luxuryNeeds.sort(Storage.CostOrder);
        }
    }
}

