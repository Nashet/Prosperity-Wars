using System;

namespace Nashet.EconomicSimulation
{
    public class InventionEventArgs : EventArgs
    {
        public Invention Invention { get; protected set; }

        public InventionEventArgs(Invention invention)
        {
            this.Invention = invention;
        }
    }
}