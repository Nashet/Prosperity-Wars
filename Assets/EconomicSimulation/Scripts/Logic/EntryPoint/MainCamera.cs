using UnityEngine;
using Nashet.UnityUIUtils;
using Nashet.Utils;
namespace Nashet.EconomicSimulation
{
    public class MainCamera : MonoBehaviour
    {   
        [SerializeField]
        private Canvas canvas;

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
        
        void FixedUpdate()
        {
            if (gameIsLoaded)
            {
                var position = this.transform.position;
                var mapBorders = game.getMapBorders();
                float xyCameraSpeed = 2f;
                float zCameraSpeed = 55f;
                float xMove = Input.GetAxis("Horizontal");
                if (xMove * xyCameraSpeed + position.x < mapBorders.x
                    || xMove * xyCameraSpeed + position.x > mapBorders.width)
                    xMove = 0;
                float yMove = Input.GetAxis("Vertical");
                if (yMove * xyCameraSpeed + position.y < mapBorders.y
                    || yMove * xyCameraSpeed + position.y > mapBorders.height)
                    yMove = 0;
                float zMove = Input.GetAxis("Mouse ScrollWheel");
                zMove = zMove * zCameraSpeed;
                if (position.z + zMove > -40f
                    || position.z + zMove < -500f)
                    zMove = 0f;
                transform.Translate(xMove * xyCameraSpeed, yMove * xyCameraSpeed, zMove);
            }
        }
        // Update is called once per frame
        void Update()
        {
            //starts loading thread
            if (game == null)// && Input.GetKeyUp(KeyCode.Backspace))
            {
                Application.runInBackground = true;
                game = new Game();
#if UNITY_WEBGL
                game.initialize(); // non multi-threading
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

                    camera = this.GetComponent<Camera>();
                    focus(Game.Player.getCapital());
                    //gameObject.transform.position = new Vector3(Game.Player.getCapital().getPosition().x,
                    //    Game.Player.getCapital().getPosition().y, gameObject.transform.position.z);
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
                if (Game.getMapMode() != 0 && Game.date.isDivisible(Options.MapRedrawRate))
                    Game.redrawMapAccordingToMapMode(Game.getMapMode());
                if (Input.GetMouseButtonDown(0)) // clicked and released left button
                {
                    int meshNumber = getRayCastMeshNumber();
                    //found something correct            
                    selectProvince(meshNumber);
                }
                if (Input.GetKeyUp(KeyCode.Space))
                    topPanel.switchHaveToRunSimulation();

                if (Input.GetKeyDown(KeyCode.Return))
                    closeToppestPanel();

                if (Game.isRunningSimulation() && !MessagePanel.IsOpenAny())
                {
                    Game.simulate();
                    refreshAllActive();
                    if (Game.selectedProvince != null)
                        //provincePanel.Refresh(Game.selectedProvince);
                        provincePanel.Refresh();
                }


                if (Message.HasUnshownMessages())
                    MessagePanel.showMessageBox(canvas);
                Game.previoslySelectedProvince = Game.selectedProvince;
            }
        }
        int getRayCastMeshNumber()
        {
            //RaycastHit hit = new RaycastHit();//temp
            //int layerMask = 1 << 8;
            //DefaultRaycastLayers
            //Physics.DefaultRaycastLayers;
            RaycastHit hit;
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                if (!Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
                    return -1;
                else; // go on
            else return -3; //hovering over UI

            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (meshCollider == null || meshCollider.sharedMesh == null)
                return -2;
            Mesh mesh = meshCollider.sharedMesh;

            //Vector3[] vertices = mesh.vertices;
            //int[] triangles = mesh.triangles;

            //Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];

            //mapPointer.transform.position = p0;
            //// Gets a vector that points from the player's position to the target's.
            //Vector3 heading = mapPointer.transform.position - Vector3.zero;
            //heading.Normalize();

            // mapPointer.transform.position += heading * 50;
            // mapPointer.transform.rotation = Quaternion.LookRotation(heading, Vector3.up) * Quaternion.Euler(-90, 0, 0);

            //return groundMesh.triangles[hit.triangleIndex * 3];
            ;
            return System.Convert.ToInt32(mesh.name);
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
            //if (Game.selectedProvince != null && number >= 0)
            //{
            //    Game.selectedProvince.setBorderMaterial(Game.defaultProvinceBorderMaterial);
            //    Game.selectedProvince.setBorderMaterials();
            //}


            if (number >= 0)
            {
                if (Province.find(number) == Game.selectedProvince)// same province clicked, hide selection
                {
                    
                    var lastSelected = Game.selectedProvince;
                    Game.selectedProvince = null;
                    lastSelected.setBorderMaterial(Game.defaultProvinceBorderMaterial);
                    lastSelected.setBorderMaterials(true);

                    provincePanel.Hide();

                }
                else // new province selected
                {
                    if (Game.selectedProvince != null)//deal with previous selection
                    {
                        Game.selectedProvince.setBorderMaterial(Game.defaultProvinceBorderMaterial);
                        Game.selectedProvince.setBorderMaterials(true);
                    }                    
                    Game.selectedProvince = Province.find(number);
                    Game.selectedProvince.setBorderMaterial(Game.selectedProvinceBorderMaterial);
                    provincePanel.Show();
                    if (Game.getMapMode() == 2) //core map mode
                        Game.redrawMapAccordingToMapMode(2);

                }
                if (buildPanel.isActiveAndEnabled)
                    buildPanel.Refresh();

            }
        }
        private void closeToppestPanel()
        {
            //canvas.GetComponentInChildren<DragPanel>();
            var lastChild = canvas.transform.GetChild(canvas.transform.childCount - 1);
            var panel = lastChild.GetComponent<DragPanel>();
            if (panel != null)
                panel.onCloseClick();
            else
            {
                lastChild.SetAsFirstSibling();
                closeToppestPanel();
            }
        }

        
        // This function is called every fixed framerate frame, if the MonoBehavior is enabled.
        public void focus(Province province)
        {
            gameObject.transform.position = new Vector3(province.getPosition().x,
                        province.getPosition().y, gameObject.transform.position.z);
        }

    }
}