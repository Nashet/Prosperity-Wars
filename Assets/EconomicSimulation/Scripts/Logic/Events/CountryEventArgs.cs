using System;

namespace Nashet.EconomicSimulation
{
    public class CountryEventArgs : EventArgs
    {
        public Country NewCountry { get; protected set; }

        public CountryEventArgs(Country country)
        {
            this.NewCountry = country;
        }
    }
}