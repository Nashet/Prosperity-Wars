using Nashet.EconomicSimulation;
using Nashet.ValueSpace;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ReformsTests
{
    [SetUp]
    public void CommonSetup()
    {
    }
    public static IEnumerable<KeyValuePair<AbstrRefrm, AbstrRefrm>> TestableReforms
    {
        get
        {
            var list = new List<AbstrRefrm> { null, new Gov(World.UncolonizedLand), new TaxRerfr("Taxation for poor", "", World.UncolonizedLand, new List<IReformValue> { new Procent(0f), new Procent(0.5f), new Procent(1f) }) };
            foreach (var item in list)
            {
                foreach (var item2 in list)
                {
                    yield return new KeyValuePair<AbstrRefrm, AbstrRefrm>(item, item2);

                }
            }
        }
    }
    public static IEnumerable<KeyValuePair<AbstrRefrm, IReformValue>> TestableReforms2
    {
        get
        {
            var list = new List<AbstrRefrm> { null, new Gov(World.UncolonizedLand), new TaxRerfr("Taxation for poor", "", World.UncolonizedLand, new List<IReformValue> { new Procent(0f), new Procent(0.5f), new Procent(1f) }) };
            foreach (var arg1 in list)
            {
                if (ReferenceEquals(arg1, null))
                    yield return new KeyValuePair<AbstrRefrm, IReformValue>(arg1, null);
                else
                {
                    foreach (var arg2 in arg1.PossibleValues)
                    {
                        yield return new KeyValuePair<AbstrRefrm, IReformValue>(arg1, arg2);

                    }
                    //yield return new KeyValuePair<AbstrRefrm, IReformValue>(null, null);
                }
            }
        }
    }

    [TestCaseSource("TestableReforms")]
    public void ReformsEqualityTest(KeyValuePair<AbstrRefrm, AbstrRefrm> pair)
    {
        Assert.True(pair.Key == pair.Value);
    }
    [TestCaseSource("TestableReforms2")]
    public void ReformsEqualityTest2(KeyValuePair<AbstrRefrm, IReformValue> pair)
    {
        Assert.True(pair.Key == pair.Value);
    }
    [TestCaseSource("TestableReforms2")]
    public void ReformsEqualityTest3(KeyValuePair<IReformValue, AbstrRefrm> pair)
    {
        Assert.True(pair.Value == pair.Key);
    }
    [TestCaseSource("TestableReforms")]
    public void ReformsEqualityTest3(KeyValuePair<AbstrRefrm, AbstrRefrm> pair)
    {
        Assert.AreEqual(pair.Key, pair.Value);
    }
    [TestCaseSource("TestableReforms2")]
    public void ReformsEqualityTest4(KeyValuePair<AbstrRefrm, IReformValue> pair)
    {
        Assert.AreEqual(pair.Key, pair.Value);
    }
    [TestCaseSource("TestableReforms2")]
    public void ReformsEqualityTest5(KeyValuePair<IReformValue, AbstrRefrm> pair)
    {
        Assert.AreEqual(pair.Value, pair.Key);
    }
}