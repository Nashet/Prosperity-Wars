using UnityEngine;
using System.Collections.Generic;
using System;

[SelectionBase]
public class VoxelGrid : MonoBehaviour
{

    public int resolution;

    public GameObject voxelPrefab;

    public VoxelGrid xNeighbor, yNeighbor, xyNeighbor;

    private Voxel[] voxels;

    private float voxelSize, gridSize;

    private Material[] voxelMaterials;

    private MeshStructure mesh;
    private Dictionary<Color, MeshStructure> bordersMeshes;
    //private List<Vector3> vertices;
    //private List<int> triangles;

    private Voxel dummyX, dummyY, dummyT;
    //private Color analyzingColor;
    public void Initialize(int resolution, float size, Texture2D texture)//, Color color)
    {
        this.resolution = resolution;
        gridSize = size;
        voxelSize = size / resolution;
        voxels = new Voxel[resolution * resolution];
        voxelMaterials = new Material[voxels.Length];

        dummyX = new Voxel();
        dummyY = new Voxel();
        dummyT = new Voxel();

        //analyzingColor = color;

        for (int i = 0, y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++, i++)
            {
                CreateVoxel(i, x, y, texture.GetPixel(x, y));
            }
        }

        //GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        // mesh = new MeshStructure();
        //mesh.name = "VoxelGrid Mesh";
        //vertices = new List<Vector3>();
        //triangles = new List<int>();

    }
    public MeshStructure getMesh(Color colorID)
    {
        mesh = new MeshStructure();
        bordersMeshes = new Dictionary<Color, MeshStructure>();
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
        int cells = resolution - 1;
        for (int i = 0, y = 0; y < cells; y++, i++)
        {
            for (int x = 0; x < cells; x++, i++)
            {
                TriangulateCell(
                    voxels[i],
                    voxels[i + 1],
                    voxels[i + resolution],
                    voxels[i + resolution + 1], colorID);
            }
            if (xNeighbor != null)
            {
                TriangulateGapCell(i, colorID);
            }
        }
    }

    private void TriangulateGapCell(int i, Color colorID)
    {
        Voxel dummySwap = dummyT;
        dummySwap.BecomeXDummyOf(xNeighbor.voxels[i + 1], gridSize);
        dummyT = dummyX;
        dummyX = dummySwap;
        TriangulateCell(voxels[i], dummyT, voxels[i + resolution], dummyX, colorID);
    }

    private void TriangulateGapRow(Color colorID)
    {
        dummyY.BecomeYDummyOf(yNeighbor.voxels[0], gridSize);
        int cells = resolution - 1;
        int offset = cells * resolution;

        for (int x = 0; x < cells; x++)
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
        return !(a.color == b.color && a.color == c.color && a.color == d.color);
    }
    private void makeBorderQuad(Voxel a, Voxel b)
    {

    }
    private MeshStructure findMesh(Color b)
    {
        MeshStructure border;
        if (!bordersMeshes.TryGetValue(b, out border))
        {
            border = new MeshStructure();
            bordersMeshes.Add(b, new MeshStructure());
        }
        return border;
    }
    private void TriangulateCell(Voxel a, Voxel b, Voxel c, Voxel d, Color analyzingColor)
    {
        // put to constant
        float borderWidth = 0.4f;
        float borderWidth2 = -0.4f;


        bool isBorder = isBorderCell(a, b, c, d);
        int cellType = 0;
        if (a.color == analyzingColor)
        {
            cellType |= 1;
        }
        if (b.color == analyzingColor)
        {
            cellType |= 2;
        }
        if (c.color == analyzingColor)
        {
            cellType |= 4;
        }
        if (d.color == analyzingColor)
        {
            cellType |= 8;
        }
        switch (cellType)
        {
            case 0:
                return;
            case 1:
                AddTriangle(a.position, a.yEdgePosition, a.xEdgePosition);

                AddBorderQuad(findMesh(b.color),
                (Vector3)a.yEdgePosition,
                MeshExtensions.makeArrow(a.yEdgePosition, a.xEdgePosition, borderWidth),
                (Vector3)a.xEdgePosition,
                MeshExtensions.makeArrow(a.xEdgePosition, a.yEdgePosition, borderWidth2),
                true
                );
                break;
            case 2:
                AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);

                AddBorderQuad(findMesh(c.color),
               (Vector3)a.xEdgePosition,
               MeshExtensions.makeArrow(a.xEdgePosition, b.yEdgePosition, borderWidth),
               (Vector3)b.yEdgePosition,
               MeshExtensions.makeArrow(b.yEdgePosition, a.xEdgePosition, borderWidth2),
               true
               );
                break;
            case 3:
                AddQuad(mesh, a.position, a.yEdgePosition, b.yEdgePosition, b.position, true);

                AddBorderQuad(findMesh(c.color),
             (Vector3)a.yEdgePosition,
             MeshExtensions.makeArrow(a.yEdgePosition, b.yEdgePosition, borderWidth),
             (Vector3)b.yEdgePosition,
             MeshExtensions.makeArrow(b.yEdgePosition, a.yEdgePosition, borderWidth2),
             true
             );
                break;
            case 4:
                AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);

                AddBorderQuad(findMesh(b.color),
                (Vector3)c.xEdgePosition,
                MeshExtensions.makeArrow(c.xEdgePosition, a.yEdgePosition, borderWidth),
                (Vector3)a.yEdgePosition,
                MeshExtensions.makeArrow(a.yEdgePosition, c.xEdgePosition, borderWidth2),
                true
                );
                break;
            case 5:
                AddQuad(mesh, a.position, c.position, c.xEdgePosition, a.xEdgePosition, true);
                break;
            case 6:
                AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);
                AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);
                break;
            case 7:
                AddPentagon(a.position, c.position, c.xEdgePosition, b.yEdgePosition, b.position);
                break;
            case 8:
                AddTriangle(d.position, b.yEdgePosition, c.xEdgePosition);

                AddBorderQuad(findMesh(b.color),
               (Vector3)b.yEdgePosition,
               MeshExtensions.makeArrow(b.yEdgePosition, c.xEdgePosition, borderWidth),
               (Vector3)c.xEdgePosition,
               MeshExtensions.makeArrow(c.xEdgePosition, b.yEdgePosition, borderWidth2),
               true
               );
                break;
            case 9:
                AddTriangle(a.position, a.yEdgePosition, a.xEdgePosition);
                AddTriangle(d.position, b.yEdgePosition, c.xEdgePosition);
                break;
            case 10:
                AddQuad(mesh, a.xEdgePosition, c.xEdgePosition, d.position, b.position, true);
                break;
            case 11:
                AddPentagon(b.position, a.position, a.yEdgePosition, c.xEdgePosition, d.position);
                break;
            case 12:
                AddQuad(mesh, a.yEdgePosition, c.position, d.position, b.yEdgePosition, true);

                AddBorderQuad(findMesh(b.color),            
           
            (Vector3)b.yEdgePosition,
            MeshExtensions.makeArrow(b.yEdgePosition, a.yEdgePosition, borderWidth),
             (Vector3)a.yEdgePosition,
            MeshExtensions.makeArrow(a.yEdgePosition, b.yEdgePosition, borderWidth2),
            true
            );
                break;
            case 13:
                AddPentagon(c.position, d.position, b.yEdgePosition, a.xEdgePosition, a.position);
                break;
            case 14:
                AddPentagon(d.position, b.position, a.xEdgePosition, a.yEdgePosition, c.position);
                break;
            case 15:
                AddQuad(mesh, a.position, c.position, d.position, b.position, true);
                break;
        }
        //detecting 3 color connecting
        if (is3ColorCornerUp(a, b, c, d) && a.color == analyzingColor)
            AddTriangle(c.xEdgePosition, b.yEdgePosition, a.yEdgePosition);

        if (is3ColorCornerDown(a, b, c, d) && c.color == analyzingColor)
            AddTriangle(b.yEdgePosition, a.xEdgePosition, a.yEdgePosition);

        if (is3ColorCornerLeft(a, b, c, d) && c.color == analyzingColor)
            AddTriangle(c.xEdgePosition, b.yEdgePosition, a.xEdgePosition);

        if (is3ColorCornerRight(a, b, c, d) && d.color == analyzingColor)
            AddTriangle(a.yEdgePosition, c.xEdgePosition, a.xEdgePosition);

        if (a.color != b.color && a.color != c.color && a.color != d.color
            && b.color != c.color && b.color != d.color
            && c.color != d.color
            && a.color == analyzingColor)
            AddQuad(mesh, a.yEdgePosition, c.xEdgePosition, b.yEdgePosition, a.xEdgePosition, true);
    }
    private bool is3ColorCornerUp(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        return a.color == b.color && a.color != c.color && a.color != d.color && c.color != d.color;
    }
    private bool is3ColorCornerDown(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        return c.color == d.color && c.color != a.color && c.color != b.color && a.color != b.color;
    }
    private bool is3ColorCornerLeft(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        return c.color == a.color && c.color != d.color && a.color != b.color && d.color != b.color;
    }
    private bool is3ColorCornerRight(Voxel a, Voxel b, Voxel c, Voxel d)
    {
        return d.color == b.color && d.color != c.color && b.color != a.color && c.color != a.color;
    }
    private void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        int vertexIndex = mesh.vertices.Count;
        mesh.vertices.Add(a);
        mesh.vertices.Add(b);
        mesh.vertices.Add(c);
        mesh.triangles.Add(vertexIndex);
        mesh.triangles.Add(vertexIndex + 1);
        mesh.triangles.Add(vertexIndex + 2);
    }

    private void AddQuad(MeshStructure targetMesh, Vector3 a, Vector3 b, Vector3 c, Vector3 d, bool addUV)
    {
        int vertexIndex = targetMesh.vertices.Count;
        targetMesh.vertices.Add(a);
        targetMesh.vertices.Add(b);
        targetMesh.vertices.Add(c);
        targetMesh.vertices.Add(d);
        targetMesh.triangles.Add(vertexIndex);
        targetMesh.triangles.Add(vertexIndex + 1);
        targetMesh.triangles.Add(vertexIndex + 2);
        targetMesh.triangles.Add(vertexIndex);
        targetMesh.triangles.Add(vertexIndex + 2);
        targetMesh.triangles.Add(vertexIndex + 3);

        if (addUV)
        {
            targetMesh.UVmap.Add(new Vector2(0f, 1f));
            targetMesh.UVmap.Add(new Vector2(1f, 1f));
            targetMesh.UVmap.Add(new Vector2(0f, 0f));
            targetMesh.UVmap.Add(new Vector2(1f, 0f));
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
    private void AddBorderQuad(MeshStructure targetMesh, Vector3 a, Vector3 b, Vector3 c, Vector3 d, bool addUV)
    {
        float borderHeight = 0.1f;
        a = a + Vector3.back * borderHeight;
        b = b + Vector3.back * borderHeight;
        c = c + Vector3.back * borderHeight;
        d = d + Vector3.back * borderHeight;

        int vertexIndex = targetMesh.vertices.Count;
        targetMesh.vertices.Add(a);
        targetMesh.vertices.Add(b);
        targetMesh.vertices.Add(c);
        targetMesh.vertices.Add(d);
        targetMesh.triangles.Add(vertexIndex);
        targetMesh.triangles.Add(vertexIndex + 2);
        targetMesh.triangles.Add(vertexIndex + 1);
        targetMesh.triangles.Add(vertexIndex + 2);
        targetMesh.triangles.Add(vertexIndex + 3);
        targetMesh.triangles.Add(vertexIndex + 1);

        if (addUV)
        {
            targetMesh.UVmap.Add(new Vector2(0f, 1f));
            targetMesh.UVmap.Add(new Vector2(1f, 1f));
            targetMesh.UVmap.Add(new Vector2(0f, 0f));
            targetMesh.UVmap.Add(new Vector2(1f, 0f));
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
    private void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
    {
        int vertexIndex = mesh.vertices.Count;
        mesh.vertices.Add(a);
        mesh.vertices.Add(b);
        mesh.vertices.Add(c);
        mesh.vertices.Add(d);
        mesh.vertices.Add(e);
        mesh.triangles.Add(vertexIndex);
        mesh.triangles.Add(vertexIndex + 1);
        mesh.triangles.Add(vertexIndex + 2);
        mesh.triangles.Add(vertexIndex);
        mesh.triangles.Add(vertexIndex + 2);
        mesh.triangles.Add(vertexIndex + 3);
        mesh.triangles.Add(vertexIndex);
        mesh.triangles.Add(vertexIndex + 3);
        mesh.triangles.Add(vertexIndex + 4);
    }

    private void SetVoxelColors()
    {
        for (int i = 0; i < voxels.Length; i++)
        {
            //  voxelMaterials[i].color = voxels[i].state ? Color.black : Color.white;
        }
    }

    internal Dictionary<Color, MeshStructure> getBorders()
    {
        return bordersMeshes;
    }

    //public void Apply(VoxelStencil stencil)
    //{
    //    int xStart = stencil.XStart;
    //    if (xStart < 0)
    //    {
    //        xStart = 0;
    //    }
    //    int xEnd = stencil.XEnd;
    //    if (xEnd >= resolution)
    //    {
    //        xEnd = resolution - 1;
    //    }
    //    int yStart = stencil.YStart;
    //    if (yStart < 0)
    //    {
    //        yStart = 0;
    //    }
    //    int yEnd = stencil.YEnd;
    //    if (yEnd >= resolution)
    //    {
    //        yEnd = resolution - 1;
    //    }

    //    for (int y = yStart; y <= yEnd; y++)
    //    {
    //        int i = y * resolution + xStart;
    //        for (int x = xStart; x <= xEnd; x++, i++)
    //        {
    //            voxels[i].state = stencil.Apply(x, y, voxels[i].state);
    //        }
    //    }
    //    Refresh();
    //}
}