using Nashet.EconomicSimulation;
using System;
using System.Collections.Generic;

namespace Nashet.UnityUIUtils
{
    /// <summary>
    /// Represent UI object that can be refreshed and hidden
    /// </summary>
    public abstract class Window : Hideable, IRefreshable
    {
        /// <summary> Remembers all windows</summary>
        //protected static List<Window> allWindows = new List<Window>();
        protected void Awake()
        {
            UIEvents.SomethingVisibleToPlayerChangedInWorld += OnSomethingVisibleToPlayerChangedInWorld;
        }

        private void OnSomethingVisibleToPlayerChangedInWorld(object sender, EventArgs e)
        {
            if (isActiveAndEnabled)
                Refresh();
        }

        public abstract void Refresh();

        public override void Show()
        {
            base.Show();
            Refresh();
        }
        private void OnDestroy()
        {
            UIEvents.SomethingVisibleToPlayerChangedInWorld -= OnSomethingVisibleToPlayerChangedInWorld;
        }
    }
}