using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

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
    internal UnemploymentSubsidies unemploymentSubsidies;
    internal TaxationForPoor taxationForPoor;
    internal TaxationForRich taxationForRich;
    internal List<AbstractReform> reforms = new List<AbstractReform>();
    public Culture culture;
    Color nationalColor;
    Province capital;
    internal Army homeArmy = new Army();

    public Bank bank = new Bank();

    /// <summary>
    /// per 1000 men
    /// </summary>
    //private Value minSalary = new Value(0.5f);
    public Value sciencePoints = new Value(0f);
    internal static readonly Country NullCountry = new Country("Uncolonised lands", new Culture("Zaoteks"), new CountryWallet(0f), Color.yellow, null);
    internal List<Army> walkingArmies = new List<Army>();

    public Country(string iname, Culture iculture, CountryWallet wallet, Color color, Province capital) : base(wallet)
    {
        government = new Government(this);

        economy = new Economy(this);
        serfdom = new Serfdom(this);

        minimalWage = new MinimalWage(this);
        unemploymentSubsidies = new UnemploymentSubsidies(this);
        taxationForPoor = new TaxationForPoor(this);
        taxationForRich = new TaxationForRich(this);
        name = iname;
        allCountries.Add(this);
        //countryTax = new Procent(0.1f);
        //TaxationForPoor.ReformValue hru = taxationForPoor.getValue() as TaxationForPoor.ReformValue;
        //countryTax = hru.tax;
        culture = iculture;
        nationalColor = color;
        this.capital = capital;
        //if (!Game.devMode)
        {
            government.status = Government.Aristocracy;

            economy.status = Economy.StateCapitalism;

            inventions.MarkInvented(InventionType.farming);
            inventions.MarkInvented(InventionType.manufactories);
            inventions.MarkInvented(InventionType.banking);
            // inventions.MarkInvented(InventionType.individualRights);
            serfdom.status = Serfdom.Abolished;
        }
        // if (this != NullCountry)
        //redrawCapital();
    }

    internal void sendArmy(Army sendingArmy, Province province)
    {
        sendingArmy.send(province);
        walkingArmies.Add(new Army(sendingArmy));
        sendingArmy.clear();
    }

    internal void mobilize()
    {        
        foreach (var province in ownedProvinces)
            foreach (var pop in province.allPopUnits)
                if (pop.type.canMobilize())
                    homeArmy.add(pop.mobilize());            
    }

    internal List<Province> getNeighborProvinces()
    {
        List<Province> result = new List<Province>();
        foreach (var province in ownedProvinces)
            result.AddRange(
                province.getNeigbors(p => p.getOwner() != this && !result.Contains(p))
                );
        return result;
    }
    //todo move to Province.cs
    internal void redrawCapital()
    {
        Transform txtMeshTransform = GameObject.Instantiate(Game.r3dTextPrefab).transform;
        txtMeshTransform.SetParent(capital.gameObject.transform, false);


        Vector3 capitalTextPosition = capital.centre;
        capitalTextPosition.y += 2f;
        txtMeshTransform.position = capitalTextPosition;

        TextMesh txtMesh = txtMeshTransform.GetComponent<TextMesh>();
        txtMesh.text = this.ToString();
        if (this == Game.player)
        {
            txtMesh.color = Color.blue;
            txtMesh.fontSize += txtMesh.fontSize / 2;

        }
        else
        {
            txtMesh.color = Color.cyan; // Set the text's color to red
            txtMesh.fontSize += txtMesh.fontSize / 3;
        }
    }
    internal void moveCapitalTo(Province pro)
    {
        redrawCapital();
    }
    internal Color getColor()
    {
        return nationalColor;
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
        Dictionary<PopType, uint> divisionVotersResult = this.getYesVotesByType(reform, ref divisionPopulationResult);
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
            foreach (Province pro in this.ownedProvinces)
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
        return (minimalWage.getValue() as MinimalWage.LocalReformValue).getWage();
        //return minSalary.get();
    }
    override public string ToString()
    {
        if (this == Game.player)
            return name + " country (you are)";
        else
            return name + " country";
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
