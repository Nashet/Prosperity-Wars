using Nashet.UnityUIUtils;
using UnityEngine.UI;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Supposed to be prefab
    /// </summary>
    public class DebugWindow : DragPanel
    {

        public static bool Exist { get; private set; }

        [SerializeField]
        private Toggle devModeToggle, logInvestmentsToggle, logMarketFailsToggle, FOWToggle
            ;

        [SerializeField]
        private Text richestAgents;

        public override void Refresh()
        {

        }

        // Use this for initialization
        private void Start()
        {
            Exist = true;
            devModeToggle.isOn = Game.devMode;
            logInvestmentsToggle.isOn = Game.logInvestments;
            logMarketFailsToggle.isOn = Game.logMarket;
            FOWToggle.isOn = Game.DrawFogOfWar;
        }

        public override void Hide()
        {
            base.Hide();
            Exist = false;
        }

        public void OnDrawFogOfWarChange(bool value)
        {
            Game.DrawFogOfWar = value;
        }
        public void OnDevModeChange(bool value)
        {
            Game.devMode = value;
        }

        public void OnLogInvestmentsChange(bool value)
        {
            Game.logInvestments = value;
        }

        public void OnLogMarketFailsChange(bool value)
        {
            Game.logMarket = value;
        }
    }
}