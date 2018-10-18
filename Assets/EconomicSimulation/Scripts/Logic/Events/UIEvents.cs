
using Nashet.Utils;
using System;

namespace Nashet.EconomicSimulation
{
    public class UIEvents : Component<Country>
    {
        public UIEvents(Country owner) : base(owner)
        {           
        }

        public event EventHandler ClickedOnDiplomacy;
        public virtual void RiseClickedOnDiplomacy(CountryEventArgs e)
        {
            ClickedOnDiplomacy?.Invoke(this, e);
        }

        public event EventHandler ClickedOnInventions;
        public virtual void RiseClickedOnInventions(InventionEventArgs e)
        {
            ClickedOnInventions?.Invoke(this, e);
        }

        public event EventHandler PlayerChangedCountry;
        public virtual void RiseChangedCountry(CountryEventArgs e)
        {
            PlayerChangedCountry?.Invoke(this, e);
            RiseSomethingVisibleToPlayerChangedInWorld(e, this);
        }

        public static event EventHandler SomethingVisibleToPlayerChangedInWorld;
        public static void RiseSomethingVisibleToPlayerChangedInWorld(EventArgs e, object sender)
        {
            SomethingVisibleToPlayerChangedInWorld?.Invoke(sender, e);
        }
    }
}