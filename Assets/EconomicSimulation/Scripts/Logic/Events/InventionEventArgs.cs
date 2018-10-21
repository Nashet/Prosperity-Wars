using System;

namespace Nashet.EconomicSimulation
{
    public class InventionEventArgs : ClickedOnArgs
    {
        public Invention Invention { get; protected set; }

        public InventionEventArgs(Invention invention) : base(UIEvents.ClickTypes.Inventions)
        {
            this.Invention = invention;
        }
    }
}