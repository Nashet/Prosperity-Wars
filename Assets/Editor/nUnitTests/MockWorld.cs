using Nashet.EconomicSimulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MockWorld : World {

    internal void RegisterCountry(Country country)
    {
        allCountries.Add(country);
    }

    internal void Clear()
    {
        allCountries.Clear();
        allLandProvinces.Clear();
    }
}
