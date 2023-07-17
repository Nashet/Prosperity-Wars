using Nashet.UISystem;
using Nashet.UnitSelection;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nashet.EconomicSimulation
{
    public class MainCamera : MonoBehaviour
    {
        [SerializeField]
        private float xzCameraSpeed = 2f;

        [SerializeField]
        private float yCameraSpeed = -55f;

        [SerializeField] protected float fogOfWarDensity;

        private float focusHeight;

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
        private static bool gameLoadingIsFinished;

        //[SerializeField]
        /// <summary>Limits simulation speed (in seconds)</summary>
        private readonly float simulationSpeedLimit = 0.10f;

        private float previousFrameTime;
        public static MainCamera Get;

        public static ISelector provinceSelector;
        public static ISelector fogOfWar;

        protected ToolTipHandler tooltip;

        private void Start()
        {
            Get = this;
            focusHeight = transform.position.z;
            provinceSelector = TimedSelectorWithMaterial.AddTo(gameObject, LinksManager.Get.ProvinceSelecionMaterial, 0);
            fogOfWar = TimedSelectorWithMaterial.AddTo(gameObject, LinksManager.Get.FogOfWarMaterial, 0);
            //
            //var window = Instantiate(LinksManager.Get.MapOptionsPrefab, LinksManager.Get.CameraLayerCanvas.transform);
            //window.GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, 150f);
            tooltip = GetComponent<ToolTipHandler>();
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            camera = Camera.main;
        }

        public void Zoom(float zMove)
        {
            var position = transform.position;
            zMove = zMove * yCameraSpeed;
            if (position.z + zMove > -40f
                || position.z + zMove < -500f)
                zMove = 0f;
            transform.Translate(0f, 0f, zMove, Space.World);
        }

        public void Move(float xMove, float yMove)
        {
            if (game == null)
                return; // map isnt done yet
            var position = transform.position;
           
            var mapBorders = game.getMapBorders();


            if (xMove * xzCameraSpeed + position.x < mapBorders.x
                || xMove * xzCameraSpeed + position.x > mapBorders.width)
                xMove = 0;

            if (yMove * xzCameraSpeed + position.y < mapBorders.y
                || yMove * xzCameraSpeed + position.y > mapBorders.height)
                yMove = 0;
            
            transform.Translate(xMove * xzCameraSpeed, yMove * xzCameraSpeed, 0f, Space.World);
        }

        private void FixedUpdate()
        {
            if (gameLoadingIsFinished)
            {
                Zoom(Input.GetAxis("Mouse ScrollWheel"));
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
			FocusOnProvince(Game.Player.Capital, false);
			loadingPanel.Hide();
			topPanel.Show();
			bottomPanel.Show();
			gameLoadingIsFinished = true;
			SelectionComponent.ArmiesGetter = (int arg) => Game.Player.AllArmiesColliders();
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
        private void UpdateMapTooltip()
        {
            if (Game.MapMode != Game.MapModes.Political
                    //&& Date.Today.isDivisible(Options.MapRedrawRate)
                    )
            {
                Game.redrawMapAccordingToMapMode();
            }

            if (!EventSystem.current.IsPointerOverGameObject())// don't force map tooltip if mouse hover UI
            {
                if (Game.MapMode == Game.MapModes.PopulationChange)
                {
                    int? meshNumber = AbstractProvince.GetIdByCollider(UnitSelection.Utils.getRayCastMeshNumber(camera));
                    var hoveredProvince = World.FindProvince(meshNumber);
                    if (hoveredProvince == null)// || hoveredProvince is Province
                    {
                        tooltip.Hide();
                    }
                    else
                    {
                        if (Game.selectedProvince == null)
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
                    int? meshNumber = AbstractProvince.GetIdByCollider(UnitSelection.Utils.getRayCastMeshNumber(camera));
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
                    int? meshNumber = AbstractProvince.GetIdByCollider(UnitSelection.Utils.getRayCastMeshNumber(camera));
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

        public static void selectProvince(int? Id)
        {
            if (!Id.HasValue || World.FindProvince(Id.Value) == Game.selectedProvince)// same province clicked, hide selection
            {
                var lastSelected = Game.selectedProvince;
                Game.selectedProvince = null;

                if (lastSelected != null)
                {
                    //lastSelected.setBorderMaterial(LinksManager.Get.defaultProvinceBorderMaterial);
                    //lastSelected.setBorderMaterials(true);
                    provinceSelector.Deselect(lastSelected.GameObject);
                }
                if (provincePanel.isActiveAndEnabled)
                    provincePanel.Hide();
            }
            else // new province selected
            {
                if (Game.selectedProvince != null)//deal with previous selection
                {
                    //Game.selectedProvince.setBorderMaterial(LinksManager.Get.defaultProvinceBorderMaterial);
                    //Game.selectedProvince.setBorderMaterials(true);
                    provinceSelector.Deselect(Game.selectedProvince.GameObject);
                }
                // freshly selected province
                Game.selectedProvince = World.FindProvince(Id.Value);
                provinceSelector.Select(Game.selectedProvince.GameObject);
                //Game.selectedProvince.setBorderMaterial(LinksManager.Get.selectedProvinceBorderMaterial);
                provincePanel.Show();
                if (Game.MapMode == Game.MapModes.Cores) //core map mode
                    Game.redrawMapAccordingToMapMode();
            }
            if (buildPanel != null && buildPanel.isActiveAndEnabled)
                buildPanel.Refresh();
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

        public void FocusOnProvince(Province province, bool select)
        {
            gameObject.transform.position = new Vector3(province.provinceMesh.Position.x, province.provinceMesh.Position.y, focusHeight);
            if (select)
                selectProvince(province.ID);
        }

        public void FocusOnPoint(Vector2 point)
        {
            gameObject.transform.position = new Vector3(point.x, point.y, focusHeight);
        }
    }
}