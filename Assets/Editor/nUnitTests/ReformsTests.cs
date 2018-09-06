using Nashet.EconomicSimulation;
using Nashet.EconomicSimulation.Reforms;
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
    public static IEnumerable<KeyValuePair<AbstractReform, AbstractReform>> TestableReforms
    {
        get
        {
            var list = new List<AbstractReform> { null, new Government(World.UncolonizedLand, 0) };
            foreach (var item in list)
            {
                foreach (var item2 in list)
                {
                    yield return new KeyValuePair<AbstractReform, AbstractReform>(item, item2);

                }
            }
        }
    }
    public static IEnumerable<KeyValuePair<AbstractReform, IReformValue>> TestableReforms2
    {
        get
        {
            var list = new List<AbstractReform> { null, new Government(World.UncolonizedLand, 0), new TaxationForPoor(World.UncolonizedLand, 0) };
            foreach (var arg1 in list)
            {
                if (ReferenceEquals(arg1, null))
                    yield return new KeyValuePair<AbstractReform, IReformValue>(arg1, null);
                else
                {
                    foreach (var arg2 in arg1.AllPossibleValues)
                    {
                        yield return new KeyValuePair<AbstractReform, IReformValue>(arg1, arg2);

                    }
                }
            }
        }
    }

    [TestCaseSource("TestableReforms")]
    public void ReformsEqualityTest(KeyValuePair<AbstractReform, AbstractReform> pair)
    {
        Assert.True(pair.Key == pair.Value);
    }
    [TestCaseSource("TestableReforms2")]
    public void ReformsEqualityTest2(KeyValuePair<AbstractReform, IReformValue> pair)
    {
        Assert.True(pair.Key == pair.Value);
    }
    [TestCaseSource("TestableReforms2")]
    public void ReformsEqualityTest3(KeyValuePair<IReformValue, AbstractReform> pair)
    {
        Assert.True(pair.Value == pair.Key);
    }
    [TestCaseSource("TestableReforms")]
    public void ReformsEqualityTest3(KeyValuePair<AbstractReform, AbstractReform> pair)
    {
        Assert.AreEqual(pair.Key, pair.Value);
    }
    [TestCaseSource("TestableReforms2")]
    public void ReformsEqualityTest4(KeyValuePair<AbstractReform, IReformValue> pair)
    {
        Assert.AreEqual(pair.Key, pair.Value);
    }
    [TestCaseSource("TestableReforms2")]
    public void ReformsEqualityTest5(KeyValuePair<IReformValue, AbstractReform> pair)
    {
        Assert.AreEqual(pair.Value, pair.Key);
    }
}