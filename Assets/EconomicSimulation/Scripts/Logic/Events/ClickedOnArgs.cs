using System;

namespace Nashet.EconomicSimulation
{
    public class ClickedOnArgs : EventArgs
    {
        public UIEvents.ClickTypes type { get; protected set; }

        public ClickedOnArgs(UIEvents.ClickTypes type)
        {
            this.type = type;
        }
    }
}