using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InventionsList
{
    Dictionary<Invention, bool> list = new Dictionary<Invention, bool>();
    public InventionsList()
    {
        foreach (var each in Invention.allInventions)
            list.Add(each, false);
    }
    public IEnumerable<KeyValuePair<Invention, bool>> getAvailable(Country country)
    {
        foreach (var invention in list)
            if (invention.Key.isAvailable(country))
                yield return invention;
    }
    public IEnumerable<KeyValuePair<Invention, bool>> getUninvented(Country country)
    {
        foreach (var invention in list)
            if (invention.Value == false && invention.Key.isAvailable(country))
                yield return invention;
    }
    public IEnumerable<KeyValuePair<Invention, bool>> getInvented(Country country)
    {
        foreach (var invention in list)
            if (invention.Value == true && invention.Key.isAvailable(country))
                yield return invention;
    }
    //TODO strange architecture
    public void markInvented(Invention type)
    {
        list[type] = true;
    }
    public bool isInvented(Invention type)
    {
        bool result = false;
        list.TryGetValue(type, out result);
        return result;
    }


}
public class Invention : AbstractCondition
{
    internal static List<Invention> allInventions = new List<Invention>();
    string name;
    string description;
    internal Value cost;
    string inventedPhrase;
    public static readonly Invention farming = new Invention("Farming", "Allows farming and farmers", new Value(100f)),
        //capitalism = new InventionType("Capitalism", "", new Value(50f)),
        banking = new Invention("Banking", "Allows national bank, credits and deposits. Also allows serfdom abolishment with compensation for aristocrats", new Value(100f)),
        manufactories = new Invention("Manufactures", "Allows building manufactures to process raw product\n Testes testosterone testes test", new Value(100f)),
        mining = new Invention("Mining", "Allows resource gathering from holes in ground, increasing it's efficiency by 50%", new Value(100f)),
        //religion = new InventionType("Religion", "Allows clerics, gives loyalty boost", new Value(100f)),
        Metal = new Invention("Metal", "Allows metal ore and smelting. Allows Cold arms", new Value(100f)),
        individualRights = new Invention("Individual rights", "Allows Capitalism, Serfdom & Slavery abolishments", new Value(100f)),
        collectivism = new Invention("Collectivism", "Allows Proletarian dictatorship & Planned Economy", new Value(100f)),
        steamPower = new Invention("Steam Power", "Increases efficiency of all enterprises by 25%", new Value(100f)),
        Welfare = new Invention("Welfare", "Allows min wage and.. other", new Value(100f)),
        Gunpowder = new Invention("Gunpowder", "Allows Artillery & Ammunition", new Value(100f)),
        Firearms = new Invention("Hand-held cannons", "Allows Firearms, very efficient in battles", new Value(200f)),
        CombustionEngine = new Invention("Combustion engine", "Allows Oil, Fuel, Cars, Rubber and Machinery", new Value(400f)),
        Tanks = new Invention("Tanks", "Allows Tanks", new Value(800f)),
        Airplanes = new Invention("Airplanes", "Allows Airplanes", new Value(1200f))
        ;
    readonly public static Condition SteamPowerInvented = new Condition(x => (x as Country).isInvented(Invention.steamPower), "Steam Power is invented", true);
    readonly public static Condition IndividualRightsInvented = new Condition(x => (x as Country).isInvented(Invention.individualRights), "Individual Rights are invented", true);
    internal Invention(string name, string description, Value cost)
    {
        this.name = name;
        this.description = description;
        this.cost = cost;
        inventedPhrase = "Invented " + name;
        allInventions.Add(this);
    }
    //internal InventionType()
    //{ }
    internal string getInventedPhrase()
    {
        return inventedPhrase;
    }
    public bool isAvailable(Country country)
    {
        if (this == Invention.collectivism
            || (this == Invention.Gunpowder && !country.isInvented(Invention.Metal))
            || (this == Invention.steamPower && !country.isInvented(Invention.Metal))
            || (this == Invention.Firearms && !country.isInvented(Invention.Gunpowder))
            || (this == Invention.CombustionEngine && !country.isInvented(Invention.steamPower))
            || (this == Invention.Tanks && !country.isInvented(Invention.CombustionEngine))
            || (this == Invention.Airplanes && !country.isInvented(Invention.CombustionEngine))
            )
            return false;
        else
            return true;

    }
    internal string getDescription()
    {
        return description;
    }
    override public string ToString()
    {
        return name;
    }
}