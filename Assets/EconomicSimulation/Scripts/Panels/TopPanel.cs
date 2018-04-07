using System.Linq;
using System.Text;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class TopPanel : Window
    {
        [SerializeField]
        private MainCamera mainCamera;

        [SerializeField]
        private Button btnPlay, btnStep, btnTrade;

        [SerializeField]
        private Text generalText, specificText;

        // Use this for initialization
        private void Awake()
        {
            MainCamera.topPanel = this;
        }

        private bool firstUpdate = true;

        private void Update()
        {
            if (firstUpdate)
                btnPlay.image.color = GUIChanger.DisabledButtonColor;
            firstUpdate = false;
        }

        public override void Refresh()
        {
            var sb = new StringBuilder();

            sb.Append("You rule: ").Append(Game.Player.FullName);

            if (!Game.Player.isAlive())
                sb.Append(" (destroyed by enemies, but could rise again)");
            sb.Append("    Month: ").Append(Date.Today);

            if (Game.Player.isAlive())
                sb.Append("   Population: ").Append(Game.Player.getFamilyPopulation().ToString("N0"))
                    .Append(" (")
                    .Append(Game.Player.getAllPopulationChanges().Where(y => y.Key == null || y.Key is Staff || (y.Key is Province && (y.Key as Province).Country != Game.Player))
                    .Sum(x=>x.Value).ToString("+0;-0;0"))
                    .Append(")");

            sb.Append("\nMoney: ").Append(Game.Player.Cash)
            .Append("   Tech points: ").Append(Game.Player.sciencePoints.get().ToString("F0"));

            if (Game.Player.isAlive())                
                sb.Append("   Loyalty: ").Append(Game.Player.GetAllPopulation().GetAverageProcent(x => x.loyalty))
                .Append("   Education: ").Append(Game.Player.GetAllPopulation().GetAverageProcent(x => x.Education));

            generalText.text = sb.ToString();
        }

        public void onTradeClick()
        {
            if (MainCamera.tradeWindow.isActiveAndEnabled)
                MainCamera.tradeWindow.Hide();
            else
                MainCamera.tradeWindow.Show();
        }

        public void onExitClick()
        {
            Application.Quit();
        }

        public void onMilitaryClick()
        {
            if (MainCamera.militaryPanel.isActiveAndEnabled)
                MainCamera.militaryPanel.Hide();
            else
                MainCamera.militaryPanel.show(null);
        }

        public void onInventionsClick()
        {
            if (MainCamera.inventionsPanel.isActiveAndEnabled)
                MainCamera.inventionsPanel.Hide();
            else
                MainCamera.inventionsPanel.Show();
        }

        public void onEnterprisesClick()
        {
            if (MainCamera.productionWindow.isActiveAndEnabled)
            {
                if (MainCamera.productionWindow.IsSelectedProvince(Game.selectedProvince) 
                    && Game.selectedProvince != null)
                {
                    MainCamera.productionWindow.ClearAllFiltres();
                }
                else
                {
                    MainCamera.productionWindow.Hide();
                }
            }
            else
            {
                MainCamera.productionWindow.ClearAllFiltres();
                MainCamera.productionWindow.Show();
            }
        }

        public void onPopulationClick()
        {
            if (MainCamera.populationPanel.isActiveAndEnabled)
                if (MainCamera.populationPanel.IsSetAnyFilter())
                    MainCamera.populationPanel.ClearAllFiltres();
                else
                    MainCamera.populationPanel.Hide();
            else
            {
                MainCamera.populationPanel.ClearAllFiltres();
                MainCamera.populationPanel.Show();
            }
        }

        public void onPoliticsClick()
        {
            if (MainCamera.politicsPanel.isActiveAndEnabled)
                MainCamera.politicsPanel.Hide();
            else
                MainCamera.politicsPanel.Show();
        }

        public void onFinanceClick()
        {
            if (MainCamera.financePanel.isActiveAndEnabled)
                MainCamera.financePanel.Hide();
            else
                MainCamera.financePanel.Show();
        }

        public void onbtnStepClick(Button button)
        {
            if (Game.isRunningSimulation())
            {
                Game.pauseSimulation();
                button.image.color = GUIChanger.DisabledButtonColor;
                Text text = button.GetComponentInChildren<Text>();
                text.text = "Pause";
            }
            else
                Game.makeOneStepSimulation();
        }

        public void onbtnPlayClick(Button button)
        {
            switchHaveToRunSimulation();
        }

        public void OnFocusOnCountryClick()
        {
            if (Game.Player != null)
                mainCamera.FocusOnProvince(Game.Player.Capital, true);
        }

        public void switchHaveToRunSimulation()
        {
            if (Game.isRunningSimulation())
                Game.pauseSimulation();
            else
                Game.continueSimulation();

            if (Game.isRunningSimulation())
            {
                //btnPlay.interactable = true;
                btnPlay.image.color = GUIChanger.ButtonsColor;
                Text text = btnPlay.GetComponentInChildren<Text>();
                text.text = "Play";
            }
            else
            {
                //btnPlay.interactable = false;
                btnPlay.image.color = GUIChanger.DisabledButtonColor;
                Text text = btnPlay.GetComponentInChildren<Text>();
                text.text = "Pause";
            }
        }
    }
}