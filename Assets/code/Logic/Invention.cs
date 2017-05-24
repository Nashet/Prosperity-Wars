using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InventionsList
{
    internal Dictionary<InventionType, bool> list = new Dictionary<InventionType, bool>();
    public InventionsList()
    {
        foreach (var each in InventionType.allInventions)
            list.Add(each, false);
    }
    public void MarkInvented(InventionType type)
    {
        //bool result = false;
        //if (list.TryGetValue(type, out result))
        //    result = true;
        //else
        //    result = false;
        list[type] = true;
    }
    public bool isInvented(InventionType type)
    {
        bool result = false;
        list.TryGetValue(type, out result);
        return result;
    }

}
public class InventionType : AbstractCondition
{
    internal static List<InventionType> allInventions = new List<InventionType>();
    string name;
    string description;
    internal Value cost;
    string inventedPhrase;
    public static InventionType farming = new InventionType("Farming", "Allows farming and farmers", new Value(100f)),
        //capitalism = new InventionType("Capitalism", "", new Value(50f)),
        banking = new InventionType("Banking", "Allows national bank, credits and deposits. Also allows serfdom abolishment with compensation for aristocrats", new Value(100f)),
        manufactories = new InventionType("Manufactures", "Allows building manufactures to process raw product\n Testes testosterone testes test", new Value(100f)),
        mining = new InventionType("Mining", "Allows resource gathering from holes in ground, increasing it's efficiency by 50%", new Value(100f)),
        //religion = new InventionType("Religion", "Allows clerics, gives loyalty boost", new Value(100f)),
        metal = new InventionType("Metal", "Allows metal ore and smelting. Metal is good for tools and weapons", new Value(100f)),
        individualRights = new InventionType("Individual rights", "Allows Capitalism, Serfdom & Slavery abolishments", new Value(100f)),
        collectivism = new InventionType("Collectivism", "Allows Proletarian dictatorship & Planned Economy", new Value(100f)),
        steamPower = new InventionType("Steam Power", "Increases efficiency of all enterprises by 25%", new Value(100f)),
        Welfare = new InventionType("Welfare", "Allows min wage and.. other", new Value(100f))
        ;
    readonly public static Condition SteamPowerInvented = new Condition(x=>(x as Country).isInvented(InventionType.steamPower), "Steam Power is invented", true);
    readonly public static Condition IndividualRightsInvented = new Condition(x => (x as Country).isInvented(InventionType.individualRights), "Individual Rights are invented", true);
    internal InventionType(string name, string description, Value cost)
    {
        this.name = name;
        this.description = description;
        this.cost = cost;
        inventedPhrase = "Invented " + name;
        allInventions.Add(this);
    }
    internal string getInventedPhrase()
    {
        return inventedPhrase;
    }
    internal InventionType()
    { }
    internal string getDescription()
    {
        return description;
    }
    override public string ToString()
    {
        return name;
    }
}