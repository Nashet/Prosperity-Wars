namespace Nashet.EconomicSimulation
{
    public class CountryEventArgs : ClickedOnArgs
    {
        public Country NewCountry { get; protected set; }

        public CountryEventArgs(Country country) : base(UIEvents.ClickTypes.Diplomacy)
        {
            this.NewCountry = country;
        }
    }
}