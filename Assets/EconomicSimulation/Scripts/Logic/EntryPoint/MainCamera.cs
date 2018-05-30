using System;
using System.Linq;
using Nashet.UnityUIUtils;
using Nashet.Utils;
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


        private float focusHeight;

        public static TopPanel topPanel;
        public static ProvincePanel provincePanel;
        public static PopulationPanel populationPanel;
        public static PopUnitPanel popUnitPanel;
        public static DiplomacyPanel diplomacyPanel;
        internal static TradeWindow tradeWindow;
        internal static ProductionWindow productionWindow;
        internal static FactoryPanel factoryPanel;
        internal static GoodsPanel goodsPanel;
        internal static InventionsPanel inventionsPanel;
        internal static BuildPanel buildPanel;
        internal static PoliticsPanel politicsPanel;
        internal static FinancePanel financePanel;
        internal static MilitaryPanel militaryPanel;

        internal static LoadingPanel loadingPanel;
        internal static BottomPanel bottomPanel;
        internal static StatisticsPanel StatisticPanel;


        private Game game;
        private static bool gameLoadingIsFinished;

        //[SerializeField]
        /// <summary>Limits simulation speed (in seconds)</summary>
        private readonly float simulationSpeedLimit = 0.10f;

        private float previousFrameTime;
        public static MainCamera Get;

        private void Start()
        {
            Get = this;
            focusHeight = transform.position.z;
        }

        public void Move(float xMove, float zMove, float yMove)
        {
            var position = transform.position;
            var mapBorders = game.getMapBorders();


            if (xMove * xzCameraSpeed + position.x < mapBorders.x
                || xMove * xzCameraSpeed + position.x > mapBorders.width)
                xMove = 0;

            if (yMove * xzCameraSpeed + position.y < mapBorders.y
                || yMove * xzCameraSpeed + position.y > mapBorders.height)
                yMove = 0;

            zMove = zMove * yCameraSpeed;
            if (position.z + zMove > -40f
                || position.z + zMove < -500f)
                zMove = 0f;
            transform.Translate(xMove * xzCameraSpeed, yMove * xzCameraSpeed, zMove, Space.World);
        }

        private void FixedUpdate()
        {
            if (gameLoadingIsFinished)
            {
                Move(0f, Input.GetAxis("Mouse ScrollWheel"), 0f);
            }
        }

        private void LoadGame()
        {
            Application.runInBackground = true;
            game = new Game();
#if UNITY_WEBGL
            game.InitializeNonUnityData(); // non multi-threading
#else
            game.Start(); //initialize is here
#endif
        }
        private void OnGameLoaded()
        {
            Game.setUnityAPI();


            FocusOnProvince(Game.Player.Capital, false);
            loadingPanel.Hide();
            topPanel.Show();
            bottomPanel.Show();
            gameLoadingIsFinished = true;
        }
        // Update is called once per frame
        private void Update()
        {
            //starts loading thread
            if (game == null)// && Input.GetKeyUp(KeyCode.Backspace))
            {
                LoadGame();
            }
            else
#if UNITY_WEBGL
            if (!gameLoadingIsFinished)  // non multi-threading
#else
            if (game.IsDone && !gameLoadingIsFinished)
#endif
            {
                OnGameLoaded();
            }
#if !UNITY_WEBGL
                else // multi-threading
                    loadingPanel.updateStatus(game.getStatus());
#endif
            if (gameLoadingIsFinished)
            {
                RefreshMap();

                if (World.Get.IsRunning && !MessagePanel.IsOpenAny())
                {
                    if (Game.isPlayerSurrended() || !Game.Player.isAlive() || Time.time - previousFrameTime >= simulationSpeedLimit)
                    {
                        World.simulate();
                        //Unit.RedrawAll();

                        previousFrameTime = Time.time;
                        refreshAllActive();
                    }
                }
                if (Game.armiesToRedraw.Count > 0)
                    Unit.RedrawAll();
                if (Message.HasUnshownMessages())
                    MessagePanel.showMessageBox(LinksManager.Get.CameraLayerCanvas, this);

            }
        }

        private void RefreshMap()
        {
            if (Game.getMapMode() != 0
                    //&& Date.Today.isDivisible(Options.MapRedrawRate)
                    )
                Game.redrawMapAccordingToMapMode(Game.getMapMode());


            if (Game.getMapMode() == 4)
            {
                int meshNumber = Province.FindByCollider(SelectionComponent.getRayCastMeshNumber());
                var hoveredProvince = World.FindProvince(meshNumber);
                if (hoveredProvince == null)
                    GetComponent<ToolTipHandler>().Hide();
                else
                {
                    if (Game.selectedProvince == null)
                        GetComponent<ToolTipHandler>().SetTextDynamic(() =>
                        "Country: " + hoveredProvince.Country + ", population (men): " + hoveredProvince.Country.GetAllPopulation().Sum(x => x.population.Get())
                        + "\n" + hoveredProvince.Country.getAllPopulationChanges()
                        .Where(y => y.Key == null || y.Key is Staff || (y.Key is Province && (y.Key as Province).Country != hoveredProvince.Country))
                        .getString("\n", "Total change: "));
                    else
                        GetComponent<ToolTipHandler>().SetTextDynamic(() =>
                        "Province: " + hoveredProvince.ShortName + ", population (men): " + hoveredProvince.GetAllPopulation().Sum(x => x.population.Get())
                        + "\n" + hoveredProvince.getAllPopulationChanges()
                        .Where(y => y.Key == null || y.Key is Province || y.Key is Staff)
                        .getString("\n", "Total change: ")
                        );
                    GetComponent<ToolTipHandler>().Show();
                }
            }
            else if (Game.getMapMode() == 5)
            {
                int meshNumber = Province.FindByCollider(SelectionComponent.getRayCastMeshNumber());
                var hoveredProvince = World.FindProvince(meshNumber);
                if (hoveredProvince == null)
                    GetComponent<ToolTipHandler>().Hide();
                else
                {
                    GetComponent<ToolTipHandler>().SetTextDynamic(() =>
                        "Province: " + hoveredProvince.ShortName + ", population (men): " + hoveredProvince.GetAllPopulation().Sum(x => x.population.Get())
                        + "\nChange: " + hoveredProvince.getAllPopulationChanges()
                        .Where(y => y.Key == null || y.Key is Province || y.Key is Staff).Sum(x => x.Value)
                        + "\nOverpopulation: " + hoveredProvince.GetOverpopulation()
                        );
                    GetComponent<ToolTipHandler>().Show();
                }
            }
            else if (Game.getMapMode() == 6) //prosperity wars
            {
                int meshNumber = Province.FindByCollider(SelectionComponent.getRayCastMeshNumber());
                var hoveredProvince = World.FindProvince(meshNumber);
                if (hoveredProvince == null)
                    GetComponent<ToolTipHandler>().Hide();
                else
                {
                    GetComponent<ToolTipHandler>().SetTextDynamic(() =>
                        "Province: " + hoveredProvince.ShortName + ", population (men): " + hoveredProvince.GetAllPopulation().Sum(x => x.population.Get())
                        + "\nAv. needs fulfilling: " + hoveredProvince.GetAllPopulation().GetAverageProcent(x => x.needsFulfilled));
                    GetComponent<ToolTipHandler>().Show();
                }
            }
        }


        internal static void refreshAllActive()
        {
            if (topPanel.isActiveAndEnabled) topPanel.Refresh();
            if (populationPanel.isActiveAndEnabled) populationPanel.Refresh();
            if (tradeWindow.isActiveAndEnabled) tradeWindow.Refresh();
            if (factoryPanel.isActiveAndEnabled) factoryPanel.Refresh();
            if (productionWindow.isActiveAndEnabled) productionWindow.Refresh();
            if (goodsPanel.isActiveAndEnabled) goodsPanel.Refresh();
            if (inventionsPanel.isActiveAndEnabled) inventionsPanel.Refresh();
            if (buildPanel.isActiveAndEnabled) buildPanel.Refresh();
            if (politicsPanel.isActiveAndEnabled) politicsPanel.Refresh();
            if (financePanel.isActiveAndEnabled) financePanel.Refresh();
            if (militaryPanel.isActiveAndEnabled) militaryPanel.Refresh();
            if (diplomacyPanel.isActiveAndEnabled) diplomacyPanel.Refresh();
            if (popUnitPanel.isActiveAndEnabled) popUnitPanel.Refresh();
            if (StatisticPanel.isActiveAndEnabled) StatisticPanel.Refresh();
            if (provincePanel.isActiveAndEnabled) provincePanel.Refresh();

            //if (bottomPanel.isActiveAndEnabled) bottomPanel.refresh();
        }

        internal static void selectProvince(int number)
        {
            if (number < 0 || World.FindProvince(number) == Game.selectedProvince)// same province clicked, hide selection
            {
                var lastSelected = Game.selectedProvince;
                Game.selectedProvince = null;
                if (lastSelected != null)
                {
                    lastSelected.setBorderMaterial(LinksManager.Get.defaultProvinceBorderMaterial);
                    lastSelected.setBorderMaterials(true);
                }
                if (provincePanel.isActiveAndEnabled)
                    provincePanel.Hide();
            }
            else // new province selected
            {
                if (Game.selectedProvince != null)//deal with previous selection
                {
                    Game.selectedProvince.setBorderMaterial(LinksManager.Get.defaultProvinceBorderMaterial);
                    Game.selectedProvince.setBorderMaterials(true);
                }
                Game.selectedProvince = World.FindProvince(number);
                Game.selectedProvince.setBorderMaterial(LinksManager.Get.selectedProvinceBorderMaterial);
                provincePanel.Show();
                if (Game.getMapMode() == 2) //core map mode
                    Game.redrawMapAccordingToMapMode(2);
            }
            if (buildPanel != null && buildPanel.isActiveAndEnabled)
                buildPanel.Refresh();
        }

        public void closeToppestPanel()
        {
            //canvas.GetComponentInChildren<DragPanel>();
            var lastChild = LinksManager.Get.CameraLayerCanvas.transform.GetChild(LinksManager.Get.CameraLayerCanvas.transform.childCount - 1);
            var panel = lastChild.GetComponent<DragPanel>();
            if (panel != null)
                panel.Hide();
            else
            {
                lastChild.SetAsFirstSibling();
                closeToppestPanel();
            }
        }

        public void FocusOnProvince(Province province, bool select)
        {
            gameObject.transform.position = new Vector3(province.getPosition().x, province.getPosition().y, focusHeight);
            if (select)
                selectProvince(province.getID());
        }

        public void FocusOnPoint(Vector2 point)
        {
            gameObject.transform.position = new Vector3(point.x, point.y, focusHeight);
        }
    }
}