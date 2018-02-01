using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    public class World : MonoBehaviour
    {
        static public IEnumerable<IInvestable> GetAllAllowedInvestments(Country includingCountry)
        {
            var countriesAllowingInvestments = Country.getAllExisting().Where(x => x.economy.getTypedValue().AllowForeighnIvestments || x == includingCountry);
            foreach (var country in countriesAllowingInvestments)
                foreach (var item in country.GetAllInvestmentProjects())
                    yield return item;
        }

    }
}