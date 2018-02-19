using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreePencil : MonoBehaviour
{
    public QuadTreeComponent quadTreeComponent;
    public int value = 1;

    // Update is called once per frame
    void Update()
    {
        var insertionPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    
        if (Input.GetMouseButton(0))
        {
            quadTreeComponent.QuadTree.InsertCircle(insertionPoint, 0.01f, 0);
        }
        else if (Input.GetMouseButton(1))
        {
            quadTreeComponent.QuadTree.InsertCircle(insertionPoint, 0.01f, value);
    
        }
    }
}
