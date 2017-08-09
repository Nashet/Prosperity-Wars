using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class Invention : Name
{
    internal readonly static List<Invention> allInventions = new List<Invention>();
    string description;
    internal Value cost;
    string inventedPhrase;
    public static readonly Invention Farming = new Invention("Farming", "Allows farming and farmers", new Value(100f)),
        Banking = new Invention("Banking", "Allows national bank, credits and deposits. Also allows serfdom abolishment with compensation for aristocrats", new Value(100f)),
        Manufactories = new Invention("Manufactures", "Allows building manufactures to process raw product", new Value(400f)),
        Mining = new Invention("Mining", "Allows resource gathering from holes in ground, increasing it's efficiency by 50%", new Value(100f)),
        //religion = new InventionType("Religion", "Allows clerics, gives loyalty boost", new Value(100f)),
        Metal = new Invention("Metal", "Allows metal ore and smelting. Allows Cold arms", new Value(100f)),
        IndividualRights = new Invention("Individual rights", "Allows Capitalism, Serfdom & Slavery abolishments", new Value(100f)),
        Collectivism = new Invention("Collectivism", "Allows Proletarian dictatorship & Planned Economy", new Value(100f)),
        SteamPower = new Invention("Steam Power", "Allows Machinery & cement, Increases efficiency of all enterprises by 25%", new Value(100f)),
        Welfare = new Invention("Welfare", "Allows min wage and.. other", new Value(100f)),
        Gunpowder = new Invention("Gunpowder", "Allows Artillery & Ammunition", new Value(100f)),
        Firearms = new Invention("Hand-held cannons", "Allows Firearms, very efficient in battles", new Value(200f)),
        CombustionEngine = new Invention("Combustion engine", "Allows Oil, Fuel, Cars, Rubber, Increases efficiency of all enterprises by 25%", new Value(400f)),
        Tanks = new Invention("Tanks", "Allows Tanks", new Value(800f)),
        Airplanes = new Invention("Airplanes", "Allows Airplanes", new Value(1200f)),
        ProfessionalArmy = new Invention("Professional Army", "Allows soldiers", new Value(200f))
        ;
    readonly public static Condition ProfessionalArmyInvented = new Condition(x => (x as Country).isInvented(Invention.ProfessionalArmy), "Professional Army is invented", true);
    readonly public static Condition SteamPowerInvented = new Condition(x => (x as Country).isInvented(Invention.SteamPower), "Steam Power is invented", true);
    readonly public static Condition CombustionEngineInvented = new Condition(x => (x as Country).isInvented(Invention.CombustionEngine), "Combustion Engine is invented", true);
    readonly public static Condition IndividualRightsInvented = new Condition(x => (x as Country).isInvented(Invention.IndividualRights), "Individual Rights are invented", true);
    readonly public static Condition BankingInvented = new Condition(x => (x as Country).isInvented(Invention.Banking), "Banking is invented", true);
    readonly public static Condition WelfareInvented = new Condition(x => (x as Country).isInvented(Invention.Welfare), "Welfare is invented", true);
    readonly public static Condition collectivismInvented = new Condition(x => (x as Country).isInvented(Invention.Collectivism), "Collectivism is invented", true);
    internal Invention(string name, string description, Value cost) : base(name)
    {
        //this.name = name;
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
        if (//this == Collectivism
            //||
            (this == Gunpowder && !country.isInvented(Metal))
            || (this == SteamPower && (!country.isInvented(Metal) || !country.isInvented(Manufactories)))
            || (this == Firearms && !country.isInvented(Gunpowder))
            || (this == CombustionEngine && !country.isInvented(SteamPower))
            || (this == Tanks && !country.isInvented(CombustionEngine))
            || (this == Airplanes && !country.isInvented(CombustionEngine))
            )
            return false;
        else
            return true;

    }
    internal string getDescription()
    {
        return description;
    }
}