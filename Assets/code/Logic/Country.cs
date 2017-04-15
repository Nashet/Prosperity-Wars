using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class Country : Owner
{
    public string name;
    public static List<Country> allCountries = new List<Country>();
    public List<Province> ownedProvinces = new List<Province>();

    public PrimitiveStorageSet storageSet = new PrimitiveStorageSet();
    public Procent countryTax;
    public Procent aristocrstTax;//= new Procent(0.10f);
    public InventionsList inventions = new InventionsList();

    internal Government government;
    internal Economy economy;
    internal Serfdom serfdom;
    internal MinimalWage minimalWage;
    internal TaxationForPoor taxationForPoor;
    internal List<AbstractReform> reforms = new List<AbstractReform>();
    public Culture culture;
    
    public Bank bank = new Bank();

    /// <summary>
    /// per 1000 men
    /// </summary>
    private Value minSalary = new Value(0.5f);
    public Value sciencePoints = new Value(0f);
    
    public Country(string iname, Culture iculture)
    {
        government = new Government(this);
        economy = new Economy(this);
        serfdom = new Serfdom(this);
        minimalWage = new MinimalWage(this);
        taxationForPoor = new TaxationForPoor(this);
        name = iname;
        allCountries.Add(this);
        //countryTax = new Procent(0.1f);
        TaxationForPoor.ReformValue hru = taxationForPoor.getValue() as TaxationForPoor.ReformValue;
        countryTax = hru.tax;
        culture = iculture;
        //government.status = Government.Democracy;
        //economy.status = Government.Despotism;
        //economy.status = Economy.StateCapitalism;
        //government.status = Economy.PlannedEconomy;
        inventions.MarkInvented(InventionType.farming);
        inventions.MarkInvented(InventionType.manufactories);
        inventions.MarkInvented(InventionType.banking);
        inventions.MarkInvented(InventionType.individualRights);



    }
    public bool isInvented(InventionType type)
    {

        return inventions.isInvented(type);
    }
    public bool isInvented(Product product)
    {
        if ((product == Product.Metal || product == Product.MetallOre) && !isInvented(InventionType.metal)
            || (!product.isResource() && !isInvented(InventionType.manufactories)))
            return false;
        else
            return true;
    }
    internal float getMinSalary()
    {
        return minSalary.get();
    }
    override public string ToString()
    {
        return name;
    }
    internal void Think()
    {
        sciencePoints.add(this.getMenPopulation() * Game.defaultSciencePointMultiplier);
        if (isInvented(InventionType.banking))
            bank.PutOnDeposit(wallet, new Value(wallet.moneyIncomethisTurn.get() / 2f));
    }
    internal uint getMenPopulation()
    {
        uint result = 0;
        foreach (Province pr in ownedProvinces)
            result += pr.getMenPopulation();
        return result;
    }
    public uint FindPopulationAmountByType(PopType ipopType)
    {
        uint result = 0;
        foreach (Province pro in ownedProvinces)
            result += pro.FindPopulationAmountByType(ipopType);
        return result;
    }

}
