using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuadTreeIndex
{
    TopLeft = 1, //00,
    TopRight = 2, //01,
    BottomLeft = 3, //10,
    BottomRight = 4, //11,
}

public class QuadTree<T> where T : IComparable
{
    private readonly QuadTreeNode<T>[] nodes;
    private int maxDepth;

    public event EventHandler quadTreeUpdated;

    public QuadTree(Vector2 position, float size, int depth)
    {
        maxDepth = depth;
        nodes = BuildQuadTree(position, size);
    }

    private QuadTreeNode<T>[] BuildQuadTree(Vector2 position, float size)
    {
        int lenght = 0;
        for (int i = 0; i <= maxDepth; i++)
        {
            lenght += (int)Mathf.Pow(4, i);
        }
        var tree = new QuadTreeNode<T>[lenght];

        tree[0] = new QuadTreeNode<T>(position, size, 0);
        BuildQuadTreeRecursive(tree, 0);
        return tree;
    }

    private void BuildQuadTreeRecursive(QuadTreeNode<T>[] tree, int index)
    {
        if (tree[index].Depth >= maxDepth)
        {
            return;
        }

        int nextNode = 4 * index;
        Vector2 deltaX = new Vector2(tree[index].Size / 4, 0);
        Vector2 deltaY = new Vector2(0, tree[index].Size / 4);
        tree[nextNode + 1] = new QuadTreeNode<T>(tree[index].Position - deltaX + deltaY, tree[index].Size / 2, tree[index].Depth + 1);
        tree[nextNode + 2] = new QuadTreeNode<T>(tree[index].Position + deltaX + deltaY, tree[index].Size / 2, tree[index].Depth + 1);
        tree[nextNode + 3] = new QuadTreeNode<T>(tree[index].Position - deltaX - deltaY, tree[index].Size / 2, tree[index].Depth + 1);
        tree[nextNode + 4] = new QuadTreeNode<T>(tree[index].Position + deltaX - deltaY, tree[index].Size / 2, tree[index].Depth + 1);
        BuildQuadTreeRecursive(tree, nextNode + 1);
        BuildQuadTreeRecursive(tree, nextNode + 2);
        BuildQuadTreeRecursive(tree, nextNode + 3);
        BuildQuadTreeRecursive(tree, nextNode + 4);
    }

    public void InsertCircle(Vector3 position, float radius, T newData)
    {
        var leafNodes = new LinkedList<QuadTreeNode<T>>();
        CircleSearch(leafNodes, position, radius, nodes, 0);
        foreach (var node in leafNodes)
        {
            node.Data = newData;
        }
        NotifyQuadTreeUpdate();
    }

    private static int GetIndexOfPosition(Vector2 lookupPosition, Vector2 nodePosition)
    {
        int index = 0;

        index |= lookupPosition.y < nodePosition.y ? 2 : 0;
        index |= lookupPosition.x > nodePosition.x ? 1 : 0;

        return index;
    }

    public IEnumerable<QuadTreeNode<T>> GetLeafeNodes()
    {
        int leafNodes = (int)Mathf.Pow(4, maxDepth);
        for (int i = nodes.Length - leafNodes; i < nodes.Length; i++)
        {
            yield return nodes[i];
        }
    }

    private void NotifyQuadTreeUpdate()
    {
        if (quadTreeUpdated != null)
        {
            quadTreeUpdated.Invoke(this, new EventArgs());
        }
    }

    public void CircleSearch(LinkedList<QuadTreeNode<T>> selectectedNodes, Vector2 targetPosition, float radius, QuadTreeNode<T>[] tree, int index)
    {
        if (tree[index].Depth >= maxDepth)
        {
            selectectedNodes.AddLast(tree[index]);
            return;
        }

        int nextNode = 4 * index;
        for (int i = 1; i <= 4; i++)
        {
            if (ContainedInCircle(targetPosition, radius, tree[nextNode + i]))
            {
                CircleSearch(selectectedNodes, targetPosition, radius, tree, nextNode + i);
            }
        }
    }

    public bool ContainedInCircle(Vector2 targetPosition, float radius, QuadTreeNode<T> node)
    {
        Vector2 difference = node.Position - targetPosition;
        difference.x = Mathf.Max(0, Mathf.Abs(difference.x) - node.Size / 2);
        difference.y = Mathf.Max(0, Mathf.Abs(difference.y) - node.Size / 2);
        return difference.magnitude < radius;
    }

    public class QuadTreeNode<T> where T : IComparable
    {
        Vector2 position;
        float size;
        int depth; // floor(log(i) / log(4))
        T data;

        public QuadTreeNode(Vector2 position, float size, int depth, T newData = default(T))
        {
            this.position = position;
            this.size = size;
            this.depth = depth;
            this.data = newData;
        }

        public Vector2 Position
        {
            get { return position; }
        }

        public float Size
        {
            get { return size; }
        }

        public int Depth
        {
            get { return depth; }
        }

        public T Data
        {
            get { return data; }
            internal set { data = value; }
        }
    }

    public QuadTreeNode<T>[] Nodes
    {
        get { return nodes; }
    }
}