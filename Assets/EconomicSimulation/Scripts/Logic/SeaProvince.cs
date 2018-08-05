
using Nashet.MarchingSquares;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class SeaProvince : AbstractProvince
    {
        public SeaProvince(string name, int ID, Color colorID) : base(name, ID, colorID)
        {
        }
        public override void setUnityAPI(MeshStructure meshStructure, Dictionary<AbstractProvince, MeshStructure> neighborBorders)
        {
            base.setUnityAPI(meshStructure, neighborBorders);
            meshRenderer.material = LinksManager.Get.waterMaterial;
            GameObject.AddComponent<UnityStandardAssets.Water.WaterBasic>();

        }
    }
}
