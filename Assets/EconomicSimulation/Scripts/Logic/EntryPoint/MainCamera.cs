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
        private Canvas canvas;

        [SerializeField]
        private float xzCameraSpeed = 2f;

        [SerializeField]
        private float yCameraSpeed = -55f;

        [SerializeField]
        private World world;

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

        private Camera camera; // it's OK
        private Game game;
        public static bool gameIsLoaded; // remove public after deletion of MyTable class

        //[SerializeField]
        /// <summary>Limits simulation speed (in seconds)</summary>
        private readonly float simulationSpeedLimit = 0.10f;

        private float previousFrameTime;
        public static MainCamera Instance;

        private void Start()
        {
            Instance = this;
            focusHeight = transform.position.z;
        }

        public void Move(float xMove, float yMove, float zMove)
        {

            var position = transform.position;
            var mapBorders = game.getMapBorders();


            if (xMove * xzCameraSpeed + position.x < mapBorders.x
                || xMove * xzCameraSpeed + position.x > mapBorders.width)
                xMove = 0;

            if (zMove * xzCameraSpeed + position.z < mapBorders.y
                || zMove * xzCameraSpeed + position.z > mapBorders.height)
                zMove = 0;

            yMove = yMove * yCameraSpeed;
            if (position.y + yMove < 40f
                || position.y + yMove > 500f)
                yMove = 0f;
            transform.Translate(xMove * xzCameraSpeed, yMove, zMove * xzCameraSpeed, Space.World);
        }

        private void FixedUpdate()
        {
            if (gameIsLoaded)
            {
                Move(0, Input.GetAxis("Mouse ScrollWheel"), 0);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            //starts loading thread
            if (game == null)// && Input.GetKeyUp(KeyCode.Backspace))
            {
                Application.runInBackground = true;
                game = new Game();
#if UNITY_WEBGL
                game.InitializeNonUnityData(); // non multi-threading
#else
                game.Start(); //initialize is here
#endif
            }
            if (game != null)
#if UNITY_WEBGL
                if (!gameIsLoaded)  // non multi-threading
#else
                if (game.IsDone && !gameIsLoaded)
#endif
                {
                    Game.setUnityAPI();

                    camera = GetComponent<Camera>();
                    FocusOnProvince(Game.Player.Capital, false);
                    //gameObject.transform.position = new Vector3(Game.Player.Capital.getPosition().x,
                    //    Game.Player.Capital.getPosition().y, gameObject.transform.position.z);
                    loadingPanel.Hide();
                    topPanel.Show();
                    bottomPanel.Show();
                    gameIsLoaded = true;
                }
#if !UNITY_WEBGL
                else // multi-threading
                    loadingPanel.updateStatus(game.getStatus());
#endif
            if (gameIsLoaded)
            {
                RefreshMap();

                if (Input.GetKeyDown(KeyCode.Return))
                    closeToppestPanel();

                if (Input.GetMouseButtonDown(0)) // clicked and released left button
                {
                    int meshNumber = getRayCastMeshNumber();
                    if (meshNumber != -3)
                        if (meshNumber <= World.GetAllProvinces().Count())
                            selectProvince(meshNumber);
                        else
                        {
                            var unit = Unit.FindByID(meshNumber);
                            if (!Game.selectedUnits.Contains(unit))
                                Game.selectedUnits.Add(unit);
                        }
                }

                if (!Game.selectedUnits.IsEmpty() && Input.GetMouseButtonDown(1))
                {
                    int meshNumber = getRayCastMeshNumber();
                    if (meshNumber > 0)
                        Game.selectedUnits.PerformAction(x => x.SendTo(World.FindProvince(meshNumber)));
                }

                if (world.IsRunning && !MessagePanel.IsOpenAny())
                {
                    if (Game.isPlayerSurrended() || !Game.Player.isAlive() || Time.time - previousFrameTime >= simulationSpeedLimit)
                    {
                        World.simulate();
                        previousFrameTime = Time.time;
                        refreshAllActive();
                    }
                }

                if (Message.HasUnshownMessages())
                    MessagePanel.showMessageBox(canvas, this);
                Game.previoslySelectedProvince = Game.selectedProvince;
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
                int meshNumber = getRayCastMeshNumber();
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
                int meshNumber = getRayCastMeshNumber();
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
                int meshNumber = getRayCastMeshNumber();
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
        // remake it to return mesh collider, on which will be chosen object
        private int getRayCastMeshNumber()
        {            
            RaycastHit hit;
            if (EventSystem.current.IsPointerOverGameObject())
                return -3; //hovering over UI
            else
            {
                if (!Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
                    return -1;                
            }

            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (meshCollider == null || meshCollider.sharedMesh == null)
                return -2;
            Mesh mesh = meshCollider.sharedMesh;

            //return mesh;
            return Convert.ToInt32(mesh.name);
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
                    lastSelected.setBorderMaterial(Game.defaultProvinceBorderMaterial);
                    lastSelected.setBorderMaterials(true);
                }
                if (provincePanel.isActiveAndEnabled)
                    provincePanel.Hide();
            }
            else // new province selected
            {
                if (Game.selectedProvince != null)//deal with previous selection
                {
                    Game.selectedProvince.setBorderMaterial(Game.defaultProvinceBorderMaterial);
                    Game.selectedProvince.setBorderMaterials(true);
                }
                Game.selectedProvince = World.FindProvince(number);
                Game.selectedProvince.setBorderMaterial(Game.selectedProvinceBorderMaterial);
                provincePanel.Show();
                if (Game.getMapMode() == 2) //core map mode
                    Game.redrawMapAccordingToMapMode(2);
            }
            if (buildPanel != null && buildPanel.isActiveAndEnabled)
                buildPanel.Refresh();
        }

        private void closeToppestPanel()
        {
            //canvas.GetComponentInChildren<DragPanel>();
            var lastChild = canvas.transform.GetChild(canvas.transform.childCount - 1);
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
            gameObject.transform.position = new Vector3(province.getPosition().x, focusHeight, province.getPosition().y);
            if (select)
                selectProvince(province.getID());
        }

        public void FocusOnPoint(Vector2 point)
        {
            gameObject.transform.position = new Vector3(point.x, point.y, focusHeight);
        }
    }
}