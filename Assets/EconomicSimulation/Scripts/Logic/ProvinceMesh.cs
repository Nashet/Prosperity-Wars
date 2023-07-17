using Nashet.MarchingSquares;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
	// put text placing separatly in file
    public class ProvinceMesh 
    {
        /// <summary> false means sea province </summary>
        //public bool IsLandProvince { get; protected set; }

        protected GameObject txtMeshGl;
        public int ID { get; protected set; }
       
        public GameObject GameObject { get; protected set; }
        public MeshFilter MeshFilter { get; protected set; }

        protected MeshRenderer meshRenderer;

        //protected Vector3 position;
        public Vector3 Position { get; protected set; }
		private readonly Dictionary<Province, MeshRenderer> bordersMeshes = new Dictionary<Province, MeshRenderer>();


		public ProvinceMesh(int ID)
        {
            this.ID = ID;            
        }
        public void setLabel()
        {
            LODGroup group = GameObject.AddComponent<LODGroup>();

            // Add 4 LOD levels
            LOD[] lods = new LOD[1];
            txtMeshGl = GameObject.Instantiate(LinksManager.Get.r3DProvinceTextPrefab);
            Transform txtMeshTransform = txtMeshGl.transform;
            txtMeshTransform.SetParent(GameObject.transform, false);
            Renderer[] renderers = new Renderer[1];
            renderers[0] = txtMeshTransform.GetComponent<Renderer>();
            lods[0] = new LOD(0.25F, renderers);

            var position = Position;
            position.z -= 0.003f;
            txtMeshTransform.position = position;

            TextMesh txtMesh = txtMeshTransform.GetComponent<TextMesh>();

            txtMesh.text = ToString();
            txtMesh.color = Color.black;

            //renderers[0].material.shader = Shader.Find("3DText");


            group.SetLODs(lods);
            //#if UNITY_WEBGL
            group.size = 20; 
        }

        public virtual void createMeshes(MeshStructure meshStructure, Dictionary<int, MeshStructure> neighborBorders, Color provinceColor)
        {
            //this.meshStructure = meshStructure;

            //spawn object
            GameObject = new GameObject(string.Format("{0}", ID));

            //Add Components
            MeshFilter = GameObject.AddComponent<MeshFilter>();
            meshRenderer = GameObject.AddComponent<MeshRenderer>();

            // in case you want the new gameobject to be a child
            // of the gameobject that your script is attached to
            GameObject.transform.parent = World.Get.transform;

            var landMesh = MeshFilter.mesh;
            landMesh.Clear();

            landMesh.vertices = meshStructure.getVertices().ToArray();
            landMesh.triangles = meshStructure.getTriangles().ToArray();
            landMesh.RecalculateNormals();
            landMesh.RecalculateBounds();
            landMesh.name = ID.ToString();

            Position = setProvinceCenter(meshStructure);
            setLabel();

			MeshCollider groundMeshCollider = GameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
			groundMeshCollider.sharedMesh = MeshFilter.mesh;

			meshRenderer.material.shader = Shader.Find("Standard");// Province");

			meshRenderer.material.color = provinceColor;

			
			//making meshes for border
			foreach (var border in neighborBorders)
			{
				//each color is one neighbor (non repeating)
				World.ProvincesById.TryGetValue(border.Key, out var neighbor);
				//var neighbor = World.ProvincesByColor[border.Key];
				if (neighbor != null)
				{
                    //if (!IsForDeletion)
					{
						GameObject borderObject = new GameObject($"Border with {neighbor}");

						//Add Components
						MeshFilter = borderObject.AddComponent<MeshFilter>();
						MeshRenderer meshRenderer = borderObject.AddComponent<MeshRenderer>();

						borderObject.transform.parent = GameObject.transform;

						Mesh borderMesh = MeshFilter.mesh;
						borderMesh.Clear();

						borderMesh.vertices = border.Value.getVertices().ToArray();
						borderMesh.triangles = border.Value.getTriangles().ToArray();
						borderMesh.uv = border.Value.getUVmap().ToArray();
						borderMesh.RecalculateNormals();
						borderMesh.RecalculateBounds();
						meshRenderer.material = LinksManager.Get.defaultProvinceBorderMaterial;
						borderMesh.name = "Border with " + neighbor; //todo delete it?

						bordersMeshes.Add(neighbor, meshRenderer);
					}
				}
			}
		}

		public void SetBorderMaterials(Province province)
		{
			foreach (var border in bordersMeshes)
			{
				if (border.Key.isNeighbor(province))
				{
					if (border.Key.isRiverNeighbor(province))
					{
						border.Value.material = LinksManager.Get.riverBorder;
					}
					else
					{
						if (province.Country == border.Key.Country) // same country
						{
							border.Value.material = LinksManager.Get.defaultProvinceBorderMaterial;
						}
						else
						{
							if (province.Country == World.UncolonizedLand)
								border.Value.material = LinksManager.Get.defaultProvinceBorderMaterial;
							else
								border.Value.material = province.Country.getBorderMaterial();
						}
					}
				}
				else
				{
					border.Value.material = LinksManager.Get.impassableBorder;
				}
			}
		}


		public void UpdateBorderMaterials(Province province)
		{
			foreach (var border in bordersMeshes)
			{
				if (border.Key.isNeighbor(province))
				{
					if (border.Key.isRiverNeighbor(province))
					{
						continue;
					}
					if (province.Country == border.Key.Country) // same country
					{
						border.Value.material = LinksManager.Get.defaultProvinceBorderMaterial;
						border.Key.provinceMesh.bordersMeshes[province].material = LinksManager.Get.defaultProvinceBorderMaterial;
					}
					else
					{
						if (province.Country == World.UncolonizedLand)
							border.Value.material = LinksManager.Get.defaultProvinceBorderMaterial;
						else
							border.Value.material = province.Country.getBorderMaterial();

						if (border.Key.Country == World.UncolonizedLand)
							border.Key.provinceMesh.bordersMeshes[province].material = LinksManager.Get.defaultProvinceBorderMaterial;
						else
							border.Key.provinceMesh.bordersMeshes[province].material = border.Key.Country.getBorderMaterial();
					}
				}
			}
		}

		public void SetColor(Color color)
		{
			meshRenderer.material.color = color;
		}

		public static Vector3 setProvinceCenter(MeshStructure meshStructure)
        {
            Vector3 accu = new Vector3(0, 0, 0);
            foreach (var c in meshStructure.getVertices())
                accu += c;
            accu = accu / meshStructure.verticesCount;
            return accu;
        }

		internal void OnSecedeGraphic(Color newColor)
		{
			if (meshRenderer != null)
				meshRenderer.material.color = newColor;
		}

		public static int? GetIdByCollider(Collider collider)
		{
			if (collider != null)
			{
				MeshCollider meshCollider = collider as MeshCollider;
				if (meshCollider == null || meshCollider.sharedMesh == null)
					return null;

				Mesh mesh = meshCollider.sharedMesh;

				if (mesh.name == "Quad")
					return null;

				int provinceNumber = Convert.ToInt32(mesh.name);
				return provinceNumber;
			}
			else
				return null;
		}
	}
}
