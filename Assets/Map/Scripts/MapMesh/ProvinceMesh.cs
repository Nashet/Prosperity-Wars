using Nashet.MeshData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.MapMeshes
{
	public class ProvinceMesh : IProvinceMesh
    {   
        public int ID { get; protected set; }
       
        public GameObject GameObject { get; protected set; }
		protected MeshFilter MeshFilter { get; private set; }

        protected MeshRenderer meshRenderer;

        //protected Vector3 position;
        public Vector3 Position { get; protected set; }
		private readonly Dictionary<int, MeshRenderer> bordersMeshes = new Dictionary<int, MeshRenderer>();
		private static readonly Dictionary<int, ProvinceMesh> look = new Dictionary<int, ProvinceMesh>();


		public ProvinceMesh(int ID, MeshStructure meshStructure, Dictionary<int, MeshStructure> neighborBorders,
			Color provinceColor, Transform parent, Material defaultBorderMaterial, string name = "")
        {
            this.ID = ID;
        
            //spawn object
            GameObject = new GameObject(string.Format("{0} {1}", ID, name));

            //Add Components
            MeshFilter = GameObject.AddComponent<MeshFilter>();
            meshRenderer = GameObject.AddComponent<MeshRenderer>();

            // in case you want the new gameobject to be a child
            // of the gameobject that your script is attached to
            GameObject.transform.parent = parent;

            var landMesh = MeshFilter.mesh;
            landMesh.Clear();

            landMesh.vertices = meshStructure.getVertices().ToArray();
            landMesh.triangles = meshStructure.getTriangles().ToArray();
            landMesh.RecalculateNormals();
            landMesh.RecalculateBounds();
            landMesh.name = ID.ToString();

            Position = setProvinceCenter(meshStructure);
			

			MeshCollider groundMeshCollider = GameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
			groundMeshCollider.sharedMesh = MeshFilter.mesh;

			meshRenderer.material.shader = Shader.Find("Standard");

			meshRenderer.material.color = provinceColor;

			
			//making meshes for border
			foreach (var border in neighborBorders)
			{
				//each color is one neighbor (non repeating)
				//World.ProvincesById.Keys.(border.Key, out var neighbor);
				var neighbor = border.Key;
				//if (neighbor != null)
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
						meshRenderer.material = defaultBorderMaterial;
						borderMesh.name = "Border with " + neighbor; //todo delete it?

						bordersMeshes.Add(neighbor, meshRenderer);						
					}
				}
			}
			look.Add(ID, this);
		}

		public void SetColor(Color color)
		{
			meshRenderer.material.color = color;
		}

		private Vector3 setProvinceCenter(MeshStructure meshStructure)
        {
            Vector3 accu = new Vector3(0, 0, 0);
            foreach (var c in meshStructure.getVertices())
                accu += c;
            accu = accu / meshStructure.verticesCount;
            return accu;
        }

		public static ProvinceMesh GetById(int id)
		{
			return look[id];
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

		public void SetBorderMaterial(int id, Material material)
		{
			bordersMeshes[id].material = material;
		}
	}
}
