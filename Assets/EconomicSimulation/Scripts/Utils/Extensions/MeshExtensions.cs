using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nashet.Utils
{
    public static class MeshExtensions
    {
        public static bool hasDuplicateOfEdge(this Mesh mesh, int pointA, int pointB)
        {
            //getAllTriangles
            //    getAlledge
            //        check every Edge with a- b
            int foundEdgeDuplicates = 0;
            for (int i = 0; i < mesh.triangles.Count(); i += 3)
            {
                if (mesh.isSameEdge(pointA, pointB, mesh.triangles[i + 0], mesh.triangles[i + 1]))
                    foundEdgeDuplicates++;
                if (mesh.isSameEdge(pointA, pointB, mesh.triangles[i + 1], mesh.triangles[i + 2]))
                    foundEdgeDuplicates++;
                if (mesh.isSameEdge(pointA, pointB, mesh.triangles[i + 2], mesh.triangles[i + 0]))
                    foundEdgeDuplicates++;
                if (foundEdgeDuplicates > 1) // 1 is this edge itself
                    return true;
            }
            return false;
        }

        public static List<EdgeHelpers.Edge> getBorders(this Mesh mesh, List<EdgeHelpers.Edge> edges)
        {
            List<EdgeHelpers.Edge> res = new List<EdgeHelpers.Edge>();
            foreach (var checkingEdge in edges)
            {
                //if (!mesh.hasDuplicateOfEdge(item.v1, item.v2))
                // check only in edges!
                // need vector by vector comprasion
                int foundDuplicates = 0;
                foreach (var comparingEdge in edges)
                {
                    //if (checkingEdge == comparingEdge)
                    if (mesh.isSameEdge(checkingEdge.v1, checkingEdge.v2, comparingEdge.v1, comparingEdge.v2))
                    {
                        foundDuplicates++;
                        if (foundDuplicates > 1) // 1 - is edge itself
                            break;
                    }
                }
                if (foundDuplicates < 2)
                    res.Add(checkingEdge);
            }
            return res;
        }

        public static int isAnyPointOnLine(this Mesh mesh, Vector3 a, Vector3 b)
        {
            int result = -1;
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                if (isPointLiesOnLine(mesh.vertices[i], a, b))
                {
                    result = i;
                    break;
                }
            }
            return result;
        }

        public static bool isSameEdge(this Mesh mesh, int a, int b, int c, int d)
        {
            //if ( (mesh.vertices[a] == mesh.vertices[c] && mesh.vertices[b] == mesh.vertices[d])
            //    || (mesh.vertices[a] == mesh.vertices[c] && mesh.vertices[d] == mesh.vertices[b]))
            if ((a == c && b == d)
                || (a == d && b == c)
                || isTwoLinesTouchEachOther(mesh.vertices[a], mesh.vertices[b], mesh.vertices[c], mesh.vertices[d]))
            {
                //var point = mesh.isAnyPointOnLine(mesh.vertices[c], mesh.vertices[d]);
                //if (point > 0)
                //{
                //    vertexNumbers.Add(a);
                //    vertexNumbers.Add(point);
                //}
                //if (
                //    isPointLiesOnLine(mesh.vertices[a], mesh.vertices[c], mesh.vertices[d])
                //   //|| isPointLiesOnLine(mesh.vertices[c], mesh.vertices[a], mesh.vertices[b]))
                //   && !(isPointLiesOnLine(mesh.vertices[a], mesh.vertices[c], mesh.vertices[d]) && isPointLiesOnLine(mesh.vertices[b], mesh.vertices[c], mesh.vertices[d])))
                ////isTwoLinesTouchEachOther(mesh.vertices[a], mesh.vertices[d], mesh.vertices[a], mesh.vertices[c]))
                //{
                //    vertexNumbers.Add(a);
                //    vertexNumbers.Add(d);
                //}
                //else
                //if (isPointLiesOnLine(mesh.vertices[b], mesh.vertices[c], mesh.vertices[d])
                //   && !(isPointLiesOnLine(mesh.vertices[b], mesh.vertices[c], mesh.vertices[d]) && isPointLiesOnLine(mesh.vertices[a], mesh.vertices[c], mesh.vertices[d])))
                ////isTwoLinesTouchEachOther(mesh.vertices[a], mesh.vertices[d], mesh.vertices[a], mesh.vertices[c]))
                //{
                //    vertexNumbers.Add(b);
                //    vertexNumbers.Add(c);
                //}

                //if (isPointLiesOnLine(mesh.vertices[c], mesh.vertices[a], mesh.vertices[b])
                //  && !(isPointLiesOnLine(mesh.vertices[b], mesh.vertices[c], mesh.vertices[d]) && isPointLiesOnLine(mesh.vertices[a], mesh.vertices[c], mesh.vertices[d])))
                ////isTwoLinesTouchEachOther(mesh.vertices[a], mesh.vertices[d], mesh.vertices[a], mesh.vertices[c]))
                //{
                //    vertexNumbers.Add(c);
                //    vertexNumbers.Add(a);
                //}
                return true;
            }
            else
            {
                return false;
            }
        }

        //public static List<int> getPerimeterVertexNumbers2(this Mesh mesh)
        //{
        //    //for (int i = 0; i < mesh.triangles.Count(); i += 6)

        //}
        public static List<int> getPerimeterVertexNumbers(this Mesh mesh)
        {
            List<int> vertexNumbers = new List<int>();
            for (int i = 0; i < mesh.triangles.Count(); i += 6)
            //for (int i = 0; i < 17; i += 6)
            //int i = 0;
            {
                if (!mesh.hasDuplicateOfEdge(
                mesh.triangles[i + 5],
                mesh.triangles[i + 1]))
                {
                    vertexNumbers.Add(mesh.triangles[i + 5]);
                    vertexNumbers.Add(mesh.triangles[i + 1]);
                }

                if (!mesh.hasDuplicateOfEdge(
                mesh.triangles[i + 1],
                mesh.triangles[i + 2]))
                {
                    vertexNumbers.Add(mesh.triangles[i + 1]);
                    vertexNumbers.Add(mesh.triangles[i + 2]);
                }

                if (!mesh.hasDuplicateOfEdge(
                mesh.triangles[i + 2],
                mesh.triangles[i + 0]))
                {
                    vertexNumbers.Add(mesh.triangles[i + 2]);
                    vertexNumbers.Add(mesh.triangles[i + 0]);
                }

                if (!mesh.hasDuplicateOfEdge(
               mesh.triangles[i + 0],
               mesh.triangles[i + 5]))
                {
                    vertexNumbers.Add(mesh.triangles[i + 0]);
                    vertexNumbers.Add(mesh.triangles[i + 5]);
                }
            }
            //List<int> realVertexNumbers = new List<int>();
            //for (int i = 1; i < vertexNumbers.Count - 1; i += 2)
            //{
            //    if (Mathf.Abs(vertexNumbers[i] - vertexNumbers[i + 1]) > 1)
            //    {
            //        realVertexNumbers.Add(vertexNumbers[i] + 1);
            //        realVertexNumbers.Add(vertexNumbers[i] + 2);
            //    }
            //}
            //vertexNumbers.AddRange(realVertexNumbers);
            return vertexNumbers;
        }

        public static bool isTwoLinesTouchEachOther(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            if (isLinesParallel(a, b, c, d))
                //return isPointLiesOnLine(a, c, d) || isPointLiesOnLine(b, c, d)
                //    || isPointLiesOnLine(c, a, b) || isPointLiesOnLine(d, a, b);
                return (a == c && b == d) || (a == d && b == c);
            // || (a == b && c == d) || (a == d && b == c);
            else
                return false;
        }

        public static float getLineSlope2D(Vector3 a, Vector3 b)
        {
            return (b.y - a.y) / (b.x - a.x);
        }

        public static bool isLinesParallel(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var slope1 = getLineSlope2D(a, b);
            var slope2 = getLineSlope2D(c, d);
            //return Mathf.Abs(slope1 - slope2) < 0.001f;
            return slope1 == slope2 || (float.IsInfinity(slope1) && float.IsInfinity(slope2));

            //if
        }

        public static bool isPointLiesOnLine(Vector3 point, Vector3 a, Vector3 b)
        {
            float AB = Mathf.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y) + (b.z - a.z) * (b.z - a.z));
            float AP = Mathf.Sqrt((point.x - a.x) * (point.x - a.x) + (point.y - a.y) * (point.y - a.y) + (point.z - a.z) * (point.z - a.z));
            float PB = Mathf.Sqrt((b.x - point.x) * (b.x - point.x) + (b.y - point.y) * (b.y - point.y) + (b.z - point.z) * (b.z - point.z));

            //if (AB == AP + PB)
            if (Mathf.Abs(AB - AP - PB) < 0.001f)
            {
                //vertexNumbers.Add()
                return true;
            }
            else
                return false;
        }

        //public static Vector3[] getPerimeterVerices2(this Mesh mesh, bool removeDuplicates)
        //{
        //    take each edge from this mesh
        //        compare it eash edge of other meshs
        //        if same add it in collection
        //    for (int i=0; i< mesh.vertexCount; i++)

        //}
        public static Vector3[] getPerimeterVerices(this Mesh mesh, bool removeDuplicates)
        {
            var edges = mesh.getPerimeterVertexNumbers();

            List<Vector3> res = new List<Vector3>();

            for (int i = 0; i < edges.Count - 1; i++)
            {
                if (removeDuplicates)
                {
                    if (edges[i] != edges[i + 1])
                        res.Add(mesh.vertices[edges[i]]);
                }
                else
                    res.Add(mesh.vertices[edges[i]]);
            }
            res.Add(mesh.vertices[edges[edges.Count - 1]]);

            return res.ToArray();
        }

        public static Vector3 makeArrow(Vector3 arrowStart, Vector3 arrowEnd, float arrowBaseWidth) // true - water
        {
            //Vector3 directionPoint, leftBasePoint, rightBasePoint;
            Vector3 leftBasePoint;
            // Vector3[] result = new Vector3[3];

            //if (value > 0f)
            Vector3 arrowDirection = arrowEnd - arrowStart;
            //else
            //    arrowDirection = a.getTotalVertex() - b.getTotalVertex();

            leftBasePoint = Vector3.Cross(arrowDirection, Vector3.forward);
            leftBasePoint.Normalize();
            leftBasePoint = leftBasePoint * arrowBaseWidth;

            //rightBasePoint = leftBasePoint * -1f;
            //rightBasePoint += arrowStart;
            leftBasePoint += arrowStart;
            //directionPoint = arrowStart + (arrowDirection.normalized * value * 250f * arrowMuliplier);

            return leftBasePoint;
        }
    }
}