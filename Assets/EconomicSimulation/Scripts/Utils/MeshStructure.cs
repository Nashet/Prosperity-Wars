using System.Collections.Generic;
using Nashet.Utils;
using UnityEngine;

namespace Nashet.MarchingSquares
{
    public class MeshStructure
    {
        private readonly List<Vector3> vertices;
        private readonly List<int> triangles;
        private readonly List<Vector2> UVmap;

        public int verticesCount
        {
            get
            {
                return vertices.Count;
            }
        }

        public MeshStructure()
        {
            vertices = new List<Vector3>();
            triangles = new List<int>();
            UVmap = new List<Vector2>();
        }

        public List<Vector3> getVertices()
        {
            return vertices;
        }

        public List<int> getTriangles()
        {
            return triangles;
        }

        public List<Vector2> getUVmap()
        {
            return UVmap;
        }

        public void Clear()
        {
            vertices.Clear();
            triangles.Clear();
        }

        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int vertexIndex = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }

        public void AddBorderQuad2(Vector2 a, Vector2 b)
        {
            //TODO put to constant
            float borderWidth = 0.4f;
            float borderWidth2 = -0.4f;

            AddBorderQuad(
    (Vector3)a,
    MeshExtensions.makeArrow(a, b, borderWidth),
    (Vector3)b,
    MeshExtensions.makeArrow(b, a, borderWidth2),
    true
    );
        }

        public void AddBorderQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, bool addUV)
        {
            float borderHeight = 0.1f;
            Vector3 liftedA = a + Vector3.back * borderHeight;
            Vector3 liftedB = b + Vector3.back * borderHeight;
            Vector3 liftedC = c + Vector3.back * borderHeight;
            Vector3 liftedD = d + Vector3.back * borderHeight;

            int vertexIndex = vertices.Count;
            vertices.Add(liftedA);
            vertices.Add(liftedB);
            vertices.Add(liftedC);
            vertices.Add(liftedD);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
            triangles.Add(vertexIndex + 1);

            if (addUV)
            {
                UVmap.Add(new Vector2(0f, 1f));
                UVmap.Add(new Vector2(1f, 1f));
                UVmap.Add(new Vector2(0f, 0f));
                UVmap.Add(new Vector2(1f, 0f));
            }
            //borderVertices[i * 2 + 0] = meshStructure.vertices[item.v1] + Vector3.back * borderHeight;
            //UVmap[i * 2 + 0] = new Vector2(0f, 1f);

            //borderVertices[i * 2 + 1] = MeshExtensions.makeArrow(meshStructure.vertices[item.v1], meshStructure.vertices[item.v2], borderWidth) + Vector3.back * borderHeight;
            //UVmap[i * 2 + 1] = new Vector2(1f, 1f);

            //borderVertices[i * 2 + 2] = meshStructure.vertices[item.v2] + Vector3.back * borderHeight;
            //UVmap[i * 2 + 2] = new Vector2(0f, 0f);

            //borderVertices[i * 2 + 3] = MeshExtensions.makeArrow(meshStructure.vertices[item.v2], meshStructure.vertices[item.v1], borderWidth2) + Vector3.back * borderHeight;
            //UVmap[i * 2 + 3] = new Vector2(1f, 0f);
        }

        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            int vertexIndex = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
        }

        public void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
        {
            int vertexIndex = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);
            vertices.Add(e);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 3);
            triangles.Add(vertexIndex + 4);
        }
    }
}