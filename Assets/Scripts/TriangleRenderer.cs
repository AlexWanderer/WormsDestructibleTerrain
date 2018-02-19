using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class TriangleRenderer : MonoBehaviour
{
    private MeshFilter filter;

    // Use this for initialization
    void Start()
    {
        filter = GetComponent<MeshFilter>();
        filter.mesh = BuildMesh();
    }

    private Mesh BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-1,-1,0),
            new Vector3(-1,1,0),
            new Vector3(1,-1,0)
        };
        mesh.triangles = new int[] { 0, 1, 2 };
        mesh.normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back };
        mesh.colors = new Color[] { Color.red, Color.green, Color.blue };
        mesh.uv = new Vector2[] { Vector2.zero, Vector2.up, Vector2.right };
        return mesh;
    }

    // Update is called once per frame
    void Update()
    {
        if (!filter)
        {
            Start();
        }
    }
}
