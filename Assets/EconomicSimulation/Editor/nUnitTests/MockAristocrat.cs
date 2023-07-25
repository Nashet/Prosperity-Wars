using Nashet.EconomicSimulation;
using Nashet.ValueSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MockAristocrats : Aristocrats
{
    public MockAristocrats(int iamount, Culture iculture, Province where) : base(iamount, iculture, where)
    {
    }    
    new public Money Cash {get {return cash;} set { cash.Set( value); } }
}
