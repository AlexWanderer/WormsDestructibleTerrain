using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class QuadMeshGenerator : MonoBehaviour
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

        foreach (var leaf in quadTreeComponent.QuadTree.GetLeafeNodes().Where(node => node.Data != 0))
        {
            var upperLeft = new Vector3(leaf.Position.x - leaf.Size / 2, leaf.Position.y - leaf.Size / 2, 0f);
            var initialIndex = vertices.Count;

            vertices.Add(upperLeft);
            vertices.Add(upperLeft + Vector3.right * leaf.Size);
            vertices.Add(upperLeft + Vector3.down * leaf.Size);
            vertices.Add(upperLeft + Vector3.down * leaf.Size + Vector3.right * leaf.Size);

            uvs.Add(upperLeft);
            uvs.Add(upperLeft + Vector3.right * leaf.Size);
            uvs.Add(upperLeft + Vector3.down * leaf.Size);
            uvs.Add(upperLeft + Vector3.down * leaf.Size + Vector3.right * leaf.Size);

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

            colors.Add(voxelColorCodes[leaf.Data]);
            colors.Add(voxelColorCodes[leaf.Data]);
            colors.Add(voxelColorCodes[leaf.Data]);
            colors.Add(voxelColorCodes[leaf.Data]);
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
}