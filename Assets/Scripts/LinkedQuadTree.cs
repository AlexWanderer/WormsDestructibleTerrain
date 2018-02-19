using System;
using System.Collections.Generic;
using UnityEngine;

public class LinkedQuadTree<T> where T : IComparable
{
    private LinkedQuadTreeNode<T> node;
    private int depth;

    public event EventHandler quadTreeUpdated;

    public LinkedQuadTree(Vector2 position, float size, int depth)
    {
        this.depth = depth;
        node = new LinkedQuadTreeNode<T>(position, size, depth);
        //node.Subdivide(depth);
    }


    public void Insert(Vector2 position, T data)
    {
        var leafNode = node.Subdivide(position, data, depth);
        leafNode.Data = data;
        NotifyLinkedQuadTreeUpdate();
    }

    public void InsertCircle(Vector3 position, float radius, T newData)
    {
        var leafNodes = new LinkedList<LinkedQuadTreeNode<T>>();
        node.CircleSubdivide(leafNodes, position, radius, newData, depth);
        NotifyLinkedQuadTreeUpdate();
    }

    private static int GetIndexOfPosition(Vector2 lookupPosition, Vector2 nodePosition)
    {
        int index = 0;

        index |= lookupPosition.y < nodePosition.y ? 2 : 0;
        index |= lookupPosition.x > nodePosition.x ? 1 : 0;

        return index;
    }

    public LinkedQuadTreeNode<T> GetRoot()
    {
        return node;
    }

    public IEnumerable<LinkedQuadTreeNode<T>> GetLeafeNodes()
    {
        return node.GetLeafeNodes();
    }

    private void NotifyLinkedQuadTreeUpdate()
    {
        if (quadTreeUpdated != null)
        {
            quadTreeUpdated.Invoke(this, new EventArgs());
        }
    }

    public class LinkedQuadTreeNode<T> where T : IComparable
    {
        Vector2 position;
        float size;
        int depth;
        T data;
        LinkedQuadTreeNode<T>[] subNodes;

        public LinkedQuadTreeNode(Vector2 position, float size, int depth, T newData = default(T))
        {
            this.position = position;
            this.size = size;
            this.depth = depth;
            this.data = newData;
        }

        public IEnumerable<LinkedQuadTreeNode<T>> Nodes
        {
            get { return subNodes; }
        }

        public Vector2 Position
        {
            get { return position; }
        }

        public float Size
        {
            get { return size; }
        }

        public T Data
        {
            get { return data; }
            internal set { data = value; }
        }

        public LinkedQuadTreeNode<T> Subdivide(Vector2 targetPosition, T newData, int depth = 0)
        {
            if (depth == 0)
            {
                return this;
            }

            var subdivideIndex = GetIndexOfPosition(targetPosition, position);

            if (subNodes == null)
            {
                subNodes = new LinkedQuadTreeNode<T>[4];

                for (int i = 0; i < subNodes.Length; i++)
                {
                    Vector2 newPos = position;
                    if ((i & 2) == 2)
                    {
                        newPos.y -= size * 0.25f;
                    }
                    else
                    {
                        newPos.y += size * 0.25f;
                    }

                    if ((i & 1) == 1)
                    {
                        newPos.x += size * 0.25f;
                    }
                    else
                    {
                        newPos.x -= size * 0.25f;
                    }

                    subNodes[i] = new LinkedQuadTreeNode<T>(newPos, size * 0.5f, depth - 1);
                }
            }
            return subNodes[subdivideIndex].Subdivide(targetPosition, newData, depth - 1);
        }

        public void CircleSubdivide(LinkedList<LinkedQuadTreeNode<T>> selectectedNodes, Vector2 targetPosition, float radius, T newData, int depth = 0)
        {
            if (depth == 0)
            {
                Data = newData; 
                selectectedNodes.AddLast(this);
                return;
            }

            var subdivideIndex = GetIndexOfPosition(targetPosition, position);

            if (IsLeaf())
            {
                subNodes = new LinkedQuadTreeNode<T>[4];

                for (int i = 0; i < subNodes.Length; i++)
                {
                    Vector2 newPosition = position;
                    if ((i & 2) == 2)
                    {
                        newPosition.y -= size * 0.25f;
                    }
                    else
                    {
                        newPosition.y += size * 0.25f;
                    }

                    if ((i & 1) == 1)
                    {
                        newPosition.x += size * 0.25f;
                    }
                    else
                    {
                        newPosition.x -= size * 0.25f;
                    }

                    subNodes[i] = new LinkedQuadTreeNode<T>(newPosition, size * 0.5f, depth - 1, Data);
                }
            }
            for (int i = 0; i < subNodes.Length; i++)
            {
                if (subNodes[i].ContainedInCircle(targetPosition, radius))
                {
                    subNodes[i].CircleSubdivide(selectectedNodes ,targetPosition, radius, newData, depth - 1);
                }
            }

            bool shouldReduce = true;
            T initialData = subNodes[0].Data;
            for (int i = 0; i < subNodes.Length; i++)
            {
                shouldReduce &= initialData.CompareTo(subNodes[i].Data) == 0;
                shouldReduce &= subNodes[i].IsLeaf();
                if (!shouldReduce)
                {
                    break;
                }
            }
            if (shouldReduce)
            {
                Data = initialData;
                subNodes = null;
            }
        }

        public bool ContainedInCircle(Vector2 targetPosition, float radius)
        {
            Vector2 difference = position - targetPosition;
            difference.x = Mathf.Max(0, Mathf.Abs(difference.x) - size / 2);
            difference.y = Mathf.Max(0, Mathf.Abs(difference.y) - size / 2);
            return difference.magnitude < radius;
        }

        public bool IsLeaf()
        {
            return Nodes == null;
        }

        public IEnumerable<LinkedQuadTreeNode<T>> GetLeafeNodes()
        {
            if (IsLeaf())
            {
                yield return this;
            }
            else
            {
                foreach (var node in subNodes)
                {
                    foreach (var leaf in node.GetLeafeNodes())
                    {
                        yield return leaf;
                    }
                }
            }
        }
    }
}