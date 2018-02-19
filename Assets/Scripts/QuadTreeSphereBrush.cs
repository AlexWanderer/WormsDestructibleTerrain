using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeSphereBrush : MonoBehaviour
{
    public QuadTreeComponent quadTreeComponent;
    public float radius = 0.5f;
    public int value = 1;

    // Update is called once per frame
    void Update()
    {
        var insertionPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            quadTreeComponent.QuadTree.InsertCircle(insertionPoint, radius, 0);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            quadTreeComponent.QuadTree.InsertCircle(insertionPoint, radius, value);

        }
    }
}
