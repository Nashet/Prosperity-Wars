using System.Collections.Generic;
using System.Linq;
using Nashet.Utils;
using UnityEngine;

namespace Nashet.MarchingSquares
{
    public interface IColorID
    {
        Color ColorID { get;}
    }

    [SelectionBase]
    public class VoxelGrid<T> where T : class, IColorID
    {
        private readonly int width, height;

        private readonly VoxelGrid<T> xNeighbor, yNeighbor, xyNeighbor;

        private readonly Voxel<T>[] voxels;

        private readonly float voxelSize, gridSize;

        private MeshStructure mesh;
        private Dictionary<T, MeshStructure> bordersMeshes;

        private Voxel<T> dummyX, dummyY, dummyT;
        //private readonly Game game;

        //public VoxelGrid(int width, int height, float size, MyTexture texture, List<Province> blockedProvinces, IEnumerable<Province> provinces)
        public VoxelGrid(int width, int height, float size, MyTexture texture, IEnumerable<T> provinces)
        {
            this.width = width;
            this.height = height;
            //this.game = game;
            // this.resolution = resolution;
            gridSize = size;
            voxelSize = size / width;
            voxels = new Voxel<T>[width * height];           

            dummyX = new Voxel<T>();
            dummyY = new Voxel<T>();
            dummyT = new Voxel<T>();

           
            for (int i = 0, y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {                    
                    CreateVoxel(i, x, y, provinces.FirstOrDefault(t => t.ColorID == texture.GetPixel(x, y)));
                    i++;
                }
            }
        }

        public MeshStructure getMesh(T analysingProvince)
        {
            mesh = new MeshStructure();
            bordersMeshes = new Dictionary<T, MeshStructure>();
            //game.updateStatus("Triangulation .." + analysingProvince);
            Triangulate(analysingProvince);
            return mesh;
        }

        private void CreateVoxel(int i, int x, int y, T state)
        {
            voxels[i] = new Voxel<T>(x, y, voxelSize, state);
        }

        private void Triangulate(T analysingProvince)
        {
            //mesh.Clear();

            if (xNeighbor != null)
            {
                dummyX.BecomeXDummyOf(xNeighbor.voxels[0], gridSize);
            }
            TriangulateCellRows(analysingProvince);
            if (yNeighbor != null)
            {
                TriangulateGapRow(analysingProvince);
            }

            //mesh.vertices = vertices.ToArray();
            //mesh.triangles = triangles.ToArray();
        }

        private void TriangulateCellRows(T analysingProvince)
        {
            //int cells = resolution - 1;
            for (int i = 0, y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                //if (voxels[i].getColor() != Color.black
                //    || voxels[i + 1].getColor() != Color.black
                //    || voxels[i + resolution].getColor() != Color.black
                //    || voxels[i + resolution + 1].getColor() != Color.black)
                {
                    TriangulateCell(
                        voxels[i],
                        voxels[i + 1],
                        voxels[i + width],
                        voxels[i + width + 1], analysingProvince);
                    i++;
                }
                if (xNeighbor != null)
                {
                    TriangulateGapCell(i, analysingProvince);
                }
                i++;
            }
        }

        private void TriangulateGapCell(int i, T analysingProvince)
        {
            Voxel<T> dummySwap = dummyT;
            dummySwap.BecomeXDummyOf(xNeighbor.voxels[i + 1], gridSize);
            dummyT = dummyX;
            dummyX = dummySwap;
            TriangulateCell(voxels[i], dummyT, voxels[i + width], dummyX, analysingProvince);
        }

        private void TriangulateGapRow(T analysingProvince)
        {
            dummyY.BecomeYDummyOf(yNeighbor.voxels[0], gridSize);
            //int cells = width - 1;
            //int offset = cells * resolution;
            int offset = (width - 1) * width;

            for (int x = 0; x < width - 1; x++)
            {
                Voxel<T> dummySwap = dummyT;
                dummySwap.BecomeYDummyOf(yNeighbor.voxels[x + 1], gridSize);
                dummyT = dummyY;
                dummyY = dummySwap;
                TriangulateCell(voxels[x + offset], voxels[x + offset + 1], dummyT, dummyY, analysingProvince);
            }

            if (xNeighbor != null)
            {
                dummyT.BecomeXYDummyOf(xyNeighbor.voxels[0], gridSize);
                TriangulateCell(voxels[voxels.Length - 1], dummyX, dummyY, dummyT, analysingProvince);
            }
        }

        private bool isBorderCell(Voxel<T> a, Voxel<T> b, Voxel<T> c, Voxel<T> d)
        {
            return !(a.getState() == b.getState() && a.getState() == c.getState() && a.getState() == d.getState());
        }

        private void findBorderMeshAndAdd(T province, Vector2 a, Vector2 b)
        {
            //var province = Province.find(color);
            if (province != null)
            //if (Game.seaProvinces.Contains(province))
            //    return null;
            //else
            {
                MeshStructure border;
                if (!bordersMeshes.TryGetValue(province, out border))
                {
                    border = new MeshStructure();
                    bordersMeshes.Add(province, border);
                }
                border.AddBorderQuad2(a, b);
            }
        }

        private void TriangulateCell(Voxel<T> a, Voxel<T> b, Voxel<T> c, Voxel<T> d, T analyzingState)
        {
            //bool isBorder = isBorderCell(a, b, c, d);
            int cellType = 0;
            if (a.getState() == analyzingState)
            {
                cellType |= 1;
            }
            if (b.getState() == analyzingState)
            {
                cellType |= 2;
            }
            if (c.getState() == analyzingState)
            {
                cellType |= 4;
            }
            if (d.getState() == analyzingState)
            {
                cellType |= 8;
            }
            switch (cellType)
            {
                case 0:
                    return;

                case 1:
                    mesh.AddTriangle(a.getPosition(), a.getYEdgePosition(), a.getXEdgePosition());

                    if (a.getState() != b.getState() && a.getState() != c.getState() && a.getState() != d.getState()
                        && b.getState() != c.getState() && b.getState() != d.getState()
                        && c.getState() != d.getState()
                        )//&& c.getColor() == analyzingColor)
                    {
                        var centre = new Vector2(a.getXEdgePosition().x, a.getYEdgePosition().y);
                        mesh.AddTriangle(a.getYEdgePosition(), centre, a.getXEdgePosition());
                        //AddQuad(mesh, a.getYEdgePosition(), c.getXEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());
                        findBorderMeshAndAdd(c.getState(), a.getYEdgePosition(), centre);
                        findBorderMeshAndAdd(b.getState(), centre, a.getXEdgePosition());
                    }
                    else
                        findBorderMeshAndAdd(d.getState(), a.getYEdgePosition(), a.getXEdgePosition());

                    break;

                case 2:
                    mesh.AddTriangle(b.getPosition(), a.getXEdgePosition(), b.getYEdgePosition());
                    if (a.getState() != b.getState() && a.getState() != c.getState() && a.getState() != d.getState()
                       && b.getState() != c.getState() && b.getState() != d.getState()
                       && c.getState() != d.getState()
                       )//&& c.getColor() == analyzingColor)
                    {
                        var centre = new Vector2(a.getXEdgePosition().x, a.getYEdgePosition().y);
                        mesh.AddTriangle(a.getXEdgePosition(), centre, b.getYEdgePosition());
                        findBorderMeshAndAdd(a.getState(), a.getXEdgePosition(), centre);
                        findBorderMeshAndAdd(d.getState(), centre, b.getYEdgePosition());
                    }
                    else
                        findBorderMeshAndAdd(c.getState(), a.getXEdgePosition(), b.getYEdgePosition());

                    break;

                case 3:
                    mesh.AddQuad(a.getPosition(), a.getYEdgePosition(), b.getYEdgePosition(), b.getPosition());
                    if (is3ColorCornerDown(a, b, c, d) && b.getState() == analyzingState)
                    {
                        mesh.AddTriangle(b.getYEdgePosition(), a.getYEdgePosition(), c.getXEdgePosition());
                        findBorderMeshAndAdd(c.getState(), a.getYEdgePosition(), c.getXEdgePosition());
                        findBorderMeshAndAdd(d.getState(), c.getXEdgePosition(), b.getYEdgePosition());
                    }
                    else
                        findBorderMeshAndAdd(c.getState(), a.getYEdgePosition(), b.getYEdgePosition());

                    break;

                case 4:
                    mesh.AddTriangle(c.getPosition(), c.getXEdgePosition(), a.getYEdgePosition());
                    if (a.getState() != b.getState() && a.getState() != c.getState() && a.getState() != d.getState()
                       && b.getState() != c.getState() && b.getState() != d.getState()
                       && c.getState() != d.getState()
                       )//&& a.getColor() == analyzingColor)
                    {
                        var centre = new Vector2(a.getXEdgePosition().x, a.getYEdgePosition().y);
                        mesh.AddTriangle(c.getXEdgePosition(), centre, a.getYEdgePosition());
                        //AddQuad(mesh, a.getYEdgePosition(), c.getXEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());
                        findBorderMeshAndAdd(a.getState(), centre, a.getYEdgePosition());
                        findBorderMeshAndAdd(d.getState(), c.getXEdgePosition(), centre);
                    }
                    else
                        findBorderMeshAndAdd(b.getState(), c.getXEdgePosition(), a.getYEdgePosition());

                    break;

                case 5:
                    mesh.AddQuad(a.getPosition(), c.getPosition(), c.getXEdgePosition(), a.getXEdgePosition());

                    if (is3ColorCornerLeft(a, b, c, d) && c.getState() == analyzingState)
                    {
                        mesh.AddTriangle(c.getXEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());
                        findBorderMeshAndAdd(d.getState(), c.getXEdgePosition(), b.getYEdgePosition());
                        findBorderMeshAndAdd(b.getState(), b.getYEdgePosition(), a.getXEdgePosition());
                    }
                    else
                        findBorderMeshAndAdd(d.getState(), c.getXEdgePosition(), a.getXEdgePosition());
                    break;

                case 6:
                    mesh.AddTriangle(b.getPosition(), a.getXEdgePosition(), b.getYEdgePosition());
                    mesh.AddTriangle(c.getPosition(), c.getXEdgePosition(), a.getYEdgePosition());
                    mesh.AddQuad(a.getXEdgePosition(), a.getYEdgePosition(), c.getXEdgePosition(), b.getYEdgePosition());

                    findBorderMeshAndAdd(d.getState(), a.getXEdgePosition(), a.getYEdgePosition());
                    findBorderMeshAndAdd(d.getState(), c.getXEdgePosition(), b.getYEdgePosition());
                    break;

                case 7:
                    mesh.AddPentagon(a.getPosition(), c.getPosition(), c.getXEdgePosition(), b.getYEdgePosition(), b.getPosition());
                    findBorderMeshAndAdd(d.getState(), c.getXEdgePosition(), b.getYEdgePosition());

                    break;

                case 8:
                    mesh.AddTriangle(d.getPosition(), b.getYEdgePosition(), c.getXEdgePosition());
                    if (a.getState() != b.getState() && a.getState() != c.getState() && a.getState() != d.getState()
                       && b.getState() != c.getState() && b.getState() != d.getState()
                       && c.getState() != d.getState()
                       )//&& a.getColor() == analyzingColor)
                    {
                        //AddQuad(mesh, a.getYEdgePosition(), c.getXEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());
                        var centre = new Vector2(a.getXEdgePosition().x, a.getYEdgePosition().y);
                        mesh.AddTriangle(c.getXEdgePosition(), b.getYEdgePosition(), centre);
                        //AddQuad(mesh, a.getYEdgePosition(), c.getXEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());
                        findBorderMeshAndAdd(b.getState(), b.getYEdgePosition(), centre);
                        findBorderMeshAndAdd(c.getState(), centre, c.getXEdgePosition());
                    }
                    else
                        findBorderMeshAndAdd(a.getState(), b.getYEdgePosition(), c.getXEdgePosition());

                    break;

                case 9:
                    mesh.AddTriangle(a.getPosition(), a.getYEdgePosition(), a.getXEdgePosition());
                    mesh.AddTriangle(d.getPosition(), b.getYEdgePosition(), c.getXEdgePosition());
                    //duplicates quad in 6:
                    //AddQuad(mesh, a.getXEdgePosition(), a.getYEdgePosition(), c.getXEdgePosition(), b.getYEdgePosition());
                    findBorderMeshAndAdd(c.getState(), a.getYEdgePosition(), a.getXEdgePosition());
                    findBorderMeshAndAdd(c.getState(), b.getYEdgePosition(), c.getXEdgePosition());
                    break;

                case 10:
                    mesh.AddQuad(a.getXEdgePosition(), c.getXEdgePosition(), d.getPosition(), b.getPosition());
                    if (is3ColorCornerRight(a, b, c, d) && d.getState() == analyzingState)
                    {
                        mesh.AddTriangle(a.getYEdgePosition(), c.getXEdgePosition(), a.getXEdgePosition());
                        findBorderMeshAndAdd(c.getState(), a.getYEdgePosition(), c.getXEdgePosition());
                        findBorderMeshAndAdd(a.getState(), a.getXEdgePosition(), a.getYEdgePosition());
                    }
                    else
                        findBorderMeshAndAdd(c.getState(), a.getXEdgePosition(), c.getXEdgePosition());
                    break;

                case 11:
                    mesh.AddPentagon(b.getPosition(), a.getPosition(), a.getYEdgePosition(), c.getXEdgePosition(), d.getPosition());
                    findBorderMeshAndAdd(c.getState(), a.getYEdgePosition(), c.getXEdgePosition());

                    break;

                case 12:
                    mesh.AddQuad(a.getYEdgePosition(), c.getPosition(), d.getPosition(), b.getYEdgePosition());
                    if (is3ColorCornerUp(a, b, c, d) && c.getState() == analyzingState)
                    {
                        mesh.AddTriangle(a.getYEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());
                        findBorderMeshAndAdd(b.getState(), b.getYEdgePosition(), a.getXEdgePosition());
                        findBorderMeshAndAdd(a.getState(), a.getXEdgePosition(), a.getYEdgePosition());
                    }
                    else
                        findBorderMeshAndAdd(a.getState(), b.getYEdgePosition(), a.getYEdgePosition());

                    break;

                case 13:
                    mesh.AddPentagon(c.getPosition(), d.getPosition(), b.getYEdgePosition(), a.getXEdgePosition(), a.getPosition());
                    findBorderMeshAndAdd(b.getState(), b.getYEdgePosition(), a.getXEdgePosition());

                    break;

                case 14:
                    mesh.AddPentagon(d.getPosition(), b.getPosition(), a.getXEdgePosition(), a.getYEdgePosition(), c.getPosition());
                    findBorderMeshAndAdd(a.getState(), a.getXEdgePosition(), a.getYEdgePosition());

                    break;

                case 15:
                    mesh.AddQuad(a.getPosition(), c.getPosition(), d.getPosition(), b.getPosition());
                    //don't add borders here, it's inside mesh
                    break;

                default:
                    Debug.Log("Unexpected triangulation data");
                    break;
            }
            //detecting 3 color connecting
            //if (is3ColorCornerUp(a, b, c, d) && a.getColor() == analyzingColor)
            //    AddTriangle(c.getXEdgePosition(), b.getYEdgePosition(), a.getYEdgePosition());

            //if (is3ColorCornerDown(a, b, c, d) && c.getColor() == analyzingColor)
            //    AddTriangle(b.getYEdgePosition(), a.getXEdgePosition(), a.getYEdgePosition());

            //if (is3ColorCornerLeft(a, b, c, d) && c.getColor() == analyzingColor)
            //    AddTriangle(c.getXEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());

            //if (is3ColorCornerRight(a, b, c, d) && d.getColor() == analyzingColor)
            //    AddTriangle(a.getYEdgePosition(), c.getXEdgePosition(), a.getXEdgePosition());
        }

        private static bool is3ColorCornerDown(Voxel<T> a, Voxel<T> b, Voxel<T> c, Voxel<T> d)
        {
            return a.getState() == b.getState() && a.getState() != c.getState() && b.getState() != d.getState() && c.getState() != d.getState();
        }

        private static bool is3ColorCornerUp(Voxel<T> a, Voxel<T> b, Voxel<T> c, Voxel<T> d)
        {
            return c.getState() == d.getState() && c.getState() != a.getState() && d.getState() != b.getState() && a.getState() != b.getState();
        }

        private static bool is3ColorCornerLeft(Voxel<T> a, Voxel<T> b, Voxel<T> c, Voxel<T> d)
        {
            return c.getState() == a.getState() && c.getState() != d.getState() && a.getState() != b.getState() && d.getState() != b.getState();
        }

        private static bool is3ColorCornerRight(Voxel<T> a, Voxel<T> b, Voxel<T> c, Voxel<T> d)
        {
            return d.getState() == b.getState() && d.getState() != c.getState() && b.getState() != a.getState() && c.getState() != a.getState();
        }

        public Dictionary<T, MeshStructure> getBorders()
        {
            return bordersMeshes;
        }
    }
}