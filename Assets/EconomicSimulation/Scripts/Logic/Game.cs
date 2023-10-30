using Nashet.Map.Utils;
using Nashet.MapMeshes;
using Nashet.MarchingSquares;
using Nashet.MeshData;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using QPathFinder;
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
        public static bool devMode = false;//false;
        private static bool surrended = devMode;
        public static bool logInvestments = false;
        public static bool logMarket = false;

        public static bool readMapFormFile = false;
        private static MyTexture mapTexture;

        public static Country Player { get; set; }
        
        public static List<Province> provincesToRedrawArmies = new List<Province>();
		public static List<Army> selectedArmies = new List<Army>();
		public static List<Province> playerVisibleProvinces = new List<Province>();
		public static bool isInSendArmyMode { get; private set; }


		public static Action<bool> OnIsInSendArmyModeChanged;
    
        private static VoxelGrid grid;
        private readonly Rect mapBorders;

        public static bool DrawFogOfWar { get; internal set; }
        public static bool IndustrialStart { get; internal set; } = true;
        public static MapModes MapMode { get; internal set; }
        private MapTextureGenerator mapGenerator = new MapTextureGenerator();

        public Game(Texture2D mapImage)
		{
			DrawFogOfWar = true;
			PrepareTexture(mapImage); // only can run in unity thread

			mapBorders = new Rect(0f, 0f, mapTexture.getWidth() * Options.cellMultiplier, mapTexture.getHeight() * Options.cellMultiplier);
            QPathFinder.Logger.SetLoggingLevel(devMode ? QPathFinder.Logger.Level.Warnings : QPathFinder.Logger.Level.None);
		}

		private void PrepareTexture(Texture2D mapImage)
		{
			if (mapImage == null)
			{
				int height;
				int width;
				//#if UNITY_WEBGL
				if (devMode)
				{
					height = 130;
					width = 150 + Rand.Get.Next(60);
				}
				else
				{
					height = 160;
					width = 250 + Rand.Get.Next(40);
				}
				//mapSize = 160000;
				//width = 420;
				int amountOfProvince = width * height / 140 + Rand.Get.Next(5);
               // amountOfProvince = 136;
				mapTexture = mapGenerator.generateMapImage(width, height, amountOfProvince);
			}
			else
			{
				//Texture2D mapImage = Resources.Load("provinces", typeof(Texture2D)) as Texture2D; ///texture;
				mapTexture = new MyTexture(mapImage);
			}
		}

		public void InitializeNonUnityData()
        {
            updateStatus("Crating map mesh data..");

			grid = new VoxelGrid(mapTexture.getWidth(), mapTexture.getHeight(), Options.cellMultiplier * mapTexture.getWidth(), mapTexture);
           
            updateStatus("Creating economic data..");
			
			World.Create(mapTexture);
			
			updateStatus("Finishing with non-unity loading..");
		}

        /// <summary>
        /// Separate method to call Unity API. WOULDN'T WORK IN MULTYTHREADING!
        /// Called after initialization of non-Unity data
        /// </summary>
        public static void setUnityMeshes()
		{
            if (!devMode)
				makeHelloMessage();

            //World.getAllExistingCountries().PerformAction(x => x.market.Initialize(x));  // should go after countries creation  

            var borderMeshesLookup = new Dictionary<int, Dictionary<int, MeshStructure>>();
			foreach (var province in World.AllProvinces)
            {
                var mesh = grid.getMesh(province.Id, out var borderMeshes);
                borderMeshesLookup.Add(province.Id, borderMeshes);
				
                province.provinceMesh = new ProvinceMesh(province.Id, mesh, borderMeshes, province.ProvinceColor, World.Get.transform,
				  LinksManager.Get.shoreMaterial, province.ShortName, new Material(Shader.Find("Standard")));//

				var label = MapTextLabel.CreateMapTextLabel(LinksManager.Get.r3DProvinceTextPrefab, province.provinceMesh.Position, province.ToString(), Color.black);

				label.transform.SetParent(province.provinceMesh.GameObject.transform, false);
				province.SetNeighbors(borderMeshes);

				//World.AllSeaProvinces.PerformAction(x =>
			}

			foreach (var province in World.AllProvinces)//second time call needed to clear mountain locked provinces
			{
                province.SetNeighbors(borderMeshesLookup[province.Id]);
			}   


			Country.setMaterial();

            //todo put it in some other file. World?
			AddRivers();
            foreach (var province in World.AllProvinces)
            {
                province.SetBorderMaterials();
            }
			SetPatchFinding();

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

			// todo clear resources
			grid = null;
			mapTexture = null;
		}

		private static void SetPatchFinding()
		{
			foreach (var province in World.AllProvinces)
			{
				//var node = province.GameObject.GetComponent<Node>();
				//node.Set(province, province.AllNeighbors());
				var node = new Node(province.provinceMesh.Position, province);
				PathFinder.instance.graphData.nodes.Add(node);
				province.Node = node;
			}

			PathFinder.instance.graphData.ReGenerateIDs();

			foreach (var province in World.AllProvinces)
			{
				foreach (var item in province.AllNeighbors())
				{
					if (PathFinder.instance.graphData.paths.Exists(x => x.IDOfA == province.Node.autoGeneratedID && x.IDOfB == item.Node.autoGeneratedID))
						continue;
					PathFinder.instance.graphData.paths.Add(
						new Path(item.Node.autoGeneratedID, province.Node.autoGeneratedID));
				}
			}
		}

		private static void AddRivers()
		{
			for (int i = 0; i < Options.MaxRiversAmount; i++)
			{
				var riverStart = World.AllProvinces.Where(x => !x.AllNeighbors().Any(y => y.isRiverNeighbor(x))).Random();
				//x.IsCoastal && 
				//x.Terrain == Province.TerrainTypes.Mountains &&
				if (riverStart == null)
                    continue;
				var riverStart2 = riverStart.AllNeighbors().Random();
				//.Where(x => x.IsCoastal)
				if (riverStart2 == null) 
                    continue;
				AddRiverBorder(riverStart, riverStart2);
			}
		}

		private static void AddRiverBorder(Province beach1, Province beach2)
		{
            var logRivers = false;
            if (beach1.Terrain == Province.TerrainTypes.Mountains && beach2.Terrain == Province.TerrainTypes.Mountains)
            {
                if (logRivers)
                    Debug.Log($"----river stoped because of mountain");
				return; 
            }

			var chanceToContinue = Rand.Get.Next(Options.RiverLenght);
            if (chanceToContinue == 1)
			{
				if (logRivers)
					Debug.Log($"----river stoped because its long enough");
				return;
			};

            Province beach3 = null;

			var potentialBeaches = beach1.AllNeighbors().Where(x => x.isNeighbor(beach2)).ToList();
            {

                if (potentialBeaches.Count == 1)
                {
					beach3 = potentialBeaches.ElementAt(0);
					if (beach3.isRiverNeighbor(beach1) || beach3.isRiverNeighbor(beach2))
					{
                        beach3 = null;
					}
				}

                if (potentialBeaches.Count == 2)
                {
                    var chooseBeach = Rand.Get.Next(2);
                    if (chooseBeach == 0)
                    {
                        beach3 = potentialBeaches.ElementAt(0);
                        if (beach3.isRiverNeighbor(beach1) || beach3.isRiverNeighbor(beach2))
                        {
                            beach3 = potentialBeaches.ElementAt(1);
                        }
                    }
                    if (chooseBeach == 1)
                    {
                        beach3 = potentialBeaches.ElementAt(1);
                        if (beach3.isRiverNeighbor(beach1) || beach3.isRiverNeighbor(beach2))
                        {
                            beach3 = potentialBeaches.ElementAt(0);
                        }
                    }
                }
			}
			if (logRivers)
				Debug.Log($"{beach1}, {beach2}");
            beach1.AddRiverBorder( beach2);
			beach2.AddRiverBorder(beach1);

			var chance = Rand.Get.Next(2);

			if (beach3 == null)
			{
				if (logRivers)
					Debug.Log($"----river stoped because cant find beach3");
				return;
			};

			if (chance == 1 && !beach3.isRiverNeighbor(beach1))
            {
                AddRiverBorder(beach3, beach1);
            }
            else
            {
                AddRiverBorder(beach3, beach2);
            }
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

        public static bool isPlayerSurrended()
        {
            return surrended;
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
                + "\n'Enter' key to close top window, space - to pause \\ unpause, left alt - to add command or unit"
                //  + "\n\n\nI have now Patreon page where I post about that game development. Try red button below!"
                + "\nAlso I would be thankful if you will share info about this project"
                , "Ok", false, OnClosed
                );
            //, Game.Player.Capital.getPosition()
        }

        private static void OnClosed()
        {
            var resosolution = Screen.currentResolution;
            if (resosolution.width < resosolution.height)
                MessageSystem.Instance.NewMessage("(╯ ° □ °) ╯ (┻━┻)", "IT LOOKS LIKE YOU ARE IN PORTRAIT MODE. CLICK FULLSCREEN AND ROTATE IT TO ALBUM FOR BETTER UI SCALE.", "Ok", false);           
        }

        protected override void ThreadFunction()
        {
            //Thread.Sleep(1000);    
            InitializeNonUnityData();
        }

        public static void ChangeIsInSendArmyMode(bool state)
        {
            if (state == isInSendArmyMode)
                return;
            isInSendArmyMode = state;
            OnIsInSendArmyModeChanged.Invoke(state);
        }
    }
}