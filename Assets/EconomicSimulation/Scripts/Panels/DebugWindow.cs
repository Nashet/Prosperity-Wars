using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Supposed to be prefab
    /// </summary>
    public class DebugWindow : DragPanel
    {

        public static bool Exist { get; private set; }

        [SerializeField]
        private Toggle devModeToggle, logInvestmentsToggle, logMarketFailsToggle, FOWToggle;

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
            richestAgents.GetComponent<ToolTipHandler>().SetTextDynamic(() => gett(World.AllAgents.OrderByDescending(x => x.Cash.Get()).Take(10)));//.ToString("\n")
        }

        private string gett(IEnumerable<Agent> collection)
        {
            var sb = new StringBuilder();
            var allMoney = World.GetAllMoney();
            foreach (var item in collection)
            {
                sb.Append(item).Append(" ").Append(item.Cash).Append(" ").Append(new Procent(item.Cash, allMoney)).Append("\n ");
            }
            return sb.ToString();
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
        public void Test1()
        {
            Debug.Log("Test1 started");
            var before = System.DateTime.Now;

            for (int i = 0; i < 200; i++)
            {
                // World.AllMarkets.PerformAction(x => x.ForceDSBRecalculation());
            }
            var tookTime = System.DateTime.Now - before;

            Debug.Log("Test1 took " + tookTime.Milliseconds / 1000f);
        }
        public void Test2()
        {
            Debug.Log("Test2 started");
            var before = System.DateTime.Now;

            for (int i = 0; i < 200; i++)
            {
                // World.AllMarkets.PerformAction(x => x.ForceDSBRecalculation2());
            }
            var tookTime = System.DateTime.Now - before;

            Debug.Log("Test2 took " + tookTime.Milliseconds / 1000f);
        }
    }
}