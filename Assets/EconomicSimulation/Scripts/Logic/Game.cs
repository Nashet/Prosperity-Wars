using Nashet.MarchingSquares;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// That's a game manager
    /// </summary>
    public class Game : ThreadedJob
    {
        public static bool devMode = true;
        private static bool surrended = devMode;
        public static bool logInvestments = false;
        public static bool logMarket = false;

        public static bool readMapFormFile = false;
        private static MyTexture mapTexture;

        public static Country Player { get; set; }

        public static Province selectedProvince;
        public static Province previoslySelectedProvince;
        public static List<Province> provincesToRedrawArmies = new List<Province>();
        public static List<Army> selectedArmies = new List<Army>();
        public static List<Province> playerVisibleProvinces = new List<Province>();

        private static MapModes mapMode;

        private static VoxelGrid<AbstractProvince> grid;
        private readonly Rect mapBorders;

        public static bool DrawFogOfWar { get; internal set; }
        public static bool IndustrialStart { get; internal set; }
        public static MapModes MapMode { get; internal set; }

        public Game(Texture2D mapImage)
        {
            DrawFogOfWar = true;
            if (mapImage == null)
                generateMapImage();
            else
            {
                //Texture2D mapImage = Resources.Load("provinces", typeof(Texture2D)) as Texture2D; ///texture;
                mapTexture = new MyTexture(mapImage);
            }

            mapBorders = new Rect(0f, 0f, mapTexture.getWidth() * Options.cellMultiplier, mapTexture.getHeight() * Options.cellMultiplier);
        }

        public void InitializeNonUnityData()
        {
            var m = new Market();
            World.Create(mapTexture);


            m.Initialize(null);
            //World.getAllExistingCountries().PerformAction(x => x.market.Initialize(x));  // should go after countries creation          

            //Game.updateStatus("Making grid..");
            grid = new VoxelGrid<AbstractProvince>(mapTexture.getWidth(), mapTexture.getHeight(), Options.cellMultiplier * mapTexture.getWidth(), mapTexture, World.AllAbstractProvinces);

            if (!devMode)
                makeHelloMessage();
            updateStatus("Finishing generation..");
        }

        /// <summary>
        /// Separate method to call Unity API. WOULDN'T WORK IN MULTYTHREADING!
        /// Called after initialization of non-Unity data
        /// </summary>
        public static void setUnityAPI()
        {
            // has to be separate circle
            World.AllProvinces.PerformAction(x => x.setUnityAPI(grid.getMesh(x), grid.getBorders()));
            World.AllSeaProvinces.PerformAction(x => x.setUnityAPI(grid.getMesh(x), grid.getBorders()));

            foreach (var province in World.AllProvinces)
            {
                var node = province.GameObject.GetComponent<Node>();
                node.Set(province, province.AllNeighbors());
                World.Get.graph.AddNode(node);
                province.SetBorderMaterials();
            }

            Country.setUnityAPI();
            //seaProvinces = null;
            // todo clear resources
            grid = null;
            mapTexture = null;
            // Annex all countries to P)layer
            //foreach (var item in World.getAllExistingCountries().Where(x => x != Game.Player))
            //{
            //    item.annexTo(Game.Player);
            //}
            //Quaternion.Ro(90f, Vector3.right);
            //World.Get.transform.Rotate(Vector3.right* 90f);
            //World.Get.transform.rotation.SetAxisAngle(Vector3.right, 90f);
            //del.x = 90f;
            //World.Get.transform.rotation = del;
        }

        public Rect getMapBorders()
        {
            return mapBorders;
        }

        public static void GivePlayerControlOf(Country newCountry)
        {
            //if (country != Country.NullCountry)
            {
                surrended = false;
                Player = newCountry;
                MainCamera.politicsPanel.selectReform(null);
                //MainCamera.inventionsPanel.selectInvention(null);
                Game.Player.events.RiseChangedCountry(new CountryEventArgs(newCountry));

                // not necessary since it will change automatically on province selection
                //MainCamera.buildPanel.selectFactoryType(null);

                //MainCamera.refreshAllActive();
                //UIEvents.RiseSomethingChangedInWorld(EventArgs.Empty, null);
            }
        }

        public static void GivePlayerControlToAI()
        {
            surrended = true;
        }

        public enum MapModes
        {
            Political, Cultures, Cores, Resources, PopulationChange, PopulationDensity, Prosperity
        }

        //case 0: //political mode
        //    case 1: //culture mode
        //    case 2: //cores mode
        //    case 3: //resource mode
        //    case 4: //population change mode
        //    case 5: //population density mode
        //    case 6: //prosperity map


        public static void redrawMapAccordingToMapMode()
        {
            foreach (var item in World.AllProvinces)
                item.SetColorAccordingToMapMode();
        }



        public static bool isPlayerSurrended()
        {
            return surrended;
        }

        private static void generateMapImage()
        {
            int mapSize;
            int width;
            //#if UNITY_WEBGL
            if (devMode)
            {
                mapSize = 20000;
                width = 150 + Rand.Get.Next(60);
            }
            else
            {
                //mapSize = 25000;
                //width = 170 + Random.Next(65);
                //mapSize = 30000;
                //width = 180 + Random.Next(65);
                mapSize = 40000;
                width = 250 + Rand.Get.Next(40);
            }
            // 140 is sqrt of 20000
            //int width = 30 + Random.Next(12);   // 140 is sqrt of 20000
            //#else
            //        int mapSize = 40000;
            //        int width = 200 + Random.Next(80);
            //#endif
            Texture2D mapImage = new Texture2D(width, mapSize / width);        // standard for webGL

            Color emptySpaceColor = Color.black;//.setAlphaToZero();
            mapImage.setColor(emptySpaceColor);

            int amountOfProvince = mapImage.width * mapImage.height / 140 + Rand.Get.Next(5);
            //amountOfProvince = 400 + Rand.random2.Next(100);
            for (int i = 0; i < amountOfProvince; i++)
                mapImage.SetPixel(mapImage.getRandomX(), mapImage.getRandomY(), ColorExtensions.getRandomColor());

            int emptyPixels = 1;//non zero
            Color currentColor = mapImage.GetPixel(0, 0);
            int emergencyExit = 0;
            while (emptyPixels != 0 && emergencyExit < 100)
            {
                emergencyExit++;
                emptyPixels = 0;
                for (int j = 0; j < mapImage.height; j++) // circle by province
                    for (int i = 0; i < mapImage.width; i++)
                    {
                        currentColor = mapImage.GetPixel(i, j);
                        if (currentColor == emptySpaceColor)
                            emptyPixels++;
                        else if (currentColor.a == 1f)
                        {
                            mapImage.drawRandomSpot(i, j, currentColor);
                        }
                    }
                mapImage.setAlphaToMax();
            }
            mapImage.Apply();
            mapTexture = new MyTexture(mapImage);
            Texture2D.Destroy(mapImage);
        }



        private static void makeHelloMessage()
        {
            MessageSystem.Instance.NewMessage("Tutorial", "Hi, this is VERY early demo of game-like economy simulator called 'Prosperity wars'" +
                "\n\nCurrently there is: "
                + "\n\tpopulation agents \\ factories \\ countries \\ national banks"
                + "\n\tbasic trade \\ production \\ consumption \n\tbasic warfare \n\tbasic inventions"
                + "\n\tbasic reforms (population can vote for reforms)"
                + "\n\tpopulation demotion \\ promotion to other classes \n\tmigration \\ immigration \\ assimilation"
                + "\n\tpolitical \\ culture \\ core \\ resource \\ population \\ prosperity map mode"
                + "\n\tmovements and rebellions"
                + "\n\nYou play as " + Player.FullName + " You can try to growth economy or conquer the world."
                + "\n\nOr, You can give control to AI and watch it"
                + "\n\nTry arrows or WASD for scrolling map and mouse wheel for scale"
                + "\n'Enter' key to close top window, space - to pause \\ unpause, left alt - to add command"
                + "\n\n\nI have now Patreon page where I post about that game development. Try red button below!"
                + "\nAlso I would be thankful if you will share info about this project"
                , "Ok", false);
            //, Game.Player.Capital.getPosition()
        }

        protected override void ThreadFunction()
        {
            InitializeNonUnityData();
        }
    }
}