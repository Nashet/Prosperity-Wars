using Nashet.GameplayControllers;
using Nashet.Map.GameplayControllers;
using Nashet.Map.Utils;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class TopPanel : Window
    {
        [SerializeField]
        private MainCamera mainCamera;

        [SerializeField]
        private Button btnPlay, btnStep, btnTrade, financeButton;

        [SerializeField]
        private Text generalText;

        [SerializeField]
        private World world;

        [SerializeField]
        private RawImage flag;

		[SerializeField]
		private ProvinceSelectionHelper provinceSelectionHelper;

        [SerializeField]
        private CameraController cameraController;

		private bool isSetFlagTexture;
        private bool firstUpdate = true;
        protected ISelector buttonSelector;

        // Use this for initialization
        new private void Awake()
        {
            base.Awake();
            MainCamera.topPanel = this;
            buttonSelector = new ColorSelector(Color.red); //UISelector.AddTo(this, LinksManager.Get.UISelectedMaterial,);
            UIEvents.PlayerChangedCountry += PlayerChangedCountryHandler;
        }

        private void Update()
        {
            if (firstUpdate)
                btnPlay.image.color = GUIChanger.DisabledButtonColor;
            if (!isSetFlagTexture && Game.Player != null && Game.Player.Flag != null)
            {
                isSetFlagTexture = true;
                SetFlag(Game.Player.Flag);
            }
            firstUpdate = false;
        }

        public override void Refresh()
        {
            var sb = new StringBuilder();

            sb.Append("You rule: ").Append(Game.Player.FullName);

            if (!Game.Player.IsAlive)
                sb.Append(" (destroyed by enemies, but could rise again)");
            sb.Append("    Year: ").Append(Date.Today);

            if (Game.Player.IsAlive)
                sb.Append("   Population: ").Append(Game.Player.Provinces.getFamilyPopulation().ToString("N0"))
                    .Append(" (")
                    .Append(Game.Player.Provinces.AllPopsChanges.Where(y => y.Key == null || y.Key is Staff || (y.Key is Province && (y.Key as Province).Country != Game.Player))
                    .Sum(x => x.Value).ToString("+0;-0;0"))
                    .Append(")");

            sb.Append("\nMoney: ").Append(Game.Player.Cash)
            .Append("   Tech points: ").Append(Game.Player.Science.Points.ToString("F0"));

            if (Game.Player.IsAlive)
                sb.Append("   Loyalty: ").Append(Game.Player.Provinces.AllPops.GetAverageProcent(x => x.loyalty))
                .Append("   Education: ").Append(Game.Player.Provinces.AllPops.GetAverageProcent(x => x.Education));

            if (Game.Player != null)
                if (Game.Player.FailedPayments.Income.isNotZero())
                {
                    buttonSelector.Select(financeButton.gameObject);
//#if !UNITY_WEBGL //allegebly causes out of memory exceptions in webgl
                    financeButton.GetComponent<ToolTipHandler>().RemoveTextStartingWith("\nCan't");
                    financeButton.GetComponent<ToolTipHandler>().AddText("\nCan't pay for:" + Game.Player.FailedPayments.GetIncomeText());
//#endif
                }
                else
                {
                    buttonSelector.Deselect(financeButton.gameObject);
                    financeButton.GetComponent<ToolTipHandler>().RemoveTextStartingWith("\nCan't");
                }

            generalText.text = sb.ToString();
        }

        private void PlayerChangedCountryHandler(object sender, EventArgs e)
        {
            var isCountryArguments = e as CountryEventArgs;
            if (isCountryArguments != null)
            {
                SetFlag(isCountryArguments.NewCountry.Flag);
            }
        }

        private void SetFlag(Texture2D newFlag)
        {
            flag.texture = newFlag;
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
            Game.Player.events.RiseClickedOn(new InventionEventArgs(null));
            //    if (MainCamera.inventionsPanel.isActiveAndEnabled)
            //        MainCamera.inventionsPanel.Hide();
            //    else
            //        MainCamera.inventionsPanel.Show();
        }

        public void onEnterprisesClick()
        {
            if (MainCamera.productionWindow.isActiveAndEnabled)
            {
                if (MainCamera.productionWindow.IsSelectedProvince(provinceSelectionHelper.selectedProvince)
                    && provinceSelectionHelper.selectedProvince != null)
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
            if (world.IsRunning)
            {
                switchHaveToRunSimulation();
            }
            else
                world.MakeOneStepSimulation();
        }

        public void onbtnPlayClick(Button button)
        {
            switchHaveToRunSimulation();
        }

        public void OnFocusOnCountryClick()
        {
            if (Game.Player != null)
				cameraController.FocusOnProvince(Game.Player.Capital.provinceMesh, true);
        }

        public void switchHaveToRunSimulation()
        {
            if (world.IsRunning)
                world.PauseSimulation();
            else
                world.ResumeSimulation();

            if (world.IsRunning)
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