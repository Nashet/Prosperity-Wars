
using Nashet.Utils;
using System;

namespace Nashet.EconomicSimulation
{
    public class UIEvents : Component<Country>
    {
        public UIEvents(Country owner) : base(owner)
        {           
        }

        public event EventHandler WantedToSeeDiplomacy;
        public virtual void OnWantedToSeeDiplomacy(CountryEventArgs e)
        {
            EventHandler handler = WantedToSeeDiplomacy;
            if (handler != null)
            {
                //var e = new CountryEventArgs(owner);
                handler(this, e);
            }
        }

        public event EventHandler WantedToSeeInventions;
        public virtual void OnWantedToSeeInventions(InventionEventArgs e)
        {
            EventHandler handler = WantedToSeeInventions;
            if (handler != null)
            {
                //var e = new CountryEventArgs(owner);
                handler(this, e);
            }
        }

        public event EventHandler ChangedCountry;
        public virtual void OnChangedCountry(CountryEventArgs e)
        {
            EventHandler handler = ChangedCountry;
            if (handler != null)
            {
                //var e = new CountryEventArgs(owner);
                handler(this, e);
            }
        }
    }
    public class CountryEventArgs : EventArgs
    {
        public Country Country { get; protected set; }

        public CountryEventArgs(Country country)
        {
            this.Country = country;
        }
    }
    public class InventionEventArgs : EventArgs
    {
        public Invention Invention { get; protected set; }

        public InventionEventArgs(Invention invention)
        {
            this.Invention = invention;
        }
    }
}