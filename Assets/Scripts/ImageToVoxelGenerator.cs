using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageToVoxelGenerator : MonoBehaviour
{
    public QuadTreeComponent quadTreeComponent;
    public Texture2D image;
    public Color threshhold;

    // Use this for initialization
    void Start()
    {
        Generate();
    }

    private void Generate()
    {
        int cells = (int)Mathf.Pow(2, quadTreeComponent.depth);
        for (int y = 0; y <= cells; y++)
        {
            for (int x = 0; x <= cells; x++)
            {
                Vector2 position = quadTreeComponent.transform.position;
                position.x += (x - cells / 2) / (float)cells * quadTreeComponent.size;
                position.y += (y - cells / 2) / (float)cells * quadTreeComponent.size;

                Color pixel = image.GetPixelBilinear(x / (float)cells, y / (float)cells);
                if (pixel != threshhold)
                {
                    quadTreeComponent.QuadTree.InsertCircle(position, 0.001f, 1);
                }
            }
        }
    }
}
