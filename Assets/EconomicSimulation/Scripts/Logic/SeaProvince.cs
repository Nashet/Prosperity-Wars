
//using Nashet.MarchingSquares;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;


//namespace Nashet.EconomicSimulation
//{
//    public class SeaProvince : AbstractProvince
//    {
//        public SeaProvince(string name, int ID) : base(name, ID)
//        {
//        }
//        public override void createMeshAndBorders(MeshStructure meshStructure, Dictionary<int, MeshStructure> neighborBorders)
//        {
//            base.createMeshAndBorders(meshStructure, neighborBorders);
//            meshRenderer.material = LinksManager.Get.waterMaterial;
//            GameObject.AddComponent<UnityStandardAssets.Water.WaterBasic>();
//            Debug.LogError("Im not happening");
//        }
//    }
//}