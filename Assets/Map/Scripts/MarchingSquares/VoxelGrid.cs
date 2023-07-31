using Nashet.Map.Utils;
using Nashet.MeshData;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.MarchingSquares
{
    [SelectionBase]
    public class VoxelGrid : IVoxelGrid
    {
        private readonly int width, height;

        private readonly VoxelGrid xNeighbor, yNeighbor, xyNeighbor;

        private readonly Voxel[] voxels;

        private readonly float voxelSize, gridSize;

        private MeshStructure mesh;
        private Dictionary<int, MeshStructure> bordersMeshes;

        private Voxel dummyX, dummyY, dummyT;
        
        public VoxelGrid(int width, int height, float size, MyTexture texture)
        {
            this.width = width;
            this.height = height;
           
            gridSize = size;
            voxelSize = size / width;
            voxels = new Voxel[width * height];           

            dummyX = new Voxel();
            dummyY = new Voxel();
            dummyT = new Voxel();

           
            for (int i = 0, y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var Id = texture.GetPixel(x, y).ToInt();
					CreateVoxel(i, x, y, Id);
                    i++;
                }
            }
        }

		public MeshStructure getMesh(int analysingProvince, out Dictionary<int, MeshStructure> borders)
        {
            mesh = new MeshStructure();
            bordersMeshes = new Dictionary<int, MeshStructure>();
            Triangulate(analysingProvince);
            borders = bordersMeshes;
			return mesh;
        }

        private void CreateVoxel(int i, int x, int y, int state)
        {
            voxels[i] = new Voxel(x, y, voxelSize, state);
        }

        private void Triangulate(int analysingProvince)
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

        private void TriangulateCellRows(int analysingProvince)
        {
            //int cells = resolution - 1;
            for (int i = 0, y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                //if (voxels[i].getint() != int.black
                //    || voxels[i + 1].getint() != int.black
                //    || voxels[i + resolution].getint() != int.black
                //    || voxels[i + resolution + 1].getint() != int.black)
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

        private void TriangulateGapCell(int i, int analysingProvince)
        {
            Voxel dummySwap = dummyT;
            dummySwap.BecomeXDummyOf(xNeighbor.voxels[i + 1], gridSize);
            dummyT = dummyX;
            dummyX = dummySwap;
            TriangulateCell(voxels[i], dummyT, voxels[i + width], dummyX, analysingProvince);
        }

        private void TriangulateGapRow(int analysingProvince)
        {
            dummyY.BecomeYDummyOf(yNeighbor.voxels[0], gridSize);
            //int cells = width - 1;
            //int offset = cells * resolution;
            int offset = (width - 1) * width;

            for (int x = 0; x < width - 1; x++)
            {
                Voxel dummySwap = dummyT;
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

        private bool isBorderCell(Voxel a, Voxel b, Voxel c, Voxel d)
        {
            return !(a.getState() == b.getState() && a.getState() == c.getState() && a.getState() == d.getState());
        }

        private void findBorderMeshAndAdd(int province, Vector2 a, Vector2 b)
        {            
           MeshStructure border;
           if (!bordersMeshes.TryGetValue(province, out border))
           {
                border = new MeshStructure();
                 bordersMeshes.Add(province, border);
           }
           border.AddBorderQuad2(a, b);
           
        }

        private void TriangulateCell(Voxel a, Voxel b, Voxel c, Voxel d, int analyzingState)
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
                        )//&& c.getint() == analyzingint)
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
                       )//&& c.getint() == analyzingint)
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
                    if (is3intCornerDown(a, b, c, d) && b.getState() == analyzingState)
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
                       )//&& a.getint() == analyzingint)
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

                    if (is3intCornerLeft(a, b, c, d) && c.getState() == analyzingState)
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
                       )//&& a.getint() == analyzingint)
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
                    if (is3intCornerRight(a, b, c, d) && d.getState() == analyzingState)
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
                    if (is3intCornerUp(a, b, c, d) && c.getState() == analyzingState)
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
            //detecting 3 int connecting
            //if (is3intCornerUp(a, b, c, d) && a.getint() == analyzingint)
            //    AddTriangle(c.getXEdgePosition(), b.getYEdgePosition(), a.getYEdgePosition());

            //if (is3intCornerDown(a, b, c, d) && c.getint() == analyzingint)
            //    AddTriangle(b.getYEdgePosition(), a.getXEdgePosition(), a.getYEdgePosition());

            //if (is3intCornerLeft(a, b, c, d) && c.getint() == analyzingint)
            //    AddTriangle(c.getXEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());

            //if (is3intCornerRight(a, b, c, d) && d.getint() == analyzingint)
            //    AddTriangle(a.getYEdgePosition(), c.getXEdgePosition(), a.getXEdgePosition());
        }

        private static bool is3intCornerDown(Voxel a, Voxel b, Voxel c, Voxel d)
        {
            return a.getState() == b.getState() && a.getState() != c.getState() && b.getState() != d.getState() && c.getState() != d.getState();
        }

        private static bool is3intCornerUp(Voxel a, Voxel b, Voxel c, Voxel d)
        {
            return c.getState() == d.getState() && c.getState() != a.getState() && d.getState() != b.getState() && a.getState() != b.getState();
        }

        private static bool is3intCornerLeft(Voxel a, Voxel b, Voxel c, Voxel d)
        {
            return c.getState() == a.getState() && c.getState() != d.getState() && a.getState() != b.getState() && d.getState() != b.getState();
        }

        private static bool is3intCornerRight(Voxel a, Voxel b, Voxel c, Voxel d)
        {
            return d.getState() == b.getState() && d.getState() != c.getState() && b.getState() != a.getState() && c.getState() != a.getState();
        }
    }
}