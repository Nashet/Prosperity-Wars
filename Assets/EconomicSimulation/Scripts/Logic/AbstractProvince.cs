using Nashet.MarchingSquares;
using Nashet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public abstract class AbstractProvince : Name, IColorID
    {
        /// <summary> false means sea province </summary>
        //public bool IsLandProvince { get; protected set; }

        protected GameObject txtMeshGl;
        public int ID { get; protected set; }

        public Color ColorID { get; protected set; }
        public GameObject GameObject { get; protected set; }
        public MeshFilter MeshFilter { get; protected set; }

        protected MeshRenderer meshRenderer;

        //protected Vector3 position;
        public Vector3 Position { get; protected set; }

        

        protected AbstractProvince(string name, int ID, Color colorID) : base(name)
        {
            this.ID = ID;
            this.ColorID = colorID;
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
            txtMesh.color = Color.black; // Set the text's color to red

            //renderers[0].material.shader = Shader.Find("3DText");


            group.SetLODs(lods);
            //#if UNITY_WEBGL
            group.size = 20; //was 30 for webgl
                             //#else
                             //group.size = 20; // for others
                             //#endif
                             //group.RecalculateBounds();
        }
        public virtual void setUnityAPI(MeshStructure meshStructure, Dictionary<AbstractProvince, MeshStructure> neighborBorders)
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
        }
        public static Vector3 setProvinceCenter(MeshStructure meshStructure)
        {
            Vector3 accu = new Vector3(0, 0, 0);
            foreach (var c in meshStructure.getVertices())
                accu += c;
            accu = accu / meshStructure.verticesCount;
            return accu;
        }
       
    }
}
