using UnityEngine;
using System.Collections.Generic;
using System;

[SelectionBase]
public class VoxelGrid
{
    private readonly int width, height;

    private readonly VoxelGrid xNeighbor, yNeighbor, xyNeighbor;

    private readonly Voxel[] voxels;

    private readonly float voxelSize, gridSize;

    private MeshStructure mesh;
    private Dictionary<Province, MeshStructure> bordersMeshes;


    private Voxel dummyX, dummyY, dummyT;
    private readonly Game game;

    public VoxelGrid(int width, int height, float size, MyTexture texture, List<Province> blockedProvinces, Game game)
    {
        this.width = width;
        this.height = height;
        this.game = game;
        // this.resolution = resolution;
        gridSize = size;
        voxelSize = size / width;
        voxels = new Voxel[width * height];
        //voxelMaterials = new Material[voxels.Length];

        dummyX = new Voxel();
        dummyY = new Voxel();
        dummyT = new Voxel();

        //analyzingColor = color;
        Color curColor, x1y1Color, x2y1Color, x1y2Color, x2y2Color;
        for (int i = 0, y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                curColor = texture.GetPixel(x, y);
                //if (!blockedProvinces.Contains(curColor))
                CreateVoxel(i, x, y, curColor);
                i++;
            }
        }

        //for (int i = 0, y = 0; y < resolution; y++)
        //{
        //    for (int x = 0; x < resolution ; x++, i++)
        //    {
        //        x1y1Color = texture.GetPixel(x, y);
        //        x2y1Color = texture.GetPixel(x + 1, y);
        //        x1y2Color = texture.GetPixel(x, y + 1);
        //        x2y2Color = texture.GetPixel(x + 1, y + 1);

        //        if (!blockedProvinces.Contains(x1y1Color)
        //            || !blockedProvinces.Contains(x2y1Color)
        //            || !blockedProvinces.Contains(x1y2Color)
        //            || !blockedProvinces.Contains(x2y2Color)
        //            )
        //            CreateVoxel(i, x, y, x1y1Color);
        //        else
        //            CreateVoxel(i, x, y, Color.black);
        //    }
        //}

    }
    public MeshStructure getMesh(Color colorID)
    {
        mesh = new MeshStructure();
        bordersMeshes = new Dictionary<Province, MeshStructure>();
        game.updateStatus("Triangulation .." + colorID);
        Triangulate(colorID);
        return mesh;
    }
    private void CreateVoxel(int i, int x, int y, Color state)
    {
        //GameObject o = Instantiate(voxelPrefab) as GameObject;
        //o.transform.parent = transform;
        //o.transform.localPosition = new Vector3((x + 0.5f) * voxelSize, (y + 0.5f) * voxelSize, -0.01f);
        //o.transform.localScale = Vector3.one * voxelSize * 0.1f;
        //voxelMaterials[i] = o.GetComponent<MeshRenderer>().material;
        voxels[i] = new Voxel(x, y, voxelSize, state);

    }



    private void Triangulate(Color colorID)
    {

        //mesh.Clear();

        if (xNeighbor != null)
        {
            dummyX.BecomeXDummyOf(xNeighbor.voxels[0], gridSize);
        }
        TriangulateCellRows(colorID);
        if (yNeighbor != null)
        {
            TriangulateGapRow(colorID);
        }

        //mesh.vertices = vertices.ToArray();
        //mesh.triangles = triangles.ToArray();
    }

    private void TriangulateCellRows(Color colorID)
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
                    voxels[i + width + 1], colorID);
                i++;
            }
            if (xNeighbor != null)
            {
                TriangulateGapCell(i, colorID);
            }
            i++;
        }
    }

    private void TriangulateGapCell(int i, Color colorID)
    {
        Voxel dummySwap = dummyT;
        dummySwap.BecomeXDummyOf(xNeighbor.voxels[i + 1], gridSize);
        dummyT = dummyX;
        dummyX = dummySwap;
        TriangulateCell(voxels[i], dummyT, voxels[i + width], dummyX, colorID);
    }

    private void TriangulateGapRow(Color colorID)
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
            TriangulateCell(voxels[x + offset], voxels[x + offset + 1], dummyT, dummyY, colorID);
        }

        if (xNeighbor != null)
        {
            dummyT.BecomeXYDummyOf(xyNeighbor.voxels[0], gridSize);
            TriangulateCell(voxels[voxels.Length - 1], dummyX, dummyY, dummyT, colorID);
        }
    }
    private bool isBorderCell(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        return !(a.getColor() == b.getColor() && a.getColor() == c.getColor() && a.getColor() == d.getColor());
    }

    private void findBorderMeshAndAdd(Color color, Vector2 a, Vector2 b)
    {
        var province = Province.find(color);
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
    private void TriangulateCell(Voxel a, Voxel b, Voxel c, Voxel d, Color analyzingColor)
    {


        //bool isBorder = isBorderCell(a, b, c, d);
        int cellType = 0;
        if (a.getColor() == analyzingColor)
        {
            cellType |= 1;
        }
        if (b.getColor() == analyzingColor)
        {
            cellType |= 2;
        }
        if (c.getColor() == analyzingColor)
        {
            cellType |= 4;
        }
        if (d.getColor() == analyzingColor)
        {
            cellType |= 8;
        }
        switch (cellType)
        {
            case 0:
                return;
            case 1:
                mesh.AddTriangle(a.getPosition(), a.getYEdgePosition(), a.getXEdgePosition());

                if (a.getColor() != b.getColor() && a.getColor() != c.getColor() && a.getColor() != d.getColor()
                    && b.getColor() != c.getColor() && b.getColor() != d.getColor()
                    && c.getColor() != d.getColor()
                    )//&& c.getColor() == analyzingColor)
                {
                    var centre = new Vector2(a.getXEdgePosition().x, a.getYEdgePosition().y);
                    mesh.AddTriangle(a.getYEdgePosition(), centre, a.getXEdgePosition());
                    //AddQuad(mesh, a.getYEdgePosition(), c.getXEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());
                    findBorderMeshAndAdd(c.getColor(), a.getYEdgePosition(), centre);
                    findBorderMeshAndAdd(b.getColor(), centre, a.getXEdgePosition());
                }
                else
                    findBorderMeshAndAdd(d.getColor(), a.getYEdgePosition(), a.getXEdgePosition());

                break;
            case 2:
                mesh.AddTriangle(b.getPosition(), a.getXEdgePosition(), b.getYEdgePosition());
                if (a.getColor() != b.getColor() && a.getColor() != c.getColor() && a.getColor() != d.getColor()
                   && b.getColor() != c.getColor() && b.getColor() != d.getColor()
                   && c.getColor() != d.getColor()
                   )//&& c.getColor() == analyzingColor)
                {
                    var centre = new Vector2(a.getXEdgePosition().x, a.getYEdgePosition().y);
                    mesh.AddTriangle(a.getXEdgePosition(), centre, b.getYEdgePosition());
                    findBorderMeshAndAdd(a.getColor(), a.getXEdgePosition(), centre);
                    findBorderMeshAndAdd(d.getColor(), centre, b.getYEdgePosition());
                }
                else
                    findBorderMeshAndAdd(c.getColor(), a.getXEdgePosition(), b.getYEdgePosition());

                break;
            case 3:
                mesh.AddQuad(a.getPosition(), a.getYEdgePosition(), b.getYEdgePosition(), b.getPosition());
                if (is3ColorCornerDown(a, b, c, d) && b.getColor() == analyzingColor)
                {
                    mesh.AddTriangle(b.getYEdgePosition(), a.getYEdgePosition(), c.getXEdgePosition());
                    findBorderMeshAndAdd(c.getColor(), a.getYEdgePosition(), c.getXEdgePosition());
                    findBorderMeshAndAdd(d.getColor(), c.getXEdgePosition(), b.getYEdgePosition());
                }
                else
                    findBorderMeshAndAdd(c.getColor(), a.getYEdgePosition(), b.getYEdgePosition());

                break;
            case 4:
                mesh.AddTriangle(c.getPosition(), c.getXEdgePosition(), a.getYEdgePosition());
                if (a.getColor() != b.getColor() && a.getColor() != c.getColor() && a.getColor() != d.getColor()
                   && b.getColor() != c.getColor() && b.getColor() != d.getColor()
                   && c.getColor() != d.getColor()
                   )//&& a.getColor() == analyzingColor)
                {
                    var centre = new Vector2(a.getXEdgePosition().x, a.getYEdgePosition().y);
                    mesh.AddTriangle(c.getXEdgePosition(), centre, a.getYEdgePosition());
                    //AddQuad(mesh, a.getYEdgePosition(), c.getXEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());
                    findBorderMeshAndAdd(a.getColor(), centre, a.getYEdgePosition());
                    findBorderMeshAndAdd(d.getColor(), c.getXEdgePosition(), centre);

                }
                else
                    findBorderMeshAndAdd(b.getColor(), c.getXEdgePosition(), a.getYEdgePosition());

                break;
            case 5:
                mesh.AddQuad(a.getPosition(), c.getPosition(), c.getXEdgePosition(), a.getXEdgePosition());

                if (is3ColorCornerLeft(a, b, c, d) && c.getColor() == analyzingColor)
                {
                    mesh.AddTriangle(c.getXEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());
                    findBorderMeshAndAdd(d.getColor(), c.getXEdgePosition(), b.getYEdgePosition());
                    findBorderMeshAndAdd(b.getColor(), b.getYEdgePosition(), a.getXEdgePosition());
                }
                else
                    findBorderMeshAndAdd(d.getColor(), c.getXEdgePosition(), a.getXEdgePosition());
                break;
            case 6:
                mesh.AddTriangle(b.getPosition(), a.getXEdgePosition(), b.getYEdgePosition());
                mesh.AddTriangle(c.getPosition(), c.getXEdgePosition(), a.getYEdgePosition());
                mesh.AddQuad(a.getXEdgePosition(), a.getYEdgePosition(), c.getXEdgePosition(), b.getYEdgePosition());

                findBorderMeshAndAdd(d.getColor(),a.getXEdgePosition(), a.getYEdgePosition());
                findBorderMeshAndAdd(d.getColor(),c.getXEdgePosition(), b.getYEdgePosition());
                break;
            case 7:
                mesh.AddPentagon(a.getPosition(), c.getPosition(), c.getXEdgePosition(), b.getYEdgePosition(), b.getPosition());
                findBorderMeshAndAdd(d.getColor(),c.getXEdgePosition(), b.getYEdgePosition());

                break;
            case 8:
                mesh.AddTriangle(d.getPosition(), b.getYEdgePosition(), c.getXEdgePosition());
                if (a.getColor() != b.getColor() && a.getColor() != c.getColor() && a.getColor() != d.getColor()
                   && b.getColor() != c.getColor() && b.getColor() != d.getColor()
                   && c.getColor() != d.getColor()
                   )//&& a.getColor() == analyzingColor)
                {
                    //AddQuad(mesh, a.getYEdgePosition(), c.getXEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());
                    var centre = new Vector2(a.getXEdgePosition().x, a.getYEdgePosition().y);
                    mesh.AddTriangle(c.getXEdgePosition(), b.getYEdgePosition(), centre);
                    //AddQuad(mesh, a.getYEdgePosition(), c.getXEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());
                    findBorderMeshAndAdd(b.getColor(),b.getYEdgePosition(), centre);
                    findBorderMeshAndAdd(c.getColor(),centre, c.getXEdgePosition());
                }
                else
                    findBorderMeshAndAdd(a.getColor(),b.getYEdgePosition(), c.getXEdgePosition());

                break;
            case 9:
                mesh.AddTriangle(a.getPosition(), a.getYEdgePosition(), a.getXEdgePosition());
                mesh.AddTriangle(d.getPosition(), b.getYEdgePosition(), c.getXEdgePosition());
                //duplicates quad in 6:
                //AddQuad(mesh, a.getXEdgePosition(), a.getYEdgePosition(), c.getXEdgePosition(), b.getYEdgePosition());
                findBorderMeshAndAdd(c.getColor(),a.getYEdgePosition(), a.getXEdgePosition());
                findBorderMeshAndAdd(c.getColor(),b.getYEdgePosition(), c.getXEdgePosition());
                break;
            case 10:
                mesh.AddQuad(a.getXEdgePosition(), c.getXEdgePosition(), d.getPosition(), b.getPosition());
                if (is3ColorCornerRight(a, b, c, d) && d.getColor() == analyzingColor)
                {
                    mesh.AddTriangle(a.getYEdgePosition(), c.getXEdgePosition(), a.getXEdgePosition());
                    findBorderMeshAndAdd(c.getColor(),a.getYEdgePosition(), c.getXEdgePosition());
                    findBorderMeshAndAdd(a.getColor(),a.getXEdgePosition(), a.getYEdgePosition());
                }
                else
                    findBorderMeshAndAdd(c.getColor(),a.getXEdgePosition(), c.getXEdgePosition());
                break;
            case 11:
                mesh.AddPentagon(b.getPosition(), a.getPosition(), a.getYEdgePosition(), c.getXEdgePosition(), d.getPosition());
                findBorderMeshAndAdd(c.getColor(),a.getYEdgePosition(), c.getXEdgePosition());

                break;
            case 12:
                mesh.AddQuad(a.getYEdgePosition(), c.getPosition(), d.getPosition(), b.getYEdgePosition());
                if (is3ColorCornerUp(a, b, c, d) && c.getColor() == analyzingColor)
                {
                    mesh.AddTriangle(a.getYEdgePosition(), b.getYEdgePosition(), a.getXEdgePosition());
                    findBorderMeshAndAdd(b.getColor(),b.getYEdgePosition(), a.getXEdgePosition());
                    findBorderMeshAndAdd(a.getColor(),a.getXEdgePosition(), a.getYEdgePosition());
                }
                else
                    findBorderMeshAndAdd(a.getColor(),b.getYEdgePosition(), a.getYEdgePosition());

                break;
            case 13:
                mesh.AddPentagon(c.getPosition(), d.getPosition(), b.getYEdgePosition(), a.getXEdgePosition(), a.getPosition());
                findBorderMeshAndAdd(b.getColor(),b.getYEdgePosition(), a.getXEdgePosition());

                break;
            case 14:
                mesh.AddPentagon(d.getPosition(), b.getPosition(), a.getXEdgePosition(), a.getYEdgePosition(), c.getPosition());
                findBorderMeshAndAdd(a.getColor(),a.getXEdgePosition(), a.getYEdgePosition());

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
    private static bool is3ColorCornerDown(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        return a.getColor() == b.getColor() && a.getColor() != c.getColor() && b.getColor() != d.getColor() && c.getColor() != d.getColor();
    }
    private static bool is3ColorCornerUp(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        return c.getColor() == d.getColor() && c.getColor() != a.getColor() && d.getColor() != b.getColor() && a.getColor() != b.getColor();
    }
    private static bool is3ColorCornerLeft(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        return c.getColor() == a.getColor() && c.getColor() != d.getColor() && a.getColor() != b.getColor() && d.getColor() != b.getColor();
    }
    private static bool is3ColorCornerRight(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        return d.getColor() == b.getColor() && d.getColor() != c.getColor() && b.getColor() != a.getColor() && c.getColor() != a.getColor();
    }
    internal Dictionary<Province, MeshStructure> getBorders()
    {
        return bordersMeshes;
    }
}