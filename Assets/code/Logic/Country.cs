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
    //public Procent countryTax;
    public Procent aristocrstTax;//= new Procent(0.10f);
    public InventionsList inventions = new InventionsList();

    internal Government government;
    internal Economy economy;
    internal Serfdom serfdom;
    internal MinimalWage minimalWage;
    internal TaxationForPoor taxationForPoor;
    internal TaxationForRich taxationForRich;
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
        taxationForRich = new TaxationForRich(this);
        name = iname;
        allCountries.Add(this);
        //countryTax = new Procent(0.1f);
        //TaxationForPoor.ReformValue hru = taxationForPoor.getValue() as TaxationForPoor.ReformValue;
        //countryTax = hru.tax;
        culture = iculture;

        if (!Game.devMode)
        {
            government.status = Government.Aristocracy;

            economy.status = Economy.StateCapitalism;

            inventions.MarkInvented(InventionType.farming);
            inventions.MarkInvented(InventionType.manufactories);
            inventions.MarkInvented(InventionType.banking);
            // inventions.MarkInvented(InventionType.individualRights);
            serfdom.status = Serfdom.Abolished;
        }
        
    }

    internal Procent getYesVotes(AbstractReformValue reform, ref Procent procentPopulationSayedYes)
    {
        // calc how much of population wants selected reform
        uint totalPopulation = this.getMenPopulation();
        uint votingPopulation = 0;
        uint populationSayedYes = 0;
        uint votersSayedYes = 0;
        Procent procentVotersSayedYes = new Procent(0);
        //Procent procentPopulationSayedYes = new Procent(0f);
        foreach (Province pro in ownedProvinces)
            foreach (PopUnit pop in pro.allPopUnits)
            {
                if (pop.canVote())
                {
                    if (pop.getSayingYes(reform))
                    {
                        votersSayedYes += pop.population;
                        populationSayedYes += pop.population;
                    }
                    votingPopulation += pop.population;
                }
                else
                {
                    if (pop.getSayingYes(reform))
                        populationSayedYes += pop.population;
                }
            }
        if (totalPopulation != 0)
            procentPopulationSayedYes.set((float)populationSayedYes / totalPopulation);
        else
            procentPopulationSayedYes.set(0);

        if (votingPopulation == 0)
            procentVotersSayedYes.set(0);
        else
            procentVotersSayedYes.set((float)votersSayedYes / votingPopulation);
        return procentVotersSayedYes;
    }
    /// <summary>
    /// Not finished, dont use it
    /// </summary>
    /// <param name="reform"></param>   
    internal Procent getYesVotes2(AbstractReformValue reform, ref Procent procentPopulationSayedYes)
    {
        uint totalPopulation = this.getMenPopulation();
        uint votingPopulation = 0;
        uint populationSayedYes = 0;
        uint votersSayedYes = 0;
        Procent procentVotersSayedYes = new Procent(0f);
        Dictionary<PopType, uint> divisionPopulationResult = new Dictionary<PopType, uint>();
        Dictionary<PopType, uint> divisionVotersResult = Game.player.getYesVotesByType(reform, ref divisionPopulationResult);
        foreach (KeyValuePair<PopType, uint> next in divisionVotersResult)
            votersSayedYes += next.Value;

        if (totalPopulation != 0)
            procentPopulationSayedYes.set((float)populationSayedYes / totalPopulation);
        else
            procentPopulationSayedYes.set(0);

        if (votingPopulation == 0)
            procentVotersSayedYes.set(0);
        else
            procentVotersSayedYes.set((float)votersSayedYes / votingPopulation);
        return procentVotersSayedYes;
    }
    internal Dictionary<PopType, uint> getYesVotesByType(AbstractReformValue reform, ref Dictionary<PopType, uint> divisionPopulationResult)
    {  // division by pop types
        Dictionary<PopType, uint> divisionVotersResult = new Dictionary<PopType, uint>();
        foreach (PopType type in PopType.allPopTypes)
        {
            divisionVotersResult.Add(type, 0);
            divisionPopulationResult.Add(type, 0);
            foreach (Province pro in Game.player.ownedProvinces)
            {
                var popList = pro.FindAllPopUnits(type);
                foreach (PopUnit pop in popList)
                    if (pop.getSayingYes(reform))
                    {
                        divisionPopulationResult[type] += pop.population;
                        if (pop.canVote())
                            divisionVotersResult[type] += pop.population;
                    }
            }
        }
        return divisionVotersResult;
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
        if (isInvented(InventionType.banking) && wallet.haveMoney.get() <= 1000f)
            bank.PutOnDeposit(wallet, new Value(wallet.moneyIncomethisTurn.get() / 2f));
        else
            bank.PutOnDeposit(wallet, new Value(wallet.moneyIncomethisTurn.get()));
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
