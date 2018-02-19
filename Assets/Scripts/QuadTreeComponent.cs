using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeComponent : MonoBehaviour
{

    public float size = 5;
    public int depth = 2;

    private Color minColor = new Color(1f, 1f, 1f, 1f);
    private Color maxColor = new Color(0f, 0.5f, 1f, 0.25f);

    private QuadTree<int> quadTree;

    public QuadTree<int> QuadTree { get { return quadTree; } }

    void Awake()
    {
        quadTree = new QuadTree<int>(this.transform.position, size, depth);
    }

    void OnDrawGizmos()
    {
        if (quadTree != null)
        {
            foreach (var node  in quadTree.Nodes)
            {
                DrawNode(node);
            }
        }
    }
    
    private void DrawNode(QuadTree<int>.QuadTreeNode<int> node)
    {
        Gizmos.color = Color.Lerp(minColor, maxColor, node.Depth / (float)depth);
        Gizmos.DrawWireCube(node.Position, new Vector3(1, 1, 0.1f) * node.Size);
    }
}
