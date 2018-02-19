using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum MarchingSqaresPatterns
{
    Empty =                 00,//0000,
    TopLeftCorner =         08,//1000,
    TopRightCorner =        04,//0100,
    BottomLeftCorner =      02,//0010,
    BottomRightCorner =     01,//0001,
    TopSide =               12,//1100,
    RightSide =             05,//0101,
    BottomSide =            03,//0011,
    LeftSide =              10,//1010,
    DiagonalLeftToRight =   09,//1001,
    DiagonalRightToLeft =   06,//0110,
    TopLeftEmpty =          07,//0111,
    TopRightEmpty =         11,//1011,
    BottomLeftEmpty =       13,//1101,
    BottomRightEmpty =      14,//1110,
    Full =                  15,//1111,
}

public class MarchingSquaresMeshGenerator : MonoBehaviour
{
    public bool generate;
    public QuadTreeComponent quadTreeComponent;
    public Material voxelMaterial;

    private GameObject chunk;
    private bool initialize;

    private readonly Color[] voxelColorCodes = new Color[] {
        Color.clear,
        Color.red,
        Color.green,
        Color.blue,
    };

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (quadTreeComponent.QuadTree != null && !initialize)
        {
            quadTreeComponent.QuadTree.quadTreeUpdated += (obj, args) => { generate = true; };
            initialize = true;
        }
        if (generate)
        {
            Destroy(chunk);
            Generate();
            generate = false;
        }
    }

    private void Generate()
    {
        chunk = new GameObject("Voxel Chunk");
        chunk.transform.SetParent(transform);
        chunk.transform.localPosition = Vector3.zero;

        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();
        var normals = new List<Vector3>();
        var colors = new List<Color>();

        var leafs = quadTreeComponent.QuadTree.GetLeafeNodes().OrderBy(node => node.Position.x).ThenBy(node => node.Position.y).ToArray();
        int size = (int)Mathf.Pow(2, quadTreeComponent.depth) - 1;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int top = x + y * (size + 1);
                int bottom = x + ((y + 1) * (size + 1));

                var topLeft = leafs[top];
                var topRight = leafs[top + 1];
                var bottomLeft = leafs[bottom];
                var bottomRight = leafs[bottom + 1];
                InsertMesh(
                    new QuadTree<int>.QuadTreeNode<int>[] { topLeft, topRight, bottomLeft, bottomRight },
                    vertices, triangles, uvs, normals, colors);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.SetNormals(normals);
        mesh.SetColors(colors);

        var meshFilter = chunk.AddComponent<MeshFilter>();
        var meshRenderer = chunk.AddComponent<MeshRenderer>();

        meshRenderer.material = voxelMaterial;
        meshFilter.mesh = mesh;
    }

    private void InsertMesh(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors)
    {
        int type = 0;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].Data != 0)
            {
                type |= 1 << 3 - i;
            }
        }
        switch ((MarchingSqaresPatterns)type)
        {
            case MarchingSqaresPatterns.Full:
                BuildSquareFull(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.TopLeftEmpty:
                BuildSquareTopLeftEmpty(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.TopRightEmpty:
                BuildSquareTopRightEmpty(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.BottomLeftEmpty:
                BuildSquareBottomLeftEmpty(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.BottomRightEmpty:
                BuildSquareBottomRightEmpty(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.LeftSide:
                BuildSquareLeftSide(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.TopSide:
                BuildSquareTopSide(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.RightSide:
                BuildSquareRightSide(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.BottomSide:
                BuildSquareBottomSide(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.DiagonalLeftToRight:
                BuildSquareDiagonalLeftToRight(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.DiagonalRightToLeft:
                BuildSquareDiagonalRightToLeft(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.TopLeftCorner:
                BuildSquareTopLeftCorner(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.TopRightCorner:
                BuildSquareTopRightCorner(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.BottomLeftCorner:
                BuildSquareBottomLeftCorner(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.BottomRightCorner:
                BuildSquareBottomRightCorner(points, vertices, uvs, normals, triangles, colors);
                break;
            case MarchingSqaresPatterns.Empty:
            default:
                BuildSquareFull(points, vertices, uvs, normals, triangles, colors);
                break;
        }
    }

    private void BuildSquareFull(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(points[0].Position);
        vertices.Add(points[1].Position);
        vertices.Add(points[2].Position);
        vertices.Add(points[3].Position);

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareTopLeftEmpty(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[0].Position, points[2].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(points[1].Position);
        vertices.Add(points[2].Position);
        vertices.Add(points[3].Position);

        uvs.Add(upperLeft);
        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareTopRightEmpty(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[0].Position, points[2].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(points[1].Position);
        vertices.Add(points[2].Position);
        vertices.Add(points[3].Position);

        uvs.Add(upperLeft);
        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareBottomLeftEmpty(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[0].Position, points[2].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(points[1].Position);
        vertices.Add(points[2].Position);
        vertices.Add(points[3].Position);

        uvs.Add(upperLeft);
        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 3);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareBottomRightEmpty(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(points[1].Position);
        vertices.Add(Vector2.Lerp(points[3].Position, points[1].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[3].Position, points[2].Position, 0.5f));
        vertices.Add(points[2].Position);
        vertices.Add(points[0].Position);

        uvs.Add(upperLeft);
        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 3);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareLeftSide(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(points[0].Position);
        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(points[2].Position);
        vertices.Add(Vector2.Lerp(points[2].Position, points[3].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareTopSide(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(points[0].Position);
        vertices.Add(points[1].Position);
        vertices.Add(Vector2.Lerp(points[0].Position, points[2].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareBottomSide(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[0].Position, points[2].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));
        vertices.Add(points[3].Position);
        vertices.Add(points[4].Position);

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareRightSide(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(points[1].Position);
        vertices.Add(Vector2.Lerp(points[2].Position, points[3].Position, 0.5f));
        vertices.Add(points[3].Position);

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareTopLeftCorner(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(points[0].Position);
        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[0].Position, points[2].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareTopRightCorner(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(points[1].Position);
        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareBottomLeftCorner(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[0].Position, points[2].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[3].Position, points[2].Position, 0.5f));
        vertices.Add(points[2].Position);

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareBottomRightCorner(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));
        vertices.Add(points[3].Position);
        vertices.Add(Vector2.Lerp(points[2].Position, points[3].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareDiagonalLeftToRight(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(points[0].Position);
        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));
        vertices.Add(points[3].Position);
        vertices.Add(Vector2.Lerp(points[3].Position, points[2].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 5);

        triangles.Add(initialIndex + 5);
        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 3);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }

    private void BuildSquareDiagonalRightToLeft(QuadTree<int>.QuadTreeNode<int>[] points, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, List<Color> colors)
    {
        var upperLeft = new Vector3(points[0].Position.x - points[0].Size / 2, points[0].Position.y - points[0].Size / 2, 0f);
        var initialIndex = vertices.Count;

        vertices.Add(points[1].Position);
        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[3].Position, points[2].Position, 0.5f));
        vertices.Add(points[2].Position);
        vertices.Add(Vector2.Lerp(points[0].Position, points[2].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 5);

        triangles.Add(initialIndex + 5);
        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 3);

        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
        colors.Add(voxelColorCodes[1]);
    }
}
