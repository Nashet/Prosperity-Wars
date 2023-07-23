using Nashet.GameplayControllers;
using Nashet.Map.GameplayControllers;
using Nashet.Map.GameplayView;
using Nashet.Map.Utils;
using Nashet.MapMeshes;
using Nashet.UISystem;
using Nashet.UnitSelection;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nashet.EconomicSimulation
{

	public class MainCamera : MonoBehaviour
    {
        [SerializeField] protected float fogOfWarDensity;
        [SerializeField] private CameraView cameraView;
        [SerializeField] ProvinceSelectionHelper provinceSelectionHelper;
        [SerializeField] public CameraController cameraController;


		public static TopPanel topPanel;
        public static ProvincePanel provincePanel;
        public static PopulationPanel populationPanel;
        public static PopUnitPanel popUnitPanel;
        // public static DiplomacyPanel diplomacyPanel;
        public static TradeWindow tradeWindow;
        public static ProductionWindow productionWindow;
        public static FactoryPanel factoryPanel;
        public static GoodsPanel goodsPanel;
        public static InventionsPanel inventionsPanel;
        public static BuildPanel buildPanel;
        public static PoliticsPanel politicsPanel;
        public static FinancePanel financePanel;
        public static MilitaryPanel militaryPanel;

        public static LoadingPanel loadingPanel;
        public static BottomPanel bottomPanel;
        public static StatisticsPanel StatisticPanel;


        private Game game;
        private new Camera camera;
        public static bool gameLoadingIsFinished; //todo refactor

        //[SerializeField]
        /// <summary>Limits simulation speed (in seconds)</summary>
        private readonly float simulationSpeedLimit = 0.10f;

        private float previousFrameTime;
        public static MainCamera Get;

       
        public static ISelector fogOfWar;

        protected ToolTipHandler tooltip;

        private void Start()
        {
            Get = this;
            
            
            fogOfWar = TimedSelectorWithMaterial.AddTo(gameObject, LinksManager.Get.FogOfWarMaterial, 0);
            //
            //var window = Instantiate(LinksManager.Get.MapOptionsPrefab, LinksManager.Get.CameraLayerCanvas.transform);
            //window.GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, 150f);
            tooltip = GetComponent<ToolTipHandler>();
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            camera = Camera.main;
			provinceSelectionHelper.ProvinceSelected += ProvinceSelectedHandler;
		}

		private void OnDestroy()
		{
			provinceSelectionHelper.ProvinceSelected -= ProvinceSelectedHandler;
		}

		private void ProvinceSelectedHandler(Province province)
		{
            if (province != null)
            {
                if (Game.MapMode == Game.MapModes.Cores) //core map mode
                {
                    redrawMapAccordingToMapMode(province);
                }
			}
		}

		private void NonUnityLoading()
        {
            Application.runInBackground = true;
            game = new Game(MapOptions.MapImage);
			
#if UNITY_WEBGL
			game.InitializeNonUnityData(); // non multi-threading version
#else
            game.Start(); //initialize is here
#endif
        }
        private void UnityThreadLoading()
		{
			Game.setUnityMeshes();
		}

		private void SetUI()
		{
			cameraController.FocusOnProvince(Game.Player.Capital.provinceMesh, false);
			loadingPanel.Hide();
			topPanel.Show();
			bottomPanel.Show();
			gameLoadingIsFinished = true;
			SelectionComponent.ArmiesGetter = (int arg) => Game.Player.AllArmiesColliders();
			cameraView.Set(game.getMapBorders());
		}

		// Update is called once per frame
		private void Update()
        {
            if (!MapOptions.MadeChoise && !Game.devMode)
                return;

            //starts loading thread
            if (game == null)
            {
                NonUnityLoading();
            }
            else
#if UNITY_WEBGL
            if (!gameLoadingIsFinished)  // non multi-threading version
#else
            if (game.IsDone && !gameLoadingIsFinished) //multi-threading version
#endif
			{
                UnityThreadLoading();
				SetUI();
			}
#if !UNITY_WEBGL
                //else // multi-threading
                //    loadingPanel.updateStatus(game.getStatus()); //loads too quickly to show that
#endif
            if (gameLoadingIsFinished)
			{
				EveryTickWork();
			}
		}

		private void EveryTickWork()
		{
			UpdateMapTooltip();

			if (World.Get.IsRunning && !MessagePanel.IsOpenAny())
			{
				if (Game.isPlayerSurrended() || !Game.Player.IsAlive || Time.time - previousFrameTime >= simulationSpeedLimit)
				{
					World.simulate();
					//Unit.RedrawAll();

					previousFrameTime = Time.time;
					//refreshAllActive();
					UIEvents.RiseSomethingVisibleToPlayerChangedInWorld(EventArgs.Empty, this);
				}
			}

			if (Game.provincesToRedrawArmies.Count > 0)
			{
				Unit.RedrawAll();
			}

			if (Input.GetKeyDown(KeyCode.Return)) // enter key
				CloseToppestPanel();

			DrawFogOfWar();

			//if (Message.HasUnshownMessages())
			//    MessagePanel.Instance.ShowMessageBox(LinksManager.Get.CameraLayerCanvas, this);
		}

		protected void DrawFogOfWar()
        {
            if (Game.devMode == false)
            {
                Game.playerVisibleProvinces.Clear();
                Game.playerVisibleProvinces.AddRange(Game.Player.AllProvinces);
                Game.Player.Provinces.AllNeighborProvinces().Distinct().PerformAction(
                    x => Game.playerVisibleProvinces.Add(x));

                if (Game.DrawFogOfWar && Game.MapMode == Game.MapModes.Political)
                {
                    World.AllProvinces.PerformAction(
                        x => x.provinceMesh.SetColor(x.ProvinceColor * fogOfWarDensity)
                        //x => fogOfWar.Select(x.GameObject)
                        );
                    Game.playerVisibleProvinces.PerformAction(x =>
                    //fogOfWar.Deselect(x.GameObject)
                    x.provinceMesh.SetColor(x.ProvinceColor)
                    );
                }



                foreach (var army in World.AllArmies())
                {
                    if (Game.playerVisibleProvinces.Contains(army.Province))
                    {
                        army.unit.Show();
                        army.unit.unitPanel.Show();
                    }
                    else
                    {
                        army.unit.Hide();
                        army.unit.unitPanel.Hide();
                    }
                }
            }
            else
            {
                foreach (var army in World.AllArmies())
                {
                    army.unit.Show();
                    army.unit.unitPanel.Show();
                }
            }
            if (!Game.DrawFogOfWar)
            {
                World.AllProvinces.PerformAction(x =>
                //fogOfWar.Deselect(x.GameObject)
                x.provinceMesh.SetColor(x.ProvinceColor)
                );
            }
        }

		//case 0: //political mode
		//    case 1: //culture mode
		//    case 2: //cores mode
		//    case 3: //resource mode
		//    case 4: //population change mode
		//    case 5: //population density mode
		//    case 6: //prosperity map
		public void redrawMapAccordingToMapMode(Province selectedProvince)
		{
			foreach (var item in World.AllProvinces)
				item.SetColorAccordingToMapMode(selectedProvince);
		}

		private void UpdateMapTooltip()
        {
            if (Game.MapMode != Game.MapModes.Political
                    //&& Date.Today.isDivisible(Options.MapRedrawRate)
                    )
            {
                redrawMapAccordingToMapMode(provinceSelectionHelper.selectedProvince);
            }

            if (!EventSystem.current.IsPointerOverGameObject())// don't force map tooltip if mouse hover UI
            {
                if (Game.MapMode == Game.MapModes.PopulationChange)
                {
                    int? meshNumber = ProvinceMesh.GetIdByCollider(UnitSelectionUtils.getRayCastMeshNumber(camera));
                    var hoveredProvince = World.FindProvince(meshNumber);
                    if (hoveredProvince == null)// || hoveredProvince is Province
                    {
                        tooltip.Hide();
                    }
                    else
                    {
                        if (provinceSelectionHelper.selectedProvince == null)
                            tooltip.SetTextDynamic(() =>
                           "Country: " + hoveredProvince.Country + ", population (men): " + hoveredProvince.Country.Provinces.AllPops.Sum(x => x.population.Get())
                           + "\n" + hoveredProvince.Country.Provinces.AllPopsChanges
                           .Where(y => y.Key == null || y.Key is Staff || (y.Key is Province && (y.Key as Province).Country != hoveredProvince.Country))
                           .ToString("\n", "Total change: "));
                        else
                            tooltip.SetTextDynamic(() =>
                           "Province: " + hoveredProvince.ShortName + ", population (men): " + hoveredProvince.AllPops.Sum(x => x.population.Get())
                           + "\n" + hoveredProvince.AllPopsChanges
                           .Where(y => y.Key == null || y.Key is Province || y.Key is Staff)
                           .ToString("\n", "Total change: ")
                            );
                        tooltip.Show();
                    }
                }
                else if (Game.MapMode == Game.MapModes.PopulationDensity)
                {
                    int? meshNumber = ProvinceMesh.GetIdByCollider(UnitSelectionUtils.getRayCastMeshNumber(camera));
                    var hoveredProvince = World.FindProvince(meshNumber);
                    if (hoveredProvince == null)
                        tooltip.Hide();
                    else
                    {
                        tooltip.SetTextDynamic(() =>
                        "Province: " + hoveredProvince.ShortName + ", population (men): " + hoveredProvince.AllPops.Sum(x => x.population.Get())
                        + "\nChange: " + hoveredProvince.AllPopsChanges
                        .Where(y => y.Key == null || y.Key is Province || y.Key is Staff).Sum(x => x.Value)
                        + "\nOverpopulation: " + hoveredProvince.GetOverpopulation()
                         );
                        tooltip.Show();
                    }
                }
                else if (Game.MapMode == Game.MapModes.Prosperity) //prosperity wars
                {
                    int? meshNumber = ProvinceMesh.GetIdByCollider(UnitSelectionUtils.getRayCastMeshNumber(camera));
                    var hoveredProvince = World.FindProvince(meshNumber);
                    if (hoveredProvince == null)
                        tooltip.Hide();
                    else
                    {
                        tooltip.SetTextDynamic(() =>
                       "Province: " + hoveredProvince.ShortName + ", population (men): " + hoveredProvince.AllPops.Sum(x => x.population.Get())
                       + "\nAv. needs fulfilling: " + hoveredProvince.AllPops.GetAverageProcent(x => x.needsFulfilled));
                        tooltip.Show();
                    }
                }
            }
            else
            {
                if (tooltip.IsInside()) // hide only if it's that tooltip is shown
                    tooltip.Hide();
            }
        }

        private void CloseToppestPanel()
        {
            //canvas.GetComponentInChildren<DragPanel>();
            var lastChild = LinksManager.Get.CameraLayerCanvas.transform.GetChild(LinksManager.Get.CameraLayerCanvas.transform.childCount - 1);
            var panel = lastChild.GetComponent<DragPanel>();
            if (panel != null)
                panel.Hide();
            else
            {
                lastChild.SetAsFirstSibling();
                CloseToppestPanel();
            }
        }
    }
}